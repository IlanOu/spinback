using System;
using Minis;
using UnityEngine;

public class TestMinisEvent : MonoBehaviour
{
    private void Start()
    {
        MidiBindingRegistry.Instance.Bind("OnZoom",  OnZoom);
        MidiBindingRegistry.Instance.Bind("OnJump",  OnJump);
        MidiBindingRegistry.Instance.Bind("OnNotJump",  OnNotJump);
    }

    private void OnZoom(MidiInput input)
    {
        Debug.Log("OnZoom");
    }
    
    private void OnJump(MidiInput input)
    {
        Debug.Log("OnJump");
    }
    
    private void OnNotJump(MidiInput input)
    {
        Debug.Log("OnNotJump");
    }
}