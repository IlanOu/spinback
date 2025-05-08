using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// User interface for the time rewind system.
/// Displays current time, rewind status, and timeline visualization.
/// </summary>
public class TimeRewindUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the TimeRewindManager (optional, will find automatically if null)")]
    [SerializeField] private TimeRewindManager rewindManager;
    
    [Tooltip("Slider representing the timeline")]
    [SerializeField] private Slider timeSlider;
    
    [Tooltip("Text displaying the current time")]
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Tooltip("Text indicating the current mode")]
    [SerializeField] private TextMeshProUGUI modeText;
    
    [Tooltip("Visual indicator of current status")]
    [SerializeField] private Image statusIndicator;
    
    [Header("Appearance")]
    [Tooltip("Color when in recording mode")]
    [SerializeField] private Color recordingColor = new Color(0.2f, 0.7f, 0.2f);
    
    [Tooltip("Color when in rewind mode")]
    [SerializeField] private Color rewindingColor = new Color(0.7f, 0.2f, 0.2f);
    
    private void Start()
    {
        // If no manager is assigned, try to find one
        if (rewindManager == null)
            rewindManager = TimeRewindManager.Instance;
            
        if (rewindManager == null)
        {
            Debug.LogError("TimeRewindUI cannot find TimeRewindManager!");
            enabled = false;
            return;
        }
    }
    
    private void Update()
    {
        if (rewindManager != null)
        {
            UpdateUI();
        }
    }
    
    /// <summary>
    /// Update all UI elements based on current rewind system state
    /// </summary>
    private void UpdateUI()
    {
        // Get values from manager using properties
        float recordingTime = rewindManager.RecordingTime;
        float currentPlaybackTime = rewindManager.CurrentPlaybackTime;
        bool isRewinding = rewindManager.IsRewinding;
        
        // Update timeline slider
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = recordingTime;
            timeSlider.SetValueWithoutNotify(isRewinding ? currentPlaybackTime : recordingTime);
            timeSlider.interactable = false; // Display only
        }
        
        // Update time text
        if (timeText != null)
        {
            float displayTime = isRewinding ? currentPlaybackTime : recordingTime;
            int minutes = Mathf.FloorToInt(displayTime / 60f);
            int seconds = Mathf.FloorToInt(displayTime % 60f);
            int milliseconds = Mathf.FloorToInt((displayTime * 100f) % 100f);
            
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
        
        // Update mode text
        if (modeText != null)
        {
            modeText.text = isRewinding ? "REWIND MODE" : "RECORD MODE";
        }
        
        // Update status indicator color
        if (statusIndicator != null)
        {
            statusIndicator.color = isRewinding ? rewindingColor : recordingColor;
        }
    }
}