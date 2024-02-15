using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileGeneration
{
    [CreateAssetMenu()]
    public class RuleTile : ScriptableObject
    {
        public GameObject defaultGameObject = null;

        public GameObject GetObject(Tile tile)
        {
            foreach (var rule in rules)
            {
                GameObject evaluatedObj = rule.Evaluate(tile);
                if (evaluatedObj != null)
                {
                    return evaluatedObj;
                }
            }
            return defaultGameObject;
        }


        public List<Rule> rules = new List<Rule>();

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

            public Rule()
            {
                slots = new Slot[26];

                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = new Slot();
                    int copyI = i;

                    if (i >= 13)
                    {
                        copyI++;
                    }

                    int x = copyI / 9;
                    int y = copyI % 9 / 3;
                    int z = copyI % 9 % 3;

                    slots[i].direction = new Vector3Int(x - 1, y - 1, z - 1);
                }
            }

            public Slot[] slots = new Slot[26];
            public GameObject newObj;

            public GameObject Evaluate(Tile tile)
            {
                foreach (var slot in slots)
                {
                    if (!slot.Evaluate(tile))
                    {
                        return null;
                    }
                }
                return newObj;
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
                private static Texture2D circleTexture;

                public Vector3Int direction;

                private const string arrowTextureString = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAnFBMVEUAAADN/825/7kk/yQP/w/Z/9kA/wDG/8Yn/ycX/xfV/9UW/xbT/9Mi/yLR/9Eg/yDP/8/F/8Uf/x8d/x3M/8wc/xzK/8ob/xsa/xrH/8cY/xjJ/8kZ/xks/yzI/8jL/8sx/zFa/1pd/12K/4qN/422/7a4/7gh/yFN/03l/+VU/1Qe/x56/3qC/4LQ/9Bc/1yw/7C1/7Vm/2b///9aBBJYAAAAAXRSTlMAQObYZgAAAAFiS0dEMzfVfF4AAAAHdElNRQfoARMXOQ/OXcdQAAAAAW9yTlQBz6J3mgAAAMFJREFUOMuFktcSgjAQRYPoVeyFpiDYe///j1MBiWnLfcveM2c2mTAmxqoxOnad7htAkwRagEP1bYBW2F+gY+67AK3o5YBR0QdoxeAHGBRDlBlpgTEHtAoLoBWTf0CjcCFEvYgnAorChxR5i0AGnAqBvEWoAk6FAJjSAmDG+wigFbEeiCsEQKQK5km64Kcw75NysMwHq3LgC4I133pTjAK+gbcVn3a3z8Zu8QaHo+aDnD7FOfsHF2bIFbix+4MReb7e4/IPsuYooH8AAAAldEVYdGRhdGU6Y3JlYXRlADIwMjQtMDEtMTlUMjM6NTc6MTMrMDA6MDDvb7VsAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDI0LTAxLTE5VDIzOjU3OjEzKzAwOjAwnjIN0AAAAABJRU5ErkJggg==";
                private const string xMarkTextureString = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAS1BMVEX/////8PD/7u7/7+//7e3//Pz/+/v//f3/9PT/9vb/8/P/+fn/+Pj/OTn/MDD/HBz/AAD/GRn/Ojr/Nzf/CQn/CAj/Li7/GBj////nzx5BAAAADXRSTlMAAAAAAAAAAAAAAAAA7Uh4SAAAAAFiS0dEAIgFHUgAAAAHdElNRQfoARQADQqBo01kAAAAAW9yTlQBz6J3mgAAAMJJREFUOMuFk9sSgyAMRCOo2NrqUtvy/39atTrkNmNeIDlLwuhCtMWzIRNhqvsZMIoGyOc+Y40oedxqmXEgGH4oDg60hu+KF2AUsdYW6mpyTmEc/ZprBedpPyEVlgvFcHO4ULjcV3DuKSS3Cs21wnKid8UfB9PAG9wtb+SIoHnUlwwX3PeHjPaC+/7A2NspnD+IRq0Q57eC6sF592+ZxD0m7/vX2pfZ3vNHYQ/D80dhT8vzRzmT7Puj1GTp7e9Ny778AA5AIjOXvvPoAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDI0LTAxLTIwVDAwOjEzOjEwKzAwOjAwtfCBMAAAACV0RVh0ZGF0ZTptb2RpZnkAMjAyNC0wMS0yMFQwMDoxMzoxMCswMDowMMStOYwAAAAASUVORK5CYII=";
                private const string circleTextureString = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgAgMAAAAOFJJnAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACVBMVEUAAAAA/wD///9JuQheAAAAAXRSTlMAQObYZgAAAAFiS0dEAmYLfGQAAAAHdElNRQfoAgERIjkR7AAlAAAAAW9yTlQBz6J3mgAAAE9JREFUGNNjYGBgCA1lAAPR0NAQEM0YCgQOQAYriBEAkYHIhYIBVAlIESuEEYBgiEIYIVgYoVBAKQOPFZjugTsV7niEd+AehHsZHgjQYAEAkaNC4USx6YsAAAAldEVYdGRhdGU6Y3JlYXRlADIwMjQtMDItMDFUMTc6MzQ6NTUrMDA6MDDgTGIEAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDI0LTAyLTAxVDE3OjM0OjU1KzAwOjAwkRHauAAAAABJRU5ErkJggg==";

                public Condition condition = Condition.None;

                /// <summary>
                /// Draws a texture based on the condition type
                /// </summary>
                public void Draw(Rect position)
                {
                    if (circleTexture == null)
                    {
                        circleTexture = new Texture2D(2, 2);
                        circleTexture.LoadImage(System.Convert.FromBase64String(circleTextureString));
                    }

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

                    // Cache proper texture
                    Texture properTexture = GetProperTexture();

                    // Cache rotation angle
                    float rotationAngle = GetAngleFromDirection();

                    GUILayout.BeginVertical();

                    if (GUI.Button(position, string.Empty))
                    {
                        NextCondition();
                    }

                    Rect textureRect = position;

                    textureRect.size *= 0.75f;
                    textureRect.center = position.center;

                    GUIUtility.RotateAroundPivot(rotationAngle, textureRect.center);

                    if (properTexture != null)
                    {
                        GUI.DrawTexture(textureRect, properTexture);
                    }

                    // Rotate back to 0 degrees
                    GUIUtility.RotateAroundPivot(-rotationAngle, new Vector2(position.x + (position.width / 2f), position.y + (position.height / 2f)));

                    GUILayout.EndVertical();
                }

                public void NextCondition()
                {
                    condition++;
                    if (condition > (Condition)2)
                    {
                        condition = 0;
                    }
                }

                Texture GetProperTexture()
                {
                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            if (direction.x == 0 && direction.z == 0) return circleTexture;
                            return arrowTexture;
                        case Condition.NoTile:
                            return xMarkTexture;
                        default:
                            return null;
                    }
                }

                public float GetAngleFromDirection()
                {
                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            bool right = direction.x > 0;
                            bool left = direction.x < 0;
                            bool forward = direction.z > 0;
                            bool backward = direction.z < 0;

                            if (right && forward)
                            {
                                return 45f;
                            }
                            if (right && backward)
                            {
                                return 135f;
                            }
                            if (left && forward)
                            {
                                return -45f;
                            }
                            if (left && backward)
                            {
                                return -135f;
                            }

                            if (right)
                            {
                                return 90f;
                            }
                            if (left)
                            {
                                return -90f;
                            }
                            if (forward)
                            {
                                return 0f;
                            }
                            if (backward)
                            {
                                return 180f;
                            }

                            return 0f;
                        default:
                            return 0;
                    }
                }

                public bool Evaluate(Tile tile)
                {
                    if (tile == null) return false;

                    Tile adjacentTile = tile.GetAdjacentTile(direction);

                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            return adjacentTile != null && adjacentTile.obj != null;
                        case Condition.NoTile:
                            return adjacentTile == null || adjacentTile.obj == null;
                        default:
                            return true;
                    }
                }
            }
        }
    }
}