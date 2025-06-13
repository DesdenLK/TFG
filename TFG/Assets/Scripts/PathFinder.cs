using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Rendering;

// Classe per calcular camins òptims en un terreny utilitzant un graf de terreny
public class PathFinder
{
    private TerrainGraph terrainGraph;
    private Terrain terrain;



    public PathFinder(Terrain terrain, TerrainLoader terrainLoader)
    {
        this.terrain = terrain;
        this.terrainGraph = new TerrainGraph(terrain, terrainLoader);
    }

    // Converteix una posició del món a coordenades de la graella
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - terrain.GetPosition();
        float x = localPos.x / terrainGraph.MetersPerCell;
        float y = localPos.z / terrainGraph.MetersPerCell;
        Vector2Int point = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        return point;
    }

    // Converteix coordenades de la graella a una posició del món
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x) * terrainGraph.MetersPerCell;
        float z = (gridPos.y) * terrainGraph.MetersPerCell;
        float y = terrain.SampleHeight(new Vector3(x + terrain.GetPosition().x, 0, z + terrain.GetPosition().z));
        Vector3 point = new Vector3(x + terrain.GetPosition().x, y, z + terrain.GetPosition().z);
        return point;
    }

    // Troba un camí entre dos punts utilitzant l'algorisme A* de manera asíncrona
    public async UniTask<Dictionary<Vector2Int, Vector2Int>> FindPathThreadedAsync(Vector2Int start, Vector2Int end, CancellationToken token = default)
    {
        return await UniTask.RunOnThreadPool(() =>
        {
            // Mapa de alçada del terreny
            float[,] heightmap = terrainGraph.Heightmap;

            // Cua de prioritats per a l'algorisme A* i estructures de dades per emmagatzemar el camí
            SimplePriorityQueue<Vector2Int> queue = new SimplePriorityQueue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            // Inicialització de la estructura de dades per al cost acumulat i la cua de prioritats
            int estimatedNodes = terrainGraph.Width * terrainGraph.Height;
            float[,] costSoFar = new float[terrainGraph.Height, terrainGraph.Width];
            for (int i = 0; i < terrainGraph.Height; i++)
            {
                for (int j = 0; j < terrainGraph.Width; j++)
                {
                    costSoFar[i, j] = float.MaxValue;
                }
            }

            // Afegim a la cua el punt d'inici amb un cost de 0
            queue.Enqueue(start, 0);
            cameFrom[start] = new Vector2Int(-1, -1);
            costSoFar[start.y, start.x] = 0;

            // Bucle principal de l'algorisme A*
            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                Vector2Int current = queue.Dequeue();

                // Si hem arribat al punt final, retornem el camí
                if (Vector2Int.Distance(current, end) <= 1.0f)
                {
                    cameFrom[end] = current;
                    float cost = CalculateCostFromHeightmapOptimized(current, end, heightmap);
                    costSoFar[end.y,end.x] = costSoFar[current.y, current.x] + cost;
                    return cameFrom;
                }

                // Exploreu els veïns del punt actual
                ExploreNeighborAsync(current, current + Vector2Int.up, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.down, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.left, heightmap, queue, costSoFar, cameFrom, end);
                ExploreNeighborAsync(current, current + Vector2Int.right, heightmap, queue, costSoFar, cameFrom, end);
                if (terrainGraph.Width >= 4096)
                {
                    ExploreNeighborAsync(current, current + Vector2Int.up + Vector2Int.left, heightmap, queue, costSoFar, cameFrom, end);
                    ExploreNeighborAsync(current, current + Vector2Int.up + Vector2Int.right, heightmap, queue, costSoFar, cameFrom, end);
                    ExploreNeighborAsync(current, current + Vector2Int.down + Vector2Int.left, heightmap, queue, costSoFar, cameFrom, end);
                    ExploreNeighborAsync(current, current + Vector2Int.down + Vector2Int.right, heightmap, queue, costSoFar, cameFrom, end);
                }
            }

            return null;
        }, cancellationToken: token);
    }

    // Explora un veí del punt actual i actualitza la cua de prioritats si es troba un camí millor
    private void ExploreNeighborAsync(Vector2Int current, Vector2Int neighbor,
                           float[,] heightmap,
                           SimplePriorityQueue<Vector2Int> queue,
                          float[,] costSoFar,
                           Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
    {
        // Comprova si el veí és vàlid i dins dels límits del graf de terreny
        if (!terrainGraph.isCellValid(neighbor)) return;

        // Si el veí ja ha estat visitat, no cal continuar
        float currentCost = costSoFar[current.y, current.x];
        float stepCost = CalculateCostFromHeightmapOptimized(current, neighbor, heightmap);
        float newCost = currentCost + stepCost;


        // Si el cost acumulat és més alt que el cost actual del veí, no cal continuar
        if (newCost < costSoFar[neighbor.y, neighbor.x])
        {
            costSoFar[neighbor.y, neighbor.x] = newCost;
            cameFrom[neighbor] = current;

            // Calcula la prioritat per a la cua de prioritats
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

    // Funció de cost pel camí
    private float CalculateCostFromHeightmapOptimized(Vector2Int from, Vector2Int to, float[,] heightmap)
    {
        float height1 = heightmap[from.y, from.x] * terrainGraph.HeightDifference;
        float height2 = heightmap[to.y, to.x] * terrainGraph.HeightDifference;

        Vector3 start = new Vector3(from.x, height1, from.y);
        Vector3 end = new Vector3(to.x, height2, to.y);

        return MetricsCalculation.getMetabolicCostBetweenTwoPoints(start, end);

    }

    // Converteix un camí trobat per l'algorisme BFS en una llista de punts del món
    public List<Vector3> ConvertBFSPathToPoints(Dictionary<Vector2Int, Vector2Int> bfsPath, Vector2Int start, Vector2Int end)
    {
        Debug.Log("Converting BFS path to points" + bfsPath.Count);
        List<Vector3> path = new List<Vector3>();
        Vector2Int current = end;
        Vector3 offset = new Vector3(0, 0.2f, 0);
        while (current != start)
        {
            path.Add(GridToWorld(current) + offset);
            current = bfsPath[current];
        }
        path.Add(GridToWorld(start) + offset);
        path.Reverse();
        Debug.Log("Path found with " + path.Count + " points.");
        return path;
    }
}
