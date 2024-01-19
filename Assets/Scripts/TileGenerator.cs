using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] Vector3Int gridCount = Vector3Int.one;
    [SerializeField] Vector3 gridSize = Vector3.one * 25;

    Tile[,,] tiles = new Tile[0,0,0];

    public List<TilePalette> palettes = new List<TilePalette>();

    public TilePalette selectedPalette;
    public GameObject selectedTilePrefab;
    public Vector3Int selectedTileIndex = -Vector3Int.one;

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
            for (int x = 0; x < newTiles.GetLength(0); x++)
            {
                for (int y = 0; y < newTiles.GetLength(1); y++)
                {
                    for (int z = 0; z < newTiles.GetLength(2); z++)
                    {
                        if (IndexIsValid(x, y, z))
                        {
                            newTiles[x, y, z] = tiles[x, y, z];
                            newTiles[x, y, z].indexPosition = new Vector3Int(x, y, z);
                        }
                        else
                        {
                            newTiles[x, y, z] = new Tile(this, new Vector3Int(x, y, z));
                        }
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
                    Gizmos.matrix = Matrix4x4.Translate(transform.TransformPoint(GetGridScalePoint(x, y, z))) * 
                        Matrix4x4.Rotate(transform.rotation) *
                        Matrix4x4.Scale(Vector3.Scale(GetGridScaleRatio(), transform.localScale));

                    if (selectedTileIndex.x == x && selectedTileIndex.y == y && selectedTileIndex.z == z)
                    {
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    }
                    else
                    {
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    }
                }
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
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

        return new Vector3(xPoint, yPoint, zPoint) - ((gridSize - Vector3.one) * 0.5f);
    }

    public void GetSelectedPoint(Ray ray)
    {
        Plane hPlane = new Plane(transform.up, Vector3.zero);
        hPlane.distance -= transform.position.y;

        if (hPlane.Raycast(ray, out float distance))
        {
            // Cache grid scale ratio
            Vector3 gridScaleRatio = GetGridScaleRatio();

            Vector3 selectedPoint = transform.InverseTransformPoint(ray.GetPoint(distance));

            // Trying to convert xPoint, yPoint, and zPoint into what the indexes will be (excluding decimals)
            float xPoint = selectedPoint.x + gridSize.x * 0.5f;
            float yPoint = selectedPoint.y + gridSize.y * 0.5f;
            float zPoint = selectedPoint.z + gridSize.z * 0.5f;

            xPoint /= gridScaleRatio.x;
            yPoint /= gridScaleRatio.y;
            zPoint /= gridScaleRatio.z;

            int xIndex = (int)xPoint;
            int yIndex = (int)yPoint;
            int zIndex = (int)zPoint;

            if (xIndex < tiles.GetLength(0) && xIndex >= 0
                && yIndex < tiles.GetLength(1) && yIndex >= 0
                && zIndex < tiles.GetLength(2) && zIndex >= 0)
            {
                selectedTileIndex = new Vector3Int(xIndex, yIndex, zIndex);
                return;
            }
        }
        selectedTileIndex = -Vector3Int.one;
    }

    public void PaintTile()
    {
        if (SelectedTileIsValid() && selectedPalette != null && selectedTilePrefab != null)
        {
            // Create the game object
        }
    }

    bool IndexIsValid(Vector3Int index)
    {
        return index.x >= 0 && index.x < tiles.GetLength(0) &&
               index.y >= 0 && index.y < tiles.GetLength(1) &&
               index.z >= 0 && index.z < tiles.GetLength(2);
    }

    bool IndexIsValid(int x, int y, int z)
    {
        return x >= 0 && x < tiles.GetLength(0) &&
               y >= 0 && y < tiles.GetLength(1) &&
               z >= 0 && z < tiles.GetLength(2);
    }

    bool SelectedTileIsValid()
    {
        return selectedTileIndex.x >= 0 && selectedTileIndex.x < tiles.GetLength(0) &&
               selectedTileIndex.y >= 0 && selectedTileIndex.y < tiles.GetLength(1) &&
               selectedTileIndex.z >= 0 && selectedTileIndex.z < tiles.GetLength(2);
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {

    }
}
