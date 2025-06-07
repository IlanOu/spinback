using System;
using System.Collections.Generic;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(MidiInputCallback))]
public class MidiBinding : MonoBehaviour
{
    [SerializeField] private MidiBindingConfig config;
    [HideInInspector] public static MidiBinding Instance;
    private MidiInputCallback _inputCallback;
    private Dictionary<MidiBind, Action<float>> _listeners = new();

    void OnWillNoteOn(MidiNoteControl note, float velocity)
    {
        Handle(MidiBindingConfig.MidiEventType.Note, note.noteNumber, note.channel, velocity);
    }

    void OnWillNoteOff(MidiNoteControl note)
    {
        Handle(MidiBindingConfig.MidiEventType.Note, note.noteNumber, note.channel, 0f);
    }

    void OnWillAftertouch(MidiNoteControl note, float pressure)
    {
        Handle(MidiBindingConfig.MidiEventType.Aftertouch, note.noteNumber, note.channel, pressure);
    }

    void OnWillControlChange(MidiValueControl cc, float value)
    {
        Handle(MidiBindingConfig.MidiEventType.Control, cc.controlNumber, cc.channel, value);
    }


    void OnWillChannelPressure(AxisControl axis, float value)
    {
        Handle(MidiBindingConfig.MidiEventType.ChannelPressure, 0, ((MidiDevice)axis.device).channel, value);
    }

    void OnWillPitchBend(AxisControl axis, float value)
    {
        Handle(MidiBindingConfig.MidiEventType.PitchBend, 0, ((MidiDevice)axis.device).channel, value);
    }

    void OnEnable()
    {
        if (_inputCallback == null) return;

        _inputCallback.OnNoteOn += OnWillNoteOn;
        _inputCallback.OnNoteOff += OnWillNoteOff;
        _inputCallback.OnAftertouch += OnWillAftertouch;
        _inputCallback.OnControlChange += OnWillControlChange;
        _inputCallback.OnChannelPressure += OnWillChannelPressure;
        _inputCallback.OnPitchBend += OnWillPitchBend;
    }

    void OnDisable()
    {
        if (_inputCallback == null) return;

        _inputCallback.OnNoteOn -= OnWillNoteOn;
        _inputCallback.OnNoteOff -= OnWillNoteOff;
        _inputCallback.OnAftertouch -= OnWillAftertouch;
        _inputCallback.OnControlChange -= OnWillControlChange;
        _inputCallback.OnChannelPressure -= OnWillChannelPressure;
        _inputCallback.OnPitchBend -= OnWillPitchBend;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        config.Initialize();
        _inputCallback = GetComponent<MidiInputCallback>();
    }

    private void Handle(MidiBindingConfig.MidiEventType type, int number, int channel, float value)
    {
        if (config.TryGetBind(type, number, channel, out var bind) &&
            _listeners.TryGetValue(bind, out var callback))
        {
            callback?.Invoke(value);
        }
    }

    public void Subscribe(MidiBind bind, Action<float> callback)
    {
        if (!_listeners.ContainsKey(bind))
            _listeners[bind] = callback;
        else
            _listeners[bind] += callback;
    }

    public void Unsubscribe(MidiBind bind, Action<float> callback)
    {
        if (_listeners.ContainsKey(bind))
            _listeners[bind] -= callback;
    }
    
    public void UnsubscribeAll()
    {
        _listeners.Clear();
    }
}
