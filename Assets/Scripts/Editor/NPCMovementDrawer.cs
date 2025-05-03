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
            case NPCMovementType.ApproachToNPC: lines += 3; break; // targetNpc + distance + lookTarget
            case NPCMovementType.Walk: lines += 2; break;          // min/max wander
            case NPCMovementType.WalkToLocation: lines += 1; break; // targetLocation
            case NPCMovementType.Talk:          lines += 1; break; // audioClip
            case NPCMovementType.LookAtTarget: lines += 2; break; // target + durée


        }

        return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Common
        var typeProp = property.FindPropertyRelative("npcMovementType");
        var timeProp = property.FindPropertyRelative("TimeToStart");

        // Approach To NPC
        var targetNpcProp = property.FindPropertyRelative("targetNpc");
        var distanceProp = property.FindPropertyRelative("distance");
        var lookTargetProp = property.FindPropertyRelative("lookTarget");
        
        // Walk
        var minWanderProp = property.FindPropertyRelative("minWanderDistance");
        var maxWanderProp = property.FindPropertyRelative("maxWanderDistance");
        
        // WalkToLocation
        var targetLocationProp = property.FindPropertyRelative("targetLocation");

        // Talk
        var talkClipProp = property.FindPropertyRelative("talkClip");
        
        // LookAtTarget
        var lookAtTargetProp = property.FindPropertyRelative("lookAtTarget");
        var lookAtDurationProp = property.FindPropertyRelative("lookAtDuration");

        
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
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, lookTargetProp, new GUIContent("Look Target (opt)"));
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
            case NPCMovementType.Talk:
                EditorGUI.PropertyField(rect, talkClipProp, new GUIContent("Voice Clip"));
                break;
            case NPCMovementType.LookAtTarget:
                EditorGUI.PropertyField(rect, lookAtTargetProp, new GUIContent("Target"));
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, lookAtDurationProp, new GUIContent("Hold (s)"));
                break;

        }

        EditorGUI.EndProperty();
    }
}
#endif