using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileGeneration
{
    public static class RuleTileVisualizer
    {
        public static bool checkForHover = false;
        public static RuleTile.Rule.Slot hoverSlot = null;
        public static bool overRuleButton = false;

        public static void Draw(Vector2 mousePosition, Tile tile, int targetRuleIndex = 0)
        {
            if (tile == null || tile.Rule == null || tile.Rule.rules == null ||
                targetRuleIndex < 0 || targetRuleIndex >= tile.Rule.rules.Count) return;

            Gizmos.color = Color.white;

            hoverSlot = null;

            RuleTile.Rule rule = tile.Rule.rules[targetRuleIndex];

            // Make a list of adjacent tiles, ordered from closest to furthest from the scene camera
            List<Tile> orderedAdjTiles = new List<Tile>();

            List<KeyValuePair<float, Vector3Int>> distances = new List<KeyValuePair<float, Vector3Int>>();

            foreach (var kvp in tile.adjacentTiles)
            {
                distances.Add(new KeyValuePair<float, Vector3Int>(Vector3.Distance(kvp.Value.GetTargetPosition(), Camera.current.transform.position), kvp.Key));
            }

            distances = distances.OrderBy(item => item.Key).ToList();
            distances.Reverse();

            Camera currentCamera = Camera.current;

            Quaternion camRotation = Quaternion.LookRotation(currentCamera.transform.forward, Vector3.up);
            Vector3 buttonScale = new Vector3(0.125f, 0.125f, 0.01f);

            foreach (var kvp in distances)
            {
                Tile adjTile = tile.adjacentTiles[kvp.Value];

                RuleTile.Rule.Slot slot = rule.GetSlot(kvp.Value);

                if (slot != null)
                {
                    // Using mesh instead of texture because I can't figure out how to draw a texture in the scene view properly
                    Mesh properMesh = slot.GetProperMesh();
                    Gizmos.color = Color.blue;

                    Vector3 position = adjTile.GetTargetPosition(false);

                    if (checkForHover && hoverSlot == null)
                    {
                        Vector2 slotScreenPos = currentCamera.WorldToScreenPoint(position);
                        if (Vector3.Distance(slotScreenPos, mousePosition) < 25)
                        {
                            Gizmos.color = Color.magenta;
                            hoverSlot = slot;
                        }
                    }

                    Gizmos.matrix = Matrix4x4.TRS(position, camRotation, buttonScale);
                    Gizmos.DrawCube(Vector3.zero, new Vector3(2.5f, 2.5f, 1f));

                    if (properMesh != null)
                    {
                        Gizmos.color = slot.GetProperColor();

                        Gizmos.DrawMesh(properMesh);
                    }
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }

            // Display the current rule being looked at on the top right of the screen

            Vector2 ruleViewportPosition = new Vector2(0.8f, 0.9f);
            Vector2 ruleScreenPosition = currentCamera.ViewportToScreenPoint(ruleViewportPosition);

            Vector3 rulePosition = currentCamera.ViewportToWorldPoint(new Vector3(ruleViewportPosition.x, ruleViewportPosition.y, 5f));
            Gizmos.matrix = Matrix4x4.TRS(rulePosition, camRotation, buttonScale);

            Gizmos.color = Color.blue;

            if (checkForHover)
            {
                if (Vector3.Distance(mousePosition, ruleScreenPosition) < 25)
                {
                    overRuleButton = true;
                    Gizmos.color = Color.magenta;
                }
                else
                {
                    overRuleButton = false;
                    Gizmos.color = Color.blue;
                }
            }

            Gizmos.DrawCube(Vector3.zero, new Vector3(2.5f, 2.5f, 1f));
        }
    }
}