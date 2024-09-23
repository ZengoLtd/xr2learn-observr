using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JsonManager : MonoBehaviour, IJsonManager
{
    public static JsonManager Instance;

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

    public void DeviceJsonSerializer(Device device)
    {
        string json = JsonConvert.SerializeObject(device, Formatting.Indented);

        string directoryPath = Path.Combine(Application.dataPath, "Devices");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, $"{device.SerialNumber}.json");

        File.WriteAllText(filePath, json);
        Debug.Log($"Device info saved to {filePath}");
    }

    public void DeviceJsonDeserializer()
    {
        string directoryPath = Path.Combine(Application.dataPath, "Devices");

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("Devices directory does not exist.");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        foreach (string filePath in jsonFiles)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                Device device = JsonConvert.DeserializeObject<Device>(json);
                if (device != null)
                {
                    ConnectionManager.Instance.ConnectedDevices.Add(device);
                    Debug.Log($"Device {device.SerialNumber} loaded from {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize device from {filePath}: {ex.Message}");
            }
        }
    }
}
