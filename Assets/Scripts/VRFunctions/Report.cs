using System;

public class Report
{
    private string reportName;
    private string reportOwnerDeviceSerial;
    private Device reportOwnerDevice;
    private string reportStartTime;
    private string reportEndTime;
    private string reportDateCreated;
    private string reportUserName;
    
    public string ReportName
    {
        get { return reportName; }
        set { reportName = value; }
    }
    
    public string ReportOwnerDeviceSerial
    {
        get { return reportOwnerDeviceSerial; }
        set { reportOwnerDeviceSerial = value; }
    }
    
    public Device ReportOwnerDevice
    {
        get { return reportOwnerDevice; }
        set { reportOwnerDevice = value; }
    }
    
    public string ReportStartTime
    {
        get { return reportStartTime; }
        set { reportStartTime = value; }
    }
    
    public string ReportEndTime
    {
        get { return reportEndTime; }
        set { reportEndTime = value; }
    }
    
    public string ReportDateCreated
    {
        get { return reportDateCreated; }
        set { reportDateCreated = value; }
    }

    public string ReportUserName
    {
        get { return reportUserName; }
        set { reportUserName = value; }
    }
    
    public Report(string reportName, string reportOwnerDeviceSerial, Device reportOwnerDevice, string reportUserName = "John Doe")
    {
        this.reportName = reportName;
        this.reportOwnerDeviceSerial = reportOwnerDeviceSerial;
        this.reportOwnerDevice = reportOwnerDevice;
        this.reportUserName = reportUserName;
        
        ReportDateCreated = DateTime.Now.ToString("dd-MM-yyyy");
        ReportStartTime = DateTime.Now.ToString("HH:mm:ss");
    }
}
