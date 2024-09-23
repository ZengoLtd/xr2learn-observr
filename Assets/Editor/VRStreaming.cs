using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public class VRStreaming : EditorWindow
{
    private Process adbProcess;
    private Process scrcpyProcess;
    public static string sdkPath;
    public static string ffmpegPath = Application.dataPath + "/StreamingAssets/ffmpeg/ffmpeg.exe";

    [SerializeField]
    public RenderTexture renderTexture;
    private UdpClient udpClient;
    private int port = 12345;
    private Texture2D videoTexture;
    
    [MenuItem("Zengo/VR Streaming")]
    public static void ShowWindow()
    {
        sdkPath = Application.dataPath + "/StreamingAssets/ADB/adb.exe";
        
        if (!string.IsNullOrEmpty(sdkPath) && !string.IsNullOrEmpty(ffmpegPath))
        {
            GetWindow<VRStreaming>("VR Streaming");
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Start Streaming"))
        {
            StartStreaming();
        }

        if (GUILayout.Button("Take a picture"))
        {
            TakeScreenshot();
        }
    }

    void Start()
    {
        udpClient = new UdpClient(port);
        videoTexture = new Texture2D(1920, 1080); // Adjust the size according to your needs
        BeginReceiving();
    }
    
    private void BeginReceiving()
    {
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint remoteEP = null;
        byte[] received = udpClient.EndReceive(ar, ref remoteEP);

        // Here you would decode the received data into video frames
        // This example assumes the data is already in a raw texture format
        videoTexture.LoadRawTextureData(received);
        videoTexture.Apply();

        // Apply the texture to the RenderTexture
        Graphics.Blit(videoTexture, renderTexture);

        // Prepare for the next packet
        BeginReceiving();
    }

    void OnDisable()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
    
    private void StartStreaming()
    {
        if (scrcpyProcess != null && !scrcpyProcess.HasExited)
        {
            UnityEngine.Debug.Log("Streaming is already running.");
            return;
        }
        
        if (ADBCommand(sdkPath, "devices -l").Contains("product:"))
        {
            new Thread(StartScrcpy).Start();
            UnityEngine.Debug.Log("Streaming Started");
        }
        else
        {
            EditorUtility.DisplayDialog("Record", "No Connected Device", "OK");
        }
    }

    private string ADBCommand(string fileName, string arguments)
    {
        Process p = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return output;
    }
    
    private void StartScrcpy()
    {
        string scrcpyPath = Application.dataPath + "/StreamingAssets/ADB/scrcpy.exe";
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = scrcpyPath,
            Arguments = "",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        scrcpyProcess = Process.Start(startInfo);
    }
    
    private void TakeScreenshot()
    {
        string screenshotPathOnDevice = "/sdcard/screenshot.png"; // Path on the device to save the screenshot
        string screenshotPathOnPC = Application.dataPath + "/StreamingAssets/screenshot.png"; // Path on your PC to save the screenshot
        
        ADBCommand(sdkPath, $"shell screencap -p \"{screenshotPathOnDevice}\"");
        
        ADBCommand(sdkPath, $"pull \"{screenshotPathOnDevice}\" \"{screenshotPathOnPC}\"");
        
        ADBCommand(sdkPath, $"shell rm \"{screenshotPathOnDevice}\"");
    }
}
