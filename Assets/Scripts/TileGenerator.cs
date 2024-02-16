using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        [SerializeField] Vector3Int gridCount = Vector3Int.one;
        [SerializeField] Vector3 gridSize = Vector3.one * 25;

        public Vector3Int GridCount => gridCount;

        [SerializeField] List<Vector3Int> tileKeys = new List<Vector3Int>();
        [SerializeField] List<Tile> tileValues = new List<Tile>();

        Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

        List<Tile> tilesInRadius = new List<Tile>();

        public List<TilePalette> palettes = new List<TilePalette>();

        [SerializeReference] public TilePalette selectedPalette;
        [SerializeReference] public RuleTile selectedRule;
        public GameObject selectedTilePrefab;
        public Vector3Int selectedTileIndex = -Vector3Int.one;

        [SerializeField] int paintSize = 5;

        public int PaintSize
        {
            get
            {
                return paintSize;
            }
            set
            {
                paintSize = Mathf.Clamp(value, 0, 10);
            }
        }

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
            if (!Application.isPlaying)
            {
                OnValidate();
            }
        }

        void OnValidate()
        {
            Undo.undoRedoPerformed = new Undo.UndoRedoCallback(ClearUnusedObjects);

            // Grid count can never go below 1
            gridCount.x = System.Math.Max(gridCount.x, 1);
            gridCount.y = System.Math.Max(gridCount.y, 1);
            gridCount.z = System.Math.Max(gridCount.z, 1);

            gridSize.x = Mathf.Max(gridSize.x, 0.01f);
            gridSize.y = Mathf.Max(gridSize.y, 0.01f);
            gridSize.z = Mathf.Max(gridSize.z, 0.01f);

            for (int x = 0; x < gridCount.x; x++)
            {
                for (int y = 0; y < gridCount.y; y++)
                {
                    for (int z = 0; z < gridCount.z; z++)
                    {
                        ValidateTile(x, y, z);
                    }
                }
            }

            // Destroy objects in unused tiles
            foreach (var tileKey in tiles.Keys)
            {
                if (tileKey.x >= gridCount.x ||
                    tileKey.y >= gridCount.y ||
                    tileKey.z >= gridCount.z)
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
                if (tileKey.x >= gridCount.x ||
                    tileKey.y >= gridCount.y ||
                    tileKey.z >= gridCount.z)
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
                tile.obj.transform.parent = transform;
                tile.obj.transform.position = tile.GetTargetPosition();
                tile.SetScale(GetGridScaleRatio());

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
            tiles = new Dictionary<Vector3Int, Tile>();
            while (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            EditorUtility.SetDirty(this);
        }

        void OnDrawGizmosSelected()
        {
            ValidateTile(selectedTileIndex);

            tilesInRadius = GetTilesInRadius(SelectedTile);

            Gizmos.color = Color.red;
            for (int x = 0; x < gridCount.x; x++)
            {
                for (int z = 0; z < gridCount.z; z++)
                {
                    if (showAllYLevels)
                    {
                        for (int y = 0; y < gridCount.y; y++)
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
            Gizmos.matrix = Matrix4x4.Translate(transform.TransformPoint(GetGridScalePoint(x, y, z))) *
                        Matrix4x4.Rotate(transform.rotation) *
                        Matrix4x4.Scale(Vector3.Scale(GetGridScaleRatio(), transform.localScale));

            Gizmos.color = Color.red;

            Tile tile = tiles[new Vector3Int(x, y, z)];

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

        public Vector3 GetGridScaleRatio()
        {
            return new Vector3(gridSize.x / gridCount.x, gridSize.y / gridCount.y, gridSize.z / gridCount.z);
        }

        public Vector3 GetGridScalePoint(Vector3Int index)
        {
            // Cache grid scale ratio
            Vector3 gridScaleRatio = GetGridScaleRatio();

            float xPoint = index.x * gridSize.x / gridCount.x;
            float yPoint = index.y * gridSize.y / gridCount.y;
            float zPoint = index.z * gridSize.z / gridCount.z;

            xPoint -= (1 - gridScaleRatio.x) * 0.5f;
            yPoint -= (1 - gridScaleRatio.y) * 0.5f;
            zPoint -= (1 - gridScaleRatio.z) * 0.5f;

            return new Vector3(xPoint, yPoint, zPoint) - ((gridSize - Vector3.one) * 0.5f);
        }

        public Vector3 GetGridScalePoint(float x, float y, float z)
        {
            // Cache grid scale ratio
            Vector3 gridScaleRatio = GetGridScaleRatio();

            float xPoint = x * gridSize.x / gridCount.x;
            float yPoint = y * gridSize.y / gridCount.y;
            float zPoint = z * gridSize.z / gridCount.z;

            xPoint -= (1 - gridScaleRatio.x) * 0.5f;
            yPoint -= (1 - gridScaleRatio.y) * 0.5f;
            zPoint -= (1 - gridScaleRatio.z) * 0.5f;

            return new Vector3(xPoint, yPoint, zPoint) - ((gridSize - Vector3.one) * 0.5f);
        }

        public bool GetSelectedPoint(Ray ray, out Vector3 point)
        {
            point = Vector3.zero;

            Plane hPlane = new Plane(transform.up, transform.TransformPoint(GetGridScalePoint(0, selectedTileIndex.y, 0)));
            hPlane.distance -= transform.position.y;

            if (hPlane.Raycast(ray, out float distance))
            {
                // Cache grid scale ratio
                Vector3 gridScaleRatio = GetGridScaleRatio();

                point = ray.GetPoint(distance);

                Vector3 selectedPoint = transform.InverseTransformPoint(ray.GetPoint(distance));

                // Trying to convert xPoint, yPoint, and zPoint into what the indexes will be (excluding decimals)
                float xPoint = selectedPoint.x + gridSize.x * 0.5f;
                float yPoint = selectedPoint.y + gridSize.y * 0.5f;
                float zPoint = selectedPoint.z + gridSize.z * 0.5f;

                xPoint /= gridScaleRatio.x;
                yPoint /= gridScaleRatio.y;
                zPoint /= gridScaleRatio.z;

                int xIndex = (int)xPoint;
                int yIndex = (int)yPoint;
                int zIndex = (int)zPoint;

                if (xIndex < gridCount.x && xIndex >= 0
                    && yIndex < gridCount.y && yIndex >= 0
                    && zIndex < gridCount.z && zIndex >= 0)
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
                selectedRule != null && selectedTilePrefab != null)
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

                selectedTile.obj.transform.position = selectedTile.GetTargetPosition();
                selectedTile.obj.transform.localRotation = selectedTilePrefab.transform.localRotation;
                selectedTile.SetScale(GetGridScaleRatio());
            }

            foreach (var tile in tilesInRadius)
            {
                PaintTile(tile);
            }

            EditorUtility.SetDirty(this);
        }

        void PaintTile(Tile tile)
        {
            if (tile != null &&
                selectedRule != null && selectedTilePrefab != null)
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

                tile.obj.transform.position = tile.GetTargetPosition();
                tile.obj.transform.localRotation = selectedTilePrefab.transform.localRotation;
                tile.SetScale(GetGridScaleRatio());
            }
            EditorUtility.SetDirty(this);
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
            List<Tile> tilesInRadius = new List<Tile>();

            if (startingTile != null)
            {
                tilesInRadius = GetTilesInRadiusRecursive(startingTile);
            }

            return tilesInRadius;
        }

        List<Tile> GetTilesInRadiusRecursive(Tile currentTile)
        {
            List<Tile> tilesInRadius = new List<Tile>();

            int y = currentTile.indexPosition.y;

            for (int x = currentTile.indexPosition.x + 1 - paintSize; x < currentTile.indexPosition.x + paintSize; x++)
            {
                if (x >= 0 && x < gridCount.x)
                {
                    for (int z = currentTile.indexPosition.z + 1 - paintSize; z < currentTile.indexPosition.z + paintSize; z++)
                    {
                        if (z >= 0 && z < gridCount.z)
                        {
                            tilesInRadius.Add(tiles[new Vector3Int(x, y, z)]);
                        }
                    }
                }
            }
            return tilesInRadius;
        }
    }
}