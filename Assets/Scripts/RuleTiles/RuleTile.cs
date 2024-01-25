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

    void OnValidate()
    {
        foreach (var rule in rules)
        {
            for (int i = 0; i < rule.slots.Length; i++)
            {
                int copyI = i;

                if (i >= 13)
                {
                    copyI++;
                }

                int x = copyI / 9;
                int y = copyI % 9 / 3;
                int z = copyI % 9 % 3;

                rule.slots[i].direction = new Vector3Int(x - 1, y - 1, z - 1);
            }
        }
    }

    [System.Serializable]
    public class Rule
    {
        [System.Serializable]
        public enum Condition
        {
            None,
            ExistingTile,
            NoTile,
        }

        public Slot[] slots = new Slot[26];
        public GameObject newObj;

        public bool Evaluate(Tile tile)
        {
            foreach (var slot in slots)
            {
                if (!slot.Evaluate(tile))
                {
                    return false;
                }
            }
            return true;
        }

        public Slot GetSlot(Vector3Int direction)
        {
            foreach (var slot in slots)
            {
                if (slot.direction == direction)
                {
                    return slot;
                }
            }

            return null;
        }

        [System.Serializable]
        public class Slot
        {
            private static Texture2D arrowTexture;
            private static Texture2D xMarkTexture;

            public Vector3Int direction;

            private const string arrowTextureString = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAnFBMVEUAAADN/825/7kk/yQP/w/Z/9kA/wDG/8Yn/ycX/xfV/9UW/xbT/9Mi/yLR/9Eg/yDP/8/F/8Uf/x8d/x3M/8wc/xzK/8ob/xsa/xrH/8cY/xjJ/8kZ/xks/yzI/8jL/8sx/zFa/1pd/12K/4qN/422/7a4/7gh/yFN/03l/+VU/1Qe/x56/3qC/4LQ/9Bc/1yw/7C1/7Vm/2b///9aBBJYAAAAAXRSTlMAQObYZgAAAAFiS0dEMzfVfF4AAAAHdElNRQfoARMXOQ/OXcdQAAAAAW9yTlQBz6J3mgAAAMFJREFUOMuFktcSgjAQRYPoVeyFpiDYe///j1MBiWnLfcveM2c2mTAmxqoxOnad7htAkwRagEP1bYBW2F+gY+67AK3o5YBR0QdoxeAHGBRDlBlpgTEHtAoLoBWTf0CjcCFEvYgnAorChxR5i0AGnAqBvEWoAk6FAJjSAmDG+wigFbEeiCsEQKQK5km64Kcw75NysMwHq3LgC4I133pTjAK+gbcVn3a3z8Zu8QaHo+aDnD7FOfsHF2bIFbix+4MReb7e4/IPsuYooH8AAAAldEVYdGRhdGU6Y3JlYXRlADIwMjQtMDEtMTlUMjM6NTc6MTMrMDA6MDDvb7VsAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDI0LTAxLTE5VDIzOjU3OjEzKzAwOjAwnjIN0AAAAABJRU5ErkJggg==";
            private const string xMarkTextureString = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAS1BMVEX/////8PD/7u7/7+//7e3//Pz/+/v//f3/9PT/9vb/8/P/+fn/+Pj/OTn/MDD/HBz/AAD/GRn/Ojr/Nzf/CQn/CAj/Li7/GBj////nzx5BAAAADXRSTlMAAAAAAAAAAAAAAAAA7Uh4SAAAAAFiS0dEAIgFHUgAAAAHdElNRQfoARQADQqBo01kAAAAAW9yTlQBz6J3mgAAAMJJREFUOMuFk9sSgyAMRCOo2NrqUtvy/39atTrkNmNeIDlLwuhCtMWzIRNhqvsZMIoGyOc+Y40oedxqmXEgGH4oDg60hu+KF2AUsdYW6mpyTmEc/ZprBedpPyEVlgvFcHO4ULjcV3DuKSS3Cs21wnKid8UfB9PAG9wtb+SIoHnUlwwX3PeHjPaC+/7A2NspnD+IRq0Q57eC6sF592+ZxD0m7/vX2pfZ3vNHYQ/D80dhT8vzRzmT7Puj1GTp7e9Ny778AA5AIjOXvvPoAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDI0LTAxLTIwVDAwOjEzOjEwKzAwOjAwtfCBMAAAACV0RVh0ZGF0ZTptb2RpZnkAMjAyNC0wMS0yMFQwMDoxMzoxMCswMDowMMStOYwAAAAASUVORK5CYII=";
            
            public Condition condition = Condition.None;

            /// <summary>
            /// Draws a texture based on the condition type
            /// </summary>
            public void Draw(Rect position)
            {
                if (arrowTexture == null)
                {
                    arrowTexture = new Texture2D(2, 2);
                    arrowTexture.LoadImage(System.Convert.FromBase64String(arrowTextureString));
                }

                if (xMarkTexture == null)
                {
                    xMarkTexture = new Texture2D(2, 2);
                    xMarkTexture.LoadImage(System.Convert.FromBase64String(xMarkTextureString));
                }

                GUI.DrawTexture(position, xMarkTexture);
            }

            public bool Evaluate(Tile tile)
            {
                switch (condition)
                {
                    case Condition.ExistingTile:
                        return tile.GetAdjacentObject(direction) != null;
                    case Condition.NoTile:
                        return tile.GetAdjacentObject(direction) == null;
                    default:
                        return false;
                }
            }
        }
    }
}
