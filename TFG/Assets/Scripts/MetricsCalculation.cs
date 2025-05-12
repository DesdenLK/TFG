using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MetricsCalculation : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Text distance3D_text;
    public Text distance2D_text;

    public Text positiveSlope_text;
    public Text negativeSlope_text;
    public Text totalSlope_text;

    public Text metabolicPathCost_text;

    private bool isTerrainLoaded = false;

    public struct Metrics
    {
        public float distance3D;
        public float distance2D;
        public float positiveSlope;
        public float negativeSlope;
        public float totalSlope;
        public float metabolicPathCost;
    }

    private Metrics metrics = new Metrics();
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

    void getMetabolicPathCost()
    {
        float metabolicPathCost = 0;

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector2 start = new Vector2(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).z);
            Vector2 end = new Vector2(lineRenderer.GetPosition(i + 1).x, lineRenderer.GetPosition(i + 1).z);
            float planarDistance = Vector2.Distance(start, end);
            float verticalDistance = Mathf.Abs(lineRenderer.GetPosition(i).y - lineRenderer.GetPosition(i + 1).y);
            float averageSlope = verticalDistance / planarDistance;

            float factor1 = 1 + 7.92f * averageSlope;

            metabolicPathCost += (planarDistance * Mathf.Pow(factor1, 1.2f));
        }
        metrics.metabolicPathCost = metabolicPathCost;
    }

    public static float getMetabolicPathCostFromArray(List<Vector3> path)
    {
        float metabolicPathCost = 0;
        for (int i = 0; i < path.Count - 1; i++) // Replaced 'Length' with 'Count'  
        {
            Vector2 start = new Vector2(path[i].x, path[i].z);
            Vector2 end = new Vector2(path[i + 1].x, path[i + 1].z);
            float planarDistance = Vector2.Distance(start, end);
            float verticalDistance = Mathf.Abs(path[i].y - path[i + 1].y);
            float averageSlope = verticalDistance / planarDistance;
            float factor1 = 1 + 7.92f * averageSlope;
            metabolicPathCost += (planarDistance * Mathf.Pow(factor1, 1.2f));
        }
        return metabolicPathCost;
    }

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
    }

    public static Metrics getAllMetricsFromArray(List<Vector3> path)
    {
        Metrics metrics = new Metrics();
        getTotalDistanceFromArray(path, out metrics.distance3D, out metrics.distance2D);
        getTotalSlopeFromArray(path, out metrics.totalSlope, out metrics.positiveSlope, out metrics.negativeSlope);
        metrics.metabolicPathCost = getMetabolicPathCostFromArray(path);
        return metrics;
    }

    private void Update()
    {
        if (PlayerPrefs.GetString("SelectedTerrain") != null) setTerrainLoadedTrue(); 
        if (!isTerrainLoaded)
            return;


        getTotalDistance();
        getTotalSlope();
        getMetabolicPathCost();

        updateMetricsTexts();
    }
}
