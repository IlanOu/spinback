using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

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
        [SerializeField] private float dragThreshold = 0.2f;
        
        private RectTransform viewportTransform;
        private float viewportWidth;
        private float contentWidth;
        private float currentPosition = 0f;
        private float dragStartPosition;
        private Vector2 lastDragPosition;
        private float dragDistance;
        private bool isDragging = false;
        
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
            UpdateButtonsState();
        }
        
        private void OnRectTransformDimensionsChange()
        {
            CalculateWidths();
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
        
        public void GoToNext()
        {
            float itemWidth = GetAverageItemWidth();
            float newPosition = currentPosition - itemWidth;
            ScrollTo(newPosition);
        }
        
        public void GoToPrevious()
        {
            float itemWidth = GetAverageItemWidth();
            float newPosition = currentPosition + itemWidth;
            ScrollTo(newPosition);
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
        
        private float GetAverageItemWidth()
        {
            if (contentContainer.childCount == 0)
                return 0;
                
            float totalWidth = 0;
            int childCount = contentContainer.childCount;
            
            for (int i = 0; i < childCount; i++)
            {
                RectTransform child = contentContainer.GetChild(i) as RectTransform;
                if (child != null)
                {
                    totalWidth += child.rect.width;
                }
            }
            
            float averageWidth = totalWidth / childCount;
            return averageWidth + layoutGroup.spacing;
        }
        
        private void ClampPosition()
        {
            float maxPosition = -(contentWidth - viewportWidth);
            currentPosition = Mathf.Clamp(currentPosition, maxPosition, 0);
            contentContainer.anchoredPosition = new Vector2(currentPosition, contentContainer.anchoredPosition.y);
        }
        
        private void UpdateButtonsState()
        {
            if (prevButton != null)
                prevButton.interactable = currentPosition < 0;
                
            if (nextButton != null)
                nextButton.interactable = currentPosition > -(contentWidth - viewportWidth);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsHorizontalDrag(eventData))
                return;
                
            isDragging = true;
            DOTween.Kill(contentContainer);
            dragStartPosition = contentContainer.anchoredPosition.x;
            lastDragPosition = eventData.position;
            dragDistance = 0;
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
            dragDistance += delta;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
                
            isDragging = false;
            currentPosition = contentContainer.anchoredPosition.x;
            
            // Snap to nearest item if enabled
            if (snapToItems)
            {
                float velocity = Mathf.Abs(dragDistance) / Time.deltaTime;
                if (velocity > dragThreshold)
                {
                    // Swipe detected, move to next/previous item
                    if (dragDistance > 0)
                        GoToPrevious();
                    else
                        GoToNext();
                }
                else
                {
                    // Snap to nearest item
                    float itemWidth = GetAverageItemWidth();
                    float offset = currentPosition % itemWidth;
                    float targetPosition;
                    
                    if (Mathf.Abs(offset) > itemWidth / 2)
                        targetPosition = currentPosition - (itemWidth + offset);
                    else
                        targetPosition = currentPosition - offset;
                        
                    ScrollTo(targetPosition);
                }
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
    }
}
