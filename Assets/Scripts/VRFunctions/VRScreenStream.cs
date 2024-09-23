// VRScreenStream.cs
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class VRScreenStream : MonoBehaviour
{
    private Process scrcpyProcess;
    public ScreenCapture screenCapture; // Reference to the ScreenCapture component
    public string bitRate = "16";

    // Start the screen mirroring process with scrcpy
    public void StartStreaming()
    {
        if (scrcpyProcess != null && !scrcpyProcess.HasExited)
        {
            UnityEngine.Debug.Log("Streaming is already running.");
            return;
        }

        // Reset hwnd in ScreenCapture
        if (screenCapture != null)
        {
            screenCapture.ResetHwnd();
        }

        new Thread(StartScreenMirroring).Start();
        UnityEngine.Debug.Log("Streaming Started");
    }

    private void StartScreenMirroring()
    {
        if (ConnectionManager.Instance.ActiveDevice == null)
        {
            UnityEngine.Debug.LogError("No connected device.");
            return;
        }

        var ipAddress = ConnectionManager.Instance.ActiveDevice.IPAddress;
        var port = ConnectionManager.Instance.ActiveDevice.Port;
        string scrcpyPath = Application.dataPath + "/StreamingAssets/ADB/scrcpy.exe";
        string cropArguments = $"-s {ipAddress}:{port} --crop={CalculateCropSetting()} --video-bit-rate {bitRate}M";

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = scrcpyPath,
            Arguments = cropArguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        scrcpyProcess = Process.Start(startInfo);
    }

    public void StopStreaming()
    {
        if (scrcpyProcess != null && !scrcpyProcess.HasExited)
        {
            scrcpyProcess.Kill();
            scrcpyProcess = null;
            UnityEngine.Debug.Log("Streaming Stopped");
        }
        else
        {
            UnityEngine.Debug.Log("No streaming process is running.");
        }
    }

    private string CalculateCropSetting()
    {
        string cropString = string.Empty;

        var modelName = ConnectionManager.Instance.ActiveDevice.ModelName;

        switch (modelName)
        {
            case "Quest 2":
                cropString = "2050:1153:1900:537";
                //cropString = "1832:1920:75:450";
                break;
            case "Quest 3":
                //cropString = "1832:1920:250:0";
                //cropString = "1832:1920:2200:540";
                cropString = "2050:1100:2100:840";
                break;
            case "Quest":
                cropString = "1280:720:1500:350";
                break;
            case "Quest Pro":
                cropString = "1600:900:100:600";
                break;
            default:
                cropString = "default";
                break;
        }

        return cropString;
    }
}
