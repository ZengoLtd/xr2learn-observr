using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class ActiveReportScreenshotsHandler : MonoBehaviour
{
    public static ActiveReportScreenshotsHandler Instance { get; private set; }


    List<Button> activeReportScreenshots = new List<Button>();

    [SerializeField] UIDocument mainUIDocument;
    private VisualElement rootMain;

    private VRScreenCap vrScreenCap;
    private ReportManager reportManager;

    private string fixedTimestamp;  

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rootMain = mainUIDocument.rootVisualElement;
    }

    void Start()
    {
        // Az összes többi inicializálás itt történik meg
        vrScreenCap = FindObjectOfType<VRScreenCap>();
        reportManager = FindObjectOfType<ReportManager>();

        if (vrScreenCap != null)
        {
         //   vrScreenCap.OnScreenshotTaken += (path) => UpdateScreenshotList(path);
        }
        else
        {
            Debug.LogError("VRScreenCap not found!");
        }

        if (reportManager != null)
        {
            LoadActiveReportScreenshots();
        }
        else
        {
            Debug.LogError("ReportManager not found!");
        }
    }

    public void LoadActiveReportScreenshots()
    {
        var rightMid = rootMain.Q<VisualElement>("RightMid");
        if (rightMid == null)
        {
            Debug.LogError("RightMid visual element not found.");
            return;
        }

        rightMid.Clear();
        Debug.Log("RightMid visual element cleared.");

        int i = 1;

        List<ScreenshotData> screenshots = reportManager.GetActiveReportScreenshots();

        if (screenshots.Count == 0)
        {
            NoScreenshotsAvailable();
            return;
        }

        foreach (var screenshot in screenshots)
        {
            Debug.LogWarning("Screenshot path: " + screenshot.Path);
            Debug.LogWarning("Screenshot timestamp: " + screenshot.Timestamp);
            Debug.LogWarning("Screenshot description: " + screenshot.Description);

            var template = Resources.Load<VisualTreeAsset>("UXMLTemplates/ActiveDeviceScreenshotButton");
            if (template == null)
            {
                Debug.LogError("Failed to load UXML template.");
                return;
            }

            var newButton = template.CloneTree();
            if (newButton == null)
            {
                Debug.LogError("Failed to clone UXML template.");
                return;
            }


            var buttonElement = newButton.Q<Button>("ActiveDeviceScreenshotButton");

            var timestampLabel = newButton.Q<Label>("DateTextLabel");

            if (buttonElement != null)
            {
                // Set the name of the button
                buttonElement.name = $"ActiveDeviceScreenshotButton{i}";

                var texture = LoadTextureFromFile(screenshot.Path);

                var screenshotBG = buttonElement.Q<VisualElement>("ScreesnhotBG");

                if (texture != null)
                {
                    screenshotBG.style.backgroundImage = new StyleBackground(texture);
                }
                else
                {
                    Debug.LogError("Failed to load texture from path: " + screenshot.Path);
                }

                string indexString = i.ToString();

                if (i < 10)
                {
                    buttonElement.Q<Label>("IndexLabel").text = "#0" + indexString;
                }
                else
                {
                    buttonElement.Q<Label>("IndexLabel").text = "#" + indexString;
                }



                if (screenshot.Timestamp != null)
                {
                    string stringDate = StringToDate(screenshot.Timestamp);
                    string stringTime = StringToTime(screenshot.Timestamp);

                    fixedTimestamp = stringTime + "   " + stringDate;
                }

                if (timestampLabel != null)
                {
                    timestampLabel.text = fixedTimestamp;
                }

                // Add the button to the RightMid visual element
                rightMid.Add(newButton);

                // Add the button to the list and subscribe to its click event
                activeReportScreenshots.Add(buttonElement);
                Debug.Log("Screenshot button added!: " + buttonElement + "  000  " + buttonElement.name);

                buttonElement.clicked += () => OnScreenshotButtonClicked(buttonElement);

                Debug.Log("Screenshot button added!");
            }
            else
            {
                Debug.LogError("Button element not found in the cloned template.");
            }
            i++;
        }
    }


    Texture2D LoadTextureFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found at path: {path}");
            return null;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    public string StringToDate(string timestamp)
    {
        string[] parts = timestamp.Split('_');
        if (parts.Length >= 3)
        {
            return $"{parts[0]}.{parts[1]}.{parts[2]}";
        }
        return string.Empty;
    }


    public string StringToTime(string timestamp)
    {
        string[] parts = timestamp.Split('_');
        if (parts.Length >= 6)
        {
            return $"{parts[3]}:{parts[4]}:{parts[5]}";
        }
        return string.Empty;
    }

    void OnScreenshotButtonClicked(Button buttonElement)
    {
        Debug.Log($"Screenshot button {buttonElement.name} clicked!");
        string buttonName = buttonElement.name;
        int index = int.Parse(buttonName.Replace("ActiveDeviceScreenshotButton", "")) - 1;
        AddNotePanel.Instance.EditSelectedScreenshot(index);
    }

    void NoScreenshotsAvailable()
    {
        Debug.Log("No screenshots available.");

        var noScreenshotTemplate = Resources.Load<VisualTreeAsset>("UXMLTemplates/NoScreenshot");

        if (noScreenshotTemplate == null)
        {
            Debug.LogError("Failed to load UXML template.");
            return;
        }

        var noScreenshotElement = noScreenshotTemplate.CloneTree();

        if (noScreenshotElement == null)
        {
            Debug.LogError("Failed to clone UXML template.");
            return;
        }

        var rightMid = rootMain.Q<VisualElement>("RightMid");
        if (rightMid != null)
        {
            rightMid.Add(noScreenshotElement);
        }
        else
        {
            Debug.LogError("RightMid visual element not found.");
        }   
    }

    public void ClearActiveReportScreenshots()
    {
        var rightMid = rootMain.Q<VisualElement>("RightMid");
        if (rightMid != null)
        {
            rightMid.Clear();
        }
        else
        {
            Debug.LogError("RightMid visual element not found.");
        }
    }
}