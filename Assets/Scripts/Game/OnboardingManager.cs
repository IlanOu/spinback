using System.ComponentModel;
using UI.Report;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

public class OnboardingManager : MonoBehaviour
{
    public static OnboardingManager Instance;
    [SerializeField] private PlayableDirector director;
    private OnBoardingStep currentStep;

    [Header("On Start")]
    [HideInInspector] public SmoothCameraFollow cameraFollow;
    [HideInInspector] public CameraZoom cameraZoom;
    [Header("Add clue step")]
    [SerializeField] public GameObject clue;
    [SerializeField] public ClueInteractiveIcon interactiveIcon;
    [HideInInspector] public InteractableClue interactableClue;
    [HideInInspector] public OutlineObject outlineClue;
    [SerializeField] public GameObject activeItemInScene;
    [SerializeField] public GameObject disactiveItemInScene;

    [Header("Open report step")]
    [SerializeField] public ReportUI reportUI;
    [Header("Toggle step")]
    [SerializeField] public GameObject carousel;
    [Header("Finish step")]
    [SerializeField] public Button validateButton;

    [SerializeField] public UnityEvent<bool> OnOpenReportUI;
    
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
        OnOpenReportUI?.Invoke(false);
        cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        interactableClue = clue.GetComponent<InteractableClue>();
        outlineClue = clue.GetComponent<OutlineObject>();

        currentStep = null;
    }

    void Update()
    {
        if (currentStep != null)
        {
            currentStep.Handle();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            TooltipActivator.Instance.DisableAllTooltips();
        }
    }

    public void StartOnboarding()
    {
        OnOpenReportUI?.Invoke(true);
        Vector2 rotation = Camera.main.transform.localEulerAngles;

        director.Pause();
        currentStep = new OnBoardingOpenReportStep(this);

        cameraZoom.enabled = true;
        cameraZoom.SetSpeed(5f);

        cameraFollow.enabled = true;
        cameraFollow.SetTargetRotation(rotation);

        interactableClue.DisableInteractability();
        outlineClue.EnableVisibility(false);
        interactiveIcon.EnableVisibility(false);

        ClueDatabase.Instance.ClearDatabase();

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
        OnOpenReportUI?.Invoke(false);
        currentStep = null;
        
        reportUI.HideUI();
        reportUI.gameObject.SetActive(false);

        ClueDatabase.Instance.ClearDatabase();

        director.Play();
    }
}
