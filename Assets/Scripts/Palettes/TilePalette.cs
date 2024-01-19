using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TilePalette : ScriptableObject
{
    [System.Serializable]
    public class ObjectSet
    {
        [SerializeField, HideInInspector]
        GameObject[] serializedObjects = new GameObject[81];

        public GameObject GetObject(int x, int y, int z)
        {
            return serializedObjects[x + (z * 3) + (y * 9)];
        }

        public void SetObject(int x, int y, int z, GameObject value)
        {
            serializedObjects[x + (z * 3) + (y * 9)] = value;
        }

        public GameObject[,,] objects = new GameObject[3, 3, 3];

        public GameObject GetTargetObject()
        {
            return GetObject(1, 1, 1);
        }
    }

    public List<ObjectSet> objectSets = new List<ObjectSet>();
}
