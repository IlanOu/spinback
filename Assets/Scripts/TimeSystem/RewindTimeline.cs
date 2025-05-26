using Minis;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class RewindTimeline : MonoBehaviour
{
    [Header("Rewind Settings")]
    [SerializeField] private float jogSensitivity = 0.001f;
    [SerializeField] private float scrollMultiplier = 5f;
    [SerializeField] private float pauseDelay = 0.1f;
    [SerializeField] private float jogSpeed = 10f;

    private PlayableDirector director;
    private bool isTouchingPlatine = false;
    private float timeSinceLastJogEvent = 0f;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_1, OnJogNote);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_1, OnRoll);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_1, OnRoll);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_2, OnJogNote);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_2, OnRoll);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_2, OnRoll);

        if (director.state != PlayState.Playing)
        {
            director.Play();
        }
    }

    void Update()
    {
        HandleMouseScroll();
        HandlePlatineState();
    }

    private void HandlePlatineState()
    {
        timeSinceLastJogEvent += Time.deltaTime;

        if (isTouchingPlatine)
        {
            if (timeSinceLastJogEvent > pauseDelay)
            {
                if (director.state == PlayState.Playing)
                    director.Pause();
            }
        }
        else
        {
            // if (director.state != PlayState.Playing)
            //     director.Play();
        }
    }

    private void HandleMouseScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.001f)
        {
            float delta = -scroll * scrollMultiplier;
            double newTime = director.time + delta;
            newTime = Mathf.Clamp((float)newTime, 0f, (float)director.duration);
            director.time = newTime;
        }
    }

    void OnJogNote(float value)
    {
        if (value > 0)
        {
            isTouchingPlatine = true;

            if (director.state == PlayState.Playing)
                director.Pause();
        }
        else
        {
            isTouchingPlatine = false;

            if (director.state != PlayState.Playing)
                director.Play();
        }
    }

    void OnRoll(float value)
    {
        timeSinceLastJogEvent = 0f;

        if (director.state != PlayState.Playing)
            director.Play();

        float direction = value < 0.5f ? +1f : -1f;

        MoveTimeline(direction);
    }

    private void MoveTimeline(float direction)
    {
        float delta = direction * jogSpeed * jogSensitivity;
        double newTime = director.time + delta;
        newTime = Mathf.Clamp((float)newTime, 0f, (float)director.duration);
        director.time = newTime;
    }
}