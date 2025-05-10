using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class RewindTimeline : MonoBehaviour
{
    public float rewindSpeedMultiplier = 5f;

    public bool isRewinding = false;
    private PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Update()
    {
        HandleRewindMode();
        HandleRewinding();
    }

    void HandleRewindMode()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRewinding = !isRewinding;

            if (isRewinding)
            {
                director.Pause();
                director.Evaluate();
            }
            else
            {
                director.Play();
            }
        }
    }

    void HandleRewinding()
    {
        if (isRewinding)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.001f)
            {
                double newTime = director.time - scroll * rewindSpeedMultiplier;
                newTime = Mathf.Clamp((float)newTime, 0f, (float)director.duration);
                director.time = newTime;
                director.Evaluate();
            }
        }
    }
}