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

        //base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        SerializedProperty defaultObjProperty = serializedObject.FindProperty("defaultGameObject");
        EditorGUILayout.PropertyField(defaultObjProperty);

        for (int i = 0; i < ruleTile.rules.Count; i++)
        {
            if (ruleTile.rules.Count == 0)
            {
                break;
            }

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

            if (GUILayout.Button("Remove Rule"))
            {
                ruleTile.rules.Remove(rule);
                i--;
                continue;
            }

            Vector3Int lastSlotDirection = -Vector3Int.one;
            lastSlotDirection.y = selectedYLevel;

            EditorGUILayout.BeginHorizontal();

            int row = 0;
            bool activeVertical = false;

            for (int j = 0; j < rule.slots.Length; j++)
            {
                var slot = rule.slots[j];

                if (slot.direction.y == selectedYLevel)
                {
                    row++;
                    GUILayoutOption widthOption = GUILayout.Width(50);
                    GUILayoutOption heightOption = GUILayout.Height(50);

                    if (row >= 3)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.EndHorizontal();

                        row = 0;
                        activeVertical = true;
                    }

                    if (GUILayout.Button(string.Empty, widthOption, heightOption))
                    {
                        slot.NextCondition();
                    }

                    Rect lastRect = GUILayoutUtility.GetLastRect();

                    slot.DrawTexture(lastRect);

                    if (activeVertical)
                    {
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginHorizontal();

                        GUI.Label(lastRect, "AV");

                        activeVertical = false;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add New Rule"))
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