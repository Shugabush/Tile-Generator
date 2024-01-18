using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    bool initialized = false;

    TileGenerator parent;

    Vector3Int indexPosition;

    public void Initialize(TileGenerator parent, Vector3Int indexPosition)
    {
        if (!initialized || this.parent == null)
        {
            this.parent = parent;
            this.indexPosition = indexPosition;
            initialized = true;
        }
    }
}
