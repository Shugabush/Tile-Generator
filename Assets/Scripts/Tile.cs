using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public TileGenerator parent;

    public Vector3Int indexPosition;

    // Prefab for this tile (we will be comparing this when deciding if we need to draw a new one or not)
    public GameObject prefab;
    // Game Object that is occupying this tile slot (if any)
    public GameObject obj;

    Tile[] adjacentTiles = new Tile[26];

    public void SetAdjacentTile(Vector3Int directionIndex, Tile newTile)
    {
        SetAdjacentTile(directionIndex.x, directionIndex.y, directionIndex.z, newTile);
    }

    public void SetAdjacentTile(int x, int y, int z, Tile newTile)
    {
        if (adjacentTiles == null || adjacentTiles.Length == 0)
        {
            adjacentTiles = new Tile[26];
        }

        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;
        if (adjacentTiles[targetIndex] != newTile)
        {
            adjacentTiles[targetIndex] = newTile;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(parent);
            }
#endif
        }
    }

    public Tile GetAdjacentTile(Vector3Int directionIndex)
    {
        return GetAdjacentTile(directionIndex.x, directionIndex.y, directionIndex.z);
    }

    public Tile GetAdjacentTile(int x, int y, int z)
    {
        if (adjacentTiles == null || adjacentTiles.Length == 0)
        {
            adjacentTiles = new Tile[26];
        }

        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;
        return adjacentTiles[targetIndex];
    }

    public GameObject GetPrefab()
    {
        if (obj == null)
        {
            prefab = null;
        }
        return prefab;
    }

    public Tile(TileGenerator parent, Vector3Int indexPosition)
    {
        this.parent = parent;
        this.indexPosition = indexPosition;
    }
}
