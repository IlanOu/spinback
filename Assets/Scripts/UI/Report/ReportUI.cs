using System;
using System.Collections.Generic;
using Cinematics;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UI.Toggle;

public class ReportUI : MonoBehaviour
{
    public Action OnCloseReportUI;
    public Action OnOpenReportUI;

    [Header("References")]
    [SerializeField] private GameObject investigationReportUI;
    [SerializeField] private GameObject itemList;
    [SerializeField] private GameObject investigationReportItemPrefab;
    [SerializeField] private GameObject gridLayoutGroupPrefab;
    [SerializeField] private CursorManager cursorManager;

    [SerializeField] private UI.Carousel.Carousel2D carousel; // Référence au carousel
    [SerializeField] private Button signButton;
    [SerializeField] private ReportIcon reportIcon;
    
    [Header("Configuration")]
    [SerializeField] private int maxReportItems = 9;
    [SerializeField] private int itemsPerGrid = 4; // Nombre d'items par grille
    
    [SerializeField] private string endCinematicName = "EndScene";

    private bool isShowing = false;
    private List<Clue> investigationReportItems = new List<Clue>();
    private Dictionary<string, GameObject> uiItemsMap = new Dictionary<string, GameObject>();
    private List<GameObject> gridContainers = new List<GameObject>();
    private bool cursorStateBeforeShow = false;

    void Start()
    {
        investigationReportUI.SetActive(false);

        MidiBinding.Instance.Subscribe(MidiBind.MASTER_BUTTON, (input) =>
        {
            if (input == 1)
                ToggleVisibilityReport();
        });
        
        signButton.onClick.AddListener(EndCinematic);
        
        // Trouver le CursorManager s'il n'est pas assigné
        if (cursorManager == null)
            cursorManager = FindObjectOfType<CursorManager>();

        if (carousel == null)
            carousel = GetComponentInChildren<UI.Carousel.Carousel2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleVisibilityReport();
    }

    public void ShowUI()
    {
        if (cursorManager != null)
        {
            cursorStateBeforeShow = cursorManager.IsCursorHidden();
            cursorManager.SetHideOnClickEnabled(false);
            cursorManager.SetCursorHidden(false);
        }

        investigationReportUI.SetActive(true);
        isShowing = true;

        reportIcon.HideAlertIcon();

        PopulateFromClues(); // 👈 CHARGER TOUS LES INDICES ICI

        if (carousel != null)
            carousel.ForceUpdate();

        OnOpenReportUI?.Invoke();
    }

    public void HideUI()
    {
        investigationReportUI.SetActive(false);
        isShowing = false;

        if (cursorManager != null)
        {
            cursorManager.SetHideOnClickEnabled(true);
            cursorManager.SetCursorHidden(cursorStateBeforeShow);
        }

        OnCloseReportUI?.Invoke();
    }
    
    void EndCinematic()
    {
        HideUI();
        SceneTransitionBlinker.Instance.TransitionToSceneWithVideo(endCinematicName);
    }

    void ToggleVisibilityReport()
    {
        if (isShowing)
            HideUI();
        else
            ShowUI();
    }

    public void AddInfoToReport(Clue clue)
    {
        if (investigationReportItems.Count >= maxReportItems) return;

        if (investigationReportItems.Any(d => d.text == clue.text)) return;

        clue.id = Guid.NewGuid().ToString();
        investigationReportItems.Add(clue);
        AddUIItem(clue);

        if (carousel != null)
            carousel.ForceUpdate();
    }

    public void ClearReport()
    {
        foreach (var item in uiItemsMap.Values)
            if (item != null) Destroy(item);

        foreach (var grid in gridContainers)
            if (grid != null) Destroy(grid);

        uiItemsMap.Clear();
        gridContainers.Clear();
        investigationReportItems.Clear();

        if (carousel != null)
            carousel.ForceUpdate();
    }

    private void AddUIItem(Clue clue)
    {
        int gridIndex = (investigationReportItems.Count - 1) / itemsPerGrid;

        EnsureGridExists(gridIndex);
        GameObject targetGrid = gridContainers[gridIndex];

        var item = Instantiate(investigationReportItemPrefab, targetGrid.transform, false);

        var textComponent = item.GetComponentInChildren<ClueUI>();
        if (textComponent != null)
            textComponent.SetClue(clue);
        var switchHandler = item.GetComponentInChildren<SwitchHandler>();
        if (switchHandler != null)
            switchHandler.SetClue(clue);

        item.name = clue.id;
        uiItemsMap[clue.id] = item;

        CheckCarouselVisibility();
    }

    private void EnsureGridExists(int gridIndex)
    {
        while (gridContainers.Count <= gridIndex)
        {
            GameObject newGrid = Instantiate(gridLayoutGroupPrefab, itemList.transform, false);
            newGrid.name = $"Grid_{gridContainers.Count}";

            GridLayoutGroup gridLayout = newGrid.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
                gridLayout.constraintCount = Mathf.CeilToInt(itemsPerGrid / 2.0f);

            gridContainers.Add(newGrid);
        }
    }

    private void CheckCarouselVisibility()
    {
        if (carousel != null)
            carousel.ForceUpdate();
    }

    private void OnDisable()
    {
        if (isShowing && cursorManager)
        {
            cursorManager.SetHideOnClickEnabled(true);
            cursorManager.SetCursorHidden(cursorStateBeforeShow);
        }
    }

    private void PopulateFromClues()
    {
        ClearReport();

        foreach (var clue in ClueDatabase.Instance.Clues)
        {
            AddInfoToReport(clue);
        }
    }
}