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
                private static Texture2D checkMarkTexture;
                private static Texture2D xMarkTexture;

                public Vector3Int direction;

                private const string checkMarkTextureString = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAQlBMVEVHcEwAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwAAfwCj5MZKAAAAFXRSTlMAFvrYOCoE5vIMbbB7S4lfzJO+oB5wJja8AAABhElEQVRYw6VX2baDIAysgLKI4jb//6uXIr1t7Tm1ZnwfSWYJ4Xajvral4D4lTx2/RgbfJT12BL6Z3cicbxbMDD5ZrATeR4e+IdpfFZaNUK8HJsJAZgK0keMHDdiBwFvAJcJ9Ga+i/PzgAMJAXcHLDbDjdcvhCQEKXgUOj1k8AVLBLw3jn0yA4fBK7ECjQRHQTgU/SQloFlAE+LXgxRHqxoJHL41QUHsD0ggMbm8gcAKKG6gCiBuoAsgbiIproBIotlBbCYTQQk1f8dIMVAfCnUzB1n8nAOtJiM1ovhJwboGk42eT/kHADwzmuC3DscyICwzm09z6XqixFf/bGNzuK0PwnxH42YP3A9UyHGfIuYRv14Zy83ZQ8FTC49zSZXfeJuByiqtsqs+mmB94jNdnP2wM/w1cWwWeygGiFHbpiawFXFxGn+LVL4gHgKyAFyKFBfyvATVFonU8KKqAVyL1Rk5S8T5cidTyB8lOJLGQFyIt8yTuchpn6knd9NZQP8hXxYWF8A93mShQW0ri4AAAAABJRU5ErkJggg==";
                private const string xMarkTextureString = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAS1BMVEX/////8PD/7u7/7+//7e3//Pz/+/v//f3/9PT/9vb/8/P/+fn/+Pj/OTn/MDD/HBz/AAD/GRn/Ojr/Nzf/CQn/CAj/Li7/GBj////nzx5BAAAADXRSTlMAAAAAAAAAAAAAAAAA7Uh4SAAAAAFiS0dEAIgFHUgAAAAHdElNRQfoARQADQqBo01kAAAAAW9yTlQBz6J3mgAAAMJJREFUOMuFk9sSgyAMRCOo2NrqUtvy/39atTrkNmNeIDlLwuhCtMWzIRNhqvsZMIoGyOc+Y40oedxqmXEgGH4oDg60hu+KF2AUsdYW6mpyTmEc/ZprBedpPyEVlgvFcHO4ULjcV3DuKSS3Cs21wnKid8UfB9PAG9wtb+SIoHnUlwwX3PeHjPaC+/7A2NspnD+IRq0Q57eC6sF592+ZxD0m7/vX2pfZ3vNHYQ/D80dhT8vzRzmT7Puj1GTp7e9Ny778AA5AIjOXvvPoAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDI0LTAxLTIwVDAwOjEzOjEwKzAwOjAwtfCBMAAAACV0RVh0ZGF0ZTptb2RpZnkAMjAyNC0wMS0yMFQwMDoxMzoxMCswMDowMMStOYwAAAAASUVORK5CYII=";
                
                Condition condition = Condition.None;

                /// <summary>
                /// Draws a texture based on the condition type
                /// </summary>
                public void Draw(Rect position)
                {
                    if (checkMarkTexture == null)
                    {
                        checkMarkTexture = new Texture2D(2, 2);
                        checkMarkTexture.LoadImage(System.Convert.FromBase64String(checkMarkTextureString));
                    }

                    if (xMarkTexture == null)
                    {
                        xMarkTexture = new Texture2D(2, 2);
                        xMarkTexture.LoadImage(System.Convert.FromBase64String(xMarkTextureString));
                    }

                    // Cache proper texture
                    Texture properTexture = GetProperTexture();

                    GUIContent buttonContent = new GUIContent(string.Empty, GetToolTip());

                    if (GUI.Button(position, buttonContent))
                    {
                        NextCondition();
                    }

                    Rect textureRect = position;

                    textureRect.size *= 0.75f;
                    textureRect.center = position.center;

                    if (properTexture != null)
                    {
                        GUI.DrawTexture(textureRect, properTexture);
                    }
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
                            return checkMarkTexture;
                        case Condition.NoTile:
                            return xMarkTexture;
                        default:
                            return null;
                    }
                }

                string GetToolTip()
                {
                    switch (condition)
                    {
                        case Condition.None:
                            return "This direction is currently ignored";
                        case Condition.ExistingTile:
                            return "There must be an existing tile in this direction";
                        case Condition.NoTile:
                            return "There must be no tile in this direction";
                        default:
                            return default;
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