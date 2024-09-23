using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJsonManager
{
    void DeviceJsonSerializer(Device device);
    void DeviceJsonDeserializer();
}
