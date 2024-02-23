using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace TileGeneration
{
    public static class RuleTileVisualizer
    {
        public static bool checkForHover = false;
        public static bool hoveringOverLabel = false;

        public static void Display(Vector2 mousePosition, Tile tile, int targetRuleIndex = 0)
        {
            if (tile == null || tile.Rule == null || targetRuleIndex < 0 ||
                tile.Rule.rules == null || tile.Rule.rules.Count <= targetRuleIndex) return;

            Gizmos.color = Color.white;

            hoveringOverLabel = false;

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

            foreach (var kvp in distances)
            {
                Tile adjTile = tile.adjacentTiles[kvp.Value];

                RuleTile.Rule.Slot slot = rule.GetSlot(kvp.Value);
                if (slot != null)
                {
                    // Using mesh instead of texture because I can't figure out how to draw a texture in the scene view properly
                    Mesh properMesh = slot.GetProperMesh(out Vector3 scale);
                    Gizmos.color = slot.GetProperColor();

                    Camera currentCamera = Camera.current;
                    Vector3 position = adjTile.GetTargetPosition(false);

                    if (checkForHover && !hoveringOverLabel)
                    {
                        Vector2 screenPosition = currentCamera.WorldToScreenPoint(position);
                        if (Vector3.Distance(screenPosition, mousePosition) < 25)
                        {
                            Gizmos.color = Color.magenta;
                            hoveringOverLabel = true;
                        }
                    }

                    Quaternion rotation = Quaternion.LookRotation(currentCamera.transform.position - position, currentCamera.transform.up);

                    if (properMesh != null)
                    {
                        Gizmos.matrix = Matrix4x4.TRS(position, rotation, scale);

                        Color meshColor = Gizmos.color;

                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(Vector3.zero, Vector3.one * 2.5f);

                        Gizmos.color = meshColor;

                        Gizmos.DrawMesh(properMesh);
                    }
                }
            }
        }
    }
}