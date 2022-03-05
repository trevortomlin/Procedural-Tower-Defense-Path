using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public bool isTaken = false;
    public TileType tileType = TileType.Empty;

}

public enum TileType
{
    Empty,
    Road,
    Obstacle,
    Start,
    Exit

}
