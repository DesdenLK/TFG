using System;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    private TerrainGraph terrainGraph;
    private Terrain terrain;



    public PathFinder(Terrain terrain)
    {
        this.terrain = terrain;
        this.terrainGraph = new TerrainGraph(terrain);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float x = worldPos.x / terrainGraph.MetersPerCell;
        float y = worldPos.z / terrainGraph.MetersPerCell;
        return new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x + 0.5f) * terrainGraph.MetersPerCell;
        float z = (gridPos.y + 0.5f) * terrainGraph.MetersPerCell;
        float y = terrain.SampleHeight(new Vector3(x, 0, z));
        return new Vector3(x, y, y);
    }

    public bool FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        Vector2Int start = WorldToGrid(startWorldPos);
        Vector2Int end = WorldToGrid(endWorldPos);

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = new Vector2Int(-1, -1);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (Vector2Int.Distance(current, end) <= 1.5f)
            {
                return true;
            }

            //Explore all directions
            ExploreNeighbor(current, current + Vector2Int.up, queue, visited, cameFrom);
            ExploreNeighbor(current, current + Vector2Int.down, queue, visited, cameFrom);
            ExploreNeighbor(current, current + Vector2Int.left, queue, visited, cameFrom);
            ExploreNeighbor(current, current + Vector2Int.right, queue, visited, cameFrom);
        }

        return false;
    }

    private void ExploreNeighbor(Vector2Int current, Vector2Int newPoint, Queue<Vector2Int> queue, HashSet<Vector2Int> visited, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        if (terrainGraph.isCellValid(newPoint) && !visited.Contains(newPoint))
        {
            queue.Enqueue(newPoint);
            visited.Add(newPoint);
            cameFrom[newPoint] = current;
        }
    }
}
