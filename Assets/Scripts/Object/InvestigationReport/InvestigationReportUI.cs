using System.Collections.Generic;
using DefaultNamespace;
using Minis;
using Object.InvestigationReport;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Object
{
    public class InvestigationReportUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DetectableGameObject detectableGameObject;
        [SerializeField] private GameObject investigationReportUI;
        [SerializeField] private GameObject itemList;
        [SerializeField] private GameObject investigationReportItemPrefab;
        [SerializeField] private CursorManager cursorManager;
        
        private bool isShowing = false;

        [SerializeField]
        private int maxReportItems = 9;
        
        private List<InvestigationData> investigationReportItems = new List<InvestigationData>();
        private Dictionary<string, GameObject> uiItemsMap = new Dictionary<string, GameObject>();
        private bool cursorStateBeforeShow = false;

#region Unity Callbacks

        void Start()
        {
            investigationReportUI.SetActive(false);
            MidiBindingRegistry.Instance.Bind(ActionEnum.InvestigationReport, (input) => DisplayReport());
            
            // Trouver le CursorManager s'il n'est pas assigné
            if (cursorManager == null)
                cursorManager = FindObjectOfType<CursorManager>();
        }

        private void Update()
        {
            // Si la touche I est pressée, simuler l'appui sur le bouton du controller
            if (Input.GetKeyDown(KeyCode.I))
            {
                DisplayReport();
            }
        
            // Vérifier si l'UI est affichée mais que l'objet n'est plus regardé
            if (isShowing && !detectableGameObject.isLookingAt)
            {
                HideUI();
            }
        }

#endregion


        void ShowUI()
        {
            // Sauvegarder l'état actuel du curseur avant d'afficher l'UI
            if (cursorManager != null)
            {
                cursorStateBeforeShow = cursorManager.IsCursorHidden();
                
                // Désactiver la fonctionnalité de clic pour cacher le curseur
                cursorManager.SetHideOnClickEnabled(false);
                
                // Afficher le curseur quand la fiche est affichée
                cursorManager.SetCursorHidden(false);
            }
            
            investigationReportUI.SetActive(true);
            isShowing = true;
        }

        void HideUI()
        {
            investigationReportUI.SetActive(false);
            isShowing = false;
            
            // Restaurer l'état précédent du curseur quand la fiche est cachée
            if (cursorManager != null)
            {
                // Réactiver la fonctionnalité de clic pour cacher le curseur
                cursorManager.SetHideOnClickEnabled(true);
                
                // Restaurer l'état précédent du curseur
                cursorManager.SetCursorHidden(cursorStateBeforeShow);
            }
        }

        void DisplayReport()
        {
            if (detectableGameObject.isLookingAt && !isShowing)
            {
                ShowUI();
            }
            else if (isShowing)
            {
                HideUI();
            }
        }
        
        public void AddInfoToReport(InvestigationData data)
        {
            if (investigationReportItems.Count < maxReportItems)
            {
                if (investigationReportItems.Contains(data))
                {
                    Debug.Log("Item" + data.id + " with description " + data.description + " already in report");
                    return;
                }
                // Assign unique id
                data.id = System.Guid.NewGuid().ToString();
                
                // Add to list
                investigationReportItems.Add(data);
                
                // Add to UI
                AddUIItem(data);
            }
        }
        
        public void ClearReport()
        {
            // Supprimer tous les éléments UI
            foreach (var item in uiItemsMap.Values)
            {
                if (item != null)
                    Destroy(item);
            }
            
            uiItemsMap.Clear();
            investigationReportItems.Clear();
        }
        
        public void RemoveInfoFromReport(InvestigationData data)
        {
            if (!investigationReportItems.Contains(data))
            {
                Debug.Log("Item" + data.id + " with description " + data.description + " not found in report");
                return;
            }
            investigationReportItems.Remove(data);
            RemoveUIItem(data);
        }
        
#region UI Management

        private void AddUIItem(InvestigationData data)
        {
            var item = Instantiate(investigationReportItemPrefab, itemList.transform, false);
            
            if (!item.GetComponentInChildren<TextMeshProUGUI>())
            {
                Debug.LogError("Missing TextMeshProUGUI component on investigationReportItemPrefab");
                return;
            }
            item.GetComponentInChildren<TextMeshProUGUI>().text = data.description;

            if (!item.GetComponentInChildren<Button>())
            {
                Debug.LogError("Missing Button component on investigationReportItemPrefab");
                return;
            }
            item.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Removing item " + data.id);
                RemoveInfoFromReport(data);
            });

            item.name = data.id;
            
            // Stocker la référence à l'élément UI dans le dictionnaire
            uiItemsMap[data.id] = item;
        }
        
        public void RemoveUIItem(InvestigationData data)
        {
            if (uiItemsMap.TryGetValue(data.id, out GameObject item))
            {
                if (item != null)
                    Destroy(item);
                
                uiItemsMap.Remove(data.id);
            }
            else
            {
                Debug.LogWarning("UI item with ID " + data.id + " not found in dictionary");
            }
        }

#endregion

        
        private void OnDisable()
        {
            if (isShowing && cursorManager)
            {
                cursorManager.SetHideOnClickEnabled(true);
                cursorManager.SetCursorHidden(cursorStateBeforeShow);
            }
        }
    }
}
