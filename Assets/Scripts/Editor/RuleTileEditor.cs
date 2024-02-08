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

        EditorGUI.BeginChangeCheck();

        SerializedProperty defaultObjProperty = serializedObject.FindProperty("defaultGameObject");
        
        EditorGUILayout.PropertyField(defaultObjProperty);
        serializedObject.ApplyModifiedProperties();

        Rect currentRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(Screen.height * 0.5f);

        Rect lastRect = currentRect;

        for (int i = 0; i < ruleTile.rules.Count; i++)
        {
            var rule = ruleTile.rules[i];
            GUILayout.Space(currentRect.y - lastRect.y);

            lastRect = currentRect;

            currentRect.y += 50;

            rule.newObj = (GameObject)EditorGUI.ObjectField(currentRect, "New Game Object", rule.newObj, typeof(GameObject));

            currentRect.y += 50;
            currentRect.x += 200;

            GUIStyle yLevelStyle = new GUIStyle();
            yLevelStyle.fontSize = 24;

            var oldColor = GUI.backgroundColor;

            // Make buttons for y levels
            Rect selectedYLevelRect = currentRect;
            selectedYLevelRect.size = new Vector2(100, 25);

            if (selectedYLevel == -1) GUI.backgroundColor = Color.gray;

            selectedYLevelRect.y += 50;

            if (GUI.Button(selectedYLevelRect, "Lower Level"))
            {
                selectedYLevel = -1;
            }

            GUI.backgroundColor = oldColor;
            if (selectedYLevel == 0) GUI.backgroundColor = Color.gray;

            selectedYLevelRect.y += 50;

            if (GUI.Button(selectedYLevelRect, "Main Level"))
            {
                selectedYLevel = 0;
            }

            GUI.backgroundColor = oldColor;
            if (selectedYLevel == 1) GUI.backgroundColor = Color.gray;

            selectedYLevelRect.y += 50;

            if (GUI.Button(selectedYLevelRect, "Upper Level"))
            {
                selectedYLevel = 1;
            }

            GUI.backgroundColor = oldColor;
            currentRect.y = selectedYLevelRect.y;

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
        }

        currentRect.y += 25;
        if (GUI.Button(currentRect, "Add New Rule"))
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
#endif