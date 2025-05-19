using UnityEngine;
using Minis;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(MidiInputCallback))]
public class MidiDebugger : MonoBehaviour
{
    private MidiInputCallback _inputCallback;
    void OnWillNoteOn(MidiNoteControl note, float velocity)
      => Debug.Log($"Ch.{note.channel,-2} " +
                   $"{note.shortDisplayName,3} ({note.noteNumber:000}) " +
                   $"Note On  {velocity * 100,3:0}%");

    void OnWillNoteOff(MidiNoteControl note)
      => Debug.Log($"Ch.{note.channel,-2} " +
                   $"{note.shortDisplayName,3} ({note.noteNumber:000}) " +
                   "Note Off");

    void OnWillAftertouch(MidiNoteControl note, float pressure)
      => Debug.Log($"Ch.{note.channel,-2} " +
                   $"{note.shortDisplayName,3} ({note.noteNumber:000}) " +
                   $"Pressure {pressure * 100,3:0}%");

    void OnWillControlChange(MidiValueControl cc, float value)
      => Debug.Log($"Ch.{cc.channel,-2} " +
                   $"CC ({cc.controlNumber:000}) {value * 100,3:0}%");

    void OnWillChannelPressure(AxisControl axis, float value)
      => Debug.Log($"Ch.{((MidiDevice)axis.device).channel,-2} " +
                   $"Pressure {value * 100,3:0}%");

    void OnWillPitchBend(AxisControl axis, float value)
      => Debug.Log($"Ch.{((MidiDevice)axis.device).channel,-2} " +
                   $"PitchBend {value * 100,3:0}%");

    void OnEnable()
    {
        _inputCallback.OnNoteOn += OnWillNoteOn;
        _inputCallback.OnNoteOff += OnWillNoteOff;
        _inputCallback.OnAftertouch += OnWillAftertouch;
        _inputCallback.OnControlChange += OnWillControlChange;
        _inputCallback.OnChannelPressure += OnWillChannelPressure;
        _inputCallback.OnPitchBend += OnWillPitchBend;
    }

    void OnDisable()
    {
        _inputCallback.OnNoteOn -= OnWillNoteOn;
        _inputCallback.OnNoteOff -= OnWillNoteOff;
        _inputCallback.OnAftertouch -= OnWillAftertouch;
        _inputCallback.OnControlChange -= OnWillControlChange;
        _inputCallback.OnChannelPressure -= OnWillChannelPressure;
        _inputCallback.OnPitchBend -= OnWillPitchBend;
    }

    void Awake()
    {
        _inputCallback = GetComponent<MidiInputCallback>();
    }
}