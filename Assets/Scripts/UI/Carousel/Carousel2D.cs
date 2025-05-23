using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UI.Carousel
{
    public class Carousel2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Carousel Components")]
        [SerializeField] private RectMask2D viewportMask;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private HorizontalLayoutGroup layoutGroup;
        
        [Header("Navigation")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutQuint;
        
        [Header("Snap Settings")]
        [SerializeField] private bool snapToItems = true;
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
            CalculateWidths();
            CalculateItemPositions();
            UpdateButtonsState();
        }
        
        private void OnRectTransformDimensionsChange()
        {
            CalculateWidths();
            CalculateItemPositions();
            ClampPosition();
            UpdateButtonsState();
        }
        
        private void CalculateWidths()
        {
            viewportWidth = viewportTransform.rect.width;
            
            // Calculate total content width including padding and spacing
            float totalPadding = layoutGroup.padding.left + layoutGroup.padding.right;
            int childCount = contentContainer.childCount;
            float spacingTotal = layoutGroup.spacing * (childCount - 1);
            
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
        }
        
        private void CalculateItemPositions()
        {
            itemPositions.Clear();
            
            if (contentContainer.childCount == 0)
                return;
                
            // Calculate each possible snap position
            float leftEdgePosition = -layoutGroup.padding.left;
            
            // Add the leftmost position (0)
            itemPositions.Add(0);
            
            // Calculate positions for each item
            for (int i = 0; i < contentContainer.childCount; i++)
            {
                RectTransform child = contentContainer.GetChild(i) as RectTransform;
                if (child != null)
                {
                    // Position where this item is at the left edge of viewport
                    float itemPosition = leftEdgePosition;
                    
                    // Only add positions that would show new content and are within bounds
                    if (itemPosition > -(contentWidth - viewportWidth) && itemPosition < 0)
                    {
                        itemPositions.Add(itemPosition);
                    }
                    
                    // Move to next item position
                    leftEdgePosition -= child.rect.width;
                    if (i < contentContainer.childCount - 1) // Add spacing except after last item
                    {
                        leftEdgePosition -= layoutGroup.spacing;
                    }
                }
            }
            
            // Add the rightmost position
            float maxPosition = -(contentWidth - viewportWidth);
            if (maxPosition < 0 && !itemPositions.Contains(maxPosition))
            {
                itemPositions.Add(maxPosition);
            }
            
            // Remove duplicates and sort
            HashSet<float> uniquePositions = new HashSet<float>(itemPositions);
            itemPositions = new List<float>(uniquePositions);
            itemPositions.Sort((a, b) => b.CompareTo(a)); // Sort descending (right to left)
            
            // Debug positions
            Debug.Log($"Calculated {itemPositions.Count} snap positions:");
            for (int i = 0; i < itemPositions.Count; i++)
            {
                Debug.Log($"Position {i}: {itemPositions[i]}");
            }
        }
        
        public void GoToNext()
        {
            int currentIndex = FindClosestPositionIndex(currentPosition);
            if (currentIndex < itemPositions.Count - 1)
            {
                ScrollTo(itemPositions[currentIndex + 1]);
            }
        }
        
        public void GoToPrevious()
        {
            int currentIndex = FindClosestPositionIndex(currentPosition);
            if (currentIndex > 0)
            {
                ScrollTo(itemPositions[currentIndex - 1]);
            }
        }
        
        public void ScrollTo(float position)
        {
            float targetPosition = Mathf.Clamp(position, -(contentWidth - viewportWidth), 0);
            
            DOTween.Kill(contentContainer);
            contentContainer.DOAnchorPosX(targetPosition, animationDuration)
                .SetEase(animationEase)
                .OnComplete(() => {
                    currentPosition = targetPosition;
                    UpdateButtonsState();
                });
        }
        
        private int FindClosestPositionIndex(float position)
        {
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
            float maxPosition = -(contentWidth - viewportWidth);
            currentPosition = Mathf.Clamp(currentPosition, maxPosition, 0);
            contentContainer.anchoredPosition = new Vector2(currentPosition, contentContainer.anchoredPosition.y);
        }
        
        private void UpdateButtonsState()
        {
            int currentIndex = FindClosestPositionIndex(currentPosition);
            
            if (prevButton != null)
                prevButton.interactable = currentIndex > 0;
                
            if (nextButton != null)
                nextButton.interactable = currentIndex < itemPositions.Count - 1;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsHorizontalDrag(eventData))
                return;
                
            isDragging = true;
            DOTween.Kill(contentContainer);
            dragStartPosition = eventData.position;
            lastDragPosition = eventData.position;
            dragTime = Time.time;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || !IsHorizontalDrag(eventData))
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
            if (!isDragging)
                return;
                
            isDragging = false;
            currentPosition = contentContainer.anchoredPosition.x;
            
            // Calculate velocity
            float dragDuration = Time.time - dragTime;
            float dragDistance = eventData.position.x - dragStartPosition.x;
            float velocity = dragDuration > 0 ? dragDistance / dragDuration : 0;
            
            if (snapToItems)
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
