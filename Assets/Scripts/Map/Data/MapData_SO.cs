using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData_SO", menuName = "Map/MapData", order = 0)]
public class MapData_SO : ScriptableObject
{
    [SceneName]
    public string SceneName;

    public int gridWidth;
    public int gridHeight;
    [Header("左下角原点")]
    public int originalX;
    public int originalY;
    public List<TileProperty> tileProperties;
}
