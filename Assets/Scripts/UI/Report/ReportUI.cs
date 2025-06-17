using System;
using System.Collections.Generic;
using System.Linq;
using Cinematics;
using TMPro;
using UI.Popup;
using UI.Toggle;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Report
{
    public class ReportUI : MonoBehaviour
    {
        public Action OnCloseReportUI;
        public Action OnOpenReportUI;

        [Header("References")]
        [SerializeField] private GameObject investigationReportUI;
        [SerializeField] private GameObject itemList;
        [SerializeField] private GameObject investigationReportItemPrefab;
        [SerializeField] private GameObject gridLayoutGroupPrefab;

        [SerializeField] private Carousel.Carousel2D carousel; // Référence au carousel
        [SerializeField] private Button signButton;
        [SerializeField] private ConfirmationPopup confirmationPopup;
        [SerializeField] private ReportIcon reportIcon;
    
        [Header("Configuration")]
        [SerializeField] private int maxReportItems = 9;
        [SerializeField] private int itemsPerGrid = 4; // Nombre d'items par grille
    
        [SerializeField] private string endCinematicName = "EndScene";

        public UnityEvent<bool> onReportDisplayed;
        
        private bool isShowing = false;
        private bool canOpenReport = true;
        private bool canCloseReport = true;
        private List<Clue> investigationReportItems = new List<Clue>();
        private Dictionary<string, GameObject> uiItemsMap = new Dictionary<string, GameObject>();
        private List<GameObject> gridContainers = new List<GameObject>();
        private bool cursorStateBeforeShow = false;
    
        // Token pour la commande de curseur
        private CursorCommandToken cursorToken;

        void Start()
        {
            investigationReportUI.SetActive(false);

            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_1, (input) =>
            {
                if (input == 1)
                    ToggleVisibilityReport();
            });
            
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_1, (input) =>
            {
                if (input == 1)
                    ToggleVisibilityReport();
            });

            if (signButton != null)
            {
                if (confirmationPopup)
                {
                    signButton.onClick.AddListener(ConfirmEndCinematic);
                }
                else
                {
                    signButton.onClick.AddListener(EndCinematic);
                } 
            }

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
            if (!canOpenReport) return;
            // Sauvegarder l'état du curseur avant d'afficher l'UI
            cursorStateBeforeShow = CursorManager.Instance.IsCursorHidden();
        
            // Désactiver temporairement le hideOnClick
            CursorManager.Instance.SetHideOnClickEnabled(false);
        
            // Demander que le curseur soit visible avec une priorité élevée (80)
            cursorToken = CursorManager.Instance.AddCommand("ReportUI", true, 80);

            investigationReportUI.SetActive(true);
            isShowing = true;

            reportIcon.ShowCloseIcon();

            PopulateFromClues(); // 👈 CHARGER TOUS LES INDICES ICI

            if (carousel != null)
                carousel.ForceUpdate();

            OnOpenReportUI?.Invoke();
        }

        public void HideUI()
        {
            if (!canCloseReport) return;
            reportIcon.ShowOpenIcon();
            
            investigationReportUI.SetActive(false);
            isShowing = false;

            // Réactiver le hideOnClick
            CursorManager.Instance.SetHideOnClickEnabled(true);
        
            // Libérer la commande de curseur
            if (cursorToken != null)
            {
                cursorToken.Dispose();
                cursorToken = null;
            
                // Restaurer l'état précédent du curseur
                CursorManager.Instance.SetDefaultCursorHidden(cursorStateBeforeShow);
            }

            OnCloseReportUI?.Invoke();
        }

        void ConfirmEndCinematic()
        {
            confirmationPopup.Show(
                "Rendre votre témoignage au policier ?",
                ConfirmationPopup.PopupType.Declaration,
                ConfirmationPopup.PopupMode.Blocking,
                () =>
                {
                    // Action à exécuter si l'utilisateur confirme
                    Debug.Log("Quitter confirmé");

                    // Utiliser le script QuitApplication
                    EndCinematic();
                },
                () =>
                {
                    // Action à exécuter si l'utilisateur annule
                    Debug.Log("Quitter annulé");
                }
            );
        }
    
        void EndCinematic()
        {
            ForceHideReport();
            SceneTransitionBlinker.Instance.TransitionToSceneWithVideo(endCinematicName);
        }

        void ToggleVisibilityReport()
        {
            if (isShowing)
                HideUI();
            else
                ShowUI();
            
            onReportDisplayed?.Invoke(isShowing);
        }

        public void CanOpenReport(bool value)
        {
            canOpenReport = value;
        }

        public void CanCloseReport(bool value)
        {
            canCloseReport = value;
        }

        void ForceShowReport()
        {
            CanOpenReport(true);
            ShowUI();
        }

        void ForceHideReport()
        {
            CanCloseReport(true);
            HideUI();
        }

        public void AddInfoToReport(Clue clue)
        {
            if (investigationReportItems.Count >= maxReportItems) return;

            if (investigationReportItems.Any(d => d.title == clue.title)) return;

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

        public void RemoveUIItem(Clue clue)
        {
            if (uiItemsMap.TryGetValue(clue.id, out GameObject item))
            {
                if (item != null)
                    Destroy(item);

                uiItemsMap.Remove(clue.id);
            }

            CheckCarouselVisibility();
        }

        private void ReorganizeItems()
        {
            List<Clue> tempItems = new List<Clue>(investigationReportItems);

            foreach (var item in uiItemsMap.Values)
                if (item != null) Destroy(item);

            uiItemsMap.Clear();

            foreach (var grid in gridContainers)
                foreach (Transform child in grid.transform)
                    Destroy(child.gameObject);

            investigationReportItems.Clear();

            foreach (var data in tempItems)
            {
                investigationReportItems.Add(data);

                int gridIndex = (investigationReportItems.Count - 1) / itemsPerGrid;

                EnsureGridExists(gridIndex);
                GameObject targetGrid = gridContainers[gridIndex];

                var item = Instantiate(investigationReportItemPrefab, targetGrid.transform, false);

                var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                    textComponent.text = data.title;

                item.name = data.id;
                uiItemsMap[data.id] = item;
            }

            for (int i = gridContainers.Count - 1; i >= 0; i--)
            {
                if (gridContainers[i].transform.childCount == 0 && i == gridContainers.Count - 1)
                {
                    Destroy(gridContainers[i]);
                    gridContainers.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            CheckCarouselVisibility();
        }

        private void CheckCarouselVisibility()
        {
            if (carousel != null)
                carousel.ForceUpdate();
        }

        private void OnDisable()
        {
            if (isShowing)
            {
                // Réactiver le hideOnClick
                CursorManager.Instance.SetHideOnClickEnabled(true);
            
                // Libérer la commande de curseur
                if (cursorToken != null)
                {
                    cursorToken.Dispose();
                    cursorToken = null;
                
                    // Restaurer l'état précédent du curseur
                    CursorManager.Instance.SetDefaultCursorHidden(cursorStateBeforeShow);
                }
            }
        }

        private void OnDestroy()
        {
            // S'assurer que la commande est libérée
            if (cursorToken != null)
            {
                cursorToken.Dispose();
                cursorToken = null;
            }
        }

        private void PopulateFromClues()
        {
            ClearReport();

            foreach (var clue in ClueDatabase.Instance.AddedClues)
            {
                AddInfoToReport(clue);
            }
        }
    }
}
