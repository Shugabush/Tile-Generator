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

        Rect currentRect = GUILayoutUtility.GetLastRect();

        for (int i = 0; i < ruleTile.rules.Count; i++)
        {
            var rule = ruleTile.rules[i];

            if (rule.slots == null || rule.slots.Length != 26)
            {
                rule.slots = new RuleTile.Rule.Slot[26];
            }

            currentRect.y += 50;

            rule.newObj = (GameObject)EditorGUI.ObjectField(currentRect, "New Game Object", rule.newObj, typeof(GameObject));

            currentRect.y += 50;
            currentRect.x += 200;

            GUIStyle yLevelStyle = new GUIStyle();
            yLevelStyle.fontSize = 24;
            switch (selectedYLevel)
            {
                case -1:
                    GUI.Label(currentRect, "Lower Level", yLevelStyle);
                    break;
                case 0:
                    GUI.Label(currentRect, "Main Level", yLevelStyle);
                    break;
                case 1:
                    GUI.Label(currentRect, "Upper Level", yLevelStyle);
                    break;
                default:
                    throw new System.Exception("Selected Y Level of " + selectedYLevel.ToString() + "is unexpected!");
            }

            currentRect.x -= 200;
            currentRect.y += 50;

            Vector3Int lastSlotDirection = -Vector3Int.one;
            lastSlotDirection.y = selectedYLevel;

            for (int j = 0; j < rule.slots.Length; j++)
            {
                var slot = rule.slots[j];

                if (slot.direction.y == selectedYLevel)
                {
                    Rect slotRect = new Rect(currentRect.x + 50 + (slot.direction.x * 50), currentRect.y - (slot.direction.z * 50), 50, 50);
                    slot.Draw(slotRect);
                }
            }

            currentRect.y += 125;

            if (GUI.Button(currentRect, "Remove Rule"))
            {
                ruleTile.rules.Remove(rule);
                i--;
                continue;
            }
            //currentRect.y += 50;
        }

        currentRect.y += 25;
        if (GUI.Button(currentRect, "Add New Rule"))
        {
            // Add a new rule
            var newRule = new RuleTile.Rule();
            ruleTile.rules.Add(newRule);
        }

        GUILayout.Space(1000);

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