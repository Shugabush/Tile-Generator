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

        [SerializeField, HideInInspector]
        GameObject[] serializedObjects = new GameObject[1];

        [SerializeField, HideInInspector]
        Vector3Int objectsLength = Vector3Int.one;

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

        public void Save()
        {
            objectsLength = new Vector3Int(objects.GetLength(0), objects.GetLength(1), objects.GetLength(2));
            serializedObjects = new GameObject[objectsLength.x * objectsLength.y * objectsLength.z];

            for (int x = 0; x < objectsLength.x; x++)
            {
                for (int y = 0; y < objectsLength.y; y++)
                {
                    for (int z = 0; z < objectsLength.z; z++)
                    {
                        int targetIndex = x + (z * objectsLength.x) + (y * objectsLength.x * objectsLength.z);
                        serializedObjects[targetIndex] = objects[x, y, z];
                    }
                }
            }
        }

        public void Load()
        {
            objects = new GameObject[objectsLength.x, objectsLength.y, objectsLength.z];

            for (int x = 0; x < objectsLength.x; x++)
            {
                for (int y = 0; y < objectsLength.y; y++)
                {
                    for (int z = 0; z < objectsLength.z; z++)
                    {
                        int targetIndex = x + (z * objectsLength.x) + (y * objectsLength.x * objectsLength.z);
                        objects[x, y, z] = serializedObjects[targetIndex];
                    }
                }
            }
        }
    }

    public List<ObjectSet> objectSets = new List<ObjectSet>();
}
