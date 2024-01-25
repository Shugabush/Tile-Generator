#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/*[CustomEditor(typeof(RuleTile)), CanEditMultipleObjects]
public class RuleTileEditor : Editor
{
    RuleTile ruleTile;

    public override void OnInspectorGUI()
    {
        if (ruleTile == null)
        {
            ruleTile = (RuleTile)target;
        }

        // This is necessary for GetLastRect() to work apparently
        GUILayout.Space(0);

        //base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        SerializedProperty currentProperty = serializedObject.GetIterator().Copy();

        // Go through all properties (no children properties, they will be drawn automatically)
        for (int i = 0; currentProperty.NextVisible(i == 0); i++)
        {
            EditorGUILayout.PropertyField(currentProperty, currentProperty.name != "rules");
            if (currentProperty.name == "rules")
            {
                currentProperty.isExpanded = EditorGUILayout.Foldout(currentProperty.isExpanded, "slots");
                for (int j = 0; j < currentProperty.arraySize; j++)
                {
                    SerializedProperty ruleElement = currentProperty.GetArrayElementAtIndex(j);

                    SerializedProperty slotsArray = ruleElement.FindPropertyRelative("slots");
                    for (int k = 0; k < slotsArray.arraySize; k++)
                    {
                        SerializedProperty slotProperty = slotsArray.GetArrayElementAtIndex(k);
                        GUIContent slotLabel = new GUIContent(k.ToString());
                        EditorGUILayout.PropertyField(slotProperty, slotLabel);
                    }
                }
            }
            if (currentProperty.type == "Slot")
            {
                RuleTile.Rule.Slot slot = currentProperty.GetValue<RuleTile.Rule.Slot>();
                Debug.Log(slot.condition);
            }
        }

        currentProperty = serializedObject.GetIterator().Copy();

        Rect lastRect = GUILayoutUtility.GetLastRect();

        *//*for (int i = 0; i < ruleTile.rules.Length; i++)
        {
            var rule = ruleTile.rules[i];
            for (int j = 0; j < rule.slots.Length; j++)
            {
                var slot = rule.slots[j];
                slot.Draw(new Rect(lastRect.x + i, lastRect.y + i, 50, 50));
            }
        }*//*

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(ruleTile);
        }
    }
}*/

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