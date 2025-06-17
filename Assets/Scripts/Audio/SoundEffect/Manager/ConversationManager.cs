using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ConversationEffectManager), typeof(ConversationUIManager))]
public class ConversationManager : MonoBehaviour
{
    [HideInInspector] public static ConversationManager Instance;
    [SerializeField] private ConversationEffectManager conversationEffectManager;
    [SerializeField] private ConversationUIManager conversationUIManager;
    [SerializeField] private VignetteController vignetteController;
    [SerializeField] private CrowdEffectManager crowdEffectManager;
    [SerializeField] private MusicEffectManager musicEffectManager;
    [SerializeField] private bool conversationEffectEnabled = false;
    [SerializeField, Range(0f, 1f)] private float balance = 0f;
    private float lastTriggeredStep = -1f;
    [SerializeField] private float marginError = 0.08f;

    // C'est uniquement ce controller qui va déterminer si l'effet sonore doit être activé ou non
    private ConversationController conversationController;

    /** Dynamic Variables */
    private float normal => conversationController != null ? conversationController.normalSoundValue : 1f;
    private float distance => conversationEffectEnabled ? GetDistance(normal, balance) : 1f;
    private List<GameObject> otherNPCs => conversationController != null ? AllNPCs.Instance.NPCs.Except(conversationController.npcs).ToList() : new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_1, OnPotentiometer);
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_2, OnPotentiometer);
    }

    void Update()
    {
        conversationEffectManager.Handle(distance, conversationEffectEnabled);
        conversationUIManager.Handle(distance, conversationEffectEnabled);
        musicEffectManager.Handle(distance, conversationEffectEnabled);
    }

    public void EnableSoundEffect(ConversationController controller)
    {
        if (conversationController != null && conversationController != controller) return;
        conversationController = controller;

        UpdateNPCVisibility(true);

        if (conversationEffectEnabled) return;
        conversationEffectEnabled = true;

        vignetteController.Enable();
        // musicEffectManager.Enable();
        crowdEffectManager.Enable();
    }

    public void DisableSoundEffect(ConversationController controller)
    {
        if (conversationController != controller) return;

        UpdateNPCVisibility(false);
        conversationController = null;

        if (!conversationEffectEnabled) return;
        conversationEffectEnabled = false;

        vignetteController.Disable();
        // musicEffectManager.Disable();
        crowdEffectManager.Disable();
    }

    float GetDistance(float normal, float balance)
    {
        float maxDistance = Mathf.Max(normal, 1 - normal);
        float distance = Mathf.Abs(balance - normal);
        float normalizedDistance = distance / maxDistance;
        return Mathf.Max(0f, normalizedDistance - marginError);
    }

    void OnPotentiometer(float value)
    {
        balance = value;
        float currentStep = Mathf.Floor(balance * 10f) / 10f;

        if (!Mathf.Approximately(currentStep, lastTriggeredStep))
        {
            lastTriggeredStep = currentStep;
            if (conversationEffectEnabled)
                UISoundManager.Instance.PlayTicPotentiometer();
        }
    }

    void UpdateNPCVisibility(bool visible)
    {
        if (conversationController == null) return;

        foreach (GameObject npc in conversationController.npcs)
        {
            OutlineCharacter outlineCharacter = npc.GetComponentInChildren<OutlineCharacter>();
            NameCharacter nameCharacter = npc.GetComponentInChildren<NameCharacter>();
            if (outlineCharacter != null)
            {
                outlineCharacter.ForceVisibility(visible);
            }
            if (nameCharacter != null)
            {
                nameCharacter.ForceVisibility(visible);
            }
        }

        foreach (GameObject npc in otherNPCs)
        {
            OutlineCharacter outlineCharacter = npc.GetComponentInChildren<OutlineCharacter>();
            NameCharacter nameCharacter = npc.GetComponentInChildren<NameCharacter>();
            if (outlineCharacter != null)
            {
                outlineCharacter.EnableVisibility(!visible);
            }
            if (nameCharacter != null)
            {
                nameCharacter.EnableVisibility(!visible);
            }
        }
    }
}
