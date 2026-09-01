namespace ArrowSwarm.Editor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Editor utility to register the most commonly used phone resolutions into both
    /// Unity GameView and Unity Device Simulator with an underscore '_' prefix to stay on top.
    /// </summary>
    [InitializeOnLoad]
    public static class TestPhoneResolutionManager
    {
        private struct ResolutionData
        {
            public string Label;
            public string FileName;
            public int Width;
            public int Height;
            public float Dpi;

            public ResolutionData(string label, string fileName, int width, int height, float dpi)
            {
                Label = label;
                FileName = fileName;
                Width = width;
                Height = height;
                Dpi = dpi;
            }
        }

        private static readonly ResolutionData[] PhoneResolutions = new ResolutionData[]
        {
            new ResolutionData("_TestPhone1 (1080x1920) (9:16)", "_TestPhone1_1080x1920_9x16", 1080, 1920, 400f),
            new ResolutionData("_TestPhone2 (1080x2400) (9:20)", "_TestPhone2_1080x2400_9x20", 1080, 2400, 400f),
            new ResolutionData("_TestPhone3 (1080x2340) (9:19.5)", "_TestPhone3_1080x2340_9x19.5", 1080, 2340, 400f),
            new ResolutionData("_TestPhone4 (1170x2532) (9:19.5)", "_TestPhone4_1170x2532_9x19.5", 1170, 2532, 460f),
            new ResolutionData("_TestPhone5 (1179x2556) (9:19.5)", "_TestPhone5_1179x2556_9x19.5", 1179, 2556, 460f),
            new ResolutionData("_TestPhone6 (1290x2796) (9:19.5)", "_TestPhone6_1290x2796_9x19.5", 1290, 2796, 460f),
            new ResolutionData("_TestPhone7 (720x1600) (9:20)", "_TestPhone7_720x1600_9x20", 720, 1600, 270f),
            new ResolutionData("_TestPhone8 (1440x3088) (9:19.3)", "_TestPhone8_1440x3088_9x19.3", 1440, 3088, 500f),
            new ResolutionData("_TestPhone9 (720x1280) (9:16)", "_TestPhone9_720x1280_9x16", 720, 1280, 270f),
            new ResolutionData("_TestPhone10 (1080x2460) (9:20.5)", "_TestPhone10_1080x2460_9x20.5", 1080, 2460, 400f),
            new ResolutionData("_TestPhone11 (1284x2778) (9:19.5)", "_TestPhone11_1284x2778_9x19.5", 1284, 2778, 460f),
            new ResolutionData("_TestPhone12 (828x1792) (9:19.5)", "_TestPhone12_828x1792_9x19.5", 828, 1792, 326f),
            new ResolutionData("_TestPhone13 (1080x2160) (9:18)", "_TestPhone13_1080x2160_9x18", 1080, 2160, 400f),
            new ResolutionData("_TestPhone14 (1536x2048) (3:4)", "_TestPhone14_1536x2048_3x4", 1536, 2048, 264f),
            new ResolutionData("_TestPhone15 (1440x2560) (9:16)", "_TestPhone15_1440x2560_9x16", 1440, 2560, 515f),
        };

        static TestPhoneResolutionManager()
        {
            EditorApplication.delayCall += SetupAll;
        }

        /// <summary>
        /// Registers test phone resolutions into GameView and creates Device Simulator assets.
        /// </summary>
        [MenuItem("Tools/Arrow Swarm/Setup Test Phones (GameView & Simulator)")]
        public static void SetupAll()
        {
            AddAllGameViewResolutions();
            CreateSimulatorDevices();
        }

        /// <summary>
        /// Registers all test phone resolutions into Unity GameView sizes.
        /// </summary>
        public static void AddAllGameViewResolutions()
        {
            try
            {
                Type gvSizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                Type singleType = typeof(ScriptableSingleton<>).MakeGenericType(gvSizesType);
                object instance = singleType.GetProperty("instance")?.GetValue(null, null);
                if (instance == null) return;

                MethodInfo getGroupMethod = gvSizesType.GetMethod("GetGroup");
                Type gvSizeType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSize");
                Type gvSizeTypeEnum = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                object fixedResTypeVal = Enum.Parse(gvSizeTypeEnum, "FixedResolution");
                ConstructorInfo ctor = gvSizeType.GetConstructor(new Type[] { gvSizeTypeEnum, typeof(int), typeof(int), typeof(string) });

                GameViewSizeGroupType[] groups = new[] { GameViewSizeGroupType.Standalone, GameViewSizeGroupType.Android, GameViewSizeGroupType.iOS };
                int addedCount = 0;

                foreach (GameViewSizeGroupType groupType in groups)
                {
                    object group = getGroupMethod.Invoke(instance, new object[] { groupType });
                    if (group == null) continue;

                    MethodInfo getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
                    MethodInfo addCustomSize = group.GetType().GetMethod("AddCustomSize");
                    string[] existingTexts = (string[])getDisplayTexts.Invoke(group, null) ?? Array.Empty<string>();

                    foreach (ResolutionData res in PhoneResolutions)
                    {
                        if (existingTexts.Any(t => t.StartsWith(res.Label, StringComparison.OrdinalIgnoreCase))) continue;
                        object newSize = ctor.Invoke(new object[] { fixedResTypeVal, res.Width, res.Height, res.Label });
                        addCustomSize.Invoke(group, new object[] { newSize });
                        addedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    gvSizesType.GetMethod("SaveToHDD")?.Invoke(instance, null);
                    Debug.Log($"[ArrowSwarm] Added {addedCount} _TestPhone resolutions to Unity GameView.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] GameView resolution error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates .device definition files in Assets/_Project/Devices for Device Simulator.
        /// </summary>
        public static void CreateSimulatorDevices()
        {
            try
            {
                string folder = Path.Combine(Application.dataPath, "_Project", "Devices");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                bool createdAny = false;
                foreach (ResolutionData res in PhoneResolutions)
                {
                    string filePath = Path.Combine(folder, $"{res.FileName}.device");
                    string json = GenerateDeviceJson(res);
                    File.WriteAllText(filePath, json);
                    createdAny = true;
                }

                if (createdAny)
                {
                    AssetDatabase.Refresh();
                    Debug.Log("[ArrowSwarm] Created 15 _TestPhone devices for Unity Device Simulator.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Device Simulator generation error: {ex.Message}");
            }
        }

        private static string GenerateDeviceJson(ResolutionData res)
        {
            string dpiStr = res.Dpi.ToString("F1", CultureInfo.InvariantCulture);
            string wStr = res.Width.ToString("F1", CultureInfo.InvariantCulture);
            string hStr = res.Height.ToString("F1", CultureInfo.InvariantCulture);

            return "{\n" +
                   $"    \"friendlyName\": \"{res.Label}\",\n" +
                   "    \"version\": 1,\n" +
                   "    \"screens\": [\n" +
                   "        {\n" +
                   $"            \"width\": {res.Width},\n" +
                   $"            \"height\": {res.Height},\n" +
                   "            \"navigationBarHeight\": 0,\n" +
                   $"            \"dpi\": {dpiStr},\n" +
                   "            \"orientations\": [\n" +
                   "                {\n" +
                   "                    \"orientation\": 1,\n" +
                   "                    \"safeArea\": { \"serializedVersion\": \"2\", \"x\": 0.0, \"y\": 0.0, \"width\": " + wStr + ", \"height\": " + hStr + " },\n" +
                   "                    \"cutouts\": []\n" +
                   "                },\n" +
                   "                {\n" +
                   "                    \"orientation\": 2,\n" +
                   "                    \"safeArea\": { \"serializedVersion\": \"2\", \"x\": 0.0, \"y\": 0.0, \"width\": " + wStr + ", \"height\": " + hStr + " },\n" +
                   "                    \"cutouts\": []\n" +
                   "                },\n" +
                   "                {\n" +
                   "                    \"orientation\": 3,\n" +
                   "                    \"safeArea\": { \"serializedVersion\": \"2\", \"x\": 0.0, \"y\": 0.0, \"width\": " + hStr + ", \"height\": " + wStr + " },\n" +
                   "                    \"cutouts\": []\n" +
                   "                },\n" +
                   "                {\n" +
                   "                    \"orientation\": 4,\n" +
                   "                    \"safeArea\": { \"serializedVersion\": \"2\", \"x\": 0.0, \"y\": 0.0, \"width\": " + hStr + ", \"height\": " + wStr + " },\n" +
                   "                    \"cutouts\": []\n" +
                   "                }\n" +
                   "            ],\n" +
                   "            \"presentation\": {\n" +
                   "                \"overlayPath\": \"\",\n" +
                   "                \"borderSize\": { \"x\": 0.0, \"y\": 0.0, \"z\": 0.0, \"w\": 0.0 },\n" +
                   "                \"cornerRadius\": 0.0\n" +
                   "            }\n" +
                   "        }\n" +
                   "    ],\n" +
                   "    \"systemInfo\": {\n" +
                   $"        \"deviceModel\": \"{res.FileName}\",\n" +
                   "        \"deviceType\": 1,\n" +
                   "        \"operatingSystem\": \"Android OS 14\",\n" +
                   "        \"operatingSystemFamily\": 0,\n" +
                   "        \"processorCount\": 8,\n" +
                   "        \"processorFrequency\": 0,\n" +
                   "        \"processorType\": \"arm64\",\n" +
                   "        \"supportsAccelerometer\": true,\n" +
                   "        \"supportsAudio\": true,\n" +
                   "        \"supportsGyroscope\": true,\n" +
                   "        \"supportsLocationService\": true,\n" +
                   "        \"supportsVibration\": true,\n" +
                   "        \"systemMemorySize\": 6000,\n" +
                   "        \"graphicsDependentData\": [\n" +
                   "            {\n" +
                   "                \"graphicsDeviceType\": 21,\n" +
                   "                \"graphicsMemorySize\": 4096,\n" +
                   "                \"graphicsDeviceName\": \"Adreno\",\n" +
                   "                \"graphicsDeviceVendor\": \"Qualcomm\",\n" +
                   "                \"graphicsDeviceID\": 0,\n" +
                   "                \"graphicsDeviceVendorID\": 0,\n" +
                   "                \"graphicsUVStartsAtTop\": true,\n" +
                   "                \"graphicsDeviceVersion\": \"Vulkan 1.1\",\n" +
                   "                \"graphicsShaderLevel\": 50,\n" +
                   "                \"graphicsMultiThreaded\": true,\n" +
                   "                \"renderingThreadingMode\": 0,\n" +
                   "                \"hasHiddenSurfaceRemovalOnGPU\": true,\n" +
                   "                \"hasDynamicUniformArrayIndexingInFragmentShaders\": true,\n" +
                   "                \"supportsShadows\": true,\n" +
                   "                \"supportsRawShadowDepthSampling\": true,\n" +
                   "                \"supportsMotionVectors\": true,\n" +
                   "                \"supports3DTextures\": true,\n" +
                   "                \"supports2DArrayTextures\": true,\n" +
                   "                \"supports3DRenderTextures\": true,\n" +
                   "                \"supportsCubemapArrayTextures\": true,\n" +
                   "                \"copyTextureSupport\": 31,\n" +
                   "                \"supportsComputeShaders\": true,\n" +
                   "                \"supportsGeometryShaders\": true,\n" +
                   "                \"supportsTessellationShaders\": true,\n" +
                   "                \"supportsInstancing\": true,\n" +
                   "                \"supportsHardwareQuadTopology\": false,\n" +
                   "                \"supports32bitsIndexBuffer\": true,\n" +
                   "                \"supportsSparseTextures\": false,\n" +
                   "                \"supportedRenderTargetCount\": 8,\n" +
                   "                \"supportsSeparatedRenderTargetsBlend\": true,\n" +
                   "                \"supportedRandomWriteTargetCount\": 8,\n" +
                   "                \"supportsMultisampledTextures\": 1,\n" +
                   "                \"supportsMultisampleAutoResolve\": true,\n" +
                   "                \"supportsTextureWrapMirrorOnce\": 1,\n" +
                   "                \"usesReversedZBuffer\": true,\n" +
                   "                \"npotSupport\": 2,\n" +
                   "                \"maxTextureSize\": 16384,\n" +
                   "                \"maxCubemapSize\": 16384,\n" +
                   "                \"maxComputeBufferInputsVertex\": 524288,\n" +
                   "                \"maxComputeBufferInputsFragment\": 524288,\n" +
                   "                \"maxComputeBufferInputsGeometry\": 524288,\n" +
                   "                \"maxComputeBufferInputsDomain\": 524288,\n" +
                   "                \"maxComputeBufferInputsHull\": 524288,\n" +
                   "                \"maxComputeBufferInputsCompute\": 524288,\n" +
                   "                \"maxComputeWorkGroupSize\": 1024,\n" +
                   "                \"maxComputeWorkGroupSizeX\": 1024,\n" +
                   "                \"maxComputeWorkGroupSizeY\": 1024,\n" +
                   "                \"maxComputeWorkGroupSizeZ\": 64,\n" +
                   "                \"supportsAsyncCompute\": false,\n" +
                   "                \"supportsGraphicsFence\": true,\n" +
                   "                \"supportsAsyncGPUReadback\": true,\n" +
                   "                \"supportsRayTracing\": false,\n" +
                   "                \"supportsSetConstantBuffer\": true,\n" +
                   "                \"minConstantBufferOffsetAlignment\": false,\n" +
                   "                \"hasMipMaxLevel\": true,\n" +
                   "                \"supportsMipStreaming\": true,\n" +
                   "                \"usesLoadStoreActions\": true,\n" +
                   "                \"supportedTextureFormats\": [1, 2, 3, 4, 5, 7, 9, 13, 14, 15, 16, 17, 18, 19, 20, 22, 34, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74],\n" +
                   "                \"supportedRenderTextureFormats\": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28],\n" +
                   "                \"ldrGraphicsFormat\": 8,\n" +
                   "                \"hdrGraphicsFormat\": 74\n" +
                   "            }\n" +
                   "        ]\n" +
                   "    }\n" +
                   "}";
        }
    }
}
