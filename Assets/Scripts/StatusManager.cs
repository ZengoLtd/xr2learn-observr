using UnityEngine;
using UnityEngine.UIElements;

public class StatusManager : MonoBehaviour
{
    public VisualElement rootIn;
    public string inputText;
    TextField givenDeviceName;


    private void Awake()
    {
        var uIdocument = GetComponent<UIDocument>();
        rootIn = uIdocument.rootVisualElement;

    }
    void Start()
    {
        //var root = GetComponent<UIDocument>().rootVisualElement;

        givenDeviceName = rootIn.Q<TextField>("GivenDeviceName");
        givenDeviceName.RegisterValueChangedCallback(OnTextFieldValueChanged);
        inputText = givenDeviceName.value;

    }


    public void RefreshGivenName()
    {
        var deviceList = ConnectionManager.Instance.ConnectedDevices;
        Device lastDevice = deviceList[deviceList.Count - 1];


        string deviceModelString;

        if (lastDevice != null)
        {
            deviceModelString = lastDevice.ModelName;
        }
        else
        {
            deviceModelString = "Wrong";
        }

        
        
        switch (deviceModelString)
        {
            case "Quest":
                givenDeviceName.value = "Meta Quest";
                break;
            case "Quest 2":
                givenDeviceName.value = "Meta Quest 2";
                break;
            case "Quest 3":
                givenDeviceName.value = "Meta Quest 3";
                break;
            case "Quest Pro":
                givenDeviceName.value = "Meta Quest Pro";
                break;
            case "Wrong":
                givenDeviceName.value = "Error";
                break;
            default:
                givenDeviceName.value = "Unknown";
                break;
        } 
    }


    private void OnTextFieldValueChanged(ChangeEvent<string> evt)
    {
        inputText = evt.newValue;
        Debug.Log("TextField value changed: " + inputText);
    }

    public void Status1()
    {
        VisualElementOn("SetupInstruction1");
        VisualElementOff("SetupInstruction1ErrorNoDevice");
        VisualElementOff("SetupInstruction1ErrorMultipleDevice");

        VisualElementOff("SetupInstruction2");
        VisualElementOff("SetupInstruction3");
        VisualElementOff("SetupInstruction1ErrorUnsucces");
        VisualElementOff("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOn("ConnectionProcess1");
        VisualElementOff("ConnectionProcess2");
        VisualElementOff("ConnectionProcess3");

        VisualElementOn("SetupNewDeviceContainer1");
        VisualElementOff("SetupNewDeviceContainer2");
        VisualElementOff("SetupNewDeviceContainer3");

        VisualElementOn("ButtonContainer1");
        VisualElementOff("ButtonContainer2");
        VisualElementOff("ButtonContainer3"); 
    }
    public void Status2() 
    {
        VisualElementOff("SetupInstruction1");
        VisualElementOn("SetupInstruction2");
        VisualElementOff("SetupInstruction3");

        VisualElementOff("SetupInstruction1ErrorNoDevice");
        VisualElementOff("SetupInstruction1ErrorMultipleDevice");
        VisualElementOff("SetupInstruction1ErrorUnsucces");
        VisualElementOff("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOff("ConnectionProcess1");
        VisualElementOn("ConnectionProcess2");
        VisualElementOff("ConnectionProcess3");

        VisualElementOff("SetupNewDeviceContainer1");
        VisualElementOn("SetupNewDeviceContainer2");
        VisualElementOff("SetupNewDeviceContainer3");

        VisualElementOff("ButtonContainer1");
        VisualElementOn("ButtonContainer2");
        VisualElementOff("ButtonContainer3");
    }
    public void Status3() 
    {
        VisualElementOff("SetupInstruction1");
        VisualElementOff("SetupInstruction2");
        VisualElementOn("SetupInstruction3");

        VisualElementOff("SetupInstruction1ErrorNoDevice");
        VisualElementOff("SetupInstruction1ErrorMultipleDevice");
        VisualElementOff("SetupInstruction1ErrorUnsucces");
        VisualElementOff("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOff("ConnectionProcess1");
        VisualElementOff("ConnectionProcess2");
        VisualElementOn("ConnectionProcess3");

        VisualElementOff("SetupNewDeviceContainer1");
        VisualElementOff("SetupNewDeviceContainer2");
        VisualElementOn("SetupNewDeviceContainer3");

        VisualElementOff("ButtonContainer1");
        VisualElementOff("ButtonContainer2");
        VisualElementOn("ButtonContainer3");
    }

    public void Status1ErrorNoDevice()
    {

        VisualElementOff("SetupInstruction1");
        VisualElementOff("SetupInstruction2");
        VisualElementOff("SetupInstruction3");

        VisualElementOn("SetupInstruction1ErrorNoDevice");
        VisualElementOff("SetupInstruction1ErrorMultipleDevice");
        VisualElementOff("SetupInstruction1ErrorUnsucces");
        VisualElementOff("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOn("ConnectionProcess1");
        VisualElementOff("ConnectionProcess2");
        VisualElementOff("ConnectionProcess3");

        VisualElementOn("SetupNewDeviceContainer1");
        VisualElementOff("SetupNewDeviceContainer2");
        VisualElementOff("SetupNewDeviceContainer3");

        VisualElementOn("ButtonContainer1");
        VisualElementOff("ButtonContainer2");
        VisualElementOff("ButtonContainer3");

    }

    public void Status1ErrorMultipleDevice()
    {

        VisualElementOff("SetupInstruction1");
        VisualElementOff("SetupInstruction2");
        VisualElementOff("SetupInstruction3");

        VisualElementOff("SetupInstruction1ErrorNoDevice");
        VisualElementOn("SetupInstruction1ErrorMultipleDevice");
        VisualElementOff("SetupInstruction1ErrorUnsucces");
        VisualElementOff("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOn("ConnectionProcess1");
        VisualElementOff("ConnectionProcess2");
        VisualElementOff("ConnectionProcess3");

        VisualElementOn("SetupNewDeviceContainer1");
        VisualElementOff("SetupNewDeviceContainer2");
        VisualElementOff("SetupNewDeviceContainer3");

        VisualElementOn("ButtonContainer1");
        VisualElementOff("ButtonContainer2");
        VisualElementOff("ButtonContainer3");
    }

    public void Status1ErrorUnsucces() 
    {

        VisualElementOff("SetupInstruction1");
        VisualElementOff("SetupInstruction2");
        VisualElementOff("SetupInstruction3");

        VisualElementOff("SetupInstruction1ErrorNoDevice");
        VisualElementOff("SetupInstruction1ErrorMultipleDevice");
        VisualElementOn("SetupInstruction1ErrorUnsucces");
        VisualElementOff("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOn("ConnectionProcess1");
        VisualElementOff("ConnectionProcess2");
        VisualElementOff("ConnectionProcess3");

        VisualElementOn("SetupNewDeviceContainer1");
        VisualElementOff("SetupNewDeviceContainer2");
        VisualElementOff("SetupNewDeviceContainer3");

        VisualElementOn("ButtonContainer1");
        VisualElementOff("ButtonContainer2");
        VisualElementOff("ButtonContainer3");
    }


    public void Status1ErrorAlreadyConnected()
    {

        VisualElementOff("SetupInstruction1");
        VisualElementOff("SetupInstruction2");
        VisualElementOff("SetupInstruction3");

        VisualElementOff("SetupInstruction1ErrorNoDevice");
        VisualElementOff("SetupInstruction1ErrorMultipleDevice");
        VisualElementOff("SetupInstruction1ErrorUnsucces");
        VisualElementOn("SetupInstruction1ErrorAlreadyConnected");

        VisualElementOn("ConnectionProcess1");
        VisualElementOff("ConnectionProcess2");
        VisualElementOff("ConnectionProcess3");

        VisualElementOn("SetupNewDeviceContainer1");
        VisualElementOff("SetupNewDeviceContainer2");
        VisualElementOff("SetupNewDeviceContainer3");

        VisualElementOn("ButtonContainer1");
        VisualElementOff("ButtonContainer2");
        VisualElementOff("ButtonContainer3");
    }



    public void VisualElementOn(string VisualElementName)

    {
        VisualElement targetElement;

        targetElement = rootIn.Q<VisualElement>(VisualElementName);

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

        targetElement = rootIn.Q<VisualElement>(VisualElementName);


        if (targetElement != null)
        {
            targetElement.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogWarning("targetElement element not found.");
        }
    }

    public void SetVisibilityOfVisualElement(string VisualElementName, bool isVisible)

    {
        VisualElement targetElement;
        Debug.Log(VisualElementName);

        targetElement = rootIn.Q<VisualElement>(VisualElementName);


        if (targetElement != null)
        {
            targetElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        else
        {
            Debug.LogWarning("targetElement element not found.");
        }
    }

 


}
