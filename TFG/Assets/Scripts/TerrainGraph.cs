using System;
using UnityEngine;

public class TerrainGraph
{
    private int width, height;
    private float metersPerCell;
    private float[,] heightmap;

    public int Width => width;
    public int Height => height;
    public float MetersPerCell => metersPerCell;

    public float[,] Heightmap => heightmap;

    public TerrainGraph(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        this.width = terrainData.heightmapResolution;
        this.height = terrainData.heightmapResolution;
        this.metersPerCell = terrainData.size.x / (width - 1);
        this.heightmap = TerrainLoader.GetHeightMap();
    }

    public bool isCellValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height;
    }
}
