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
    }

    public void StartStep()
    {
        currentStep.Show();
    }

    public void NextStep()
    {
        currentStep.Hide();
        currentStep = currentStep.NextStep();

        if (currentStep != null)
        {
            currentStep.Show();
        }
        else
        {
            FinishOnboarding();
        }
    }

    public void FinishStep()
    {
        NextStep();
    }
    
    public void FinishOnboarding()
    {
        director.Play();
    }
}
