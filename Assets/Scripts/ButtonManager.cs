using UnityEngine;
using UnityEngine.UIElements;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] UIDocument testUIDocumentGameObject;
    [SerializeField] UIDocument mainUIDocument;
    [SerializeField] UIDocument setupPopupUIDocument;
    [SerializeField] UIDocument createReportPopupUIDocument;
    [SerializeField] VRScreenCap vrScreenCap;
    [SerializeField] VRScreenStream vrScreenStream;
    [SerializeField] ADBConnection adbConnection;

    public PopupManager addNewDevicePanel;
    public PopupManager createReportPanel;
    public PopupManager addNotePanel;
    public StatusManager statusManagerScript;
    public CreateReportPanel createReportPanelScript;

    private VisualElement rootMain;

    private bool isFirstFrame = true;
    public bool isDeviceListOpen = true;


    private static ButtonManager instance;
    public static ButtonManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ButtonManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("ButtonManager");
                    instance = obj.AddComponent<ButtonManager>();
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
        if (vrScreenCap != null)
        {
            vrScreenCap.OnScreenshotTaken += ScreenshotDone;
        }

        rootMain = mainUIDocument.rootVisualElement;

        var repLibScrshotsContainer = rootMain.Q<VisualElement>("RLScrnshtsCont");
        if (repLibScrshotsContainer != null)
        {
            repLibScrshotsContainer.Clear();
        }

        HideMakeScreenshotButton();

        // Add new device button
        Button addNewDevice = mainUIDocument.rootVisualElement.Q<Button>("AddNewDeviceButton");

        // Setup New Device Panel Buttons 
        Button cancelDeviceSetup = setupPopupUIDocument.rootVisualElement.Q<Button>("CancelSetupButton");
        Button continueDeviceSetup = setupPopupUIDocument.rootVisualElement.Q<Button>("ContinueSetupButton");
        Button setupDeviceDone = setupPopupUIDocument.rootVisualElement.Q<Button>("DoneSetupButton");

        // Active Device Buttons
        Button createReportPanelButton = mainUIDocument.rootVisualElement.Q<Button>("CreateReportPanelButton");
        Button endReportButton = mainUIDocument.rootVisualElement.Q<Button>("EndReportButton");
        Button moreButton = mainUIDocument.rootVisualElement.Q<Button>("MoreButton");


        // Create Report Panel Buttons
        Button closeReportPanelButton = createReportPopupUIDocument.rootVisualElement.Q<Button>("CancelCreateReportButton");
        Button createReportButton = createReportPopupUIDocument.rootVisualElement.Q<Button>("CreateReportButton");


        Button deviceListButton = mainUIDocument.rootVisualElement.Q<Button>("DeviceListButton2");
        Button reportLibaryButton = mainUIDocument.rootVisualElement.Q<Button>("ReportLibaryButton");
        Button makeScreenshotButton = mainUIDocument.rootVisualElement.Q<Button>("MakeScreenshotButton");

        // Buttons for future functionalities
        //Button renamePanelButton = mainUIDocument.rootVisualElement.Q<Button>("RenameDevicePanelButton");
        //Button deletePanelButton = mainUIDocument.rootVisualElement.Q<Button>("DeleteConnectionPanelButton");
        //Button deleteActiveDevice = setupPopupUIDocument.rootVisualElement.Q<Button>("DeleteActiveDevice");

        // Button Subscribtions
        if (createReportButton != null)
        {
            createReportButton.clicked += () => CreateReport();
        }
        if (createReportPanelButton != null)
        {
            createReportPanelButton.clicked += () => OpenCreateReportPanel();
        }
        if (endReportButton != null)
        {
            endReportButton.clicked += () => EndReport();
        }
        if (closeReportPanelButton != null)
        {
            closeReportPanelButton.clicked += () => CloseCreateReportPanel();
        }
        if (makeScreenshotButton != null)
        {
            makeScreenshotButton.clicked += () => MakeScreenShot();
        }
        if (moreButton != null)
        {
            moreButton.clicked += () => OpenMoreOptions();
        }
        if (continueDeviceSetup != null)
        {
            continueDeviceSetup.clicked += () => ContinueDeviceSetup();
        }
        if (addNewDevice != null)
        {
            addNewDevice.clicked += () => AddNewDevice();
        }
        if (cancelDeviceSetup != null)
        {
            cancelDeviceSetup.clicked += () => NewDeviceSetupCancel();
        }
        if (setupDeviceDone != null)
        {
            setupDeviceDone.clicked += () => DeviceSetupDone();
        }
        if (deviceListButton != null)
        {
            deviceListButton.clicked += () => OpenDeviceList();
        }
        if (reportLibaryButton != null)
        {
            reportLibaryButton.clicked += () => OpenReportLibary();
        }
        statusManagerScript = FindAnyObjectByType<StatusManager>();
        createReportPanelScript = FindAnyObjectByType<CreateReportPanel>();
    }
    void OpenDeviceList()
    {
        Debug.Log("DeviceListButton megnyomva");


        VisualElementOff("ReportLibaryView");
        VisualElementOn("DeviceListView");

        var repLibScrshotsContainer = rootMain.Q<VisualElement>("RLScrnshtsCont");
        if (repLibScrshotsContainer != null)
        {
            repLibScrshotsContainer.Clear();
        }

        if (ConnectionManager.Instance.ActiveDevice != null)
        {
            vrScreenStream.StartStreaming();
        }
        isDeviceListOpen = true;
    }

    void OpenReportLibary()
    {
        Debug.Log("ReportLibaryButton megnyomva");

        if (isFirstFrame)
        {
            ReportManager.Instance.LoadFinishedReports();
            isFirstFrame = false;
        }

        var finishedReports = ReportManager.Instance.FinishedReports;

        ReportLibary.Instance.CreateReportButtons();

        VisualElementOff("DeviceListView");
        VisualElementOn("ReportLibaryView");

        isDeviceListOpen = false;
        vrScreenStream.StopStreaming();
    }

    void ContinueDeviceSetup()
    {
        statusManagerScript.Status2();
        Invoke(nameof(ConnectToDevice), 0.1f);
    }

    public void AddNewDevice()
    {
        statusManagerScript.Status1();
        addNewDevicePanel.ShowPopup();
    }

    void OpenCreateReportPanel()
    {  
        createReportPanelScript.UpdateCreatReportPanelDetails();
        createReportPanel.ShowPopup();
    }

    void EndReport()
    {
        ShowCreateReportButton();
        Invoke(nameof(CloseReport), 0.1f);
    }

    void CloseReport()
    {
        HideMakeScreenshotButton();
        ReportManager.Instance.CloseReport();
        ActiveReportScreenshotsHandler.Instance.ClearActiveReportScreenshots();
    }

    void CloseCreateReportPanel()
    {
        createReportPanel.ClosePopup();
        SetActiveDeviceWithDelay();
    }

    void NewDeviceSetupCancel()
    {
        addNewDevicePanel.ClosePopup();
    }
    void DeviceSetupDone()
    {

        DeviceList.Instance.UpdateInactivatedDeviceLastFrame();
        Device Objectum = ConnectionManager.Instance.ActiveDevice;
        Debug.LogWarning(Objectum);

        SaveDeviceName();
        addNewDevicePanel.ClosePopup();
        DeviceList.Instance.UpdateDeviceList();

        DeviceList.Instance.LastAddedDeviceIsTheChosenOne();
        SetActiveDeviceWithDelay();
        StreamScreen.Instance.SetStreamViewBGColorToBlack();
    }

    public void SetActiveDeviceWithDelay()
    {
        Invoke(nameof(SetActiveDevice), 0.1f);
    }

    void SetActiveDevice()
    {
        DeviceList.Instance.SetActiveDevice(ConnectionManager.Instance.ActiveDeviceIndex + 1);

        Device Objectum = ConnectionManager.Instance.ActiveDevice;
        Debug.LogWarning(Objectum);
        DeviceList.Instance.UpdateDeviceList();

        StatusBarManager.Instance.UpdateTitleOnLiveView();
        StatusBarManager.Instance.UpdateBatteryLevel();
        StreamScreen.Instance.SwitchRatio();

        if (ReportManager.Instance.IsReportActiveOnSelectedDevice())
        {
            ButtonManager.Instance.ShowMakeScreenshotButton();
        }

        else
        {
            ButtonManager.Instance.HideMakeScreenshotButton();
        }

        DeviceList.Instance.HandleOngoingReportsForActiveDevices();
    }

    void SaveDeviceName()
    {
        string textFieldValue = statusManagerScript.inputText;
        Debug.Log("textFieldValue: " + textFieldValue);
        ConnectionManager.Instance.ActiveDevice.UniqueName = textFieldValue;
        JsonManager.Instance.DeviceJsonSerializer(ConnectionManager.Instance.ActiveDevice);
    }

    void SaveUserName()
    {
        string textFieldValue = GetUserNameString();
        Debug.Log("textFieldValue: " + textFieldValue);
        ReportManager.Instance.ActiveReport.ReportUserName = textFieldValue;
    }

    void OpenAddNotePanel()
    {
        addNotePanel.ShowPopup();
    }

    void CloseAddNotePanel()
    {
        addNotePanel.ClosePopup();
    }

    void DeleteActiveDevice()
    {
        DisconnecttFromDevice();
    }

    void OpenMoreOptions()
    {
        Debug.Log("OpenMoreOptions");
    }

    void CreateReport()
    {
        Debug.Log("Create Report()");

        string reportName = GetUserNameString();
        ReportManager.Instance.CreateReport(reportName);

        SaveUserName();
        CloseCreateReportPanel();

        DeviceList.Instance.UpdateDeviceList();
        ShowEndReportButton();
        ShowMakeScreenshotButton();
    }
    public string GetUserNameString()
    {
        string userName = createReportPanelScript.userNameInputText;
        return userName;
    }

    void MakeScreenShot()
    {

        if (vrScreenCap != null)
        {
            HideMakeScreenshotButton();
            DeviceList.Instance.isScreenshotMakingInProgress = true;
            vrScreenCap.TakeScreenshotAsync();
        }
        else
        {
            Debug.LogWarning("VRScreenCap reference not set in ButtonManager.");
        }
    }

    void ConnectToDevice()
    {
        if (adbConnection != null)
        {
            adbConnection.ConnectToDeviceWirelessly();
        }
        else
        {
            Debug.LogWarning("adbConnection reference not set in ButtonManager.");
        }
    }

    void DisconnecttFromDevice()
    {
        if (ConnectionManager.Instance.ActiveDevice == null)
        {
            return;
        }

        var ipAddress = ConnectionManager.Instance.ActiveDevice.IPAddress;
        var port = ConnectionManager.Instance.ActiveDevice.Port;

        if (adbConnection != null && DeviceCleaner.Instance != null)
        {
            DeviceCleaner.Instance.DeleteDevice();
            DeviceCleaner.Instance.DisconnectDevice();
        }
        else
        {
            Debug.LogError("adbConnection reference not set in ButtonManager.");
        }
    }

    void StartStreaming()
    {
        if (vrScreenStream != null)
        {
            vrScreenStream.StartStreaming();
        }
        else
        {
            Debug.LogError("VRScreenStream reference not set in ButtonManager.");
        }
    }
    public void ScreenshotDone()
    {
        AddNotePanel.Instance.EditLastScreenshot();
        ShowMakeScreenshotButton();
        ActiveReportScreenshotsHandler.Instance.LoadActiveReportScreenshots();
        DeviceList.Instance.isScreenshotMakingInProgress = false;
    }


    public void ShowMakeScreenshotButton()
    {
        VisualElementOn("MakeScreenshotButton");
    }

    public void HideMakeScreenshotButton()
    {
        VisualElementOff("MakeScreenshotButton");
    }

    public void ShowEndReportButton()
    {
        VisualElementOff("CreateReportPanelButton");
        VisualElementOn("EndReportButton");
    }

    public void ShowCreateReportButton()
    {
        VisualElementOff("EndReportButton");
        VisualElementOn("CreateReportPanelButton");
    }

    public void VisualElementOn(string VisualElementName)
    {
        VisualElement targetElement;

        targetElement = rootMain.Q<VisualElement>(VisualElementName);

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
        VisualElement targetElement;

        targetElement = rootMain.Q<VisualElement>(VisualElementName);


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
