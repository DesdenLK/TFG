using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Rendering;

public class PathFinder
{
    private TerrainGraph terrainGraph;
    private Terrain terrain;



    public PathFinder(Terrain terrain, TerrainLoader terrainLoader)
    {
        this.terrain = terrain;
        this.terrainGraph = new TerrainGraph(terrain, terrainLoader);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - terrain.GetPosition();
        float x = localPos.x / terrainGraph.MetersPerCell;
        float y = localPos.z / terrainGraph.MetersPerCell;
        Vector2Int point = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        return point;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x) * terrainGraph.MetersPerCell;
        float z = (gridPos.y) * terrainGraph.MetersPerCell;
        float y = terrain.SampleHeight(new Vector3(x + terrain.GetPosition().x, 0, z + terrain.GetPosition().z));
        Vector3 point = new Vector3(x + terrain.GetPosition().x, y, z + terrain.GetPosition().z);
        return point;
    }

    public async UniTask<Dictionary<Vector2Int, Vector2Int>> FindPathThreadedAsync(Vector2Int start, Vector2Int end, CancellationToken token = default)
    {
        return await UniTask.RunOnThreadPool(() =>
        {

            float[,] heightmap = terrainGraph.Heightmap;

            SimplePriorityQueue<Vector2Int> queue = new SimplePriorityQueue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            int estimatedNodes = terrainGraph.Width * terrainGraph.Height;
            float[,] costSoFar = new float[terrainGraph.Height, terrainGraph.Width];
            for (int i = 0; i < terrainGraph.Height; i++)
            {
                for (int j = 0; j < terrainGraph.Width; j++)
                {
                    costSoFar[i, j] = float.MaxValue;
                }
            }

            queue.Enqueue(start, 0);
            cameFrom[start] = new Vector2Int(-1, -1);
            costSoFar[start.y, start.x] = 0;

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                Vector2Int current = queue.Dequeue();

                if (Vector2Int.Distance(current, end) <= 1.0f)
                {
                    cameFrom[end] = current;
                    float cost = CalculateCostFromHeightmapOptimized(current, end, heightmap);
                    costSoFar[end.y,end.x] = costSoFar[current.y, current.x] + cost;
                    return cameFrom;
                }

                // Explorar vecinos secuencialmente para evitar problemas de token
                ExploreNeighborAsync(current, current + Vector2Int.up, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.down, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.left, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.right, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.up + Vector2Int.left, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.up + Vector2Int.right, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.down + Vector2Int.left, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.down + Vector2Int.right, heightmap, queue, costSoFar, cameFrom, end);
            }

            return null;
        }, cancellationToken: token);
    }

    private void ExploreNeighborAsync(Vector2Int current, Vector2Int neighbor,
                           float[,] heightmap,
                           SimplePriorityQueue<Vector2Int> queue,
                          float[,] costSoFar,
                           Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
    {
        if (!terrainGraph.isCellValid(neighbor)) return;

        float currentCost = costSoFar[current.y, current.x];
        float stepCost = CalculateCostFromHeightmapOptimized(current, neighbor, heightmap);
        float newCost = currentCost + stepCost;
   

        if (newCost < costSoFar[neighbor.y, neighbor.x])
        {
            costSoFar[neighbor.y, neighbor.x] = newCost;
            cameFrom[neighbor] = current;


            float heuristic = Vector2Int.Distance(neighbor, end);
            float priority = newCost + heuristic;

            if (!queue.Contains(neighbor))
            {
                queue.Enqueue(neighbor, priority);
            }
            else
            {
                queue.TryUpdatePriority(neighbor, priority);
            }
        }
    }

    private float CalculateCostFromHeightmapOptimized(Vector2Int from, Vector2Int to, float[,] heightmap)
    {
        float height1 = heightmap[from.y, from.x] * terrainGraph.HeightDifference;
        float height2 = heightmap[to.y, to.x] * terrainGraph.HeightDifference;

        Vector3 start = new Vector3(from.x, height1, from.y);
        Vector3 end = new Vector3(to.x, height2, to.y);

        return MetricsCalculation.getMetabolicCostBetweenTwoPoints(start, end);

    }

    public List<Vector3> ConvertBFSPathToPoints(Dictionary<Vector2Int, Vector2Int> bfsPath, Vector2Int start, Vector2Int end)
    {
        Debug.Log("Converting BFS path to points" + bfsPath.Count);
        List<Vector3> path = new List<Vector3>();
        Vector2Int current = end;
        while (current != start)
        {
            path.Add(GridToWorld(current));
            current = bfsPath[current];
        }
        path.Add(GridToWorld(start));
        path.Reverse();
        Debug.Log("Path found with " + path.Count + " points.");
        return path;
    }
}
