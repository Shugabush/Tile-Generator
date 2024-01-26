using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Tile
{
    public TileGenerator parent;

    public Vector3Int indexPosition;

    public RuleTile rule;

    // Prefab for this tile (we will be comparing this when deciding if we need to draw a new one or not)
    public GameObject prefab;
    // Game Object that is occupying this tile slot (if any)
    public GameObject obj;

    [SerializeReference]
    public Tile[] adjacentTiles2 = new Tile[26];

    Dictionary<Vector3Int, Tile> adjacentTiles = new Dictionary<Vector3Int, Tile>();

    public void SetAdjacentTile(Vector3Int directionIndex, Tile newTile)
    {
        if (adjacentTiles == null)
        {
            adjacentTiles = new Dictionary<Vector3Int, Tile>();
        }

        if (adjacentTiles.ContainsKey(directionIndex))
        {
            adjacentTiles[directionIndex] = newTile;
        }
        else
        {
            adjacentTiles.Add(directionIndex, newTile);
        }

        //SetAdjacentTile(directionIndex.x, directionIndex.y, directionIndex.z, newTile);
    }

    public void SetAdjacentTile(int x, int y, int z, Tile newTile)
    {
        if (adjacentTiles2 == null || adjacentTiles2.Length == 0)
        {
            adjacentTiles2 = new Tile[26];
        }

        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;

        if (targetIndex >= 0 && targetIndex < adjacentTiles2.Length)
        {
            if (adjacentTiles2[targetIndex] != newTile)
            {
                adjacentTiles2[targetIndex] = newTile;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(parent);
                }
#endif
            }
        }
    }

    public Tile GetAdjacentTile(Vector3Int directionIndex)
    {
        if (adjacentTiles.ContainsKey(directionIndex))
        {
            return adjacentTiles[directionIndex];
        }
        return null;

        //return GetAdjacentTile(directionIndex.x, directionIndex.y, directionIndex.z);
    }

    public Tile GetAdjacentTile(int x, int y, int z)
    {
        if (adjacentTiles2 == null || adjacentTiles2.Length == 0)
        {
            adjacentTiles2 = new Tile[26];
        }

        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;
        return adjacentTiles2[targetIndex];
    }

#if UNITY_EDITOR
    public void FixObject()
    {
        if (rule == null) return;

        GameObject rulePrefab = rule.GetObject(this);
        if (rulePrefab != PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj))
        {
            // Destroy obj and re-instantiate the new prefab
            GameObject objToDestroy = obj;
            if (objToDestroy != null)
            {
                EditorApplication.delayCall += () => Object.DestroyImmediate(objToDestroy);
            }

            obj = (GameObject)PrefabUtility.InstantiatePrefab(rulePrefab);
            obj.transform.parent = parent.transform;
            obj.transform.position = GetTargetPosition();
            obj.transform.localScale = parent.GetGridScaleRatio();
            prefab = rulePrefab;
        }
    }
#endif

    public void AdjacentTileIndexDebug(int x, int y, int z)
    {
        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;
        Debug.Log(new Vector3Int(x, y, z).ToString() + " " + targetIndex.ToString());
        Debug.DrawRay(GetTargetPosition(), Vector3.up, Color.black, 100);
        Debug.DrawRay(adjacentTiles2[targetIndex].GetTargetPosition(), Vector3.up, Color.gray, 100);
    }

    public Vector3 GetTargetLocalPosition()
    {
        return parent.GetGridScalePoint(indexPosition);
    }

    public Vector3 GetTargetPosition()
    {
        return parent.transform.TransformPoint(GetTargetLocalPosition());
    }

    public Tile(TileGenerator parent, Vector3Int indexPosition)
    {
        this.parent = parent;
        this.indexPosition = indexPosition;
    }
}
