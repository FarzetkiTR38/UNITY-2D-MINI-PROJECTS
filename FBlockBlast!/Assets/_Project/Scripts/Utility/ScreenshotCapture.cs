using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

public class ScreenshotCapture : MonoBehaviour
{
    public int superSize = 2;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            string folder = Path.Combine(Application.dataPath, "../Screenshots");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = "game_ss_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
            string path = Path.Combine(folder, fileName);

            ScreenCapture.CaptureScreenshot(path, superSize);

            Debug.Log("Screenshot saved: " + path);
        }
    }
}
