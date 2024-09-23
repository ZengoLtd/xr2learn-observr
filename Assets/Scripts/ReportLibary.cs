using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using System;

public class ReportLibary : MonoBehaviour
{

    string relativePath;
    string segment;

    public string editorSegment = "/Assets/Report/";
    public string buildSegment = "/ObserVR_Data/Report/";


    private int selectedReport = -1;


    private static ReportLibary _instance;
    public static ReportLibary Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("ReportLibary instance is null. Make sure the script is attached to a GameObject in the scene.");
            }
            return _instance;
        }
    }

    private List<Button> buttonList;

    [SerializeField] UIDocument mainUIDocument;
    private VisualElement rootMain;
    string fixedTimestamp;

    private ReportLibary() { }

    void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        rootMain = mainUIDocument.rootVisualElement;
        buttonList = new List<Button>();


        #if UNITY_EDITOR
                segment = editorSegment;
        #else
                segment = buildSegment;
        #endif


    }

    public void CreateReportButtons()
    {
        buttonList.Clear();

        var template = Resources.Load<VisualTreeAsset>("UXMLTemplates/ReportButtonTemplate");

        var reportsContainer = rootMain.Q<VisualElement>("ReportsContainer");

        if (reportsContainer == null)
        {
            Debug.LogError("RightMid visual element not found.");
            return;
        }

        reportsContainer.Clear();

        if (template == null)
        {
            Debug.LogError("Failed to load UXML template.");
            return;
        }




        List<Report> finishedReports = new List<Report>(ReportManager.Instance.FinishedReports);


        List<Report> uniqueReports = new List<Report>();

        HashSet<string> uniqueReportNames = new HashSet<string>();
        foreach (var report in finishedReports)
        {
            // If the report name is not in the HashSet, add it to the uniqueReports list and the HashSet
            if (uniqueReportNames.Add(report.ReportName))
            {
                uniqueReports.Add(report);
            }
        }

        foreach (var report in uniqueReports)
        {
            Debug.Log($"Finished Report: {report.ReportName}, Created: {report.ReportDateCreated}, Ended: {report.ReportEndTime}");
        }

        List<Report> reportsInRightOrder = new List<Report>();

        reportsInRightOrder = uniqueReports.OrderByDescending(report => DateTime.ParseExact(report.ReportDateCreated, "dd-MM-yyyy", null))
                 .ThenByDescending(report => DateTime.ParseExact(report.ReportEndTime, "HH:mm:ss", null))
                 .ToList();

        int i = 1;

        foreach (var report in reportsInRightOrder)
        {
            Debug.Log($"Right Order Report: {report.ReportName}, Created: {report.ReportDateCreated}, Ended: {report.ReportEndTime}");
        }

        foreach (var report in reportsInRightOrder)
        {
            Button button = new Button();
            button.text = report.ReportName;

            var newButton = template.CloneTree();
            if (newButton == null)
            {
                Debug.LogError("Failed to clone UXML template.");
                return;
            }

            // Find the "Slot" VisualElement
            var slotElement = newButton.Q<VisualElement>("Slot");
            if (slotElement != null)
            {
                // Find the Button inside the "Slot" VisualElement
                var buttonElement = slotElement.Q<Button>("ReportButton");
                int buttonID = i;

                if (buttonElement != null)
                {
                    // Set the name of the button
                    buttonElement.name = $"ReportButton{i}";

                    var usernameLabel = buttonElement.Q<Label>("Username");
                    var startTimeLabel = buttonElement.Q<Label>("StartTime");
                    var finishedTimeLabel = buttonElement.Q<Label>("FinishedTime");
                    var reportListReportDate = buttonElement.Q<Label>("ReportListReportDate");

                    usernameLabel.text = report.ReportUserName;
                    startTimeLabel.text = report.ReportStartTime;
                    finishedTimeLabel.text = report.ReportEndTime;

                    DateTime parsedDate = DateTime.ParseExact(report.ReportDateCreated, "dd-MM-yyyy", null);
                    reportListReportDate.text = parsedDate.ToString("yyyy.MM.dd");

                    // Add the button to the ReportContainer visual element
                    reportsContainer.Add(newButton);

                    // Add the button to the list and subscribe to its click event
                    buttonElement.clicked += () => LoadReportScreenshots(report, buttonElement, buttonID);

                }
                else
                {
                    Debug.LogError("Button element not found in the Slot element.");
                }
            }
            else
            {
                Debug.LogError("Slot element not found in the cloned template.");
            }
            i++;
        }

    }


    public void UpdateSelectedButton()
    {
        foreach (var button in buttonList)
        {
            if (button.name == $"ReportButton{selectedReport}")
            {
                button.AddToClassList("selectedReportClass");
            }
            else
            {
                button.RemoveFromClassList("selectedReportClass");
            }
        }
    }

    void LoadReportScreenshots(Report report, Button button, int buttonID)
    {
        Debug.Log("reportName: " + report.ReportName + " button: " + button.name);

        selectedReport = buttonID;

        UpdateSelectedButton();

        var screenshotsTemplate = Resources.Load<VisualTreeAsset>("UXMLTemplates/RepLibScreenshotsContainer");

        var screenshotsContainer = rootMain.Q<VisualElement>("RLScrnshtsCont");

        if (screenshotsContainer == null)
        {
            Debug.LogError("Screenshots container not found.");
            return;
        }

        screenshotsContainer.Clear();

        if (screenshotsTemplate == null)
        {
            Debug.LogError("Failed to load screenshots template.");
            return;
        }

        List<ScreenshotData> screenshots = new List<ScreenshotData>();

        foreach (var currentReport in ReportManager.Instance.FinishedReports)
        {
            if (currentReport == report)
            {
                screenshots = ReportManager.Instance.GetReportScreenshots(currentReport.ReportName);
                break;
            }
        }

        int i = 1;

        foreach (var screenshot in screenshots)
        {
            var newScreenshotButton = screenshotsTemplate.CloneTree();
            if (newScreenshotButton == null)
            {
                Debug.LogError("Failed to clone screenshots template.");
                continue;
            }

            var screenshotImage = newScreenshotButton.Q<VisualElement>("ScreenshotImage");
            var timestampLabel = newScreenshotButton.Q<Label>("ScreenshotDate");
            var screenshotNumber = newScreenshotButton.Q<Label>("ScreenshotNoText");
            var templDescriptionField = newScreenshotButton.Q<TextField>("ScreenshotDescriptionField");

            templDescriptionField.name = $"ScreenshotDescriptionField{i}";
            var descriptionField = newScreenshotButton.Q<TextField>($"ScreenshotDescriptionField{i}");

            string screenshotNo;
            if (i < 10)
            {
                screenshotNo = "#0" + i.ToString();
            }
            else
            {
                screenshotNo = "#" + i.ToString();
            }

            if (screenshot.Timestamp != null)
            {
                string stringDate = StringToDate(screenshot.Timestamp);
                string stringTime = StringToTime(screenshot.Timestamp);

                fixedTimestamp = stringTime + "  " + stringDate;
            }

            if (screenshotImage != null)
            {

                var pathString = screenshot.Path;
                Debug.Log("screenshot.Path: " + screenshot.Path);
                int index = pathString.IndexOf(segment);

                if (index != -1)
                {
                    // Calculate the start index of the relative path
                    int startIndex = index + segment.Length;

                    // Extract the relative path
                    relativePath = pathString.Substring(startIndex);

                    Debug.Log("relativePath: " + relativePath);
                }
                else
                {
                    Console.WriteLine("Segment not found in the path string.");
                }

                string newPathString = Application.dataPath + "/Report/" + relativePath;
                Debug.Log("newPathString: " + newPathString);

                var texture = LoadTextureFromFile(newPathString);

                if (texture != null)
                {

                    screenshotImage.style.backgroundImage = new StyleBackground(texture);
                }
                else
                {
                    Debug.LogError("Failed to load texture from path: " + screenshot.Path);
                }
            }

            if (screenshotNumber != null)
            {
                screenshotNumber.text = screenshotNo;
            }


            if (timestampLabel != null)
            {
                timestampLabel.text = fixedTimestamp;
            }

            if (descriptionField != null)
            {
                descriptionField.value = screenshot.Description;

                descriptionField.RegisterValueChangedCallback(evt =>
                {
                    SaveDescriptionChanges(report, screenshot.Path, evt.newValue);
                });
            }
            screenshotsContainer.Add(newScreenshotButton);

            i++;
        }

        void SaveDescriptionChanges(Report report, string screenshotPath, string newDescription)
        {

            List<ScreenshotData> screenshots = new List<ScreenshotData>();

            foreach (var currentReport in ReportManager.Instance.FinishedReports)
            {
                if (currentReport == report)
                {
                    screenshots = ReportManager.Instance.GetReportScreenshots(currentReport.ReportName);
                    break;
                }
            }

            for (int i = 0; i < screenshots.Count; i++)
            {
                if (screenshots[i].Path == screenshotPath)
                {
                    screenshots[i].Description = newDescription;
                    break;
                }
            }

            ReportManager.Instance.UpdateScreenshotDescriptionInJson(report.ReportName, screenshotPath, newDescription);
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
}