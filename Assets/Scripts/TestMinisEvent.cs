using System;
using Minis;
using UnityEngine;

public class TestMinisEvent : MonoBehaviour
{
    private void Start()
    {
        MinisEventManager.Instance.NoteOn += NoteOn;
        MinisEventManager.Instance.NoteOff += NoteOff;
        MinisEventManager.Instance.ControlChange += ControlChange;
    }

    private void NoteOn(MinisInput input)
    {
        Debug.Log(string.Format(
            "Note On #{0} ({1}) vel:{2:0.00} ch:{3}",
            input.Number,
            input.ShortName,
            input.Velocity,
            input.Channel
        ));
    }

    private void NoteOff(MinisInput input)
    {
        Debug.Log(string.Format(
            "Note Off #{0} ({1}) ch:{2}",
            input.Number,
            input.ShortName,
            input.Channel
        ));
    }
    
    private void ControlChange(MinisInput input)
    {
        Debug.Log(string.Format(
            "CC #{0} ({1}) value:{2:0.00} ch:{3}",
            input.Number,
            input.ShortName,
            input.Value,
            input.Channel
        ));
    }
}