using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TilePalette : ScriptableObject
{
    public List<RuleTile> ruleTiles = new List<RuleTile>();
}
