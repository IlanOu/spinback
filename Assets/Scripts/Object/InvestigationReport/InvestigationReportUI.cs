using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UI.Toggle; // important pour .ToList()

namespace Object.InvestigationReport
{
    public class InvestigationReportUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DetectableGameObject detectableGameObject;
        [SerializeField] private GameObject investigationReportUI;
        [SerializeField] private GameObject itemList;
        [SerializeField] private GameObject investigationReportItemPrefab;
        [SerializeField] private GameObject gridLayoutGroupPrefab;
        [SerializeField] private CursorManager cursorManager;
        [SerializeField] private UI.Carousel.Carousel2D carousel;

        [Header("Configuration")]
        [SerializeField] private int maxReportItems = 9;
        [SerializeField] private int itemsPerGrid = 4;

        private bool isShowing = false;
        private List<Clue> investigationReportItems = new List<Clue>();
        private Dictionary<string, GameObject> uiItemsMap = new Dictionary<string, GameObject>();
        private List<GameObject> gridContainers = new List<GameObject>();
        private bool cursorStateBeforeShow = false;

        void Start()
        {
            investigationReportUI.SetActive(false);

            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_1, (input) => DisplayReport());
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_2, (input) => DisplayReport());
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_1, (input) => DisplayReport());
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_2, (input) => DisplayReport());

            if (cursorManager == null)
                cursorManager = FindObjectOfType<CursorManager>();

            if (carousel == null)
                carousel = GetComponentInChildren<UI.Carousel.Carousel2D>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                DisplayReport();

            if (isShowing && !detectableGameObject.isLookingAt)
                HideUI();
        }

        void ShowUI()
        {
            if (cursorManager != null)
            {
                cursorStateBeforeShow = cursorManager.IsCursorHidden();
                cursorManager.SetHideOnClickEnabled(false);
                cursorManager.SetCursorHidden(false);
            }

            investigationReportUI.SetActive(true);
            isShowing = true;

            PopulateFromClues(); // 👈 CHARGER TOUS LES INDICES ICI

            if (carousel != null)
                carousel.ForceUpdate();
        }

        void HideUI()
        {
            investigationReportUI.SetActive(false);
            isShowing = false;

            if (cursorManager != null)
            {
                cursorManager.SetHideOnClickEnabled(true);
                cursorManager.SetCursorHidden(cursorStateBeforeShow);
            }
        }

        void DisplayReport()
        {
            if (detectableGameObject.isLookingAt && !isShowing)
                ShowUI();
            else if (isShowing)
                HideUI();
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

        public void RemoveUIItem(InvestigationData data)
        {
            if (uiItemsMap.TryGetValue(data.id, out GameObject item))
            {
                if (item != null)
                    Destroy(item);

                uiItemsMap.Remove(data.id);
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
                    textComponent.text = data.text;

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
}