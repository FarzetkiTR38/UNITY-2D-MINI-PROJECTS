#if UNITY_EDITOR
using UnityEditor;
using MCPForUnity.Editor.Services;

namespace ArrowSwarm.Editor
{
    /// <summary>
    /// Automatically connects Unity Editor to the MCP bridge upon domain reload.
    /// </summary>
    [InitializeOnLoad]
    public static class MCPAutoConnector
    {
        static MCPAutoConnector()
        {
            EditorApplication.delayCall += TryConnect;
        }

        private static async void TryConnect()
        {
            try
            {
                if (!MCPServiceLocator.Bridge.IsRunning)
                {
                    await MCPServiceLocator.Bridge.StartAsync();
                }
            }
            catch
            {
                // Ignore if already active or during compile
            }
        }
    }
}
#endif
