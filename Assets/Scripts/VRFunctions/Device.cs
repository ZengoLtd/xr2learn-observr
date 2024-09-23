

public class Device : IDevice
{
    private string serialNumber;
    private string ipAddress;
    private int port;
    private string uniqueName;
    private string modelName;
    private string status;
    private int batteryLevel;
    private string screenCrop;
    
    public string SerialNumber 
    { 
        get { return serialNumber; } 
        set { serialNumber = value; } 
    }
    
    public string IPAddress 
    { 
        get { return ipAddress; } 
        set { ipAddress = value; } 
    }
    
    public int Port 
    { 
        get { return port; } 
        set { port = value; } 
    }
    
    public string UniqueName 
    { 
        get { return uniqueName; } 
        set { uniqueName = value; } 
    }
    
    public string ModelName 
    { 
        get { return modelName; } 
        set { modelName = value; } 
    }
    
    public string Status 
    { 
        get { return status; } 
        set { status = value; } 
    }
    
    public int BatteryLevel 
    { 
        get { return batteryLevel; } 
        set { batteryLevel = value; } 
    }
    
    public string ScreenCrop 
    { 
        get { return screenCrop; } 
        set { screenCrop = value; } 
    }

    public Device(string serialNumber, string ipAddress, int port, string uniqueName = "", string modelName = "", string status = "", int batteryLevel = 0, string screenCrop = "")
    {
        this.SerialNumber = serialNumber;
        this.IPAddress = ipAddress;
        this.Port = port;
        this.UniqueName = uniqueName;
        this.ModelName = modelName;
        this.Status = status;
        this.BatteryLevel = batteryLevel;
        this.ScreenCrop = screenCrop;
    }
}
