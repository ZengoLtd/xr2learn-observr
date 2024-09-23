using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;

public class Test
{
    private Mock<IDevice> mockDevice;
    private Mock<IJsonManager> mockJsonManager;
    private Mock<IConnectionManager> mockConnectionManager;
    private Mock<IADBConnection> mockADBConnection;
    private Mock<IReportManager> mockReportManager;

    public static Device ConvertToDevice(IDevice iDevice)
    {
        return new Device(
            iDevice.SerialNumber,
            iDevice.IPAddress,
            iDevice.Port,
            iDevice.UniqueName,
            iDevice.ModelName,
            iDevice.Status,
            iDevice.BatteryLevel,
            iDevice.ScreenCrop
        );
    }
    
    [SetUp]
    public void Setup()
    {
        // Initialize Moq mocks
        mockJsonManager = new Mock<IJsonManager>();
        mockConnectionManager = new Mock<IConnectionManager>();
        mockADBConnection = new Mock<IADBConnection>();
        mockDevice = new Mock<IDevice>();
        mockReportManager = new Mock<IReportManager>();

        // Set up mock behavior for Device
        mockDevice.SetupGet(d => d.SerialNumber).Returns("12345");
        mockDevice.SetupGet(d => d.IPAddress).Returns("192.168.1.1");
        mockDevice.SetupGet(d => d.Port).Returns(8080);
        mockDevice.SetupGet(d => d.UniqueName).Returns("TestDevice");
        mockDevice.SetupGet(d => d.ModelName).Returns("Quest 3");
        mockDevice.SetupGet(d => d.Status).Returns("Active");
        mockDevice.SetupGet(d => d.BatteryLevel).Returns(100);
        mockDevice.SetupGet(d => d.ScreenCrop).Returns("16:9");
        
        // Set up mock behavior
        mockConnectionManager.Setup(m => m.ADBCommand(It.Is<string>(s => s.Contains("devices"))))
            .Returns("List of devices attached\n1234567890\tdevice\n");
        mockConnectionManager.Setup(m => m.ADBCommand(It.Is<string>(s => s.Contains("shell ip -f inet addr show wlan0"))))
            .Returns("inet 192.168.1.2/24 brd 192.168.1.255 scope global wlan0\n");
        mockConnectionManager.Setup(m => m.ADBCommand(It.Is<string>(s => s.Contains("connect"))))
            .Returns("connected to 192.168.1.2:5555");
        mockConnectionManager.Setup(m => m.ADBCommand(It.Is<String>(s => s.Contains("shell dumpsys battery | grep level"))))
            .Returns("100");
    }

    [UnityTest]
    public IEnumerator TestDevicePropertiesWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions
        Assert.AreEqual("12345", mockDevice.Object.SerialNumber);
        Assert.AreEqual("192.168.1.1", mockDevice.Object.IPAddress);
        Assert.AreEqual(8080, mockDevice.Object.Port);
        Assert.AreEqual("TestDevice", mockDevice.Object.UniqueName);
        Assert.AreEqual("Quest 3", mockDevice.Object.ModelName);
        Assert.AreEqual("Active", mockDevice.Object.Status);
        Assert.AreEqual(100, mockDevice.Object.BatteryLevel);
        Assert.AreEqual("16:9", mockDevice.Object.ScreenCrop);

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestConnectToDeviceWirelessly()
    {
        // Arrange
        mockADBConnection.Setup(m => m.ConnectToDeviceWirelessly()).Callback(() =>
        {
            Device connectedDevice = new Device("1234567890", "192.168.1.2", 5555);
            ConnectionManager.Instance.ConnectedDevices.Add(connectedDevice);
        });

        // Act
        mockADBConnection.Object.ConnectToDeviceWirelessly();

        // Assert
        yield return new WaitForSeconds(1); // Wait for coroutine to complete
        Assert.AreEqual(1, ConnectionManager.Instance.ConnectedDevices.Count);
        Assert.AreEqual("1234567890", ConnectionManager.Instance.ConnectedDevices[0].SerialNumber);
        Assert.AreEqual("192.168.1.2", ConnectionManager.Instance.ConnectedDevices[0].IPAddress);
        Assert.AreEqual(5555, ConnectionManager.Instance.ConnectedDevices[0].Port);
    }

    [UnityTest]
    public IEnumerator TestADBCommandDevices()
    {
        // Act
        string result = mockConnectionManager.Object.ADBCommand("devices");

        // Assert
        Assert.AreEqual("List of devices attached\n1234567890\tdevice\n", result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestADBCommandIP()
    {
        // Act
        string result = mockConnectionManager.Object.ADBCommand("shell ip -f inet addr show wlan0");

        // Assert
        Assert.AreEqual("inet 192.168.1.2/24 brd 192.168.1.255 scope global wlan0\n", result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestADBCommandDevice()
    {
        string result = mockConnectionManager.Object.ADBCommand("devices");
        
        Assert.AreEqual("List of devices attached\n1234567890\tdevice\n", result);
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator TestADBCommandBatteryLevel()
    {
        // Arrange
        mockConnectionManager.Setup(m => m.ADBCommand(It.IsAny<string>())).Returns("100");

        // Act
        string result = mockConnectionManager.Object.ADBCommand("shell dumpsys battery | grep level");

        // Assert
        Assert.AreEqual("100", result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestADBCommandConnect()
    {
        // Act
        string result = mockConnectionManager.Object.ADBCommand("connect");

        // Assert
        Assert.AreEqual("connected to 192.168.1.2:5555", result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestDeviceJsonSerializer()
    {
        // Arrange
        mockJsonManager.Setup(m => m.DeviceJsonSerializer(It.IsAny<Device>())).Callback<Device>(device =>
        {
            string json = JsonConvert.SerializeObject(device, Formatting.Indented);
            string directoryPath = Path.Combine(Application.dataPath, "MockDevices");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = Path.Combine(directoryPath, $"{device.SerialNumber}.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"Mock Device info saved to {filePath}");
        });

        // Act
        mockJsonManager.Object.DeviceJsonSerializer(ConvertToDevice(mockDevice.Object));

        // Assert
        string filePath = Path.Combine(Application.dataPath, "MockDevices", $"{mockDevice.Object.SerialNumber}.json");
        yield return new WaitForSeconds(1); // Wait for file write
        Assert.IsTrue(File.Exists(filePath));
        string json = File.ReadAllText(filePath);
        Assert.IsNotNull(json);
        Device deserializedDevice = JsonConvert.DeserializeObject<Device>(json);
        Assert.AreEqual(mockDevice.Object.SerialNumber, deserializedDevice.SerialNumber);
        Assert.AreEqual(mockDevice.Object.IPAddress, deserializedDevice.IPAddress);
        Assert.AreEqual(mockDevice.Object.Port, deserializedDevice.Port);
        Assert.AreEqual(mockDevice.Object.UniqueName, deserializedDevice.UniqueName);
        Assert.AreEqual(mockDevice.Object.ModelName, deserializedDevice.ModelName);
        Assert.AreEqual(mockDevice.Object.Status, deserializedDevice.Status);
        Assert.AreEqual(mockDevice.Object.BatteryLevel, deserializedDevice.BatteryLevel);
        Assert.AreEqual(mockDevice.Object.ScreenCrop, deserializedDevice.ScreenCrop);
    }

    [UnityTest]
    public IEnumerator TestDeviceJsonDeserializer()
    {
        // Arrange
        mockJsonManager.Setup(m => m.DeviceJsonDeserializer()).Callback(() =>
        {
            string directoryPath = Path.Combine(Application.dataPath, "MockDevices");
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError("MockDevices directory does not exist.");
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
                        Debug.Log($"Mock Device {device.SerialNumber} loaded from {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to deserialize mock device from {filePath}: {ex.Message}");
                }
            }
        });

        // Act
        mockJsonManager.Object.DeviceJsonSerializer(ConvertToDevice(mockDevice.Object));
        mockJsonManager.Object.DeviceJsonDeserializer();

        // Assert
        yield return new WaitForSeconds(1); // Wait for deserialization
        Assert.AreEqual(1, ConnectionManager.Instance.ConnectedDevices.Count);
        Device deserializedDevice = ConnectionManager.Instance.ConnectedDevices[0];
        Assert.AreEqual(mockDevice.Object.SerialNumber, deserializedDevice.SerialNumber);
        Assert.AreEqual(mockDevice.Object.IPAddress, deserializedDevice.IPAddress);
        Assert.AreEqual(mockDevice.Object.Port, deserializedDevice.Port);
        Assert.AreEqual(mockDevice.Object.UniqueName, deserializedDevice.UniqueName);
        Assert.AreEqual(mockDevice.Object.ModelName, deserializedDevice.ModelName);
        Assert.AreEqual(mockDevice.Object.Status, deserializedDevice.Status);
        Assert.AreEqual(mockDevice.Object.BatteryLevel, deserializedDevice.BatteryLevel);
        Assert.AreEqual(mockDevice.Object.ScreenCrop, deserializedDevice.ScreenCrop);
    }

    [UnityTest]
    public IEnumerator TestCreateReport()
    {
        //Arrange
        string reportName = "TestReport";
        mockReportManager.Setup(m => m.CreateReport(reportName)).Verifiable();
        
        //Act
        mockReportManager.Object.CreateReport(reportName);
        
        //Assert
        mockReportManager.Verify(m => m.CreateReport(reportName), Times.Once);
        
        yield return null;
    }
}