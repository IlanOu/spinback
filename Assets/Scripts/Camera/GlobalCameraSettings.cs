using System;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Conversation = 0, 
    Object = 1,
    InvestigationAttachment = 2,
    Character = 3,
    Other = 4
};

[Serializable]
public class DetectionZoneSettings
{
    public ObjectType objectType;
    public Vector2 detectionCenter;
    public Vector2 detectionSize;
}

[Serializable]
public class CameraZoomSettings
{
    public ObjectType objectType;
    public float zoomValue;
}

[CreateAssetMenu(fileName = "GlobalCameraSettings", menuName = "Camera/GlobalCameraSettings")]
public class GlobalCameraSettings : ScriptableObject
{
    [SerializeField] private List<DetectionZoneSettings> detectionZoneSettings;
    [SerializeField] private List<CameraZoomSettings> cameraZoomSettings;
    private static GlobalCameraSettings _instance;
    public static GlobalCameraSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GlobalCameraSettings>("GlobalCameraSettings");
                if (_instance == null)
                {
                    Debug.LogError("GlobalCameraSettings asset not found in Resources!");
                }
            }
            return _instance;
        }
    }

    public T GetSettings<T>(ObjectType type) where T : class
    {
        if (typeof(T) == typeof(DetectionZoneSettings))
        {
            return detectionZoneSettings.Find((s) => s.objectType == type) as T;
        }
        else if (typeof(T) == typeof(CameraZoomSettings))
        {
            return cameraZoomSettings.Find((s) => s.objectType == type) as T;
        }

        Debug.LogWarning($"Unsupported settings type: {typeof(T)}");
        return null;
    }
}
