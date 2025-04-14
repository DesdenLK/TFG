using System;
using UnityEngine;

public class TerrainGraph
{
    private int width, height;
    private float metersPerCell;
    private float[,] heightmap;
    private int heightDifference;

    public int Width => width;
    public int Height => height;
    public float MetersPerCell => metersPerCell;

    public float[,] Heightmap => heightmap;
    public int HeightDifference => heightDifference;

    public TerrainGraph(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        this.width = terrainData.heightmapResolution;
        this.height = terrainData.heightmapResolution;
        this.metersPerCell = terrainData.size.x / (width - 1);
        this.heightmap = TerrainLoader.GetHeightMap();
        this.heightDifference = Mathf.FloorToInt(terrainData.size.y);
        Debug.Log($"TerrainGraph created with width: {width}, height: {height}, metersPerCell: {metersPerCell}, heightDifference: {heightDifference}");
    }

    public bool isCellValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height;
    }
}
