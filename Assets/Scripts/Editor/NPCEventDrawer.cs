
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NPCEvent))]
public class NPCEventDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 2; // npcEventType + TimeToStart
        NPCEventType type = (NPCEventType)property.FindPropertyRelative("npcEventType").enumValueIndex;

        switch (type)
        {
            case NPCEventType.ApproachToNPC: lines += 2; break; // targetNpc + distance
            case NPCEventType.Walk: lines += 2; break;          // min/max wander
            case NPCEventType.WalkToLocation: lines += 1; break; // targetLocation
        }

        return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProp = property.FindPropertyRelative("npcEventType");
        var timeProp = property.FindPropertyRelative("TimeToStart");

        var targetNpcProp = property.FindPropertyRelative("targetNpc");
        var distanceProp = property.FindPropertyRelative("distance");
        var minWanderProp = property.FindPropertyRelative("minWanderDistance");
        var maxWanderProp = property.FindPropertyRelative("maxWanderDistance");
        var targetLocationProp = property.FindPropertyRelative("targetLocation");

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(rect, typeProp, new GUIContent("Event Type"));
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(rect, timeProp, new GUIContent("Start Time"));
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        switch ((NPCEventType)typeProp.enumValueIndex)
        {
            case NPCEventType.ApproachToNPC:
                // Default value
                if (Mathf.Approximately(distanceProp.floatValue, 0f))
                {
                    distanceProp.floatValue = 2f;
                }
                EditorGUI.PropertyField(rect, targetNpcProp, new GUIContent("Target NPC"));
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, distanceProp, new GUIContent("Distance"));
                break;
            case NPCEventType.Walk:
                // Default value
                if (Mathf.Approximately(minWanderProp.floatValue, 0f) && Mathf.Approximately(maxWanderProp.floatValue, 0f))
                {
                    minWanderProp.floatValue = 5f;
                    maxWanderProp.floatValue = 15f;
                }
                EditorGUI.PropertyField(rect, minWanderProp, new GUIContent("Min Wander Distance"));
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, maxWanderProp, new GUIContent("Max Wander Distance"));
                break;
            case NPCEventType.WalkToLocation:
                EditorGUI.PropertyField(rect, targetLocationProp, new GUIContent("Target Location"));
                break;
        }

        EditorGUI.EndProperty();
    }
}
#endif