using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileGeneration
{
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
        public Dictionary<Vector3Int, Tile> adjacentTiles = new Dictionary<Vector3Int, Tile>();

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
        }

        public Tile GetAdjacentTile(Vector3Int directionIndex)
        {
            if (adjacentTiles.ContainsKey(directionIndex))
            {
                return adjacentTiles[directionIndex];
            }
            return null;
        }

#if UNITY_EDITOR
        public void EnsurePrefabIsInstantiated()
        {
            if (rule == null || prefab == null || obj != null) return;

            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.SetParent(parent.transform);
            obj.transform.position = GetTargetPosition();
            obj.transform.localRotation = prefab.transform.localRotation;
            SetScale(parent.GetGridScaleRatio());
        }

        public void FixObject()
        {
            if (rule == null || prefab == null || obj == null)
            {
                prefab = null;
                if (obj != null)
                {
                    EditorApplication.delayCall += () => Object.DestroyImmediate(obj);
                    obj = null;
                }

                return;
            }

            GameObject rulePrefab = rule.GetObject(this);
            if (rulePrefab != null && rulePrefab != PrefabUtility.GetCorrespondingObjectFromSource(obj))
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
                obj.transform.localRotation = rulePrefab.transform.localRotation;
                SetScale(parent.GetGridScaleRatio());
                prefab = rulePrefab;
            }
        }
#endif

        public Vector3 GetTargetLocalPosition()
        {
            return parent.GetGridScalePoint(indexPosition);
        }

        public Vector3 GetTargetPosition()
        {
            return parent.transform.TransformPoint(GetTargetLocalPosition());
        }

        public void SetScale(Vector3 baseScale)
        {
            if (obj == null) return;

            Renderer rend = obj.GetComponentInChildren<Renderer>();

            obj.transform.localScale = baseScale;

            Vector3 size = rend.localBounds.size;

            size.x = Mathf.Max(0.025f, size.x);
            size.y = Mathf.Max(0.025f, size.y);
            size.z = Mathf.Max(0.025f, size.z);

            Vector3 scaledSize = new Vector3(baseScale.x / size.x, baseScale.y / size.y, baseScale.z / size.z);

            scaledSize.x = Mathf.Max(0.025f, scaledSize.x);
            scaledSize.y = Mathf.Max(0.025f, scaledSize.y);
            scaledSize.z = Mathf.Max(0.025f, scaledSize.z);

            obj.transform.localScale = scaledSize;
        }

        public Tile(TileGenerator parent, Vector3Int indexPosition)
        {
            this.parent = parent;
            this.indexPosition = indexPosition;
        }
    }
}