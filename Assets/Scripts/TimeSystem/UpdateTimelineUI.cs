using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class UpdateTimelineUI : MonoBehaviour
{
    [SerializeField] private MoveCursor moveCursor;
    private PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Update()
    {
        moveCursor.position = (float)(director.time / director.duration);
    }
}
