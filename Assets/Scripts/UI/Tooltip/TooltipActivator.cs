using System;
using System.Collections.Generic;
using UnityEngine;

public class TooltipActivator : MonoBehaviour
{
    public static TooltipActivator Instance;
    [SerializeField] private List<TooltipEntry> tooltipEntries;

    private Dictionary<TooltipType, GameObject> tooltipMap;
    private Dictionary<TooltipType, List<Action>> deactivationSubscribers = new();
    private Vector3 lastMousePosition;
    private Vector3 lastCameraRotation;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialise le dictionnaire
        tooltipMap = new Dictionary<TooltipType, GameObject>();
        foreach (var entry in tooltipEntries)
        {
            if (!tooltipMap.ContainsKey(entry.type) && entry.tooltipObject != null)
            {
                tooltipMap.Add(entry.type, entry.tooltipObject);
            }
        }
    }

    void Start()
    {
        lastMousePosition = Input.mousePosition;
        lastCameraRotation = Camera.main.transform.rotation.eulerAngles;

        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_1, (float value) => DisableTooltip(TooltipType.Potentiometer));
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_2, (float value) => DisableTooltip(TooltipType.Potentiometer));

        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_1, (float value) => DisableTooltip(TooltipType.Slider));
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_2, (float value) => DisableTooltip(TooltipType.Slider));

        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_1, (float value) => DisableTooltip(TooltipType.JogWheel));
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_2, (float value) => DisableTooltip(TooltipType.JogWheel));
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_1, (float value) => DisableTooltip(TooltipType.JogWheel));
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_2, (float value) => DisableTooltip(TooltipType.JogWheel));
    }

    void Update()
    {
        if (tooltipMap.TryGetValue(TooltipType.Mouse, out var mouseTooltip) && mouseTooltip.activeSelf)
        {
            if (Input.mousePosition != lastMousePosition || Camera.main.transform.rotation.eulerAngles != lastCameraRotation)
            {
                DisableTooltip(TooltipType.Mouse);
            }
        }
    }

    public void EnableTooltip(TooltipType type)
    {
        if (tooltipMap.TryGetValue(type, out var tooltip))
        {
            if (type == TooltipType.Mouse)
            {
                lastMousePosition = Input.mousePosition;
                lastCameraRotation = Camera.main.transform.rotation.eulerAngles;
            }
            tooltip.SetActive(true);
        }
    }

    public void DisableTooltip(TooltipType type)
    {
        if (tooltipMap.TryGetValue(type, out var tooltip) && tooltip.activeSelf)
        {
            tooltip.SetActive(false);

            if (deactivationSubscribers.TryGetValue(type, out var subscribers))
            {
                foreach (var callback in subscribers)
                {
                    callback?.Invoke();
                }
            }
        }
    }

    public void DisableAllTooltips(TooltipType[] exceptions = null)
    {
        HashSet<TooltipType> exceptionSet = exceptions != null ? new HashSet<TooltipType>(exceptions) : new HashSet<TooltipType>();
        foreach (var kvp in tooltipMap)
        {
            if (!exceptionSet.Contains(kvp.Key))
            {
                kvp.Value.SetActive(false);
            }
        }
    }

    public void SubscribeToDeactivation(TooltipType type, Action callback)
    {
        if (!deactivationSubscribers.ContainsKey(type))
        {
            deactivationSubscribers[type] = new List<Action>();
        }

        if (!deactivationSubscribers[type].Contains(callback))
        {
            deactivationSubscribers[type].Add(callback);
        }
    }

    public void UnsubscribeFromDeactivation(TooltipType type, Action callback)
    {
        if (deactivationSubscribers.ContainsKey(type))
        {
            deactivationSubscribers[type].Remove(callback);
            if (deactivationSubscribers[type].Count == 0)
            {
                deactivationSubscribers.Remove(type);
            }
        }
    }
}