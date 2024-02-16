#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TileGeneration
{
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

            Undo.RecordObject(tileGenerator, "Tile Generator Change");

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

                        GUIContent asset = new GUIContent(AssetPreview.GetAssetPreview(ruleTile.defaultGameObject),
                            "This is the " + ruleTile.defaultGameObject.name + " prefab");

                        if (GUILayout.Button(asset))
                        {
                            tileGenerator.selectedRule = ruleTile;
                            tileGenerator.selectedTilePrefab = targetObj;
                        }
                    }
                }
            }

            GUILayout.Space(25);

            GUI.backgroundColor = tileGenerator.shouldPaint ? Color.red : oldColor;
            if (GUILayout.Button("Draw"))
            {
                tileGenerator.shouldPaint = true;
            }

            GUI.backgroundColor = !tileGenerator.shouldPaint ? Color.red : oldColor;
            if (GUILayout.Button("Erase"))
            {
                tileGenerator.shouldPaint = false;
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

            if (GUILayout.Button("Clear Unused Objects"))
            {
                tileGenerator.ClearUnusedObjects();
            }

            if (GUILayout.Button("Reset Tiles"))
            {
                tileGenerator.ResetTiles();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tileGenerator);
            }
        }
    }
}
#endif