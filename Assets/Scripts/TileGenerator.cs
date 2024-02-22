using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileGeneration
{
    [ExecuteInEditMode]
    public class TileGenerator : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static readonly Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.forward,
            Vector3Int.left,
            Vector3Int.down,
            Vector3Int.back,
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 0, -1),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 0, -1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(0, 1, -1),
            new Vector3Int(0, -1, 1),
            new Vector3Int(0, -1, -1),
        };

        [SerializeField] Vector3Int tileSize = Vector3Int.one;

        public Vector3Int TileSize => tileSize;

        [SerializeField] List<Vector3Int> tileKeys = new List<Vector3Int>();
        [SerializeField] List<Tile> tileValues = new List<Tile>();

        Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

        List<Tile> tilesInRadius = new List<Tile>();

        public List<TilePalette> palettes = new List<TilePalette>();

        [SerializeReference] public TilePalette selectedPalette;
        [SerializeReference] public RuleTile selectedRule;
        public GameObject selectedTilePrefab;
        public Vector3Int selectedTileIndex = -Vector3Int.one;

        [SerializeField] Vector3 gridPivotPoint = new Vector3(0.5f, 0f, 0.5f);

        [Tooltip("Show what rule number is being used on each tile? (if any)")]
        public bool debugRuleUsage = true;

        public enum PaintMode
        {
            Draw,
            Fill,
        }

        [SerializeField] int paintRadius = 5;

        public int PaintRadius
        {
            get
            {
                return paintRadius;
            }
            set
            {
                paintRadius = Mathf.Clamp(value, 1, 10);
            }
        }

        public PaintMode paintMode = PaintMode.Draw;

        public bool shouldPaint = true;
        [SerializeField] bool showAllYLevels = true;

        public Tile SelectedTile
        {
            get
            {
                if (tiles.TryGetValue(selectedTileIndex, out Tile tile))
                {
                    if (selectedTileIndex.x < 0 || selectedTileIndex.y < 0 || selectedTileIndex.z < 0)
                    {
                        tiles.Remove(selectedTileIndex);
                        return null;
                    }

                    return tile;
                }
                return null;
            }
        }

        int debugCount;
        Queue<Tile> pendingTiles;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                foreach (var tile in tiles.Values)
                {
                    tile.parent = this;
                    if (tile.obj != null)
                    {
                        tile.obj.SetActive(true);
                    }
                }
            }
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                enabled = false;
            }
            else
            {
                OnValidate();
            }
        }

        void OnValidate()
        {
            Undo.undoRedoPerformed = new Undo.UndoRedoCallback(ClearUnusedObjects);

            // Grid count can never go below 1
            tileSize.x = System.Math.Max(tileSize.x, 1);
            tileSize.y = System.Math.Max(tileSize.y, 1);
            tileSize.z = System.Math.Max(tileSize.z, 1);

            for (int x = 0; x < tileSize.x; x++)
            {
                for (int y = 0; y < tileSize.y; y++)
                {
                    for (int z = 0; z < tileSize.z; z++)
                    {
                        ValidateTile(x, y, z);
                    }
                }
            }

            // Destroy objects in unused tiles
            foreach (var tileKey in tiles.Keys)
            {
                if (tileKey.x >= tileSize.x ||
                    tileKey.y >= tileSize.y ||
                    tileKey.z >= tileSize.z)
                {
                    Tile tile = tiles[tileKey];
                    if (tile.obj != null)
                    {
#if UNITY_EDITOR
                        EditorApplication.delayCall += () => DestroyImmediate(tile.obj);
#endif
                    }
                }
            }
        }

        public void ClearUnusedTiles()
        {
            // Remove unused tiles
            List<Vector3Int> keysToRemove = new List<Vector3Int>();

            foreach (var tileKey in tiles.Keys)
            {
                if (tileKey.x >= tileSize.x ||
                    tileKey.y >= tileSize.y ||
                    tileKey.z >= tileSize.z)
                {
                    keysToRemove.Add(tileKey);
                }
            }

            foreach (var key in keysToRemove)
            {
                Tile tile = tiles[key];
                if (tile.obj != null)
                {
#if UNITY_EDITOR
                    EditorApplication.delayCall += () => DestroyImmediate(tile.obj);
#endif
                }

                tiles.Remove(key);
            }
        }

        public void ClearUnusedObjects()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                bool trackedByTile = false;
                foreach (var tile in tiles.Values)
                {
                    if (tile.obj == child.gameObject)
                    {
                        trackedByTile = true;
                        break;
                    }
                }
                
                if (!trackedByTile)
                {
                    DestroyImmediate(child);
                }
            }
        }

        void ValidateTile(Vector3Int vectorIndex)
        {
            if (!tiles.TryGetValue(vectorIndex, out Tile tile))
            {
                tile = new Tile(this, vectorIndex);
                tiles.Add(vectorIndex, tile);
            }

            tile.parent = this;
            tile.indexPosition = vectorIndex;

            if (tile.obj != null)
            {
                tile.SetTransform(tile.GetTargetLocalPosition(), tile.prefab.transform.localRotation, Vector3.one);

                if (showAllYLevels)
                {
                    tile.obj.SetActive(true);
                }
                else
                {
                    tile.obj.SetActive(vectorIndex.y == selectedTileIndex.y);
                }
            }
            tile.EnsurePrefabIsInstantiated();
            ManageAdjacentTiles(tile);

#if UNITY_EDITOR
            tile.FixObject();
#endif
        }

        void ValidateTile(int x, int y, int z)
        {
            ValidateTile(new Vector3Int(x, y, z));
        }

        void ManageAdjacentTiles(Tile targetTile)
        {
            ManageSingleAdjacentTile(targetTile, Vector3Int.right);
            ManageSingleAdjacentTile(targetTile, Vector3Int.left);
            ManageSingleAdjacentTile(targetTile, Vector3Int.up);
            ManageSingleAdjacentTile(targetTile, Vector3Int.down);
            ManageSingleAdjacentTile(targetTile, Vector3Int.forward);
            ManageSingleAdjacentTile(targetTile, Vector3Int.back);

            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, 1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, -1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, 0, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, 0, -1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, 1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, -1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, 0, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, 0, -1));

            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, 1, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, 1, -1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, -1, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, -1, -1));
        }

        void ManageSingleAdjacentTile(Tile targetTile, Vector3Int direction)
        {
            int x = targetTile.indexPosition.x;
            int y = targetTile.indexPosition.y;
            int z = targetTile.indexPosition.z;

            Vector3Int adjacentVectorIndex = new Vector3Int(x + direction.x, y + direction.y, z + direction.z);
            if (tiles.ContainsKey(adjacentVectorIndex))
            {
                targetTile.SetAdjacentTile(direction, tiles[adjacentVectorIndex]);
            }
        }

#if UNITY_EDITOR
        public void ResetTiles()
        {
            tiles.Clear();
            while (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            EditorUtility.SetDirty(this);
        }

        void OnDrawGizmos()
        {
            if (debugRuleUsage)
            {
                foreach (var tile in tiles.Values)
                {
                    GUIContent content = GUIContent.none;
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    if (tile.ignoreRule)
                    {
                        // The tile is using this rule
                        content = new GUIContent("Using default object");
                        //Handles.Label(tile.GetTargetPosition(), new GUIContent("Using default object"));
                    }
                    else if (tile.rule != null)
                    {
                        for (int i = 0; i < tile.rule.rules.Count; i++)
                        {
                            var rule = tile.rule.rules[i];
                            if (rule.Evaluate(tile))
                            {
                                // The tile is using this rule
                                content = new GUIContent("Rule " + (i + 1).ToString());
                                //Handles.Label(tile.GetTargetPosition(), new GUIContent("Rule " + (i + 1).ToString()));
                                break;
                            }
                        }
                    }
                    if (content != GUIContent.none)
                    {
                        Handles.Label(tile.GetTargetPosition(), content, style);
                    }
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            ValidateTile(selectedTileIndex);

            switch (paintMode)
            {
                case PaintMode.Draw:
                    tilesInRadius = GetTilesInRadius(SelectedTile);
                    break;
                case PaintMode.Fill:
                    tilesInRadius = GetTilesToFill(SelectedTile);
                    break;
            }

            Gizmos.color = Color.red;
            for (int x = 0; x < tileSize.x; x++)
            {
                for (int z = 0; z < tileSize.z; z++)
                {
                    if (showAllYLevels)
                    {
                        for (int y = 0; y < tileSize.y; y++)
                        {
                            DrawTile(x, y, z);
                        }
                    }
                    else
                    {
                        DrawTile(x, selectedTileIndex.y, z);
                    }
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        void DrawTile(int x, int y, int z)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(GetGridScalePoint(x, y, z)), transform.rotation, transform.localScale);

            Gizmos.color = Color.red;

            Vector3Int indexPosition = new Vector3Int(x, y, z);

            tiles.TryAdd(indexPosition, new Tile(this, indexPosition));

            Tile tile = tiles[indexPosition];

            if (SelectedTile == tile || tilesInRadius.Contains(tile))
            {
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
            else
            {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
#endif

        public Vector3 GetGridScalePoint(Vector3Int index)
        {
            return GetGridScalePoint(index.x, index.y, index.z);
        }

        public Vector3 GetGridScalePoint(float x, float y, float z)
        {
            float xPoint = x + (0.5f - gridPivotPoint.x);
            float yPoint = y + (0.5f - gridPivotPoint.y);
            float zPoint = z + (0.5f - gridPivotPoint.z);

            return new Vector3(xPoint, yPoint, zPoint) - Vector3.Scale(tileSize - Vector3.one, gridPivotPoint);
        }

        public bool GetSelectedPoint(Ray ray, out Vector3 point)
        {
            point = Vector3.zero;

            Plane hPlane = new Plane(transform.up, transform.TransformPoint(GetGridScalePoint(0, selectedTileIndex.y, 0)));
            hPlane.distance -= transform.position.y;

            if (hPlane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);

                Vector3 selectedPoint = transform.InverseTransformPoint(ray.GetPoint(distance));

                // Trying to convert xPoint, yPoint, and zPoint into what the indexes will be (excluding decimals)
                float xPoint = selectedPoint.x + tileSize.x * 0.5f;
                float yPoint = selectedPoint.y + tileSize.y * 0.5f;
                float zPoint = selectedPoint.z + tileSize.z * 0.5f;

                int xIndex = (int)xPoint;
                int yIndex = (int)yPoint;
                int zIndex = (int)zPoint;

                if (xIndex < tileSize.x && xIndex >= 0
                    && yIndex < tileSize.y && yIndex >= 0
                    && zIndex < tileSize.z && zIndex >= 0)
                {
                    selectedTileIndex.x = xIndex;
                    selectedTileIndex.z = zIndex;
                    return true;
                }
            }
            selectedTileIndex.x = -1;
            selectedTileIndex.z = -1;
            return false;
        }

#if UNITY_EDITOR
        public void ChangeTile()
        {
            Undo.RecordObject(this, "Tile Generator Change");

            if (shouldPaint)
            {
                // Paint tile
                PaintTile();
            }
            else
            {
                // Erase tile
                EraseTile();
            }
        }

        void PaintTile()
        {
            // Cache selected tile
            Tile selectedTile = SelectedTile;

            if (selectedTile != null &&
                selectedRule != selectedTile.rule &&
                selectedTilePrefab != null)
            {
                if (selectedTile.obj != null)
                {
                    // Destroy existing obj
                    DestroyImmediate(selectedTile.obj);
                }

                // Create the game object
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(selectedTilePrefab, transform);

                Undo.RegisterCreatedObjectUndo(newObj, "New Object Created");

                selectedTile.obj = newObj;
                selectedTile.rule = selectedRule;
                selectedTile.prefab = selectedTilePrefab;

                selectedTile.obj.transform.parent = transform;

                selectedTile.SetTransform(selectedTile.GetTargetLocalPosition(), selectedTilePrefab.transform.localRotation, Vector3.one);
            }

            foreach (var tile in tilesInRadius)
            {
                PaintTile(tile);
            }

            // Fix objects in a separate loop because
            // we only want to do that after all tiles in radius
            // have had their objects created
            foreach (var tile in tiles.Keys)
            {
                ValidateTile(tile);
            }

            EditorUtility.SetDirty(this);
        }

        void PaintTile(Tile tile)
        {
            if (tile != null &&
                selectedRule != tile.rule &&
                selectedTilePrefab != null)
            {
                if (tile.obj != null)
                {
                    // Destroy existing obj
                    DestroyImmediate(tile.obj);
                }

                // Create the game object
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(selectedTilePrefab, transform);

                Undo.RegisterCreatedObjectUndo(newObj, "New Object Created");

                tile.obj = newObj;
                tile.rule = selectedRule;
                tile.prefab = selectedTilePrefab;

                tile.obj.transform.parent = transform;

                tile.SetTransform(tile.GetTargetLocalPosition(), selectedTilePrefab.transform.localRotation, Vector3.one);
            }
        }

        void EraseTile()
        {
            // Cache selected tile
            Tile selectedTile = SelectedTile;
            if (selectedTile != null && selectedTile.obj != null)
            {
                // Destroy the existing object
                DestroyImmediate(selectedTile.obj);
                selectedTile.obj = null;
                selectedTile.prefab = null;
                selectedTile.rule = null;
            }
            foreach (var tile in tilesInRadius)
            {
                EraseTile(tile);
            }
            EditorUtility.SetDirty(this);
        }

        void EraseTile(Tile tile)
        {
            if (tile != null && tile.obj != null)
            {
                // Destroy the existing object
                DestroyImmediate(tile.obj);
                tile.obj = null;
                tile.prefab = null;
                tile.rule = null;
            }
            EditorUtility.SetDirty(this);
        }
#endif

        public void OnBeforeSerialize()
        {
            tileKeys.Clear();
            tileValues.Clear();

            foreach (var kvp in tiles)
            {
                tileKeys.Add(kvp.Key);
                tileValues.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            tiles = new Dictionary<Vector3Int, Tile>();
            for (int i = 0; i < Mathf.Min(tileKeys.Count, tileValues.Count); i++)
            {
                tiles.Add(tileKeys[i], tileValues[i]);
            }
        }

        List<Tile> GetTilesInRadius(Tile startingTile)
        {
            debugCount = 0;
            pendingTiles = new Queue<Tile>();
            List<Tile> tilesInRadius = new List<Tile>();

            if (startingTile != null)
            {
                pendingTiles.Enqueue(startingTile);
                while (pendingTiles.Count > 0)
                {
                    GetTilesInRadiusRecursive(tilesInRadius, startingTile, pendingTiles.Dequeue());
                }
            }

            return tilesInRadius;
        }

        void GetTilesInRadiusRecursive(List<Tile> tilesInRadius, Tile startingTile, Tile currentTile)
        {
            if (startingTile != null && currentTile != null)
            {
                ManageAdjacentTiles(currentTile);
                foreach (var pair in currentTile.adjacentTiles)
                {
                    Vector3Int key = pair.Key;
                    Tile value = pair.Value;

                    if (Vector3.Distance(startingTile.indexPosition, value.indexPosition) < paintRadius &&
                        !tilesInRadius.Contains(value) && key.y == 0)
                    {
                        debugCount++;
                        tilesInRadius.Add(value);
                        pendingTiles.Enqueue(value);
                    }
                }
            }
        }

        List<Tile> GetTilesToFill(Tile startingTile)
        {
            debugCount = 0;
            pendingTiles = new Queue<Tile>();
            List<Tile> tilesToFill = new List<Tile>();

            if (startingTile != null)
            {
                pendingTiles.Enqueue(startingTile);
                while (pendingTiles.Count > 0)
                {
                    GetTilesToFillRecursive(startingTile.rule, tilesToFill, pendingTiles.Dequeue());
                }
            }

            return tilesToFill;
        }

        void GetTilesToFillRecursive(RuleTile startingRule, List<Tile> tilesToFill, Tile currentTile)
        {
            if (currentTile != null)
            {
                ManageAdjacentTiles(currentTile);
                foreach (var pair in currentTile.adjacentTiles)
                {
                    Vector3Int key = pair.Key;
                    Tile value = pair.Value;

                    if (key.magnitude != 1 || key.y != 0) continue;

                    if (value.rule == startingRule && !tilesToFill.Contains(value))
                    {
                        debugCount++;
                        tilesToFill.Add(value);
                        pendingTiles.Enqueue(value);
                    }
                }
            }
        }
    }
}