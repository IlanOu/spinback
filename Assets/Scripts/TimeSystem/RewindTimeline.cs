using DefaultNamespace;
using Minis;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class RewindTimeline : MonoBehaviour
{
    [Header("Rewind Settings")]
    [SerializeField] private float jogSensitivity = 0.001f;
    [SerializeField] private float jogMinimumVelocity = 0.01f;
    [SerializeField] private float scrollMultiplier = 5f;
    [SerializeField] private float pauseDelay = 0.1f;

    private PlayableDirector director;
    private bool isTouchingPlatine = false;
    private bool platineIsMoving = false;
    private float timeSinceLastJogEvent = 0f;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        MidiBindingRegistry.Instance.Bind(ActionEnum.TriggerRewind, OnMidiTriggerRewind);
        MidiBindingRegistry.Instance.Bind(ActionEnum.UnTriggerRewind, OnMidiUnTriggerRewind);
        MidiBindingRegistry.Instance.Bind(ActionEnum.ScrubTimeline, OnMidiJog);
        MidiBindingRegistry.Instance.Bind(ActionEnum.ScrubTimelineVelocity, OnMidiJogWithVelocity);

        // Démarrer en lecture
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
        // Incrémenter le temps depuis le dernier événement jog
        timeSinceLastJogEvent += Time.deltaTime;
        
        if (isTouchingPlatine)
        {
            // Si on n'a pas reçu d'événement jog depuis un moment, considérer que la platine ne bouge plus
            if (timeSinceLastJogEvent > pauseDelay)
            {
                platineIsMoving = false;
                
                // Mettre en pause si on ne bouge pas la platine
                if (director.state == PlayState.Playing)
                {
                    director.Pause();
                }
            }
        }
        else
        {
            // Si on ne touche pas la platine, s'assurer que la lecture est active
            if (director.state != PlayState.Playing)
            {
                director.Play();
            }
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

    private void OnMidiJog(MidiInput input)
    {
        // Quand je fais tourner le disque avec le doigt dessus
        float raw = -(input.Value - 64); // -63 à +63
        
        // Réinitialiser le timer
        timeSinceLastJogEvent = 0f;
        
        // Vérifier si le mouvement est significatif
        bool significantMovement = Mathf.Abs(raw) > jogMinimumVelocity;
        
        if (significantMovement)
        {
            platineIsMoving = true;
            
            // S'assurer que la lecture est active pendant le scrub
            if (director.state != PlayState.Playing)
            {
                director.Play();
            }
            
            // Déplacer le temps
            MoveTimeline(raw);
        }
    }
    
    private void OnMidiJogWithVelocity(MidiInput input)
    {
        // Quand je fais tourner le disque sans le doigt dessus
        float raw = -(input.Value - 64); // -63 à +63
        
        // Réinitialiser le timer
        timeSinceLastJogEvent = 0f;
        
        // Vérifier si le mouvement est significatif
        bool significantMovement = Mathf.Abs(raw) > jogMinimumVelocity;
        
        if (significantMovement)
        {
            platineIsMoving = true;
            
            // S'assurer que la lecture est active pendant le scrub
            if (director.state != PlayState.Playing)
            {
                director.Play();
            }
            
            // Déplacer le temps
            MoveTimeline(raw);
        }
    }
    
    private void MoveTimeline(float rawValue)
    {
        float delta = rawValue * jogSensitivity;
        double newTime = director.time + delta;
        newTime = Mathf.Clamp((float)newTime, 0f, (float)director.duration);
        director.time = newTime;
    }
    
    private void OnMidiTriggerRewind(MidiInput input)
    {
        // Au moment où je touche le disque
        isTouchingPlatine = true;
        
        // Mettre en pause immédiatement
        if (director.state == PlayState.Playing)
        {
            director.Pause();
        }
    }
    
    private void OnMidiUnTriggerRewind(MidiInput input)
    {
        // Au moment où j'arrête de toucher le disque
        isTouchingPlatine = false;
        platineIsMoving = false;
        
        // S'assurer que la lecture est active quand on relâche la platine
        if (director.state != PlayState.Playing)
        {
            director.Play();
        }
    }
}
