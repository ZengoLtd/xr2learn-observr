using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IConnectionManager
{
    string ADBCommand(string arguments);
    List<string> GetConnectedDevices();
    List<string> GetDeviceIPs();
    List<(string DeviceSerial, string IpAddress)> PairDeviceSerialsWithIPs();
    void ActivateDevice(int index);
}
