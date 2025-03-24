using UnityEngine;
using UnityEngine.InputSystem;

public class MidiDeviceDetector : MonoBehaviour
{
    void Start()
    {
        var allDevices = InputSystem.devices;
        Debug.Log($"Nombre total de périphériques: {allDevices.Count}");
        
        foreach (var device in allDevices)
        {
            Debug.Log($"Périphérique: {device.name}, Type: {device.GetType().Name}");
        }
    }
}