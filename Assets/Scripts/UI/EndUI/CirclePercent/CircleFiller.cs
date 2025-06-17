using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

public class CircleFiller : MonoBehaviour
{
    [System.Serializable]
    public class CircleSegment
    {
        public Sprite sprite;
        public float percentage;
    }

    [Header("Configuration")] public List<CircleSegment> availableSegments = new List<CircleSegment>();
    public RectTransform circleCenter;
    public float circleRadius = 200f;
    public float baseImageSize = 50f;
    public RectTransform parent;
    public TextMeshProUGUI percentageText;

    [Header("Animation")] public float animationDuration = 0.5f;
    public float delayBetweenImages = 0.1f;
    public Ease animationEase = Ease.OutBack;
    public bool animatePercentageText = true;
    public bool clockwiseDirection = true;
    public bool clockwiseAnimation = true;

    [Header("Contrôle dynamique")] [Range(0f, 100f)]
    public float targetPercentage = 0f;

    [SerializeField] private float currentPercentage = 0f;

    private List<Image> activeImages = new List<Image>();
    private float lastTargetPercentage = -1f;
    private float lastRadius = -1f;
    private Sequence currentAnimation;

    private void Start()
    {
        if (parent == null)
        {
            parent = GetComponent<RectTransform>();
            if (parent == null)
            {
                Debug.LogError("CircleFillerUI nécessite un RectTransform parent.");
                return;
            }
        }

        UpdatePercentageText(0, false);

        FillCircle(targetPercentage);
        lastTargetPercentage = targetPercentage;
        lastRadius = circleRadius;
    }

    private void Update()
    {
        if (targetPercentage != lastTargetPercentage || circleRadius != lastRadius)
        {
            FillCircle(targetPercentage);
            lastTargetPercentage = targetPercentage;
            lastRadius = circleRadius;
        }
    }

    public void FillCircle(float percentage)
    {
        if (currentAnimation != null && currentAnimation.IsActive())
        {
            currentAnimation.Kill();
        }

        ClearActiveSegments();

        List<CircleSegment> sortedSegments = new List<CircleSegment>(availableSegments);
        sortedSegments.Sort((a, b) => b.percentage.CompareTo(a.percentage));

        List<CircleSegment> bestCombination = FindBestCombination(sortedSegments, percentage);

        PlaceSegmentsInCircle(bestCombination);

        float newPercentage = bestCombination.Sum(s => s.percentage);
        UpdatePercentageText(newPercentage, true);
        currentPercentage = newPercentage;

        Debug.Log($"Pourcentage cible: {percentage}%, Pourcentage atteint: {currentPercentage}%");
    }

    private List<CircleSegment> FindBestCombination(List<CircleSegment> segments, float targetPercentage)
    {
        List<CircleSegment> result = new List<CircleSegment>();
        float currentSum = 0f;
        bool[] used = new bool[segments.Count];

        while (currentSum < targetPercentage)
        {
            float bestDiff = float.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < segments.Count; i++)
            {
                if (!used[i])
                {
                    float newSum = currentSum + segments[i].percentage;
                    float diff = Mathf.Abs(targetPercentage - newSum);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestIndex = i;
                    }
                }
            }

            if (bestIndex == -1)
                break;

            result.Add(segments[bestIndex]);
            currentSum += segments[bestIndex].percentage;
            used[bestIndex] = true;

            if (currentSum > targetPercentage)
            {
                float diffWithSegment = currentSum - targetPercentage;
                float diffWithoutSegment = targetPercentage - (currentSum - segments[bestIndex].percentage);

                if (diffWithoutSegment < diffWithSegment)
                {
                    result.RemoveAt(result.Count - 1);
                    currentSum -= segments[bestIndex].percentage;
                }

                break;
            }
        }

        return result;
    }

    private void PlaceSegmentsInCircle(List<CircleSegment> segments)
    {
        currentAnimation = DOTween.Sequence();

        float maxPercentage = availableSegments.Max(s => s.percentage);
        float currentAngle = 0f;

        List<SegmentInfo> segmentInfos = new List<SegmentInfo>();

        foreach (var segment in segments)
        {
            float segmentAngle = (segment.percentage / 100f) * 360f;

            float middleAngle;
            if (clockwiseDirection)
            {
                middleAngle = 90f - (currentAngle + segmentAngle / 2f);
            }
            else
            {
                middleAngle = 90f + (currentAngle + segmentAngle / 2f);
            }

            float radians = middleAngle * Mathf.Deg2Rad;
            Vector2 position = new Vector2(
                circleCenter.anchoredPosition.x + circleRadius * Mathf.Cos(radians),
                circleCenter.anchoredPosition.y + circleRadius * Mathf.Sin(radians)
            );

            float sizeRatio = segment.percentage / maxPercentage;
            float imageSize = baseImageSize * sizeRatio;

            Vector2 directionFromCenter = (position - circleCenter.anchoredPosition).normalized;
            float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;

            segmentInfos.Add(new SegmentInfo
            {
                Segment = segment,
                Position = position,
                Angle = angle,
                ImageSize = imageSize,
                StartAngle = currentAngle
            });

            currentAngle += segmentAngle;
        }

        if (clockwiseAnimation)
        {
            segmentInfos.Sort((a, b) => a.StartAngle.CompareTo(b.StartAngle));
        }
        else
        {
            segmentInfos.Sort((a, b) => b.StartAngle.CompareTo(a.StartAngle));
        }

        float delay = 0f;

        foreach (var info in segmentInfos)
        {
            GameObject imageObj = new GameObject($"Segment_{info.Segment.percentage}%");
            imageObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = info.Position;

            Image image = imageObj.AddComponent<Image>();
            image.sprite = info.Segment.sprite;
            image.preserveAspect = true;

            rectTransform.sizeDelta = Vector2.zero;

            rectTransform.rotation = Quaternion.Euler(0, 0, info.Angle - 90);

            currentAnimation.Insert(delay,
                rectTransform.DOSizeDelta(new Vector2(info.ImageSize, info.ImageSize), animationDuration)
                    .SetEase(animationEase));

            activeImages.Add(image);

            delay += delayBetweenImages;
        }

        currentAnimation.Play();
    }

    private class SegmentInfo
    {
        public CircleSegment Segment;
        public Vector2 Position;
        public float Angle;
        public float ImageSize;
        public float StartAngle;
    }

    private void UpdatePercentageText(float percentage, bool animate)
    {
        if (percentageText != null)
        {
            if (animate && animatePercentageText)
            {
                float startValue = currentPercentage;
                DOTween.To(() => startValue, x => { percentageText.text = $"{Mathf.Round(x)}%"; }, percentage,
                        animationDuration)
                    .SetEase(animationEase);
            }
            else
            {
                percentageText.text = $"{Mathf.Round(percentage)}%";
            }
        }
    }

    private void ClearActiveSegments()
    {
        foreach (var image in activeImages)
        {
            if (image != null)
            {
                Destroy(image.gameObject);
            }
        }

        activeImages.Clear();
    }

    public void SetTargetPercentage(float percentage)
    {
        targetPercentage = Mathf.Clamp(percentage, 0f, 100f);
    }

    public float GetCurrentPercentage()
    {
        return currentPercentage;
    }

    public void SetClockwiseDirection(bool clockwise)
    {
        clockwiseDirection = clockwise;
        if (targetPercentage > 0)
        {
            FillCircle(targetPercentage);
        }
    }

    public void SetClockwiseAnimation(bool clockwise)
    {
        clockwiseAnimation = clockwise;
    }
}