using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;



public class ConnectionManager : MonoBehaviour, IConnectionManager
{
    private static string sdkPath = Application.dataPath + "/StreamingAssets/ADB/adb.exe";
    private static ConnectionManager instance;
    private int activeDeviceIndex = 0;
    
    // Active device index, this is the index of the connected devices list that is currently active
    // This should be used to get the active device from the connected devices list
    public int ActiveDeviceIndex
    {
        get { return activeDeviceIndex; }
        set { activeDeviceIndex = value; }
    }
    
    public Device ActiveDevice
    {
        get
        {
            if (ConnectedDevices.Count == 0)
            {
                return null;
            }
            
            return ConnectedDevices[ActiveDeviceIndex];
        }
        set { ConnectedDevices[ActiveDeviceIndex] = value; }
    }
    
    // List of connected devices
    public List<Device> ConnectedDevices = new List<Device>();
    
    public static ConnectionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ConnectionManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("ConnectionManager");
                    instance = obj.AddComponent<ConnectionManager>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void ActivateDevice(int index)
    {
        VRScreenStream vrScreenStream = FindObjectOfType<VRScreenStream>();
        vrScreenStream.StopStreaming();
        
        ActiveDeviceIndex = index;

        vrScreenStream.StartStreaming();
    }
    
    public virtual string ADBCommand(string arguments)
    {
        Process p = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = sdkPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };                                               

        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        p.OutputDataReceived += (sender, eventArgs) => Debug.Log("Simple Process"+eventArgs.Data);
        
        return output;
    }
    public string ADBCommand(UnityAction<string> callbackEvent,  string arguments)
    {
        Process p = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = sdkPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        StringBuilder output = new StringBuilder();

        p.OutputDataReceived += (sender, eventArgs) =>
        {
           // Debug.Log("Process with argument." + eventArgs.Data);
            if (!String.IsNullOrEmpty(eventArgs.Data))
            {
                callbackEvent.Invoke(eventArgs.Data);
                output.AppendLine(eventArgs.Data);
            }
        };

        p.Start();
        p.BeginOutputReadLine();
        p.WaitForExit();

        return output.ToString();
    }
    
    public List<string> GetConnectedDevices()
    {
        List<string> connectedDevices = new List<string>();
        string adbOutput = ADBCommand("devices -l");
        string[] lines = adbOutput.Split('\n').Skip(1).ToArray();

        foreach (string line in lines)
        {
            if (line.Contains("device") && !line.Contains("emulator"))
            {
                string[] parts = line.Split(' ');
                if (parts.Length > 0)
                {
                    connectedDevices.Add(parts[0]);
                }
            }
        }

        return connectedDevices;
    }
    
    public List<string> GetDeviceIPs()
    {
        List<string> deviceIPs = new List<string>();
        List<string> connectedDevices = GetConnectedDevices();

        foreach (string device in connectedDevices)
        {
            string adbShellCommand = $"-s {device} shell ifconfig wlan0";
            string output = ADBCommand(adbShellCommand);
            string[] outputLines = output.Split('\n');
            foreach (string line in outputLines)
            {
                if (line.Contains("inet addr:"))
                {
                    string[] parts = line.Split(' ');
                    foreach (string part in parts)
                    {
                        if (part.StartsWith("addr:"))
                        {
                            string ip = part.Substring(5);
                            deviceIPs.Add(ip);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        return deviceIPs;
    }
    
    public List<(string DeviceSerial, string IpAddress)> PairDeviceSerialsWithIPs()
    {
        var deviceSerials = GetConnectedDevices();
        var deviceIPs = GetDeviceIPs();
        List<(string DeviceSerial, string IpAddress)> pairedList = new List<(string, string)>();

        for (int i = 0; i < deviceSerials.Count; i++)
        {
            string deviceSerial = deviceSerials[i];
            string deviceIp = i < deviceIPs.Count ? deviceIPs[i] : "IP not found";
            pairedList.Add((deviceSerial, deviceIp));
        }

        return pairedList;
    }
}
