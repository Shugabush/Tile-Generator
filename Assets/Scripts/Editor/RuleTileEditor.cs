#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RuleTile)), CanEditMultipleObjects]
public class RuleTileEditor : Editor
{
    RuleTile ruleTile;

    int selectedYLevel = 0;

    public override void OnInspectorGUI()
    {
        if (ruleTile == null)
        {
            ruleTile = (RuleTile)target;
        }

        // This is necessary for GetLastRect() to work apparently
        GUILayout.Space(25);

        //base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        SerializedProperty defaultObjProperty = serializedObject.FindProperty("defaultGameObject");
        EditorGUILayout.PropertyField(defaultObjProperty);

        Rect lastRect = GUILayoutUtility.GetLastRect();
        for (int i = 0; i < ruleTile.rules.Count; i++)
        {
            var rule = ruleTile.rules[i];

            if (rule.slots == null || rule.slots.Length != 26)
            {
                rule.slots = new RuleTile.Rule.Slot[26];
            }

            rule.newObj = (GameObject)EditorGUILayout.ObjectField("New Game Object", rule.newObj, typeof(GameObject));

            switch (selectedYLevel)
            {
                case -1:
                    GUILayout.Label("Lower Level");
                    break;
                case 0:
                    GUILayout.Label("Main Level");
                    break;
                case 1:
                    GUILayout.Label("Upper Level");
                    break;
            }

            Rect removeRuleRect = GUILayoutUtility.GetLastRect();

            if (GUI.Button(removeRuleRect, "Remove Rule"))
            {
                ruleTile.rules.Remove(rule);
                i--;
                continue;
            }

            for (int j = 0; j < rule.slots.Length; j++)
            {
                var slot = rule.slots[j];

                if (slot.direction.y == selectedYLevel)
                {
                    slot.Draw(new Rect(lastRect.x + 50 + (slot.direction.x * 50), lastRect.y + (125 + 175 * i) - (slot.direction.z * 50), 50, 50));
                }
            }
        }
        Rect addRuleRect = GUILayoutUtility.GetLastRect();

        if (GUI.Button(addRuleRect, "Add New Rule"))
        {
            // Add a new rule
            var newRule = new RuleTile.Rule();
            ruleTile.rules.Add(newRule);
        }

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
        EditorGUI.BeginChangeCheck();

        Rect conditionRect = new Rect(position.x, position.y + 25, position.width, position.height);

        SerializedProperty childProperty = property.Copy();
        for (int i = 0; childProperty.Next(true); i++)
        {
            Rect childRect = new Rect(position.x, position.y, position.width, position.height);
        }
        
        SerializedProperty conditionProperty = property.FindPropertyRelative("condition");

        EditorGUI.PropertyField(position, property);
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);

        if (property.isExpanded)
        {
            EditorGUI.PropertyField(conditionRect, conditionProperty);
        }

        if (EditorGUI.EndChangeCheck())
        {
            
        }
    }
}*/
#endif