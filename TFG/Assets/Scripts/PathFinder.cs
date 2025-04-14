using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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
        Vector2Int point = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        //Debug.Log($"World to Grid: {worldPos} -> {point}");
        return point;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x) * terrainGraph.MetersPerCell;
        float z = (gridPos.y) * terrainGraph.MetersPerCell;
        float y = terrain.SampleHeight(new Vector3(x, 0, z));
        Vector3 point = new Vector3(x, y, z);
        //Debug.Log($"Grid to World: {gridPos} -> {point}");
        return point;
    }

    private async UniTask<Vector3> GridToWorldAsync(Vector2Int gridPos, CancellationToken token)
    {
        // Mueve la llamada a SampleHeight al hilo principal
        return await UniTask.RunOnThreadPool(async () =>
        {
            // Primero realiza el cálculo de la posición sin SampleHeight
            float x = gridPos.x * terrainGraph.MetersPerCell;
            float z = gridPos.y * terrainGraph.MetersPerCell;

            // Cambia al hilo principal para llamar a SampleHeight
            await UniTask.SwitchToMainThread();

            // Ahora llama a SampleHeight en el hilo principal
            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            return new Vector3(x, y, z);
        }, cancellationToken: token);
    }



    public Dictionary<Vector2Int, Vector2Int> FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        Vector2Int start = WorldToGrid(startWorldPos);
        Vector2Int end = WorldToGrid(endWorldPos);

        SimplePriorityQueue<Vector2Int> queue = new SimplePriorityQueue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start,0);
        cameFrom[start] = new Vector2Int(-1, -1);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Debug.Log("Queue count: " + queue.Count);
            Vector2Int current = queue.Dequeue();

            if (Vector2Int.Distance(current, end) <= 1.0f)
            {
                cameFrom[end] = current;
                return cameFrom;
            }

            //Explore all directions
            ExploreNeighbor(current, current + Vector2Int.up, queue, visited, cameFrom);
            ExploreNeighbor(current, current + Vector2Int.down, queue, visited, cameFrom);
            ExploreNeighbor(current, current + Vector2Int.left, queue, visited, cameFrom);
            ExploreNeighbor(current, current + Vector2Int.right, queue, visited, cameFrom);
        }

        return null;
    }

    public async UniTask<Dictionary<Vector2Int, Vector2Int>> FindPathThreadedAsync(Vector3 startWorldPos, Vector3 endWorldPos, CancellationToken token = default)
    {
        return await UniTask.RunOnThreadPool(() =>
        {
            Vector2Int start = WorldToGrid(startWorldPos);
            Vector2Int end = WorldToGrid(endWorldPos);

            float[,] heightmap = terrainGraph.Heightmap;

            SimplePriorityQueue<Vector2Int> queue = new SimplePriorityQueue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();

            queue.Enqueue(start, 0);
            cameFrom[start] = new Vector2Int(-1, -1);
            costSoFar[start] = 0;

            while (queue.Count > 0)
            {
                Debug.Log("Queue count: " + queue.Count);
                token.ThrowIfCancellationRequested();
                Vector2Int current = queue.Dequeue();

                if (Vector2Int.Distance(current, end) <= 1.0f)
                {
                    cameFrom[end] = current;
                    return cameFrom;
                }

                // Explorar vecinos secuencialmente para evitar problemas de token
                ExploreNeighborAsync(current, current + Vector2Int.up, heightmap, queue, costSoFar, cameFrom);
                ExploreNeighborAsync(current, current + Vector2Int.down, heightmap, queue, costSoFar, cameFrom);
                ExploreNeighborAsync(current, current + Vector2Int.left, heightmap, queue, costSoFar, cameFrom);
                ExploreNeighborAsync(current, current + Vector2Int.right, heightmap, queue, costSoFar, cameFrom);
            }

            return null;
        }, cancellationToken: token);
    }


    private void ExploreNeighbor(Vector2Int current, Vector2Int newPoint, SimplePriorityQueue<Vector2Int> queue, HashSet<Vector2Int> visited, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        if (terrainGraph.isCellValid(newPoint) && !visited.Contains(newPoint))
        {
            float cost = MetricsCalculation.getMetabolicCostBetweenTwoPoints(GridToWorld(newPoint), GridToWorld(current));
            queue.Enqueue(newPoint, cost);
            visited.Add(newPoint);
            cameFrom[newPoint] = current;
        }
    }

    private void ExploreNeighborAsync(Vector2Int current, Vector2Int neighbor,
                           float[,] heightmap,
                           SimplePriorityQueue<Vector2Int> queue,
                          Dictionary<Vector2Int, float> costSoFar,
                           Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        float currentCost = costSoFar[current];
        if (!terrainGraph.isCellValid(neighbor)) return;

        // Calcula costos usando el heightmap precargado
        float cost = CalculateCostFromHeightmapOptimized(current, neighbor, heightmap);
        if (!costSoFar.ContainsKey(neighbor) || currentCost + cost < costSoFar[neighbor])
        {
            float newCost = currentCost + cost;
            costSoFar[neighbor] = newCost;
            queue.Enqueue(neighbor, newCost);
            cameFrom[neighbor] = current;
        }
    }

    private float CalculateCostFromHeightmapOptimized(Vector2Int from, Vector2Int to, float[,] heightmap)
    {
        float height1 = heightmap[from.y, from.x];
        float height2 = heightmap[to.y, to.x];

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
