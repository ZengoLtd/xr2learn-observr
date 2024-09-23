using UnityEngine;
using UnityEngine.UIElements;

public class CreateReportPanel : MonoBehaviour
{
    public VisualElement root;
    public string userNameInputText;

    TextField deviceModelField;
    TextField deviceNameField;

    public string defaultUsername = "User";


    private void Awake()
    {
        var uIdocument = GetComponent<UIDocument>();
        root = uIdocument.rootVisualElement;

        deviceModelField = root.Q<TextField>("DeviceModelField");
        deviceNameField = root.Q<TextField>("GivenDeviceNameField");
    }
    void Start()
    {

        var userName = root.Q<TextField>("GivenUserName");
        userName.RegisterValueChangedCallback(OnTextFieldValueChanged);
        userNameInputText = userName.value;

    }

    private void OnTextFieldValueChanged(ChangeEvent<string> evt)
    {
        userNameInputText = evt.newValue;
        Debug.Log("TextField value changed: " + userNameInputText);
    }

    public void UpdateCreatReportPanelDetails()
    {

        Device activeDevice = ConnectionManager.Instance.ActiveDevice;

        string deviceModel;
        string uniqueDeviceName;

        if (activeDevice != null)
        {
            deviceModel = activeDevice.ModelName;
            uniqueDeviceName = activeDevice.UniqueName;
        }
        else
        {
            deviceModel = "No active device";
            uniqueDeviceName = "No active device";
        }

        if (deviceNameField != null)
        {
            deviceNameField.value = uniqueDeviceName;
        }

        if (deviceModelField != null)
        {
            deviceModelField.value = deviceModel;
        }

        if (userNameInputText != null)
        {
            root.Q<TextField>("GivenUserName").value = defaultUsername;
        }

    }
}