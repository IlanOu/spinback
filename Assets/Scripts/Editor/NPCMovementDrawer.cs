
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NPCMovement))]
public class NPCMovementDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 2; // npcMovementType + TimeToStart
        NPCMovementType type = (NPCMovementType)property.FindPropertyRelative("npcMovementType").enumValueIndex;

        switch (type)
        {
            case NPCMovementType.ApproachToNPC: lines += 2; break; // targetNpc + distance
            case NPCMovementType.Walk: lines += 2; break;          // min/max wander
            case NPCMovementType.WalkToLocation: lines += 1; break; // targetLocation
        }

        return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProp = property.FindPropertyRelative("npcMovementType");
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

        switch ((NPCMovementType)typeProp.enumValueIndex)
        {
            case NPCMovementType.ApproachToNPC:
                // Default value
                if (Mathf.Approximately(distanceProp.floatValue, 0f))
                {
                    distanceProp.floatValue = 2f;
                }
                EditorGUI.PropertyField(rect, targetNpcProp, new GUIContent("Target NPC"));
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, distanceProp, new GUIContent("Distance"));
                break;
            case NPCMovementType.Walk:
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
            case NPCMovementType.WalkToLocation:
                EditorGUI.PropertyField(rect, targetLocationProp, new GUIContent("Target Location"));
                break;
        }

        EditorGUI.EndProperty();
    }
}
#endif