using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReportManager
{
    bool IsReportActiveOnSelectedDevice();
    void SerializeReport();
    void LoadFinishedReports();
    void CreateDirectory(string path);
    void CreateReport(string reportName);
    void CloseReport();
    int ChangeActiveReportTo();
    string CreateReportDescriptionText(string text, string descriptionName);
    void AddScreenshotToJson(string reportName, string screenshotPath, string timestamp);
    void AddDescriptionToJson(string reportName, string description);
    void UpdateScreenshotDescriptionInJson(string reportName, string screenshotPath, string description);
    string GetLastUsernameOfDevice(Device device);
    string GetLastReportDateOfDevice(Device device);
    void DeleteReport(string reportName);
    List<ScreenshotData> GetActiveReportScreenshots();
    List<ScreenshotData> GetReportScreenshots(string currentReport);
    Texture2D GetLastScreenshotOfDevice(Device device);
}
