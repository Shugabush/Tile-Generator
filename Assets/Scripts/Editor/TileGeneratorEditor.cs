#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileGenerator))]
public class TileGeneratorEditor : Editor
{
    TileGenerator tileGenerator;

    bool drawSelected;
    bool eraseSelected;

    public override void OnInspectorGUI()
    {
        if (tileGenerator == null)
        {
            tileGenerator = (TileGenerator)target;
        }

        var oldColor = GUI.backgroundColor;

        EditorGUI.BeginChangeCheck();

        SerializedProperty gridSize = serializedObject.FindProperty("gridSize");
        EditorGUILayout.PropertyField(gridSize);

        SerializedProperty gridCount = serializedObject.FindProperty("gridCount");
        EditorGUILayout.PropertyField(gridCount);

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

        GUI.backgroundColor = drawSelected ? Color.red : oldColor;
        if (GUILayout.Button("Draw"))
        {
            drawSelected = !drawSelected;
            eraseSelected = false;
        }

        GUI.backgroundColor = eraseSelected ? Color.red : oldColor;
        if (GUILayout.Button("Erase"))
        {
            eraseSelected = !eraseSelected;
            drawSelected = false;
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