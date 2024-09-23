using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class VRScreenCap : MonoBehaviour
{
    private string fileLocation;
    private string fileName;
    private ScreenCapture screenCapture;

    public event Action OnScreenshotTaken;

    void Start()
    {
        screenCapture = FindObjectOfType<ScreenCapture>();
    }

    public async void TakeScreenshotAsync()
    {
        await Task.Run(() => TakeScreenshot());
    }

    public void TakeScreenshot()
    {
        if (ConnectionManager.Instance.ActiveDevice != null && ReportManager.Instance.ActiveReport != null)
        {
            if (screenCapture != null)
            {
                // Ensure CaptureScreen is called on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Texture2D screenshot = screenCapture.CaptureScreen();
                    if (screenshot != null)
                    {
                        var report = ReportManager.Instance.ActiveReport;
                        string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                        string screenshotPathOnPC = Application.dataPath + $"/Report/{report.ReportName}/{timestamp}.png";

                        byte[] bytes = screenshot.EncodeToPNG();
                        File.WriteAllBytes(screenshotPathOnPC, bytes);

                        Debug.Log("Picture saved!");

                        // Add screenshot to JSON
                        ReportManager.Instance.AddScreenshotToJson(report.ReportName, screenshotPathOnPC, timestamp);

                        // Add description to JSON
                        ReportManager.Instance.CreateReportDescriptionText("Add a description here", timestamp);

                        OnScreenshotTaken?.Invoke();
                    }
                    else
                    {
                        Debug.LogError("Failed to capture screen.");
                    }
                });
            }
            else
            {
                Debug.LogError("ScreenCapture component not found.");
            }
        }
        else
        {
            Debug.Log("No Connected Device or Active Report!");
        }
    }

    public void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log($"Directory created at: {path}");
        }
        else
        {
            Debug.Log($"Directory already exists at: {path}");
        }
    }
}