using DefaultNamespace;
using Minis;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class RewindTimeline : MonoBehaviour
{
    [Header("Rewind Settings")]
    [SerializeField] private float jogSensitivity = 0.1f;
    [SerializeField] private float jogMinimumVelocity = 0.01f;
    [SerializeField] private float scrollMultiplier = 5f;

    private float jogVelocity = 0f;
    private bool isJogging = false;

    private PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        MidiBindingRegistry.Instance.Bind(ActionEnum.ScrubTimeline, OnMidiJog);

        // ⏯️ On laisse le Play actif
        if (director.state != PlayState.Playing)
            director.Play();
    }

    void Update()
    {
        HandleMouseScroll();
        HandleJogRewind();
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

    private void HandleJogRewind()
    {
        if (isJogging)
        {
            float delta = jogVelocity * Time.deltaTime;
            double newTime = director.time + delta;
            newTime = Mathf.Clamp((float)newTime, 0f, (float)director.duration);
            director.time = newTime;
        }
    }

    private void OnMidiJog(MidiInput input)
    {
        float raw = -(input.Value - 64); // -63 à +63
        jogVelocity = raw * jogSensitivity;
        isJogging = Mathf.Abs(jogVelocity) > jogMinimumVelocity;
    }
}