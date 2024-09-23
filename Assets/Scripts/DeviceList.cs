using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class DeviceList : MonoBehaviour
{
    public Texture2D quest1Image;
    public Texture2D quest2Image;
    public Texture2D quest3Image;
    public Texture2D questProImage;

    public Texture2D lastFrame1;
    public Texture2D lastFrame2;
    public Texture2D lastFrame3;
    public Texture2D lastFrame4;
    public Texture2D lastFrame5;
    public Texture2D lastFrame6;
    public Texture2D lastFrame7;
    public Texture2D lastFrame8;
    public Texture2D lastFrame9;
    public Texture2D lastFrame10;

    private ConnectionManager connectionManager;
    private ReportManager reportManager;
    private ScreenCapture screenCapture;
    private StatusBarManager statusBarManager;
    private ActiveReportScreenshotsHandler activeReportScreenshotsHandler;

    public bool isScreenshotMakingInProgress = false;

    public int deviceListRefreshRate = 60;
    public int selectedDevice = 0;


    [SerializeField] UIDocument mainUIDocument;

    private static DeviceList instance;
    public static DeviceList Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DeviceList>();
                if (instance == null)
                {
                    Debug.LogError("DeviceList instance not found!");
                }
            }
            return instance;
        }
    }

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
        statusBarManager = FindObjectOfType<StatusBarManager>();
        connectionManager = FindObjectOfType<ConnectionManager>();
        reportManager = FindObjectOfType<ReportManager>();
        screenCapture = FindObjectOfType<ScreenCapture>();
        activeReportScreenshotsHandler = FindObjectOfType<ActiveReportScreenshotsHandler>();

        for (int i = 1; i <= 10; i++)
        {
            Button deviceButton = mainUIDocument.rootVisualElement.Q<Button>($"DeviceButton{i}");
            int index = i;
            deviceButton.clicked += () => DeviceButton(index);
        }


        for (int i = 1; i <= 10; i++)
        {
            Button activeDeviceButton = mainUIDocument.rootVisualElement.Q<Button>($"DeviceButtonActive{i}");
            int index = i;
            activeDeviceButton.clicked += () => ActiveDeviceButton(index);
        }


        for (int i = 1; i <= 10; i++)
        {
            Button activeDeviceButton = mainUIDocument.rootVisualElement.Q<Button>($"AddNewDeviceButton{i}");
            int index = i;
            activeDeviceButton.clicked += () => AddNewDeviceButton();
        }

        UpdateDeviceList();

        StartCoroutine(RefreshDeviceList());

        lastFrame1 = screenCapture.trimmedTexture;
        lastFrame2 = screenCapture.trimmedTexture;
        lastFrame3 = screenCapture.trimmedTexture;
        lastFrame4 = screenCapture.trimmedTexture;
        lastFrame5 = screenCapture.trimmedTexture;
        lastFrame6 = screenCapture.trimmedTexture;
        lastFrame7 = screenCapture.trimmedTexture;
        lastFrame8 = screenCapture.trimmedTexture;
        lastFrame9 = screenCapture.trimmedTexture;
        lastFrame10 = screenCapture.trimmedTexture;

    }

    IEnumerator RefreshDeviceList()
    {
        while (true)
        {
            UpdateDeviceList();
            yield return new WaitForSeconds(deviceListRefreshRate);
        }
    }


    void DeviceButton(int deviceButtonIndex)
    {
        if (isScreenshotMakingInProgress)
        {
            Debug.Log("Ongoing screenshot is in progress!");
            return;
        }

        selectedDevice = deviceButtonIndex;
        UpdateInactivatedDeviceLastFrame();
        UpdateDeviceList();
        SetActiveDevice(deviceButtonIndex);
        HandleOngoingReportsForActiveDevices();
        StartCoroutine(DeviceButtonActions(deviceButtonIndex));
    }


    public void UpdateInactivatedDeviceLastFrame()
    {
        List<Device> connectedDevices = connectionManager.ConnectedDevices;

        int inactivatedDeviceIndex = 1;
        int i = 1;

        foreach (Device device in connectedDevices)
        {
            if (device == connectionManager.ActiveDevice)
            {
                inactivatedDeviceIndex = i;
                break;
            }
            i++;
        }

        if(screenCapture.trimmedTexture == null)
        {
            Debug.Log("screenCapture.trimmedTexture is null!");
            return;
        }

        Texture2D deviceLastFrame = new Texture2D(screenCapture.trimmedTexture.width, screenCapture.trimmedTexture.height, screenCapture.trimmedTexture.format, false);

        CopyTexture(screenCapture.trimmedTexture, deviceLastFrame);
        deviceLastFrame.Apply();

        switch (inactivatedDeviceIndex)
        {
            case 1: Debug.Log("lastFrame1 updating"); lastFrame1 = deviceLastFrame; break;
            case 2: Debug.Log("lastFrame2 updating"); lastFrame2 = deviceLastFrame; break;
            case 3: Debug.Log("lastFrame3 updating"); lastFrame3 = deviceLastFrame; break;
            case 4: Debug.Log("lastFrame4 updating"); lastFrame4 = deviceLastFrame; break;
            case 5: Debug.Log("lastFrame5 updating"); lastFrame5 = deviceLastFrame; break;
            case 6: Debug.Log("lastFrame6 updating"); lastFrame6 = deviceLastFrame; break;
            case 7: Debug.Log("lastFrame7 updating"); lastFrame7 = deviceLastFrame; break;
            case 8: Debug.Log("lastFrame8 updating"); lastFrame8 = deviceLastFrame; break;
            case 9: Debug.Log("lastFrame9 updating"); lastFrame9 = deviceLastFrame; break;
            case 10: Debug.Log("lastFrame10 updating"); lastFrame10 = deviceLastFrame; break;
        }
    }

    IEnumerator DeviceButtonActions(int deviceButtonIndex)
    {
        yield return new WaitForEndOfFrame();
        int deviceButton = deviceButtonIndex;
        Debug.Log("Button " + deviceButton + " pressed!");

        UpdateDeviceList();
        SetActiveDevice(deviceButtonIndex);
        HandleOngoingReportsForActiveDevices();
        ReportManager.Instance.ChangeActiveReportTo();
        ActiveReportScreenshotsHandler.Instance.LoadActiveReportScreenshots();

        if (ReportManager.Instance.IsReportActiveOnSelectedDevice())
        {
            ButtonManager.Instance.ShowMakeScreenshotButton();
        }

        else
        {
            ButtonManager.Instance.HideMakeScreenshotButton();
        }
        StreamScreen.Instance.SwitchRatio();
    }

    void CopyTexture(Texture2D source, Texture2D target)
    {
        // Copy pixels from the source to the target texture
        target.SetPixels(source.GetPixels());
    }

    void ActiveDeviceButton(int deviceButtonIndex)
    {
        selectedDevice = deviceButtonIndex;

        int deviceButton = deviceButtonIndex;
        Debug.Log("Button " + deviceButton + " pressed!");

        UpdateDeviceList();
        SetActiveDevice(deviceButtonIndex);
        HandleOngoingReportsForActiveDevices();
    }

    public void HandleOngoingReportsForActiveDevices()
    {
        StatusBarManager statusBarManager = FindObjectOfType<StatusBarManager>();
        if (ReportManager.Instance.IsReportActiveOnSelectedDevice())
        {

            statusBarManager.DeviceStatusConnected();
            statusBarManager.EndReportButtonOn();
        }
        else
        {
            statusBarManager.DeviceStatusConnected();
            statusBarManager.CreateReportButtonOn();
        }

        statusBarManager.UpdateTitleOnLiveView();
    }

    public void LastAddedDeviceIsTheChosenOne()
    {

        selectedDevice = connectionManager.ConnectedDevices.Count;

    }

    public void UpdateDeviceList()
    {

        if (connectionManager != null)
        {
            List<Device> devices = connectionManager.ConnectedDevices;

            for (int i = 0; i < 10; i++)
            {
                VisualElement slot = mainUIDocument.rootVisualElement.Q<VisualElement>($"Slot{i + 1}");
                VisualElement deviceButtonContainer = slot.Q<VisualElement>("DeviceButtonContainer");
                VisualElement activeDeviceButtonContainer = slot.Q<VisualElement>("ActiveDeviceButtonContainer");
                VisualElement emptySlot = slot.Q<VisualElement>("EmptySlot");
                VisualElement addNewDevice = slot.Q<VisualElement>("AddNewDeviceContainer");

                if (i < devices.Count)
                {
                    Device device = devices[i];
                    deviceButtonContainer.style.display = DisplayStyle.Flex;
                    activeDeviceButtonContainer.style.display = DisplayStyle.None;
                    emptySlot.style.display = DisplayStyle.None;
                    addNewDevice.style.display = DisplayStyle.None;


                    // Device button elements
                    Button deviceButton = deviceButtonContainer.Q<Button>($"DeviceButton{i + 1}");
                    Label deviceTitleText = deviceButton.Q<Label>("DeviceTitleText");
                    VisualElement imageContainer = deviceButton.Q<VisualElement>("LastScreenshot");
                    Label deviceLastReportTimeText = deviceButton.Q<Label>("LastReportText");


                    // Active device button elements
                    Button activeDeviceButton = activeDeviceButtonContainer.Q<Button>($"DeviceButtonActive{i + 1}");
                    Label activeDeviceTitleText = activeDeviceButton.Q<Label>("DeviceTitleText");
                    VisualElement activeImageContainer = activeDeviceButton.Q<VisualElement>("LastScreenshot");
                    Label activeDeviceLastReportTimeText = activeDeviceButton.Q<Label>("LastReportText");

                    string lastUsername = reportManager?.GetLastUsernameOfDevice(device);
                    Debug.Log("Last username: " + lastUsername);


                    UpdateInactivatedDeviceLastFrame();

                    switch (i)
                    {
                        case 0: imageContainer.style.backgroundImage = new StyleBackground(lastFrame1); break;
                        case 1: imageContainer.style.backgroundImage = new StyleBackground(lastFrame2); break;
                        case 2: imageContainer.style.backgroundImage = new StyleBackground(lastFrame3); break;
                        case 3: imageContainer.style.backgroundImage = new StyleBackground(lastFrame4); break;
                        case 4: imageContainer.style.backgroundImage = new StyleBackground(lastFrame5); break;
                        case 5: imageContainer.style.backgroundImage = new StyleBackground(lastFrame6); break;
                        case 6: imageContainer.style.backgroundImage = new StyleBackground(lastFrame7); break;
                        case 7: imageContainer.style.backgroundImage = new StyleBackground(lastFrame8); break;
                        case 8: imageContainer.style.backgroundImage = new StyleBackground(lastFrame9); break;
                        case 9: imageContainer.style.backgroundImage = new StyleBackground(lastFrame10); break;
                    }
                    
                    if (device != connectionManager.ActiveDevice)
                    {

                        UpdateInactivatedDeviceLastFrame();

                        switch (i)
                        {
                            case 0: imageContainer.style.backgroundImage = new StyleBackground(lastFrame1); break;
                            case 1: imageContainer.style.backgroundImage = new StyleBackground(lastFrame2); break;
                            case 2: imageContainer.style.backgroundImage = new StyleBackground(lastFrame3); break;
                            case 3: imageContainer.style.backgroundImage = new StyleBackground(lastFrame4); break;
                            case 4: imageContainer.style.backgroundImage = new StyleBackground(lastFrame5); break;
                            case 5: imageContainer.style.backgroundImage = new StyleBackground(lastFrame6); break;
                            case 6: imageContainer.style.backgroundImage = new StyleBackground(lastFrame7); break;
                            case 7: imageContainer.style.backgroundImage = new StyleBackground(lastFrame8); break;
                            case 8: imageContainer.style.backgroundImage = new StyleBackground(lastFrame9); break;
                            case 9: imageContainer.style.backgroundImage = new StyleBackground(lastFrame10); break;
                        }
                    }

                    else
                    {
                        switch (device.ModelName)
                        {
                            case "Quest":
                                activeImageContainer.style.backgroundImage = new StyleBackground(quest1Image);
                                break;
                            case "Quest 2":
                                activeImageContainer.style.backgroundImage = new StyleBackground(quest2Image);
                                break;
                            case "Quest 3":
                                activeImageContainer.style.backgroundImage = new StyleBackground(quest3Image);
                                break;
                            case "Quest Pro":
                                activeImageContainer.style.backgroundImage = new StyleBackground(questProImage);
                                break;
                            default:
                                activeImageContainer.style.backgroundImage = new StyleBackground(quest1Image);
                                break;
                        }
                    }

                    // Display duration of last report
                    string lastReportDate = reportManager?.GetLastReportDateOfDevice(device);

                    Debug.Log("Last report date: " + lastReportDate);

                    string displayTimeText = "No active report";

                    if (lastReportDate != null && lastReportDate != "")
                    {
                        DateTime lastReportDateTime = DateTime.Parse(lastReportDate);
                        Debug.Log("lastReportDateTime: " + lastReportDateTime);

                        DateTime currentDateTime = DateTime.Now;
                        Debug.Log("currentDateTime: " + currentDateTime);

                        // Fix midnight issue
                        if (currentDateTime < lastReportDateTime)
                        {
                            currentDateTime = currentDateTime + new TimeSpan(24, 0, 0);
                        }


                        TimeSpan timeDifference = currentDateTime - lastReportDateTime;
                        Debug.Log("timeDifference: " + timeDifference);

                        int deltaTime = (int)timeDifference.TotalMinutes;


                        Debug.Log("deltaTime: " + deltaTime);

                        /*
                        if (deltaTime < 1)
                        {
                            int deltaTimeInSeconds = (int)timeDifference.TotalSeconds;
                            Debug.Log("deltaTime: " + deltaTime);

                            displayTimeText = deltaTimeInSeconds + " seconds ago";
                        }*/

                        if (deltaTime <= 1)
                        {
                            displayTimeText = deltaTime + " minute ago";
                        }

                        else
                        {
                            displayTimeText = deltaTime + " minutes ago";
                        }

                        deviceLastReportTimeText.text = displayTimeText;
                    }

                    deviceLastReportTimeText.text = displayTimeText;
                    activeDeviceLastReportTimeText.text = displayTimeText;


                    if (lastUsername != null && lastUsername != "")
                    {

                        deviceTitleText.text = lastUsername;
                        activeDeviceTitleText.text = lastUsername;
                    }

                    else
                    {
                        deviceTitleText.text = device.UniqueName;
                        activeDeviceTitleText.text = device.UniqueName;
                    }


                    if (i + 1 == selectedDevice)
                    {
                        deviceButtonContainer.style.display = DisplayStyle.None;
                        activeDeviceButtonContainer.style.display = DisplayStyle.Flex;
                    }

                }

                else
                {
                    deviceButtonContainer.style.display = DisplayStyle.None;
                    activeDeviceButtonContainer.style.display = DisplayStyle.None;
                    emptySlot.style.display = DisplayStyle.Flex;
                    addNewDevice.style.display = DisplayStyle.None;
                }


                if (i == devices.Count)
                {
                    deviceButtonContainer.style.display = DisplayStyle.None;
                    activeDeviceButtonContainer.style.display = DisplayStyle.None;
                    emptySlot.style.display = DisplayStyle.None;
                    addNewDevice.style.display = DisplayStyle.Flex;
                }
            }
        }

        else
        {
            Debug.LogError("ConnectionManager not found!");
        }

    }

    public void SetActiveDevice(int deviceButtonIndex)

    {
        VisualElement slot = mainUIDocument.rootVisualElement.Q<VisualElement>($"Slot{deviceButtonIndex}");
        if (slot != null)
        {
            VisualElement deviceButtonContainer = slot.Q<VisualElement>("DeviceButtonContainer");
            VisualElement activeDeviceButtonContainer = slot.Q<VisualElement>("ActiveDeviceButtonContainer");

            if (deviceButtonContainer != null && activeDeviceButtonContainer != null)
            {
                deviceButtonContainer.style.display = DisplayStyle.None;
                activeDeviceButtonContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                Debug.LogError($"DeviceButtonContainer or ActiveDeviceButtonContainer not found in Slot{deviceButtonIndex}");
            }
        }
        else
        {
            Debug.LogError($"Slot{deviceButtonIndex} not found");
        }



        Debug.Log("Button" + deviceButtonIndex + " pressed!");



        int deviceIndex = deviceButtonIndex - 1;

        if (deviceIndex >= 0 && deviceIndex < ConnectionManager.Instance.ConnectedDevices.Count)
        {
            ConnectionManager.Instance.ActivateDevice(deviceIndex);

            var activeDevice = ConnectionManager.Instance.ActiveDevice;
            if (activeDevice != null)
            {
                string serialNumber = activeDevice.SerialNumber;
                Debug.Log("Device " + serialNumber + " is Active!");
            }
            else
            {
                Debug.LogError("Active device is null!");
            }
        }
        else
        {
            Debug.LogError("Invalid device index: " + deviceIndex);
        }
    }

    void AddNewDeviceButton()
    {
        ButtonManager buttonManager = FindObjectOfType<ButtonManager>();
        buttonManager.AddNewDevice();
    }
}
