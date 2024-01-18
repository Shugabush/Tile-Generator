using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] Vector3Int gridCount = Vector3Int.one;
    [SerializeField] Vector3 gridSize = Vector3.one * 25;

    Tile[,,] tiles = new Tile[0,0,0];

    public List<TilePalette> palettes = new List<TilePalette>();

    public TilePalette selectedPalette;

    void OnValidate()
    {
        // Grid count can never go below 1
        gridCount.x = System.Math.Max(gridCount.x, 1);
        gridCount.y = System.Math.Max(gridCount.y, 1);
        gridCount.z = System.Math.Max(gridCount.z, 1);

        gridSize.x = Mathf.Max(gridSize.x, 0.01f);
        gridSize.y = Mathf.Max(gridSize.y, 0.01f);
        gridSize.z = Mathf.Max(gridSize.z, 0.01f);

        if (tiles.GetLength(0) != gridCount.x || tiles.GetLength(1) != gridCount.y || tiles.GetLength(2) != gridCount.z)
        {
            // Make a new tile array with a new size
            // and populate it with data from the original
            // Then set the original tile array to the new one

            Tile[,,] newTiles = new Tile[gridCount.x, gridCount.y, gridCount.z];
            for (int x = 0; x < Mathf.Min(tiles.GetLength(0), newTiles.GetLength(0)); x++)
            {
                for (int y = 0; y < Mathf.Min(tiles.GetLength(1), newTiles.GetLength(1)); y++)
                {
                    for (int z = 0; z < Mathf.Min(tiles.GetLength(2), newTiles.GetLength(2)); z++)
                    {
                        newTiles[x, y, z] = tiles[x, y, z];
                    }
                }
            }

            tiles = newTiles;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int z = 0; z < tiles.GetLength(2); z++)
                {
                    Gizmos.matrix = Matrix4x4.Translate(GetGridScalePoint(x, y, z) - ((gridSize - Vector3.one) * 0.5f));

                    Gizmos.DrawWireCube(transform.position, GetGridScaleRatio());
                }
            }
        }

        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.DrawWireCube(transform.position, gridSize);
    }

    Vector3 GetGridScaleRatio()
    {
        return new Vector3(gridSize.x / gridCount.x, gridSize.y / gridCount.y, gridSize.z / gridCount.z);
    }

    Vector3 GetGridScaleRatioInverse()
    {
        return new Vector3(gridCount.x / gridSize.x, gridCount.y / gridSize.y, gridCount.z / gridSize.z);
    }

    Vector3 GetGridScalePoint(float x, float y, float z)
    {
        // Cache grid scale ratio
        Vector3 gridScaleRatio = GetGridScaleRatio();

        float xPoint = x * gridSize.x / gridCount.x;
        float yPoint = y * gridSize.y / gridCount.y;
        float zPoint = z * gridSize.z / gridCount.z;

        xPoint -= (1 - gridScaleRatio.x) * 0.5f;
        yPoint -= (1 - gridScaleRatio.y) * 0.5f;
        zPoint -= (1 - gridScaleRatio.z) * 0.5f;

        return new Vector3(xPoint, yPoint, zPoint);
    }
}
