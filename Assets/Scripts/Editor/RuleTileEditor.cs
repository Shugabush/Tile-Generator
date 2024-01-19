#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RuleTile)), CanEditMultipleObjects]
public class RuleTileEditor : Editor
{
    RuleTile ruleTile;

    public override void OnInspectorGUI()
    {
        if (ruleTile == null)
        {
            ruleTile = (RuleTile)target;
        }

        EditorGUI.BeginChangeCheck();

        serializedObject.Update();

        SerializedProperty ruleArray = serializedObject.FindProperty("rules");
        EditorGUILayout.PropertyField(ruleArray);

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(ruleTile);
        }
    }
}

/*[CustomPropertyDrawer(typeof(RuleTile.Rule.Slot))]
public class RuleTileSlotEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect conditionRect = new Rect(position.x, position.y, position.width, position.height);

        SerializedProperty conditionProperty = property.FindPropertyRelative("condition");
        EditorGUI.PropertyField(conditionRect, conditionProperty);

        EditorGUI.EndProperty();
    }
}*/
#endif