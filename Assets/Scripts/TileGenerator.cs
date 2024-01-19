using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] Vector3Int gridCount = Vector3Int.one;
    [SerializeField] Vector3 gridSize = Vector3.one * 25;

    Vector3Int tileCount = Vector3Int.one;

    [SerializeField] List<Vector3Int> tileKeys = new List<Vector3Int>();
    [SerializeField] List<Tile> tileValues = new List<Tile>();

    Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

    public List<TilePalette> palettes = new List<TilePalette>();

    public TilePalette selectedPalette;
    public GameObject selectedTilePrefab;
    public Vector3Int selectedTileIndex = -Vector3Int.one;

    public bool shouldPaint = true;

    Tile SelectedTile
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
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(tile.obj);
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
                    Vector3Int vectorIndex = new Vector3Int(x, y, z);

                    if (tiles.TryGetValue(vectorIndex, out Tile tile))
                    {
                        
                    }
                    else
                    {
                        tile = new Tile(this, vectorIndex);
                        tiles.Add(vectorIndex, tile);
                    }

                    if (tile.obj != null)
                    {
                        tile.obj.transform.position = GetGridScalePoint(vectorIndex);
                        tile.obj.transform.localScale = GetGridScaleRatio();
                    }
                }
            }
        }

        tileCount = gridCount;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        for (int x = 0; x < tileCount.x; x++)
        {
            for (int y = 0; y < tileCount.y; y++)
            {
                for (int z = 0; z < tileCount.z; z++)
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
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    Vector3 GetGridScaleRatio()
    {
        return new Vector3(gridSize.x / gridCount.x, gridSize.y / gridCount.y, gridSize.z / gridCount.z);
    }

    Vector3 GetGridScaleRatioInverse()
    {
        return new Vector3(gridCount.x / gridSize.x, gridCount.y / gridSize.y, gridCount.z / gridSize.z);
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

    public void GetSelectedPoint(Ray ray)
    {
        Plane hPlane = new Plane(transform.up, Vector3.zero);
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

            if (xIndex < tileCount.x && xIndex >= 0
                && yIndex < tileCount.y && yIndex >= 0
                && zIndex < tileCount.z && zIndex >= 0)
            {
                selectedTileIndex = new Vector3Int(xIndex, yIndex, zIndex);
                return;
            }
        }
        selectedTileIndex = -Vector3Int.one;
    }

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
            // Create the game object
            GameObject newObj = Instantiate(selectedTilePrefab, transform);
            newObj.transform.position = GetGridScalePoint(selectedTile.indexPosition);

            selectedTile.obj = newObj;
            selectedTile.prefab = selectedTilePrefab;
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
        }
    }

    bool SelectedTileIsValid()
    {
        return selectedTileIndex.x >= 0 && selectedTileIndex.x < tileCount.x &&
               selectedTileIndex.y >= 0 && selectedTileIndex.y < tileCount.y &&
               selectedTileIndex.z >= 0 && selectedTileIndex.z < tileCount.z;
    }

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

    void OnGUI()
    {
        foreach (var kvp in tiles)
        {
            GUILayout.Label("Key: " + kvp.Key + " value: " + kvp.Value);
        }
    }
}
