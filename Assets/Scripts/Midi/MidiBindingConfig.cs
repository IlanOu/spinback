using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MidiBindingConfig", menuName = "MIDI/MidiBindingConfig")]
public class MidiBindingConfig : ScriptableObject
{
    public enum MidiEventType
    {
        Control,
        Note,
        Aftertouch,
        ChannelPressure,
        PitchBend
    }

    [Serializable]
    public class MidiBindingEntry
    {
        public MidiBind bind;
        public MidiEventType eventType;
        public int channel;
        public int number;
    }

    [SerializeField] private MidiBindingEntry[] bindings;

    private Dictionary<(MidiEventType, int, int), MidiBind> _lookup;

    public void Initialize()
    {
        _lookup = new();

        foreach (var entry in bindings)
        {
            var key = (entry.eventType, entry.number, entry.channel);
            _lookup[key] = entry.bind;
        }
    }

    public bool TryGetBind(MidiEventType type, int number, int channel, out MidiBind bind)
    {
        return _lookup.TryGetValue((type, number, channel), out bind);
    }
}
