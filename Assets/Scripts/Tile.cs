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

    GameObject[] adjacentObjects = new GameObject[26];

    public void SetAdjacentObject(Vector3Int directionIndex, GameObject newObj)
    {
        SetAdjacentObject(directionIndex.x, directionIndex.y, directionIndex.z, newObj);
    }

    public void SetAdjacentObject(int x, int y, int z, GameObject newObj)
    {
        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;
        Debug.Log(targetIndex + " from " + $"{x}, {y}, {z}");
        //adjacentObjects[targetIndex] = newObj;
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
