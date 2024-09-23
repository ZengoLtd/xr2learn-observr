using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ReportManager : MonoBehaviour, IReportManager
{

    private static ReportManager instance;

    private int activeReportIndex = -1;
    private List<Report> ongoingReports = new List<Report>();
    private List<Report> finishedReports = new List<Report>();

    public static ReportManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ReportManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("ReportManager");
                    instance = obj.AddComponent<ReportManager>();
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

    public List<Report> OngoingReports
    {
        get { return ongoingReports; }
        set { ongoingReports = value; }
    }

    public List<Report> FinishedReports
    {
        get { return finishedReports; }
        set { finishedReports = value; }
    }

    public Report ActiveReport
    {
        get
        {
            if (OngoingReports.Count == 0 || ActiveReportIndex < 0)
            {
                return null;
            }
            return OngoingReports[ActiveReportIndex];
        }
        set
        {
            OngoingReports[ActiveReportIndex] = value;
        }
    }

    private void Start()
    {
        CreateDirectory(Application.dataPath + "/Report");
    }

    public bool IsReportActiveOnSelectedDevice()
    {
        return OngoingReports.Any(report => report.ReportOwnerDevice == ConnectionManager.Instance.ActiveDevice);
    }

    public void SerializeReport()
    {
        if (ActiveReport == null)
        {
            Debug.LogWarning("No active report to serialize.");
            return;
        }

        string reportDirectory = Application.dataPath + $"/Report/{ActiveReport.ReportName}";
        CreateDirectory(reportDirectory);

        string jsonFilePath = Path.Combine(reportDirectory, "Report.json");
        string jsonContent = JsonConvert.SerializeObject(ActiveReport, Formatting.Indented);

        File.WriteAllText(jsonFilePath, jsonContent);
        Debug.Log($"Report serialized to: {jsonFilePath}");
    }

    public void LoadFinishedReports()
    {
        string reportDirectory = Application.dataPath + "/Report";
        if (!Directory.Exists(reportDirectory))
        {
            Debug.LogWarning("Report directory does not exist.");
            return;
        }

        string[] subdirectories = Directory.GetDirectories(reportDirectory);
        foreach (string subdirectory in subdirectories)
        {
            string jsonFilePath = Path.Combine(subdirectory, "Report.json");
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    Report report = JsonConvert.DeserializeObject<Report>(jsonContent);
                    finishedReports.Add(report);
                    Debug.Log($"Report loaded from: {jsonFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to deserialize report from {jsonFilePath}: {ex.Message}");
                }
            }
        }
    }

    public int ActiveReportIndex
    {
        get { return activeReportIndex; }
        set { activeReportIndex = value; }
    }

    public void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log($"Directory created at: {path}");
        }
        else
        {
            Debug.Log($"Directory already exists at: {path}");
        }
    }

    public void CreateReport(string reportName)
    {
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

        Report newReport = new Report(reportName + "_" + timestamp, ConnectionManager.Instance.ActiveDevice.SerialNumber, ConnectionManager.Instance.ActiveDevice);

        OngoingReports.Add(newReport);

        CreateDirectory(Application.dataPath + $"/Report/{newReport.ReportName}");

        ActiveReportIndex = ChangeActiveReportTo();
    }

    public void CloseReport()
    {
        if (ActiveReport != null)
        {
            ActiveReport.ReportEndTime = DateTime.Now.ToString("HH:mm:ss");
            SerializeReport();
            FinishedReports.Add(ActiveReport);
            OngoingReports.Remove(ActiveReport);
            ActiveReportIndex = -1;
        }
    }

    public int ChangeActiveReportTo()
    {
        foreach (Report report in OngoingReports)
        {
            if (report.ReportOwnerDevice == ConnectionManager.Instance.ActiveDevice)
            {
                ActiveReportIndex = OngoingReports.IndexOf(report);
                return ActiveReportIndex;
            }
        }

        return ActiveReportIndex = -1;
    }

    public string CreateReportDescriptionText(string text, string descriptionName)
    {
        if (ActiveReport != null)
        {
            string reportDirectory = Application.dataPath + $"/Report/{ActiveReport.ReportName}";
            CreateDirectory(reportDirectory);

            //TODO: description name to screenshot name
            string filePath = Path.Combine(reportDirectory, $"{descriptionName}.txt");
            File.WriteAllText(filePath, text);

            Debug.Log($"Description text saved to: {filePath}");

            // Add description to JSON
            AddDescriptionToJson(ActiveReport.ReportName, text);
            ScreenshotDone();
        }
        else
        {
            Debug.Log("No Active Report to save description text!");
            return null;
        }

        return text;
    }

    void ScreenshotDone()
    {

        if (ActiveReport != null)
        {
            StatusBarManager.Instance.newScreenshotDone = true;
        }
    }


    public void AddScreenshotToJson(string reportName, string screenshotPath, string timestamp)
    {
        string jsonFilePath = Application.dataPath + $"/Report/{reportName}/{reportName}.json";
        List<ReportEntry> reportEntries = new List<ReportEntry>();

        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            reportEntries = JsonUtility.FromJson<ReportEntryList>(jsonContent).Reports;
        }

        ReportEntry newEntry = new ReportEntry
        {
            ScreenshotPath = screenshotPath,
            Timestamp = timestamp
        };

        reportEntries.Add(newEntry);

        ReportEntryList reportEntryList = new ReportEntryList { Reports = reportEntries };
        string newJsonContent = JsonUtility.ToJson(reportEntryList, true);
        File.WriteAllText(jsonFilePath, newJsonContent);
    }

    public void AddDescriptionToJson(string reportName, string description)
    {
        string jsonFilePath = Application.dataPath + $"/Report/{reportName}/{reportName}.json";
        List<ReportEntry> reportEntries = new List<ReportEntry>();

        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            reportEntries = JsonUtility.FromJson<ReportEntryList>(jsonContent).Reports;
        }

        if (reportEntries.Count > 0)
        {
            reportEntries[reportEntries.Count - 1].Description = description;
        }

        ReportEntryList reportEntryList = new ReportEntryList { Reports = reportEntries };
        string newJsonContent = JsonUtility.ToJson(reportEntryList, true);
        File.WriteAllText(jsonFilePath, newJsonContent);
    }

    // Update screenshot description in JSON file
    public void UpdateScreenshotDescriptionInJson(string reportName, string screenshotPath, string description)
    {
        string jsonFilePath = Application.dataPath + $"/Report/{reportName}/{reportName}.json";
        List<ReportEntry> reportEntries = new List<ReportEntry>();

        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            reportEntries = JsonUtility.FromJson<ReportEntryList>(jsonContent).Reports;
        }

        foreach (var entry in reportEntries)
        {
            if (entry.ScreenshotPath == screenshotPath)
            {
                entry.Description = description;
                break;
            }
        }

        ReportEntryList reportEntryList = new ReportEntryList { Reports = reportEntries };
        string newJsonContent = JsonUtility.ToJson(reportEntryList, true);
        File.WriteAllText(jsonFilePath, newJsonContent);
    }


    public string GetLastUsernameOfDevice(Device device)
    {
        return OngoingReports.Where(report => report.ReportOwnerDevice == device).Select(report => report.ReportUserName).LastOrDefault();
    }

    public string GetLastReportDateOfDevice(Device device)
    {
        return OngoingReports.Where(report => report.ReportOwnerDevice == device).Select(report => report.ReportStartTime).LastOrDefault();
    }

    public void DeleteReport(string reportName)
    {
        string jsonFilePath = Application.dataPath + $"/Report/{reportName}/{reportName}.json";

        if (File.Exists(jsonFilePath))
        {
            Debug.Log("Json file is exist to delete report.");


        }
    }

    public List<ScreenshotData> GetActiveReportScreenshots()
    {
        if (ActiveReport == null)
        {
            Debug.LogWarning("No active report found.");
            return new List<ScreenshotData>();
        }

        string reportName = ActiveReport.ReportName;
        string reportPath = Path.Combine(Application.dataPath, "Report", reportName, $"{reportName}.json");


        if (!File.Exists(reportPath))
        {
            Debug.LogError($"Screenshot file not found at path: {reportPath}");
            return new List<ScreenshotData>();
        }

        try
        {
            string json = File.ReadAllText(reportPath);
            ReportEntryList reportEntryList = JsonConvert.DeserializeObject<ReportEntryList>(json);
            List<ScreenshotData> screenshots = reportEntryList.Reports.Select(entry => new ScreenshotData
            {
                Path = entry.ScreenshotPath,
                Timestamp = entry.Timestamp,
                Description = entry.Description
            }).ToList();

            return screenshots;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read screenshots from JSON: {ex.Message}");
            return new List<ScreenshotData>();
        }
    }

    public List<ScreenshotData> GetReportScreenshots(string currentReport)
    {


        string reportName = currentReport;
        string reportPath = Path.Combine(Application.dataPath, "Report", reportName, $"{reportName}.json");


        if (!File.Exists(reportPath)) // L�tezik a file m�gsem tal�lja
        {
            Debug.LogError($"Screenshot file not found at path: {reportPath}");
            return new List<ScreenshotData>();
        }

        try
        {
            string json = File.ReadAllText(reportPath);
            ReportEntryList reportEntryList = JsonConvert.DeserializeObject<ReportEntryList>(json);
            List<ScreenshotData> screenshots = reportEntryList.Reports.Select(entry => new ScreenshotData
            {
                Path = entry.ScreenshotPath,
                Timestamp = entry.Timestamp,
                Description = entry.Description
            }).ToList();

            return screenshots;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read screenshots from JSON: {ex.Message}");
            return new List<ScreenshotData>();
        }
    }


    public Texture2D GetLastScreenshotOfDevice(Device device)
    {
        Texture2D lastScreenhotOfDevice = null;

        // itt a lastScreenhotOfDevice -nak adjuk meg az utoljára készül screesnhot-ot


        if (lastScreenhotOfDevice != null)
        {
            return lastScreenhotOfDevice;
        }


        else
        {
            return null;
        }
    }
    

}

[Serializable]
public class ReportEntry
{
    public string ScreenshotPath;
    public string Timestamp;
    public string Description;
}

[Serializable]
public class ReportEntryList
{
    public List<ReportEntry> Reports;
}
