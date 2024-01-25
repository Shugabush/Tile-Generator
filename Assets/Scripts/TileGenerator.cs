using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] Vector3Int gridCount = Vector3Int.one;
    [SerializeField] Vector3 gridSize = Vector3.one * 25;

    public Vector3Int GridCount => gridCount;

    [SerializeField] List<Vector3Int> tileKeys = new List<Vector3Int>();
    [SerializeField] List<Tile> tileValues = new List<Tile>();

    Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

    public List<TilePalette> palettes = new List<TilePalette>();

    public TilePalette selectedPalette;
    public GameObject selectedTilePrefab;
    public Vector3Int selectedTileIndex = -Vector3Int.one;

    public bool shouldPaint = true;
    public bool showAllYLevels = true;

    public Tile SelectedTile
    {
        get
        {
            if (tiles.TryGetValue(selectedTileIndex, out Tile tile))
            {
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
                if (tile.obj != null)
                {
                    tile.obj.SetActive(true);
                }
            }
        }
    }

    void OnValidate()
    {
        // Grid count can never go below 1
        gridCount.x = System.Math.Max(gridCount.x, 1);
        gridCount.y = System.Math.Max(gridCount.y, 1);
        gridCount.z = System.Math.Max(gridCount.z, 1);

        gridSize.x = Mathf.Max(gridSize.x, 0.01f);
        gridSize.y = Mathf.Max(gridSize.y, 0.01f);
        gridSize.z = Mathf.Max(gridSize.z, 0.01f);

        // Remove unnecessary tiles
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
            // Before removing the tile at the key location,
            // Destroy any game object that exists there
            Tile tile = tiles[key];
            if (tile.obj != null)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () => DestroyImmediate(tile.obj);
#endif
            }

            tiles.Remove(key);
        }

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
    }

    void ValidateTile(int x, int y, int z)
    {
        Vector3Int vectorIndex = new Vector3Int(x, y, z);

        if (!tiles.TryGetValue(vectorIndex, out Tile tile))
        {
            tile = new Tile(this, vectorIndex);
            tiles.Add(vectorIndex, tile);
        }

        if (tile.obj != null)
        {
            tile.obj.transform.position = transform.TransformPoint(GetGridScalePoint(vectorIndex));
            tile.obj.transform.localScale = GetGridScaleRatio();

            if (showAllYLevels)
            {
                tile.obj.SetActive(true);
                tile.obj.hideFlags = hideFlags;
            }
            else
            {
                tile.obj.SetActive(y == selectedTileIndex.y);
            }
            ManageAdjacentTiles(tile);
        }
    }

    void ManageAdjacentTiles(Tile targetTile)
    {
        int x = targetTile.indexPosition.x;
        int y = targetTile.indexPosition.y;
        int z = targetTile.indexPosition.z;

        bool adjLeft = x > 0;
        bool adjRight = x < gridCount.x;

        bool adjDown = y > 0;
        bool adjUp = y < gridCount.y;

        bool adjBackward = z > 0;
        bool adjForward = z < gridCount.y;

        if (adjLeft)
        {
            targetTile.SetAdjacentTile(Vector3Int.left, tiles[new Vector3Int(x - 1, y, z)]);
        }
        if (adjRight)
        {
            targetTile.SetAdjacentTile(Vector3Int.right, tiles[new Vector3Int(x + 1, y, z)]);
        }

        if (adjDown)
        {
            targetTile.SetAdjacentTile(Vector3Int.down, tiles[new Vector3Int(x, y - 1, z)]);
        }
        if (adjUp)
        {
            targetTile.SetAdjacentTile(Vector3Int.up, tiles[new Vector3Int(x, y + 1, z)]);
        }

        if (adjBackward)
        {
            targetTile.SetAdjacentTile(Vector3Int.back, tiles[new Vector3Int(x, y, z - 1)]);
        }
        if (adjForward)
        {
            targetTile.SetAdjacentTile(Vector3Int.forward, tiles[new Vector3Int(x, y, z + 1)]);
        }

        if (adjLeft && adjDown)
        {
            targetTile.SetAdjacentTile(Vector3Int.left + Vector3Int.down, tiles[new Vector3Int(x, y, z + 1)]);
        }
    }

    void ManageSingleAdjacentTile(Tile targetTile, Vector3Int direction)
    {

    }

    void OnDrawGizmosSelected()
    {
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

        if (selectedTileIndex.x == x && selectedTileIndex.y == y && selectedTileIndex.z == z)
        {
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
        else
        {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }

    Vector3 GetGridScaleRatio()
    {
        return new Vector3(gridSize.x / gridCount.x, gridSize.y / gridCount.y, gridSize.z / gridCount.z);
    }

    Vector3 GetGridScalePoint(Vector3Int index)
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

    Vector3 GetGridScalePoint(float x, float y, float z)
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

    public bool GetSelectedPoint(Ray ray)
    {
        Plane hPlane = new Plane(transform.up, transform.TransformPoint(GetGridScalePoint(0, selectedTileIndex.y, 0)));
        hPlane.distance -= transform.position.y;

        if (hPlane.Raycast(ray, out float distance))
        {
            // Cache grid scale ratio
            Vector3 gridScaleRatio = GetGridScaleRatio();

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
        if (selectedTile != null && selectedTile.GetPrefab() != selectedTilePrefab &&
            selectedPalette != null && selectedTilePrefab != null)
        {
            if (selectedTile.obj != null)
            {
                // Destroy existing obj
                DestroyImmediate(selectedTile.obj);
            }

            // Create the game object
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(selectedTilePrefab, transform);
            newObj.transform.position = transform.TransformPoint(GetGridScalePoint(selectedTile.indexPosition));
            newObj.transform.localScale = GetGridScaleRatio();

            selectedTile.obj = newObj;
            selectedTile.prefab = selectedTilePrefab;
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
}
