using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TilePalette : ScriptableObject
{
    [System.Serializable]
    public class ObjectSet
    {
        public GameObject prefab;

        public GameObject[,,] objects = new GameObject[1, 1, 1];

        public bool ChangeObjectDimensions(Vector3Int indexCount)
        {
            if (indexCount.x == objects.GetLength(0) && indexCount.y == objects.GetLength(1) && indexCount.z == objects.GetLength(2))
            {
                return false;
            }

            GameObject[,,] newObjects = new GameObject[indexCount.x, indexCount.y, indexCount.z];

            for (int x = 0; x < System.Math.Max(indexCount.x, objects.GetLength(0)); x++)
            {
                for (int y = 0; y < System.Math.Max(indexCount.y, objects.GetLength(1)); y++)
                {
                    for (int z = 0; z < System.Math.Max(indexCount.z, objects.GetLength(2)); z++)
                    {
                        newObjects[x, y, z] = objects[x, y, z];
                    }
                }
            }
            return true;
        }

        public bool ChangeObjectDimensions(int xCount, int yCount, int zCount)
        {
            if (xCount == objects.GetLength(0) && yCount == objects.GetLength(1) && zCount == objects.GetLength(2))
            {
                return false;
            }

            GameObject[,,] newObjects = new GameObject[xCount, yCount, zCount];

            for (int x = 0; x < System.Math.Min(xCount, objects.GetLength(0)); x++)
            {
                for (int y = 0; y < System.Math.Min(yCount, objects.GetLength(1)); y++)
                {
                    for (int z = 0; z < System.Math.Min(zCount, objects.GetLength(2)); z++)
                    {
                        newObjects[x, y, z] = objects[x, y, z];
                    }
                }
            }

            objects = newObjects;
            return true;
        }
    }

    public List<ObjectSet> objectSets = new List<ObjectSet>();
}
