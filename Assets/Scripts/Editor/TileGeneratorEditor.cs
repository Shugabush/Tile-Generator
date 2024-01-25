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

        Color oldColor = GUI.backgroundColor;

        EditorGUI.BeginChangeCheck();

        SerializedProperty gridSize = serializedObject.FindProperty("gridSize");
        EditorGUILayout.PropertyField(gridSize);

        SerializedProperty gridCount = serializedObject.FindProperty("gridCount");
        EditorGUILayout.PropertyField(gridCount);

        SerializedProperty selectedTileIndexProperty = serializedObject.FindProperty("selectedTileIndex");
        Vector3Int selectedTileIndex = selectedTileIndexProperty.vector3IntValue;
        selectedTileIndex.y = EditorGUILayout.IntField("Y Level", selectedTileIndex.y);
        selectedTileIndex.y = Mathf.Clamp(selectedTileIndex.y, 0, tileGenerator.GridCount.y);

        selectedTileIndexProperty.vector3IntValue = selectedTileIndex;

        SerializedProperty showAllYLevels = serializedObject.FindProperty("showAllYLevels");
        EditorGUILayout.PropertyField(showAllYLevels);

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
                GameObject targetObj = obj.defaultGameObject;
                if (obj != null && targetObj != null)
                {
                    GUI.backgroundColor = tileGenerator.selectedTilePrefab == targetObj ? Color.green : oldColor;
                    if (GUILayout.Button(targetObj.name))
                    {
                        tileGenerator.selectedTilePrefab = tileGenerator.selectedTilePrefab == targetObj ? null : targetObj;
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

        tileGenerator.shouldPaint = drawSelected;

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(tileGenerator);
        }
    }
}
#endif