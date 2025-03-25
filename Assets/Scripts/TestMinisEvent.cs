using System;
using DefaultNamespace;
using Minis;
using UnityEngine;

public class TestMinisEvent : MonoBehaviour
{
    private void Start()
    {
        MidiBindingRegistry.Instance.Bind(ActionEnum.Zoom,  OnZoom);
        MidiBindingRegistry.Instance.Bind(ActionEnum.Jump,  OnJump);
        MidiBindingRegistry.Instance.Bind(ActionEnum.NotJump,  OnNotJump);
    }

    private void OnZoom(MidiInput input)
    {
        Debug.Log($"OnZoom:  {input.Value}");
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