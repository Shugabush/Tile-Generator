#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileGenerator))]
public class TileGeneratorEditor : Editor
{
    TileGenerator tileGenerator;

    bool drawSelected = false;
    bool eraseSelected = false;

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
        selectedTileIndex.y = Mathf.Clamp(selectedTileIndex.y, 0, tileGenerator.GridCount.y - 1);

        selectedTileIndexProperty.vector3IntValue = selectedTileIndex;

        SerializedProperty showAllYLevels = serializedObject.FindProperty("showAllYLevels");
        EditorGUILayout.PropertyField(showAllYLevels);

        SerializedProperty showGrid = serializedObject.FindProperty("showGrid");
        EditorGUILayout.PropertyField(showGrid);

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
            foreach (var ruleTile in tileGenerator.selectedPalette.ruleTiles)
            {
                if (ruleTile == null) continue;

                GameObject targetObj = ruleTile.GetObject(tileGenerator.SelectedTile);
                if (targetObj != null)
                {
                    GUI.backgroundColor = tileGenerator.selectedRule == ruleTile ? Color.green : oldColor;
                    if (GUILayout.Button(AssetPreview.GetAssetPreview(ruleTile.defaultGameObject)))
                    {
                        tileGenerator.selectedRule = ruleTile;
                        tileGenerator.selectedTilePrefab = targetObj;
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

        GUILayout.Space(25);

        if (GUILayout.Button("Add/Remove Palettes"))
        {
            AddTilePaletteWindow window = (AddTilePaletteWindow)EditorWindow.GetWindow(typeof(AddTilePaletteWindow), false, "Add Palette");
            window.tileGenerator = tileGenerator;
        }

        if (GUILayout.Button("Clear Unused Tiles"))
        {
            tileGenerator.ClearUnusedTiles();
        }

        tileGenerator.shouldPaint = drawSelected;
        tileGenerator.shouldErase = eraseSelected;

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(tileGenerator);
        }
    }
}
#endif