#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileGenerator))]
public class TileGeneratorEditor : Editor
{
    TileGenerator tileGenerator;
    public override void OnInspectorGUI()
    {
        if (tileGenerator == null)
        {
            tileGenerator = (TileGenerator)target;
        }

        var oldColor = GUI.backgroundColor;

        EditorGUI.BeginChangeCheck();

        SerializedProperty property = serializedObject.FindProperty("gridSize");
        EditorGUILayout.PropertyField(property);
        serializedObject.ApplyModifiedProperties();

        foreach (TilePalette palette in tileGenerator.palettes)
        {
            if (palette != null)
            {
                GUI.backgroundColor = tileGenerator.selectedPalette == palette ? Color.blue : oldColor;
                
                if (GUILayout.Button(palette.name))
                {
                    tileGenerator.selectedPalette = palette;
                }
            }
        }

        GUI.backgroundColor = oldColor;

        if (GUILayout.Button("Add/Remove Palettes"))
        {
            AddTilePaletteWindow window = (AddTilePaletteWindow)EditorWindow.GetWindow(typeof(AddTilePaletteWindow), false, "Add Palette");
            window.tileGenerator = tileGenerator;
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(tileGenerator);
        }
    }
}
#endif