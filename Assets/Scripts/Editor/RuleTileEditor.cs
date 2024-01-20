#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RuleTile.Rule.Slot)), CanEditMultipleObjects]
public class RuleTileEditor : Editor
{
    /*RuleTile ruleTile;

    public override void OnInspectorGUI()
    {
        if (ruleTile == null)
        {
            ruleTile = (RuleTile)target;
        }

        base.OnInspectorGUI();

        Rect lastRect = GUILayoutUtility.GetLastRect();

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < ruleTile.rules.Length; i++)
        {
            var rule = ruleTile.rules[i];
            for (int j = 0; j < rule.slots.Length; j++)
            {
                var slot = rule.slots[j];
                slot.Draw(new Rect(lastRect.x + i, lastRect.y + i, 50, 50));
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(ruleTile);
        }
    }*/
}

[CustomPropertyDrawer(typeof(RuleTile.Rule.Slot))]
public class RuleTileSlotEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUILayout.BeginVertical();

        Rect conditionRect = new Rect(position.x, position.y + 25, position.width, position.height);

        SerializedProperty conditionProperty = property.FindPropertyRelative("condition");

        EditorGUILayout.PropertyField(property);
        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, GUIContent.none);

        if (property.isExpanded)
        {
            EditorGUILayout.PropertyField(conditionProperty);
        }

        EditorGUILayout.EndVertical();
    }
}
#endif