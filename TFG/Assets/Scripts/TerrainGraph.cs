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

    public TerrainGraph(Terrain terrain, TerrainLoader terrainLoader)
    {
        TerrainData terrainData = terrain.terrainData;
        this.heightmap = terrainLoader.GetHeightMap();
        this.width = heightmap.GetLength(1);
        this.height = heightmap.GetLength(0);
        this.heightDifference = Mathf.FloorToInt(terrainData.size.y);
        this.metersPerCell = terrainData.size.x / (width);
        Debug.Log($"TerrainGraph created with width: {width}, height: {height}, metersPerCell: {metersPerCell}, heightDifference: {heightDifference}");
    }

    public bool isCellValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height;
    }
}
