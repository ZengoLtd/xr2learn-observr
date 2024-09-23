using System.IO;
using System.Linq;
using UnityEngine;

public class DeviceCleaner : MonoBehaviour
{
    public static DeviceCleaner Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Disconnects the active device from the ADB server.
    public void DisconnectDevice()
    {
        var deviceToRemove = ConnectionManager.Instance.ConnectedDevices.FirstOrDefault(device => device.IPAddress == ConnectionManager.Instance.ActiveDevice.IPAddress);
        if (deviceToRemove != null)
        {
            string adbDisconnectCommand = $"disconnect {ConnectionManager.Instance.ActiveDevice.IPAddress}:{deviceToRemove.Port}";
            
            string disconnectResult = ConnectionManager.Instance.ADBCommand(adbDisconnectCommand);
            
            if (disconnectResult.Contains("disconnected"))
            {
                Debug.Log($"Successfully disconnected from {ConnectionManager.Instance.ActiveDevice.IPAddress} on port {deviceToRemove.Port}.");
                
                ConnectionManager.Instance.ConnectedDevices.Remove(deviceToRemove);
                if (ConnectionManager.Instance.ActiveDevice != null)
                {
                    Debug.Log($"Device with IP {ConnectionManager.Instance.ActiveDevice.IPAddress} removed from connected devices list.");
                }
            }
            else
            {
                if (ConnectionManager.Instance.ActiveDevice != null)
                {
                    Debug.LogError($"Failed to disconnect from {ConnectionManager.Instance.ActiveDevice.IPAddress} on port {deviceToRemove.Port}.");
                }
            }
        }
        else
        {
            Debug.LogError($"Failed to find and remove device with IP {ConnectionManager.Instance.ActiveDevice.IPAddress} from connected devices list.");
        }
    }

    // Deletes the folder for the active device.
    // If you would like to delete connection, you need to use this method
    public void DeleteDevice()
    {
        string directoryPath = Path.Combine(Application.dataPath, "Devices");
        
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("Devices directory does not exist.");
            return;
        }

        if (ConnectionManager.Instance.ActiveDevice != null)
        {
            string deviceFolderPath = Path.Combine(directoryPath, ConnectionManager.Instance.ActiveDevice.SerialNumber);

            if (Directory.Exists(deviceFolderPath))
            {
                Debug.Log($"Folder for device {ConnectionManager.Instance.ActiveDevice.SerialNumber} exists.");
                Directory.Delete(deviceFolderPath, true);
            }
            else
            {
                Debug.LogError($"Folder for device {ConnectionManager.Instance.ActiveDevice.SerialNumber} does not exist.");
            }
        }
    }
}
