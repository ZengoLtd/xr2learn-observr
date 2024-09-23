using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class StatusBarManager : MonoBehaviour
{
    [SerializeField] UIDocument mainUIDocument;
    private VisualElement rootMain;
    private Label batteryLevelLabel;

    private float refreshConnectionFrequency = 2000;
    private int refreshBatteryFrequency = 60000;

    public Color batteryLowColor;
    public Color batteryMediumColor;
    public Color batteryHighColor;
    public bool newScreenshotDone = false;

    private static StatusBarManager instance;
    public static StatusBarManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StatusBarManager>();
                if (instance == null)
                {
                    Debug.LogError("DeviceList instance not found!");
                }
            }
            return instance;
        }
    }

    private Thread connectionStatusThread;
    private Thread batteryLevelThread;

    private void Awake()
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

    void Start()
    {
        rootMain = mainUIDocument.rootVisualElement;
        batteryLevelLabel = rootMain.Q<Label>("BatteryLevelText");

        int batteryLevel = GetCurrentBatteryLevel();
        UpdateBatteryLevel();
        UpdateTitleOnLiveView();

        ReportManager reportManager = FindObjectOfType<ReportManager>();

        // Start the threads
        connectionStatusThread = new Thread(UpdateConnectionStatusThread);
        batteryLevelThread = new Thread(UpdateBatteryLevelThread);

        connectionStatusThread.Start();
        batteryLevelThread.Start();
    }

    private void UpdateConnectionStatusThread()
    {
        while (true)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateConnectionStatus());
            Thread.Sleep((int)(refreshConnectionFrequency));
        }
    }

    private void UpdateBatteryLevelThread()
    {
        while (true)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateBatteryLevel());
            Thread.Sleep(refreshBatteryFrequency);
        }
    }

    void OnDestroy()
    {
        // Ensure threads are properly terminated
        if (connectionStatusThread != null && connectionStatusThread.IsAlive)
        {
            connectionStatusThread.Abort();
        }

        if (batteryLevelThread != null && batteryLevelThread.IsAlive)
        {
            batteryLevelThread.Abort();
        }
    }

    private void UpdateConnectionStatus()
    {
        if (ConnectionManager.Instance.ActiveDevice == null)
        {
            ThereIsNoActiveDevice();
            return;
        }
        else if (ADBHelper.Instance.IsDeviceAwake(ConnectionManager.Instance.ActiveDevice.IPAddress))
        {
            DeviceStatusConnected();
        }
        else
        {
            DeviceStatusDisconnected();
        }
    }

    public void UpdateTitleOnLiveView()
    {
        if (ConnectionManager.Instance.ActiveDevice != null)
        {
            string lastUsername = ReportManager.Instance.GetLastUsernameOfDevice(ConnectionManager.Instance.ActiveDevice);
            string deviceUniqueName = ConnectionManager.Instance.ActiveDevice.UniqueName;
            string modelName = ConnectionManager.Instance.ActiveDevice.ModelName;

            if (lastUsername != null)
            {
                rootMain.Q<Label>("UniqueNameOrUserNameText").text = lastUsername;
            }
            else
            {
                rootMain.Q<Label>("UniqueNameOrUserNameText").text = deviceUniqueName;
            }

            rootMain.Q<Label>("ModelTypeText").text = modelName;
        }
        else
        {
            rootMain.Q<Label>("UniqueNameOrUserNameText").text = "No active device";
            rootMain.Q<Label>("ModelTypeText").text = "No active device";
        }
    }

    int GetCurrentBatteryLevel()
    {
        if (ConnectionManager.Instance.ActiveDevice != null)
        {
            UnityAction<int> batteryLevelCallback = (int level) =>
            {
                ConnectionManager.Instance.ActiveDevice.BatteryLevel = level;
            };

            ADBHelper.Instance.GetBatteryLevel(batteryLevelCallback, ConnectionManager.Instance.ActiveDevice.IPAddress);

            int batteryLevel = ConnectionManager.Instance.ActiveDevice.BatteryLevel;
            return batteryLevel;
        }
        else
        {
            return 200;
        }
    }

    public void UpdateBatteryLevel()
    {
        int batteryLevel = GetCurrentBatteryLevel();

        string batteryLevelString;
        if (batteryLevel <= 100 && batteryLevel >= 0)
        {
            batteryLevelString = $"{batteryLevel}%";
            rootMain.Q<VisualElement>("BatteryBar").style.backgroundColor = batteryLevel > 50 ? new StyleColor(batteryHighColor) : batteryLevel > 30 ? new StyleColor(batteryMediumColor) : new StyleColor(batteryLowColor);
        }
        else
        {
            batteryLevelString = "N/A";
            rootMain.Q<VisualElement>("BatteryBar").style.backgroundColor = new StyleColor(Color.black);
        }
        batteryLevelLabel.text = batteryLevelString;

        rootMain.Q<VisualElement>("BatteryBar").style.width = batteryLevel * 0.14f;
    }

    public void ThereIsNoActiveDevice()
    {
        VisualElementOff("ActiveDeviceActionButtonContainer");
        VisualElementOn("StatusNoActiveDevice");
        VisualElementOff("StatusConnected");
        VisualElementOff("StatusDisconnected");
    }

    public void DeviceStatusConnected()
    {
        VisualElementOff("StatusDisconnected");
        VisualElementOn("StatusConnected");
        VisualElementOff("StatusNoActiveDevice");
        VisualElementOn("ActiveDeviceActionButtonContainer");
    }

    public void DeviceStatusDisconnected()
    {
        VisualElementOff("StatusConnected");
        VisualElementOn("StatusDisconnected");
        VisualElementOff("StatusNoActiveDevice");
        VisualElementOn("ActiveDeviceActionButtonContainer");
    }

    public void CreateReportButtonOn()
    {
        VisualElementOn("CreateReportPanelButton");
        VisualElementOff("EndReportButton");
    }

    public void EndReportButtonOn()
    {
        VisualElementOff("CreateReportPanelButton");
        VisualElementOn("EndReportButton");
    }

    public void VisualElementOn(string VisualElementName)
    {
        VisualElement targetElement = rootMain.Q<VisualElement>(VisualElementName);

        if (targetElement != null)
        {
            targetElement.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.LogWarning("targetElement element not found.");
        }
    }

    public void VisualElementOff(string VisualElementName)
    {
        VisualElement targetElement = rootMain.Q<VisualElement>(VisualElementName);

        if (targetElement != null)
        {
            targetElement.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogWarning("targetElement element not found.");
        }
    }
}
