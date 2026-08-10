using System;
using System.IO;
using UnityEngine;
using NeonGalaxy.Utility;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Handles device gallery interaction, image cropping, and local file management
    /// for custom profile pictures. Single responsibility: image I/O and processing.
    /// 
    /// Does NOT depend on MonoBehaviour — can be registered in ServiceLocator.
    /// NativeGallery callbacks are handled via static delegates.
    /// </summary>
    public class ProfilePictureService
    {
        // ── Constants ───────────────────────────────────────────
        private const string CUSTOM_AVATAR_FILENAME = "custom_avatar.png";
        private const int MAX_TEXTURE_SIZE = 512; // Max resolution for saved avatar
        private const int MIN_TEXTURE_SIZE = 64;  // Minimum acceptable resolution

        // ── Cached State ────────────────────────────────────────
        private Sprite _cachedSprite;
        private Texture2D _cachedTexture;

        // ── Events ──────────────────────────────────────────────

        /// <summary>
        /// Fired when a new profile picture has been picked and saved successfully.
        /// The string parameter is the local file path.
        /// </summary>
        public event Action<string> OnPictureSaved;

        // ── Public API ──────────────────────────────────────────

        /// <summary>
        /// Opens the device gallery to pick a profile picture.
        /// On success, crops the image to a center-square and saves it locally.
        /// Invokes OnPictureSaved with the local path when complete.
        /// </summary>
        public void PickImageFromGallery()
        {
#if UNITY_EDITOR
            // In Editor, use NativeGallery which supports Editor file dialog
            PickWithNativeGallery();
#elif UNITY_ANDROID || UNITY_IOS
            PickWithNativeGallery();
#else
            Debug.LogWarning("[ProfilePictureService] Gallery picking not supported on this platform.");
#endif
        }

        /// <summary>
        /// Returns true if a custom profile picture exists on disk.
        /// </summary>
        public bool HasCustomAvatar()
        {
            string path = GetLocalAvatarPath();
            return File.Exists(path);
        }

        /// <summary>
        /// Returns the local file path for the custom avatar.
        /// </summary>
        public string GetLocalAvatarPath()
        {
            return Path.Combine(Application.persistentDataPath, CUSTOM_AVATAR_FILENAME);
        }

        /// <summary>
        /// Loads the custom avatar as a Sprite from local storage.
        /// Returns null if no custom avatar exists.
        /// Uses caching to avoid repeated disk reads.
        /// </summary>
        public Sprite GetCustomAvatarSprite()
        {
            if (_cachedSprite != null) return _cachedSprite;

            string path = GetLocalAvatarPath();
            if (!File.Exists(path)) return null;

            _cachedSprite = LoadSpriteFromFile(path);
            return _cachedSprite;
        }

        /// <summary>
        /// Clears the cached sprite, forcing a reload on next access.
        /// Call this after the avatar file has been updated.
        /// </summary>
        public void InvalidateCache()
        {
            if (_cachedTexture != null)
            {
                UnityEngine.Object.Destroy(_cachedTexture);
                _cachedTexture = null;
            }
            _cachedSprite = null;
        }

        /// <summary>
        /// Reads the custom avatar file from local storage and converts it to a Base64 string.
        /// Suitable for uploading public avatar data to UGS Cloud Save.
        /// </summary>
        public string GetCustomAvatarBase64()
        {
            string path = GetLocalAvatarPath();
            if (!File.Exists(path)) return null;

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProfilePictureService] Failed to get Base64: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Converts a Base64 string back into a Unity Sprite (for other players' avatars loaded from cloud).
        /// </summary>
        public Sprite Base64ToSprite(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                if (tex.LoadImage(bytes))
                {
                    return Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProfilePictureService] Base64ToSprite failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Deletes the custom avatar file and clears cache.
        /// </summary>
        public void DeleteCustomAvatar()
        {
            string path = GetLocalAvatarPath();
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[ProfilePictureService] Custom avatar deleted.");
            }
            InvalidateCache();
        }

        // ── Gallery Integration ─────────────────────────────────

        private void PickWithNativeGallery()
        {
            // NativeGallery handles permissions internally when calling GetImageFromGallery.
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.Log("[ProfilePictureService] Gallery pick cancelled by user.");
                    return;
                }

                Debug.Log($"[ProfilePictureService] Image picked: {path}");
                ProcessAndSaveImage(path);
            }, "Select Profile Picture");
        }

        // ── Image Processing ────────────────────────────────────

        /// <summary>
        /// Loads the picked image, center-crops to square, resizes, and saves locally.
        /// </summary>
        private void ProcessAndSaveImage(string sourcePath)
        {
            try
            {
                // Load texture from file
                Texture2D sourceTexture = NativeGallery.LoadImageAtPath(sourcePath, MAX_TEXTURE_SIZE);
                if (sourceTexture == null)
                {
                    Debug.LogError("[ProfilePictureService] Failed to load image from gallery.");
                    return;
                }

                // GPU-accelerated center crop + resize (100% readable, works on any JPG/PNG)
                Texture2D finalTexture = CropAndResizeToSquare(sourceTexture, MAX_TEXTURE_SIZE);

                // Save to persistent data path as PNG
                string destPath = GetLocalAvatarPath();
                byte[] pngBytes = finalTexture.EncodeToPNG();
                File.WriteAllBytes(destPath, pngBytes);

                Debug.Log($"[ProfilePictureService] Avatar saved successfully to: {destPath} " +
                          $"({finalTexture.width}x{finalTexture.height})");

                // Cleanup temporary textures
                UnityEngine.Object.Destroy(sourceTexture);
                if (finalTexture != sourceTexture)
                    UnityEngine.Object.Destroy(finalTexture);

                // Invalidate cache so the new image is loaded immediately
                InvalidateCache();

                // Notify listeners
                OnPictureSaved?.Invoke(destPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfilePictureService] ProcessAndSaveImage failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Center-crops and resizes a texture to a square using GPU (Graphics.Blit).
        /// Completely avoids CPU GetPixels() calls, eliminating Texture Unreadable errors.
        /// </summary>
        private Texture2D CropAndResizeToSquare(Texture2D source, int targetSize)
        {
            int width = source.width;
            int height = source.height;
            int minDim = Mathf.Min(width, height);

            // Calculate UV scale and offset for center cropping
            float scaleX = (float)minDim / width;
            float scaleY = (float)minDim / height;
            float offsetX = (1f - scaleX) * 0.5f;
            float offsetY = (1f - scaleY) * 0.5f;

            Vector2 scale = new Vector2(scaleX, scaleY);
            Vector2 offset = new Vector2(offsetX, offsetY);

            int finalSize = Mathf.Min(minDim, targetSize);

            RenderTexture rt = RenderTexture.GetTemporary(finalSize, finalSize, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            // Blit with scale and offset (center crop + resize directly on GPU)
            Graphics.Blit(source, rt, scale, offset);

            Texture2D result = new Texture2D(finalSize, finalSize, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, finalSize, finalSize), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        // ── File Loading ────────────────────────────────────────

        /// <summary>
        /// Loads a sprite from a file path on disk.
        /// </summary>
        private Sprite LoadSpriteFromFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;

                byte[] bytes = File.ReadAllBytes(path);
                _cachedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                _cachedTexture.filterMode = FilterMode.Bilinear;

                if (_cachedTexture.LoadImage(bytes))
                {
                    return Sprite.Create(
                        _cachedTexture,
                        new Rect(0, 0, _cachedTexture.width, _cachedTexture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProfilePictureService] Failed to load sprite: {ex.Message}");
            }

            return null;
        }
    }
}
