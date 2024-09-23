using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class ADBConnection : MonoBehaviour, IADBConnection
{
    public StatusManager statusManager;

    //Connect to device wirelessly and save the device to the connected devices list and to the json file
    //If you would like to connect to a device wirelessly you need to use this method
    public void ConnectToDeviceWirelessly()
    {
        string wiredDevicesList = ConnectionManager.Instance.ADBCommand("devices");
        string[] lines = wiredDevicesList.Split('\n');
        int usbDevicesCount = 0;
        string deviceSerial = null;
        
        foreach (var line in lines)
        {
            if (!line.Trim().EndsWith("device")) continue;
            
            if (line.Trim().EndsWith("unauthorized"))
            {
                Debug.Log("Your device is unauthorized. Please check your device and allow the connection.");
                return;
            }

            if (!line.Contains("emulator") && !line.Contains(":"))
            {
                usbDevicesCount++;
                if (usbDevicesCount > 1) break;
                deviceSerial = line.Split('\t')[0];
            }
        }

        if (usbDevicesCount == 1)
        {
            statusManager.Status2();
            string ip = GetDeviceIP(deviceSerial);
            int port = 5554 + ConnectionManager.Instance.ConnectedDevices.Count + 1;

            foreach (var device in ConnectionManager.Instance.ConnectedDevices)
            {
                if ((device.IPAddress == ip) && (IsDeviceConnected(device.SerialNumber)))
                {
                    Debug.LogWarning("Device already connected.");
                    
                    statusManager.Status1ErrorAlreadyConnected();
                    
                    return;
                }
                else if((device.IPAddress == ip) && (!IsDeviceConnected(device.SerialNumber)))
                {
                    ADBHelper.Instance.KillDeviceServer(deviceSerial);
                    Debug.Log("Reconnecting to the device.");

                    break;
                }
            }
            
            ConnectionManager.Instance.ADBCommand($"-s {deviceSerial} tcpip {port}");
            string connectResult = ConnectionManager.Instance.ADBCommand($"-s {deviceSerial} connect {ip}:{port}");
            Debug.Log(connectResult);
            string devicesList = ConnectionManager.Instance.ADBCommand("devices");

            if (devicesList.Contains(ip))
            {
                Debug.Log($"Successfully connected to {ip} over TCP/IP on port {port}.");
                
                Device connectedDevice = new Device(deviceSerial, ip, port);
                
                StartCoroutine(TestRoutine(connectedDevice));
                
                ConnectionManager.Instance.ConnectedDevices.Add(connectedDevice);
                
            }
            else
            {
                Debug.LogError($"Failed to connect to {ip} on port {port}.");

                statusManager.Status1ErrorUnsucces();
            }
        }
        else
        {
            if (usbDevicesCount < 1) // No connected device
            {
                statusManager.Status1ErrorNoDevice();
                Debug.Log("Please connect device via USB to proceed.");
            }
            else // More then one device connected 
            { 

                statusManager.Status1ErrorMultipleDevice();
                Debug.Log("Please connect only one device via USB to proceed.");
            }
        }
    }

    IEnumerator TestRoutine(Device device)
    {
        ModelName:
        Debug.Log("TestRoutine");
    
        yield return new WaitForSeconds(2);
        UnityAction<string> modelNameCallback = (string arg) =>
        {
            Debug.Log("Model name callback in couroutine" + arg);
            device.ModelName = arg;
            JsonManager.Instance.DeviceJsonSerializer(device);
        };
        ADBHelper.Instance.GetModelName(modelNameCallback, device.SerialNumber);

        if (String.IsNullOrEmpty(device.ModelName))
        {
            goto ModelName;
        }
        else
        {
            statusManager.RefreshGivenName();
        }
    
        yield return new WaitForSeconds(2);
    

        UnityAction<int> batteryLevelCallback = (int level) =>
        {
            Debug.Log("battery level callback in coroutine: " + level);
            device.BatteryLevel = level;
            JsonManager.Instance.DeviceJsonSerializer(device);

            ConnectionManager.Instance.ActiveDeviceIndex = ConnectionManager.Instance.ConnectedDevices.Count - 1;
            var deviceasd = ConnectionManager.Instance.ActiveDevice;
        };
    
        ADBHelper.Instance.GetBatteryLevel(batteryLevelCallback, device.SerialNumber);
        statusManager.Status3();
    }
    
    private string GetDeviceIP(string deviceSerial)
    {
        string ipCommandOutput = ConnectionManager.Instance.ADBCommand($"-s {deviceSerial} shell ip -f inet addr show wlan0");
        string ipAddress = null;
        
        string[] lines = ipCommandOutput.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("inet"))
            {
                int inetIndex = line.IndexOf("inet") + 5; // +5 to skip "inet " itself.
                int endIndex = line.IndexOf('/', inetIndex); // Assuming the IP address ends before the '/' character.
                if (inetIndex > 0 && endIndex > inetIndex)
                {
                    ipAddress = line.Substring(inetIndex, endIndex - inetIndex).Trim();
                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(ipAddress))
        {
            Debug.Log($"Device IP Address: {ipAddress}");
        }
        else
        {
            statusManager.Status1ErrorUnsucces();
            Debug.LogError("Failed to obtain device IP address.");
        }

        return ipAddress;
    }
    
    private bool IsDeviceConnected(string deviceSerial)
    {
        string ip = GetDeviceIP(deviceSerial);
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogError("Failed to obtain device IP address.");
            return false;
        }

        string devicesList = ConnectionManager.Instance.ADBCommand("devices");
        string[] lines = devicesList.Split('\n');

        foreach (var line in lines)
        {
            if (line.Contains(ip))
            {
                Debug.Log($"Device with IP {ip} is connected.");
                return true;
            }
        }

        Debug.LogError($"Device with IP {ip} is not connected.");
        return false;
    }

    private void OnApplicationQuit()
    {
        if (ADBHelper.Instance != null)
        {
            ADBHelper.Instance.KillServer();
        }
    }
}
