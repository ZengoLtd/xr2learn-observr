using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDevice
{
    string SerialNumber { get; set; }
    string IPAddress { get; set; }
    int Port { get; set; }
    string UniqueName { get; set; }
    string ModelName { get; set; }
    string Status { get; set; }
    int BatteryLevel { get; set; }
    string ScreenCrop { get; set; }
}
