using NPC.NPCEvent;
using UnityEngine;
using UnityEngine.AI;

public class NPCEventManager : MonoBehaviour
{
    [SerializeField] NPCEvent[] npcEvents;
    [SerializeField] Animator animator; // Référence centrale à l'Animator
    NavMeshAgent _mainAgent;
    float margeTime = 0.01f;

    void Start()
    {
        _mainAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Assure-toi qu'il est bien là

        if (animator == null)
        {
            Debug.LogError("Animator manquant sur " + gameObject.name);
            return;
        }

        foreach (NPCEvent npcEvent in npcEvents)
        {
            npcEvent.InitStrategy(this, animator); // Passe l'Animator à chaque NPCEvent
        }
    }

    void Update()
    {
        float currentTime = TimeRewindManager.Instance.RecordingTime;

        foreach (NPCEvent npcEvent in npcEvents)
        {
            if (npcEvent.InRangeToStart(currentTime, margeTime) && npcEvent.Enabled)
            {
                npcEvent.StartEvent(_mainAgent);
            }
        }
    }
}