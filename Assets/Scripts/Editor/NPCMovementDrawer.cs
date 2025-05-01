#if UNITY_EDITOR
using NPC.NPCEvent;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NPCEvent))]
public class NPCEventDrawer : PropertyDrawer
{
    private const float DefaultApproachDistance = 2f;
    private const float DefaultMinWanderDistance = 5f;
    private const float DefaultMaxWanderDistance = 15f;
    
    private struct PropertyInfo
    {
        public SerializedProperty TypeProp;
        public SerializedProperty TimeProp;
        public SerializedProperty AnimProp;
        public SerializedProperty TargetNpcProp;
        public SerializedProperty DistanceProp;
        public SerializedProperty MinWanderProp;
        public SerializedProperty MaxWanderProp;
        public SerializedProperty TargetLocationProp;
        
        public NPCEventType EventType => (NPCEventType)TypeProp.enumValueIndex;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lineCount = 3; // Base fields: npcEventType + TimeToStart + animationType
        
        switch (GetEventType(property))
        {
            case NPCEventType.ApproachToNPC: lineCount += 2; break; // targetNpc + distance
            case NPCEventType.Walk: lineCount += 2; break;          // min/max wander
            case NPCEventType.WalkToLocation: lineCount += 1; break; // targetLocation
        }

        return CalculateHeight(lineCount);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var propInfo = GetProperties(property);
        Rect rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;
        
        // Draw common properties
        DrawCommonProperties(ref rect, propInfo);
        
        // Draw type-specific properties
        switch (propInfo.EventType)
        {
            case NPCEventType.ApproachToNPC:
                DrawApproachProperties(ref rect, propInfo);
                break;
            case NPCEventType.Walk:
                DrawWalkProperties(ref rect, propInfo);
                break;
            case NPCEventType.WalkToLocation:
                DrawWalkToLocationProperties(ref rect, propInfo);
                break;
        }

        EditorGUI.EndProperty();
    }
    
    private NPCEventType GetEventType(SerializedProperty property)
    {
        var typeProp = property.FindPropertyRelative("npcEventType");
        return (NPCEventType)typeProp.enumValueIndex;
    }
    
    private PropertyInfo GetProperties(SerializedProperty property)
    {
        return new PropertyInfo
        {
            TypeProp = property.FindPropertyRelative("npcEventType"),
            TimeProp = property.FindPropertyRelative("TimeToStart"),
            AnimProp = property.FindPropertyRelative("animationType"),
            TargetNpcProp = property.FindPropertyRelative("targetNpc"),
            DistanceProp = property.FindPropertyRelative("distance"),
            MinWanderProp = property.FindPropertyRelative("minWanderDistance"),
            MaxWanderProp = property.FindPropertyRelative("maxWanderDistance"),
            TargetLocationProp = property.FindPropertyRelative("targetLocation")
        };
    }
    
    private float CalculateHeight(int lineCount)
    {
        return lineCount * EditorGUIUtility.singleLineHeight + 
               (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
    }
    
    private void DrawCommonProperties(ref Rect rect, PropertyInfo propInfo)
    {
        // Event Type
        EditorGUI.PropertyField(rect, propInfo.TypeProp, new GUIContent("Event Type"));
        AdvanceRect(ref rect);
        
        // Animation Type
        EditorGUI.PropertyField(rect, propInfo.AnimProp, new GUIContent("Animation"));
        AdvanceRect(ref rect);

        // Start Time
        EditorGUI.PropertyField(rect, propInfo.TimeProp, new GUIContent("Start Time"));
        AdvanceRect(ref rect);
    }
    
    private void DrawApproachProperties(ref Rect rect, PropertyInfo propInfo)
    {
        // Set default distance if needed
        if (Mathf.Approximately(propInfo.DistanceProp.floatValue, 0f))
        {
            propInfo.DistanceProp.floatValue = DefaultApproachDistance;
        }
        
        EditorGUI.PropertyField(rect, propInfo.TargetNpcProp, new GUIContent("Target NPC"));
        AdvanceRect(ref rect);
        
        EditorGUI.PropertyField(rect, propInfo.DistanceProp, new GUIContent("Distance"));
        AdvanceRect(ref rect);
    }
    
    private void DrawWalkProperties(ref Rect rect, PropertyInfo propInfo)
    {
        // Set default values if needed
        if (Mathf.Approximately(propInfo.MinWanderProp.floatValue, 0f) && 
            Mathf.Approximately(propInfo.MaxWanderProp.floatValue, 0f))
        {
            propInfo.MinWanderProp.floatValue = DefaultMinWanderDistance;
            propInfo.MaxWanderProp.floatValue = DefaultMaxWanderDistance;
        }
        
        EditorGUI.PropertyField(rect, propInfo.MinWanderProp, new GUIContent("Min Wander Distance"));
        AdvanceRect(ref rect);
        
        EditorGUI.PropertyField(rect, propInfo.MaxWanderProp, new GUIContent("Max Wander Distance"));
        AdvanceRect(ref rect);
    }
    
    private void DrawWalkToLocationProperties(ref Rect rect, PropertyInfo propInfo)
    {
        EditorGUI.PropertyField(rect, propInfo.TargetLocationProp, new GUIContent("Target Location"));
        AdvanceRect(ref rect);
    }
    
    private void AdvanceRect(ref Rect rect)
    {
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif