using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
                        // Using mesh instead of texture because I can't figure out how to draw a texture in the scene view properly
                        Mesh properMesh = slot.GetProperMesh(out Vector3 scale);
                        Gizmos.color = slot.GetProperColor();

                        if (properMesh != null)
                        {
                            Gizmos.DrawMesh(properMesh, kvp.Value.GetTargetPosition(), kvp.Value.parent.transform.rotation, scale);
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