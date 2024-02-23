using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileGeneration
{
    public static class RuleTileVisualizer
    {
        public static void Display(Tile tile, int targetRuleIndex = 0)
        {
            if (tile == null || tile.rule == null || targetRuleIndex < 0 ||
                tile.rule.rules == null || tile.rule.rules.Count <= targetRuleIndex) return;

            Gizmos.color = Color.white;

            RuleTile.Rule rule = tile.rule.rules[targetRuleIndex];
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
                        Gizmos.DrawMesh(properMesh, kvp.Value.GetTargetPosition(),
                            Quaternion.LookRotation(-Camera.current.transform.forward, Camera.current.transform.up),
                            scale);
                    }
                }
            }
        }
    }
}