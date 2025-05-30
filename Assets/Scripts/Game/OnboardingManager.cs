using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

public class OnboardingManager : MonoBehaviour
{
    public static OnboardingManager Instance;
    [SerializeField] private PlayableDirector director;
    private OnBoardingStep currentStep;

    [Header("On Start")]
    [HideInInspector] public SmoothCameraFollow cameraFollow;
    [HideInInspector] public CameraZoom cameraZoom;
    [Header("Focus step")]
    [SerializeField] public TooltipActivator tooltipActivator;
    [Header("Add clue step")]
    [SerializeField] public GameObject clue;

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
        cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        currentStep = null;
    }

    void Update()
    {
        if (currentStep != null)
        {
            currentStep.Handle();
        }
    }

    public void StartOnboarding()
    {
        Vector2 rotation = Camera.main.transform.localEulerAngles;

        director.Pause();
        currentStep = new OnBoardingFocusStep(this);

        cameraZoom.enabled = true;
        cameraZoom.SetSpeed(5f);

        cameraFollow.enabled = true;
        cameraFollow.SetTargetRotation(rotation);

        StartStep();
    }

    public void StartStep()
    {
        if (currentStep != null)
        {
            currentStep.Show();
        }
    }

    public void NextStep()
    {
        currentStep.Hide();
        currentStep = currentStep.NextStep();

        if (currentStep != null)
        {
            StartStep();
        }
        else
        {
            FinishOnboarding();
        }
    }

    public void FinishOnboarding()
    {
        currentStep = null;
        director.Play();
    }
}
