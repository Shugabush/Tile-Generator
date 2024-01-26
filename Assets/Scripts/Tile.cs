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
    public Tile[] adjacentTiles = new Tile[26];

    public void SetAdjacentTile(Vector3Int directionIndex, Tile newTile)
    {
        Vector3 positionOffset = newTile.GetTargetLocalPosition() - GetTargetLocalPosition();
        directionIndex.x = System.Math.Sign(positionOffset.x);
        directionIndex.y = System.Math.Sign(positionOffset.y);
        directionIndex.z = System.Math.Sign(positionOffset.z);

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

        if (targetIndex >= 0 && targetIndex < adjacentTiles.Length)
        {
            if (adjacentTiles[targetIndex] != newTile)
            {
                adjacentTiles[targetIndex] = newTile;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(parent);
                }
#endif
            }
        }
    }

#if UNITY_EDITOR
    public void FixObject()
    {
        if (rule == null) return;

        GameObject rulePrefab = rule.GetObject(this);
        Debug.Log(rulePrefab == rule.defaultGameObject);
        if (rulePrefab != prefab)
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

    public void AdjacentTileIndexDebug(int x, int y, int z)
    {
        int targetIndex = (x + 1) * 9;
        targetIndex += (y + 1) * 3;
        targetIndex += z + 1;
        Debug.Log(new Vector3Int(x, y, z).ToString() + " " + targetIndex.ToString());
        Debug.DrawRay(GetTargetPosition(), Vector3.up, Color.black, 100);
        Debug.DrawRay(adjacentTiles[targetIndex].GetTargetPosition(), Vector3.up, Color.gray, 100);
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
