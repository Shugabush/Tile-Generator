using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileGeneration
{
    public static class RuleTileVisualizer
    {
        public static void Display(Tile tile)
        {
            if (tile == null || tile.rule == null) return;

            Gizmos.color = Color.white;

            foreach (var rule in tile.rule.rules)
            {
                foreach (var kvp in tile.adjacentTiles)
                {
                    RuleTile.Rule.Slot slot = rule.GetSlot(kvp.Key);
                    if (slot != null)
                    {
                        Texture properTexture = slot.GetProperTexture();

                        //Gizmos.DrawCube(kvp.Value.GetTargetPosition(), kvp.Value.parent.transform.localScale);
                        if (properTexture != null)
                        {
                            Gizmos.matrix = Matrix4x4.Translate(kvp.Value.GetTargetPosition());
                            Gizmos.DrawGUITexture(new Rect(Vector2.zero, Vector2.one), properTexture);
                            //GUI.DrawTexture(new Rect(Camera.current.WorldToScreenPoint(kvp.Value.GetTargetPosition()), Vector2.one), properTexture);
                        }
                    }
                }
            }
        }

        public static Vector2 WorldToGUIPoint(this Camera cam, Vector3 objPosition)
        {
            var guiPosition = cam.WorldToScreenPoint(objPosition);
            // Y axis coordinate in screen is reversed relative to world Y coordinate
            guiPosition.y = Screen.height - guiPosition.y;

            return guiPosition;
        }
    }
}