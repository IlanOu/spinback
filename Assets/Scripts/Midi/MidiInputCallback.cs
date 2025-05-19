using System;
using System.Collections.Generic;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MidiInputCallback : MonoBehaviour
{
    List<MidiDevice> _midiDevices = new();

    public event Action<MidiNoteControl, float> OnNoteOn;
    public event Action<MidiNoteControl> OnNoteOff;
    public event Action<MidiNoteControl, float> OnAftertouch;
    public event Action<MidiValueControl, float> OnControlChange;
    public event Action<AxisControl, float> OnChannelPressure;
    public event Action<AxisControl, float> OnPitchBend;

    void OnWillNoteOn(MidiNoteControl note, float velocity)
      => OnNoteOn?.Invoke(note, velocity);

    void OnWillNoteOff(MidiNoteControl note)
      => OnNoteOff?.Invoke(note);

    void OnWillAftertouch(MidiNoteControl note, float pressure)
      => OnAftertouch?.Invoke(note, pressure);

    void OnWillControlChange(MidiValueControl cc, float value)
      => OnControlChange?.Invoke(cc, value);

    void OnWillChannelPressure(AxisControl axis, float value)
      => OnChannelPressure?.Invoke(axis, value);

    void OnWillPitchBend(AxisControl axis, float value)
      => OnPitchBend?.Invoke(axis, value);

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        DisconnectAllDevices();
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        var midiDevice = device as MidiDevice;
        if (midiDevice == null) return;

        ConnectMidiDevice(midiDevice);
    }

    void ConnectMidiDevice(MidiDevice midiDevice)
    {
        midiDevice.onWillNoteOn += OnWillNoteOn;
        midiDevice.onWillNoteOff += OnWillNoteOff;
        midiDevice.onWillAftertouch += OnWillAftertouch;
        midiDevice.onWillControlChange += OnWillControlChange;
        midiDevice.onWillChannelPressure += OnWillChannelPressure;
        midiDevice.onWillPitchBend += OnWillPitchBend;

        _midiDevices.Add(midiDevice);
    }

    void DisconnectAllDevices()
    {
        foreach (var midiDevice in _midiDevices)
        {
            midiDevice.onWillNoteOn -= OnWillNoteOn;
            midiDevice.onWillNoteOff -= OnWillNoteOff;
            midiDevice.onWillAftertouch -= OnWillAftertouch;
            midiDevice.onWillControlChange -= OnWillControlChange;
            midiDevice.onWillChannelPressure -= OnWillChannelPressure;
            midiDevice.onWillPitchBend -= OnWillPitchBend;
        }
        _midiDevices.Clear();
    }
}
