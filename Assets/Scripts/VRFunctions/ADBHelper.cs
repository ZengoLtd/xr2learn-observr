using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class ADBHelper : MonoBehaviour
{
    public static ADBHelper Instance;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isMonitoring;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void GetModelName(UnityAction<string> callback ,string serialNumber)
    {
        string command = $"-s {serialNumber} shell getprop ro.product.model";
        UnityAction<string> trimcallback = (string arg) =>
        {
            Debug.Log("test callback");
            callback.Invoke(arg.Trim());
        };
    
        ConnectionManager.Instance.ADBCommand(trimcallback, command);
    }

    public void GetBatteryLevel(UnityAction<int> callback, string serialNumber)
    {
        string command = $"-s {serialNumber} shell dumpsys battery";
        
        UnityAction<string> processOutput = (string output) =>
        {
           // Debug.Log($"ADB Output for GetBatteryLevel: {output}");

            string[] lines = output.Split('\n');
            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("level:"))
                {
                    string levelStr = line.Split(':')[1].Trim();
                    if (int.TryParse(levelStr, out int level))
                    {
                        callback.Invoke(level);
                        return;
                    }
                }
            }
        };

        ConnectionManager.Instance.ADBCommand(processOutput, command);
    }

    public void GetLogcatWithFilter(string ipAddress, string filter)
    {
        string getLogcatCommand = $"-s {ipAddress} logcat -d";
        
        string logcatOutoput = ConnectionManager.Instance.ADBCommand(getLogcatCommand);
        
        foreach (string line in logcatOutoput.Split('\n'))
        {
            if (line.Contains(filter))
            {
                Debug.Log("Filtered line: " + line);
            }
        }
        
        Debug.Log("Logcat output: " + logcatOutoput);
    }
    
    public void LogcatMonitoring(string ipAddress, string filter)
    {
        if (_isMonitoring)
        {
            Debug.Log("Logcat monitoring is already running.");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;
        _isMonitoring = true;

        Thread logcatThread = new Thread(() =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    GetLogcatWithFilter(ipAddress, filter);
                    Thread.Sleep(1000); // Optional: Add a delay to avoid excessive CPU usage
                }
            }
            finally
            {
                _isMonitoring = false;
            }
        });

        logcatThread.IsBackground = true;
        logcatThread.Start();
    }

    public void StopLogcatMonitoring()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    //If the device is disconnected, but added to the application, you can use this method to reconnect
    public void KillDeviceServer(string serialNumber)
    {
        string killServerOnDevice = ConnectionManager.Instance.ADBCommand($"-s {serialNumber} kill-server");
        ConnectionManager.Instance.ConnectedDevices.Remove(ConnectionManager.Instance.ConnectedDevices.Find(device => device.SerialNumber == serialNumber));
    }
    
    public void KillServer()
    {
        string killServer = ConnectionManager.Instance.ADBCommand("kill-server");
        Debug.Log("ADB Server killed.");
    }

    public bool IsDeviceAwake(string serialNumber)
    {
        string command = $"-s {serialNumber} shell dumpsys power";
        
        var awakeCommand = ConnectionManager.Instance.ADBCommand(command);

        foreach (string line in awakeCommand.Split('\n'))
        {
            if (line.Contains("mWakefulness=Awake"))
            {
                return true;
            }
        }

        return false;
    }
}