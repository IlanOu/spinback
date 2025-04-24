using UnityEngine;
using UnityEngine.AI;

public class NPCEventManager : MonoBehaviour
{
    [SerializeField] NPCEvent[] npcEvents;
    NavMeshAgent _mainAgent;
    float margeTime = 0.01f;

    void Start()
    {
        _mainAgent = GetComponent<NavMeshAgent>();
        foreach (NPCEvent npcEvent in npcEvents)
        {
            npcEvent.InitStrategy(this);
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