using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UI.Carousel
{
    public class Carousel2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Carousel Components")] [SerializeField]
        private RectMask2D viewportMask;

        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private HorizontalLayoutGroup layoutGroup;

        [Header("Navigation")] [SerializeField]
        private Button prevButton;

        [SerializeField] private Button nextButton;
        [SerializeField] private GameObject navigationContainer; // Parent des boutons de navigation
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutQuint;

        [SerializeField]
        private bool hideButtonsWhenUnavailable = true; // Option pour cacher les boutons au lieu de les désactiver

        [Header("Snap Settings")] [SerializeField]
        private bool snapToItems = true;

        [SerializeField] private float swipeThreshold = 0.3f;
        [SerializeField] private float velocityThreshold = 500f;

        private RectTransform viewportTransform;
        private float viewportWidth;
        private float contentWidth;
        private float currentPosition = 0f;
        private Vector2 dragStartPosition;
        private Vector2 lastDragPosition;
        private float dragTime;
        private bool isDragging = false;
        private List<float> itemPositions = new List<float>();
        private bool isContentLargerThanViewport = false;

        private void Awake()
        {
            viewportTransform = viewportMask.GetComponent<RectTransform>();

            if (prevButton != null)
                prevButton.onClick.AddListener(GoToPrevious);

            if (nextButton != null)
                nextButton.onClick.AddListener(GoToNext);
        }

        private void Start()
        {
            // Délai pour s'assurer que tous les layouts sont initialisés
            Invoke("ForceUpdate", 0.1f);
        }

        private void OnEnable()
        {
            // Mettre à jour quand le carousel devient visible
            Invoke("ForceUpdate", 0.1f);
        }

        private void OnRectTransformDimensionsChange()
        {
            // Délai pour s'assurer que tous les layouts sont mis à jour
            Invoke("ForceUpdate", 0.1f);
        }

        // Méthode publique pour forcer la mise à jour du carousel
        public void ForceUpdate()
        {
            // Forcer la mise à jour des layouts
            Canvas.ForceUpdateCanvases();

            if (contentContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);

            if (viewportTransform != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewportTransform);

            CalculateWidths();
            CalculateItemPositions();
            ClampPosition();
            UpdateNavigationVisibility();
            UpdateButtonsState();
        }

        private void CalculateWidths()
        {
            if (viewportTransform == null || contentContainer == null || layoutGroup == null)
                return;

            viewportWidth = viewportTransform.rect.width;

            // Calculate total content width including padding and spacing
            float totalPadding = layoutGroup.padding.left + layoutGroup.padding.right;
            int childCount = contentContainer.childCount;
            float spacingTotal = childCount > 1 ? layoutGroup.spacing * (childCount - 1) : 0;

            contentWidth = 0;
            for (int i = 0; i < childCount; i++)
            {
                RectTransform child = contentContainer.GetChild(i) as RectTransform;
                if (child != null)
                {
                    contentWidth += child.rect.width;
                }
            }

            contentWidth += totalPadding + spacingTotal;

            // Déterminer si le contenu est plus large que le viewport
            isContentLargerThanViewport = contentWidth > viewportWidth && childCount > 1;
        }

        private void CalculateItemPositions()
        {
            itemPositions.Clear();

            if (contentContainer == null || contentContainer.childCount == 0)
                return;

            // Si le contenu n'est pas plus large que le viewport, une seule position est nécessaire
            if (!isContentLargerThanViewport)
            {
                itemPositions.Add(0);
                return;
            }

            // Add the leftmost position (0)
            itemPositions.Add(0);

            // Calculate positions for each item
            float maxPosition = -(contentWidth - viewportWidth);

            // Si nous avons seulement 2 positions (début et fin), les ajouter directement
            if (contentContainer.childCount == 2)
            {
                itemPositions.Add(maxPosition);
                return;
            }

            // Sinon, calculer une position pour chaque élément
            for (int i = 0; i < contentContainer.childCount; i++)
            {
                // Calculer la position où cet élément est aligné à gauche du viewport
                float position = 0;

                for (int j = 0; j < i; j++)
                {
                    RectTransform child = contentContainer.GetChild(j) as RectTransform;
                    if (child != null)
                    {
                        position -= child.rect.width;
                        position -= layoutGroup.spacing;
                    }
                }

                // Ajuster avec le padding
                position -= layoutGroup.padding.left;

                // Vérifier que la position est dans les limites et n'est pas déjà dans la liste
                if (position < 0 && position >= maxPosition && !itemPositions.Contains(position))
                {
                    itemPositions.Add(position);
                }
            }

            // Ajouter la position la plus à droite si elle n'est pas déjà présente
            if (maxPosition < 0 && !itemPositions.Contains(maxPosition))
            {
                itemPositions.Add(maxPosition);
            }

            // Trier les positions
            itemPositions.Sort((a, b) => b.CompareTo(a)); // Sort descending (right to left)
        }

        private void UpdateNavigationVisibility()
        {
            bool shouldShowNavigation = isContentLargerThanViewport && itemPositions.Count > 1;

            // Si le contenu est plus large que le viewport et qu'il y a plus d'une position, afficher la navigation
            if (navigationContainer != null)
            {
                navigationContainer.SetActive(shouldShowNavigation);
            }
            else
            {
                // Si le container de navigation n'est pas défini, gérer les boutons individuellement
                if (prevButton != null && !hideButtonsWhenUnavailable)
                    prevButton.gameObject.SetActive(shouldShowNavigation);

                if (nextButton != null && !hideButtonsWhenUnavailable)
                    nextButton.gameObject.SetActive(shouldShowNavigation);
            }
        }

        public void GoToNext()
        {
            if (!isContentLargerThanViewport || itemPositions.Count <= 1)
                return;

            int currentIndex = FindClosestPositionIndex(currentPosition);
            if (currentIndex < itemPositions.Count - 1)
            {
                ScrollTo(itemPositions[currentIndex + 1]);
            }
        }

        public void GoToPrevious()
        {
            if (!isContentLargerThanViewport || itemPositions.Count <= 1)
                return;

            int currentIndex = FindClosestPositionIndex(currentPosition);
            if (currentIndex > 0)
            {
                ScrollTo(itemPositions[currentIndex - 1]);
            }
        }

        public void ScrollTo(float position)
        {
            if (!isContentLargerThanViewport)
            {
                // Si le contenu n'est pas plus large que le viewport, toujours revenir à 0
                position = 0;
            }

            float targetPosition = Mathf.Clamp(position, -(contentWidth - viewportWidth), 0);

            DOTween.Kill(contentContainer);
            contentContainer.DOAnchorPosX(targetPosition, animationDuration)
                .SetEase(animationEase)
                .OnComplete(() =>
                {
                    currentPosition = targetPosition;
                    UpdateButtonsState();
                });
        }

        private int FindClosestPositionIndex(float position)
        {
            if (itemPositions.Count == 0)
                return 0;

            float minDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < itemPositions.Count; i++)
            {
                float distance = Mathf.Abs(position - itemPositions[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void ClampPosition()
        {
            if (contentWidth <= 0 || viewportWidth <= 0)
                return;

            float maxPosition = -(contentWidth - viewportWidth);
            currentPosition = Mathf.Clamp(currentPosition, maxPosition, 0);

            if (contentContainer != null)
                contentContainer.anchoredPosition = new Vector2(currentPosition, contentContainer.anchoredPosition.y);
        }

        private void UpdateButtonsState()
        {
            if (!isContentLargerThanViewport || itemPositions.Count <= 1)
            {
                // Si le contenu n'est pas plus large que le viewport, désactiver/cacher les boutons
                if (prevButton != null)
                {
                    if (hideButtonsWhenUnavailable)
                        prevButton.gameObject.SetActive(false);
                    else
                        prevButton.interactable = false;
                }

                if (nextButton != null)
                {
                    if (hideButtonsWhenUnavailable)
                        nextButton.gameObject.SetActive(false);
                    else
                        nextButton.interactable = false;
                }

                return;
            }

            int currentIndex = FindClosestPositionIndex(currentPosition);
            bool canGoPrevious = currentIndex > 0;
            bool canGoNext = currentIndex < itemPositions.Count - 1;

            // Mettre à jour le bouton précédent
            if (prevButton != null)
            {
                if (hideButtonsWhenUnavailable)
                    prevButton.gameObject.SetActive(canGoPrevious);
                else
                    prevButton.interactable = canGoPrevious;
            }

            // Mettre à jour le bouton suivant
            if (nextButton != null)
            {
                if (hideButtonsWhenUnavailable)
                    nextButton.gameObject.SetActive(canGoNext);
                else
                    nextButton.interactable = canGoNext;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isContentLargerThanViewport || !IsHorizontalDrag(eventData))
                return;

            isDragging = true;
            DOTween.Kill(contentContainer);
            dragStartPosition = eventData.position;
            lastDragPosition = eventData.position;
            dragTime = Time.time;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || !isContentLargerThanViewport || !IsHorizontalDrag(eventData))
                return;

            Vector2 currentDragPosition = eventData.position;
            float delta = currentDragPosition.x - lastDragPosition.x;
            lastDragPosition = currentDragPosition;

            float newPosition = contentContainer.anchoredPosition.x + delta;
            float maxPosition = -(contentWidth - viewportWidth);

            // Add resistance when dragging beyond bounds
            if (newPosition > 0)
                newPosition = newPosition * 0.3f;
            else if (newPosition < maxPosition)
                newPosition = maxPosition + (newPosition - maxPosition) * 0.3f;

            contentContainer.anchoredPosition = new Vector2(newPosition, contentContainer.anchoredPosition.y);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging || !isContentLargerThanViewport)
                return;

            isDragging = false;
            currentPosition = contentContainer.anchoredPosition.x;

            // Calculate velocity
            float dragDuration = Time.time - dragTime;
            float dragDistance = eventData.position.x - dragStartPosition.x;
            float velocity = dragDuration > 0 ? dragDistance / dragDuration : 0;

            if (snapToItems && itemPositions.Count > 1)
            {
                int targetIndex;

                // Determine target based on swipe velocity
                if (Mathf.Abs(velocity) > velocityThreshold)
                {
                    int currentIndex = FindClosestPositionIndex(currentPosition);
                    if (velocity > 0) // Swiping right (previous)
                    {
                        targetIndex = Mathf.Max(0, currentIndex - 1);
                    }
                    else // Swiping left (next)
                    {
                        targetIndex = Mathf.Min(itemPositions.Count - 1, currentIndex + 1);
                    }
                }
                else if (Mathf.Abs(dragDistance) > viewportWidth * swipeThreshold)
                {
                    // Determine direction based on drag distance
                    int currentIndex = FindClosestPositionIndex(currentPosition);
                    if (dragDistance > 0) // Dragged right (previous)
                    {
                        targetIndex = Mathf.Max(0, currentIndex - 1);
                    }
                    else // Dragged left (next)
                    {
                        targetIndex = Mathf.Min(itemPositions.Count - 1, currentIndex + 1);
                    }
                }
                else
                {
                    // Snap to closest position
                    targetIndex = FindClosestPositionIndex(currentPosition);
                }

                ScrollTo(itemPositions[targetIndex]);
            }
            else
            {
                ClampPosition();
                UpdateButtonsState();
            }
        }

        private bool IsHorizontalDrag(PointerEventData eventData)
        {
            return Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
        }

        // Méthode publique pour aller à un élément spécifique par index
        public void GoToItem(int itemIndex)
        {
            if (!isContentLargerThanViewport || contentContainer == null)
                return;

            if (itemIndex >= 0 && itemIndex < contentContainer.childCount)
            {
                // Recalculer les positions au cas où
                CalculateItemPositions();

                // Calculer la position exacte de l'élément
                float position = 0;
                position -= layoutGroup.padding.left;

                for (int i = 0; i < itemIndex; i++)
                {
                    RectTransform child = contentContainer.GetChild(i) as RectTransform;
                    if (child != null)
                    {
                        position -= child.rect.width;
                        position -= layoutGroup.spacing;
                    }
                }

                // Trouver la position de snap la plus proche
                ScrollTo(position);
            }
        }
    }
}