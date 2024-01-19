#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

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
                    tileGenerator.selectedPalette = tileGenerator.selectedPalette == palette ? null : palette;
                }
            }
        }

        GUILayout.Space(25);

        if (tileGenerator.selectedPalette != null)
        {
            // Display selected palette's prefabs
            foreach (var obj in tileGenerator.selectedPalette.objectSets)
            {
                if (obj != null && obj.prefab != null)
                {
                    GUI.backgroundColor = tileGenerator.selectedTilePrefab == obj.prefab ? Color.green : oldColor;
                    if (GUILayout.Button(obj.prefab.name))
                    {
                        tileGenerator.selectedTilePrefab = tileGenerator.selectedTilePrefab == obj.prefab ? null : obj.prefab;
                    }
                }
            }
        }


        GUILayout.Space(25);

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