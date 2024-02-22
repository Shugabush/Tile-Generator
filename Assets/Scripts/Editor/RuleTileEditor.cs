#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileGeneration
{
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

            Undo.RecordObject(ruleTile, "Rule Tile Change");

            SerializedProperty defaultObjProperty = serializedObject.FindProperty("defaultGameObject");
            EditorGUILayout.PropertyField(defaultObjProperty);

            ruleTile.fixBoundsPosition = EditorGUILayout.Toggle("Fix Bounds Position", ruleTile.fixBoundsPosition);
            ruleTile.fixBoundsScale = EditorGUILayout.Toggle("Fix Bounds Scale", ruleTile.fixBoundsScale);

            SerializedProperty rotationOffsetProperty = serializedObject.FindProperty("rotationOffset");
            EditorGUILayout.PropertyField(rotationOffsetProperty);

            serializedObject.ApplyModifiedProperties();

            Rect currentRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(Screen.height * 0.75f);

            Rect lastRect = currentRect;

            for (int i = 0; i < ruleTile.rules.Count; i++)
            {
                var rule = ruleTile.rules[i];
                GUILayout.Space(currentRect.y - lastRect.y);

                lastRect = currentRect;

                currentRect.y += 25;

                GUIContent foldoutContent = new GUIContent(AssetPreview.GetAssetPreview(rule.newObj));

                rule.open = EditorGUI.Foldout(currentRect, rule.open, foldoutContent);

                if (GUI.Button(new Rect(currentRect.x + 50, currentRect.y, 100, currentRect.height), "Remove Rule"))
                {
                    ruleTile.rules.Remove(rule);
                    i--;
                    continue;
                }

                // If the rule should be open, display it
                if (rule.open)
                {
                    AddRectYOffset(ref currentRect, 25);

                    rule.newObj = (GameObject)EditorGUI.ObjectField(currentRect, "New Game Object", rule.newObj, typeof(GameObject));

                    AddRectYOffset(ref currentRect, 50);
                    currentRect.x += 200;

                    GUIStyle yLevelStyle = new GUIStyle();
                    yLevelStyle.fontSize = 24;

                    Color oldColor = GUI.backgroundColor;

                    // Explain what the fix bounds properties are for
                    EditorGUI.LabelField(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        "The bounds of a mesh may not be centered or scaled properly,");
                    AddRectYOffset(ref currentRect, 15);

                    EditorGUI.LabelField(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        "so checking these fields will fix that.");
                    AddRectYOffset(ref currentRect, 25);

                    rule.fixBoundsPosition = EditorGUI.Toggle(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        new GUIContent("Fix Bounds Position"), rule.fixBoundsPosition);
                    AddRectYOffset(ref currentRect, 25);

                    rule.fixBoundsScale = EditorGUI.Toggle(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        new GUIContent("Fix Bounds Scale"), rule.fixBoundsScale);
                    AddRectYOffset(ref currentRect, 25);

                    // Explain what offsets are for
                    EditorGUI.LabelField(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height), 
                        "Test");
                    AddRectYOffset(ref currentRect, 25);

                    rule.positionOffset = EditorGUI.Vector3Field(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        new GUIContent("Position Offset"), rule.positionOffset);
                    AddRectYOffset(ref currentRect, 25);

                    rule.rotationOffset = EditorGUI.Vector3Field(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        new GUIContent("Rotation Offset"), rule.rotationOffset);
                    AddRectYOffset(ref currentRect, 25);

                    rule.scaleMultiplier = EditorGUI.Vector3Field(new Rect(lastRect.x, currentRect.y, currentRect.width, currentRect.height),
                        new GUIContent("Scale Multiplier"), rule.scaleMultiplier);

                    // Make buttons for y levels
                    Rect selectedYLevelRect = currentRect;
                    selectedYLevelRect.size = new Vector2(100, 25);

                    if (selectedYLevel == -1) GUI.backgroundColor = Color.gray;

                    AddRectYOffset(ref selectedYLevelRect, 50);

                    if (GUI.Button(selectedYLevelRect, "Lower Level"))
                    {
                        selectedYLevel = -1;
                    }

                    GUI.backgroundColor = oldColor;
                    if (selectedYLevel == 0) GUI.backgroundColor = Color.gray;

                    AddRectYOffset(ref selectedYLevelRect, 50);

                    if (GUI.Button(selectedYLevelRect, "Main Level"))
                    {
                        selectedYLevel = 0;
                    }

                    GUI.backgroundColor = oldColor;
                    if (selectedYLevel == 1) GUI.backgroundColor = Color.gray;

                    AddRectYOffset(ref selectedYLevelRect, 50);

                    if (GUI.Button(selectedYLevelRect, "Upper Level"))
                    {
                        selectedYLevel = 1;
                    }

                    GUI.backgroundColor = oldColor;
                    currentRect.y = selectedYLevelRect.y;

                    currentRect.x -= 200;
                    AddRectYOffset(ref currentRect, 50);

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

                    if (selectedYLevel == 0)
                    {
                        Rect rect = new Rect(currentRect.x + 50, currentRect.y, 50, 50);

                        rule.newObj.transform.rotation *= Quaternion.Euler(rule.rotationOffset);

                        GUI.DrawTexture(rect, AssetPreview.GetAssetPreview(rule.newObj));

                        rule.newObj.transform.rotation *= Quaternion.Inverse(Quaternion.Euler(rule.rotationOffset));
                    }

                    AddRectYOffset(ref currentRect, 100);
                }
            }

            AddRectYOffset(ref currentRect, 25);
            if (GUI.Button(currentRect, "Add New Rule"))
            {
                // Add a new rule
                RuleTile.Rule newRule;

                if (ruleTile.rules.Count > 0)
                {
                    newRule = new RuleTile.Rule(ruleTile.rules[ruleTile.rules.Count - 1]);
                }
                else
                {
                    newRule = new RuleTile.Rule();
                }

                ruleTile.rules.Add(newRule);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(ruleTile);
            }
        }

        void AddRectYOffset(ref Rect rect, float offset)
        {
            rect.y += offset;
            //GUILayout.Space(offset);
        }
    }
}
#endif