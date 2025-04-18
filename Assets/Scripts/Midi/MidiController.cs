// Exemple de script C# pour détection MIDI

using Minis;
using UnityEngine;
using UnityEngine.InputSystem;

public class MidiController : MonoBehaviour
{
    void OnEnable()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change == InputDeviceChange.Added && device is MidiDevice)
                Debug.Log($"Connecté: {device.name}");
        };
    }
}
