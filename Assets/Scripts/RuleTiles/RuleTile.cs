using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RuleTile : ScriptableObject
{
    public GameObject defaultGameObject = null;

    public GameObject GetObject()
    {
        return defaultGameObject;
    }

    public Rule[] rules = new Rule[0];

    [System.Serializable]
    public class Rule
    {
        public Slot[] slots = new Slot[9];

        [System.Serializable]
        public class Slot
        {
            [System.Serializable]
            public enum Condition
            {
                None,
                ExistingTile,
                NoTile,
            }
            public Condition condition = Condition.None;
        }
    }
}
