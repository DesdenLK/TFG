using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR.Input;

public class MetricsCalculation : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Text distance3D_text;
    public Text distance2D_text;

    public Text positiveSlope_text;
    public Text negativeSlope_text;
    public Text totalSlope_text;

    public Text metabolicPathCost_text;

    public Text avalancheCoount;

    public TerrainLoader terrainLoader;
    private int[] avalancheValues;
    private int mapWidth;
    private Vector3 terrainPos;
    private float metersPerCell;

    private bool isTerrainLoaded = false;

    public struct Metrics
    {
        public float distance3D;
        public float distance2D;
        public float positiveSlope;
        public float negativeSlope;
        public float totalSlope;
        public float metabolicPathCost;
        public int accumulatedAvalancheValue;
    }

    private Metrics metrics = new Metrics();

    // Funci� per calcular la dist�ncia total del LineRenderer
    void getTotalDistance()
    {
        float totalDistance3D = 0;
        float totalDistance2D = 0;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            totalDistance3D += Vector3.Distance(lineRenderer.GetPosition(i), lineRenderer.GetPosition(i + 1));
            totalDistance2D += Vector2.Distance(new Vector2(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).z), new Vector2(lineRenderer.GetPosition(i + 1).x, lineRenderer.GetPosition(i + 1).z));
        }
        metrics.distance3D = totalDistance3D;
        metrics.distance2D = totalDistance2D;
    }

    // Funci� per calcular la dist�ncia total d'un array de Vector3
    public static void getTotalDistanceFromArray(List<Vector3> path, out float totalDistance3D, out float totalDistance2D)
    {
        totalDistance3D = 0;
        totalDistance2D = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance3D += Vector3.Distance(path[i], path[i + 1]);
            totalDistance2D += Vector2.Distance(new Vector2(path[i].x, path[i].z), new Vector2(path[i + 1].x, path[i + 1].z));
        }
    }

    // Funci� per calcular la pendent total del LineRenderer
    void getTotalSlope()
    {
        float totalSlope = 0;
        float positiveSlope = 0;
        float negativeSlope = 0;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            totalSlope += Mathf.Abs(lineRenderer.GetPosition(i).y - lineRenderer.GetPosition(i + 1).y);
            if (lineRenderer.GetPosition(i).y < lineRenderer.GetPosition(i + 1).y)
            {
                positiveSlope += Mathf.Abs(lineRenderer.GetPosition(i).y - lineRenderer.GetPosition(i + 1).y);
            }
            else
            {
                negativeSlope += Mathf.Abs(lineRenderer.GetPosition(i).y - lineRenderer.GetPosition(i + 1).y);
            }
        }
        metrics.totalSlope = totalSlope;
        metrics.positiveSlope = positiveSlope;
        metrics.negativeSlope = negativeSlope;
    }

    // Funci� per calcular la pendent total d'un array de Vector3
    public static void getTotalSlopeFromArray(List<Vector3> path, out float totalSlope, out float positiveSlope, out float negativeSlope)
    {
        totalSlope = 0;
        positiveSlope = 0;
        negativeSlope = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalSlope += Mathf.Abs(path[i].y - path[i + 1].y);
            if (path[i].y < path[i + 1].y)
            {
                positiveSlope += Mathf.Abs(path[i].y - path[i + 1].y);
            }
            else
            {
                negativeSlope += Mathf.Abs(path[i].y - path[i + 1].y);
            }
        }
    }

    // Funci� per calcular el cost metab�lic del cam� tra�at pel LineRenderer
    void getMetabolicPathCost()
    {
        float metabolicPathCost = 0;

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector2 start = new Vector2(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).z);
            Vector2 end = new Vector2(lineRenderer.GetPosition(i + 1).x, lineRenderer.GetPosition(i + 1).z);
            float planarDistance = Vector2.Distance(start, end);
            if (planarDistance < 0.001f) continue;
            float verticalDistance = Mathf.Abs(lineRenderer.GetPosition(i).y - lineRenderer.GetPosition(i + 1).y);
            float averageSlope = verticalDistance / planarDistance;

            float factor1 = 1 + 7.92f * averageSlope;

            metabolicPathCost += (planarDistance * Mathf.Pow(factor1, 1.2f));
        }
        metrics.metabolicPathCost = metabolicPathCost;
    }

    // Funci� per calcular el cost metab�lic d'un cam� tra�at per un array de Vector3
    public static float getMetabolicPathCostFromArray(List<Vector3> path)
    {
        float metabolicPathCost = 0;
        for (int i = 0; i < path.Count - 1; i++) // Replaced 'Length' with 'Count'  
        {
            Vector2 start = new Vector2(path[i].x, path[i].z);
            Vector2 end = new Vector2(path[i + 1].x, path[i + 1].z);
            float planarDistance = Vector2.Distance(start, end);
            if (planarDistance < 0.001f) continue;
            float verticalDistance = Mathf.Abs(path[i].y - path[i + 1].y);
            float averageSlope = verticalDistance / planarDistance;
            float factor1 = 1 + 7.92f * averageSlope;
            metabolicPathCost += (planarDistance * Mathf.Pow(factor1, 1.2f));
        }
        return metabolicPathCost;
    }

    // Funci� per calcular el valor acumulat d'allaus del cam� tra�at per un array de Vector3
    public int getAccumulateAvalancheValueFromArray(List<Vector3> path)
    {
        int totalAvalancheValue = 0;
        int mapHeight = avalancheValues.Length / mapWidth;

        HashSet<int> indexGridPositionsVisited = new HashSet<int>();

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 worldPos = path[i];
            float localZ = (worldPos.z - terrainPos.z) / metersPerCell;
            float localX = (worldPos.x - terrainPos.x) / metersPerCell;

            int x = Mathf.FloorToInt(localX);
            int z = Mathf.FloorToInt(localZ);

            Debug.Log($"World Position: {worldPos}, Local X: {localX}, Local Z: {localZ}, X: {x}, Z: {z}");


            if (x >= 0 && x < mapWidth && z >= 0 && z < mapHeight)
            {
                int index = z * mapWidth + x;
                if (!indexGridPositionsVisited.Contains(index))
                {
                    float value = avalancheValues[index];
                    totalAvalancheValue += (int)value;
                    indexGridPositionsVisited.Add(index);
                }
            }
            else
            {
                Debug.LogWarning($"Position out of bounds: X={x}, Z={z} for map width {mapWidth} and height {mapHeight}");
            }
        }

        return totalAvalancheValue;
    }

    // Funci� est�tica per calcular el valor acumulat d'allaus d'un cam� tra�at per un array de Vector3
    public static int getAccumulateAvalancheValueFromArrayStatic(List<Vector3> path, int[] avalancheValues, Vector3 terrainPos, int mapWidth, float metersPerCell)
    {
        int totalAvalancheValue = 0;
        int mapHeight = avalancheValues.Length / mapWidth;

        HashSet<int> indexGridPositionsVisited = new HashSet<int>();

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 worldPos = path[i];
            float localZ = (worldPos.z - terrainPos.z) / metersPerCell;
            float localX = (worldPos.x - terrainPos.x) / metersPerCell;

            int x = Mathf.FloorToInt(localX);
            int z = Mathf.FloorToInt(localZ);

            //Debug.Log($"World Position: {worldPos}, Local X: {localX}, Local Z: {localZ}, X: {x}, Z: {z}");


            if (x >= 0 && x < mapWidth && z >= 0 && z < mapHeight)
            {
                int index = z * mapWidth + x;
                if (!indexGridPositionsVisited.Contains(index)) {
                    float value = avalancheValues[index];
                    totalAvalancheValue += (int)value;
                    indexGridPositionsVisited.Add(index);
                }
            }
            else
            {
                Debug.LogWarning($"Position out of bounds: X={x}, Z={z} for map width {mapWidth} and height {mapHeight}");
            }
        }

        return totalAvalancheValue;
    }

    public int getAccumulatedAvalancheValue() //Funci� per lineRenderer, Path que no segueix grid
    {
        int totalAvalancheValue = 0;
        int mapHeight = avalancheValues.Length / mapWidth;
        HashSet<int> indexGridPositionsVisited = new HashSet<int>();


        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 start = lineRenderer.GetPosition(i);
            Vector3 end = lineRenderer.GetPosition(i + 1);

            int startX = Mathf.FloorToInt((start.x - terrainPos.x) / metersPerCell);
            int startZ = Mathf.FloorToInt((start.z - terrainPos.z) / metersPerCell);

            int endX = Mathf.FloorToInt((end.x - terrainPos.x) / metersPerCell);
            int endZ = Mathf.FloorToInt((end.z - terrainPos.z) / metersPerCell);

            List<Vector2Int> segmentPositions = GridUtils.Bresenham(new Vector2Int(startX, startZ), new Vector2Int(endX, endZ));

            foreach (var pos in segmentPositions)
            {
                if (pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight)
                {
                    int index = pos.y * mapWidth + pos.x;
                    if (!indexGridPositionsVisited.Contains(index))
                    {
                        float value = avalancheValues[index];
                        totalAvalancheValue += (int)value;
                        indexGridPositionsVisited.Add(index);
                    }
                }
                else
                {
                    Debug.LogWarning($"Position out of bounds: X={pos.x}, Z={pos.y} for map width {mapWidth} and height {mapHeight}");
                }
            }
        }

        return totalAvalancheValue;
    }


    // Funci� est�tica per calcular el cost metab�lic entre dos punts
    public static float getMetabolicCostBetweenTwoPoints(Vector3 start, Vector3 end)
    {
        Vector2 start2D = new Vector2(start.x, start.z);
        Vector2 end2D = new Vector2(end.x, end.z);
        float planarDistance = Vector2.Distance(start2D, end2D);
        float verticalDistance = Mathf.Abs(start.y - end.y);
        float averageSlope = verticalDistance / planarDistance;
        float factor1 = 1 + 7.92f * averageSlope;
        return planarDistance * Mathf.Pow(factor1, 1.2f);
    }

    // Funci� per calcular el valor acumulat d'allaus del cam� tra�at pel LineRenderer
    public void getAccumulatedAvalancheValueWrapper()
    {
        if (lineRenderer == null || lineRenderer.positionCount == 0 || avalancheValues == null || avalancheValues.Length == 0)
        {
            metrics.accumulatedAvalancheValue = 0;
            return;
        }
        metrics.accumulatedAvalancheValue =  getAccumulatedAvalancheValue();
    }

    public void setTerrainLoadedTrue()
    {
        isTerrainLoaded = true;
    }

    private void updateMetricsTexts()
    {
        distance3D_text.text = metrics.distance3D.ToString();
        distance2D_text.text = metrics.distance2D.ToString();
        totalSlope_text.text = metrics.totalSlope.ToString();
        positiveSlope_text.text = metrics.positiveSlope.ToString();
        negativeSlope_text.text = metrics.negativeSlope.ToString();
        metabolicPathCost_text.text = metrics.metabolicPathCost.ToString();
        avalancheCoount.text = metrics.accumulatedAvalancheValue.ToString();
    }

    // Funci� est�tica per obtenir totes les m�triques d'un array de Vector3
    public static Metrics getAllMetricsFromArray(List<Vector3> path)
    {
        Metrics metrics = new Metrics();
        getTotalDistanceFromArray(path, out metrics.distance3D, out metrics.distance2D);
        getTotalSlopeFromArray(path, out metrics.totalSlope, out metrics.positiveSlope, out metrics.negativeSlope);
        metrics.metabolicPathCost = getMetabolicPathCostFromArray(path);
        metrics.accumulatedAvalancheValue = 0;
        return metrics;
    }

    private void Update()
    {
        if (PlayerPrefs.GetInt("hasAvalancheFile",0) == 1 && (avalancheValues == null || avalancheValues.Length == 0))
        {
            if (terrainLoader == null)
            {
                Debug.LogWarning("TerrainLoader not found in the scene.");
                return;
            }
            avalancheValues = terrainLoader.GetAvalancheValues();
            mapWidth = terrainLoader.GetMapWidth();
            terrainPos = terrainLoader.GetTerrainPosition();
            metersPerCell = terrainLoader.getMetersPerCell();
        }
        if (PlayerPrefs.GetString("SelectedTerrain") != null) setTerrainLoadedTrue(); 
        if (!isTerrainLoaded)
            return;


        getTotalDistance();
        getTotalSlope();
        getMetabolicPathCost();
        getAccumulatedAvalancheValueWrapper();
        updateMetricsTexts();
    }
}
