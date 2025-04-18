using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Threading;
using SystemDebug = System.Diagnostics.Debug;

public class WaypointPlacement : MonoBehaviour
{
    public GameObject waypointPrefab;
    public GameObject flagPrefab;
    public Button placeStartButton;
    public Button placeEndButton;


    public LineRenderer lineRenderer;
    public float minDistance = 0.1f;
    private List<Vector3> waypoints = new List<Vector3>();
    private bool isDrawing = false;


    private GameObject waypointStart;
    private GameObject waypointEnd;

    private bool isPlacingStart = false;
    private bool isPlacingEnd = false;

    private bool startAddedLine = false;
    private bool canDraw = false;

    private bool computedBFS = false;

    private PathFinder pathFinder;
    private GameObject bfsLineRenderer;
    public Terrain terrain;
    private CancellationTokenSource bfsCancellationTokenSource;

    public void PlaceStart()
    {
        isPlacingStart = true;
        isPlacingEnd = false;
        placeStartButton.interactable = false;
        placeEndButton.interactable = true;
        waypoints.Clear();
        computedBFS = false;
        Destroy(bfsLineRenderer);
        bfsLineRenderer = null;
    }

    public void PlaceEnd()
    {
        isPlacingEnd = true;
        isPlacingStart = false;
        placeStartButton.interactable = true;
        placeEndButton.interactable = false;
        waypoints.Clear();
        computedBFS = false;
        Destroy(bfsLineRenderer);
        bfsLineRenderer = null;
    }

    private void UpdatePoints()
    {
        if ((isPlacingStart || isPlacingEnd) && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (isPlacingStart)
                {
                    if (waypointStart != null)
                    {
                        Destroy(waypointStart);
                    }
                    waypointStart = Instantiate(waypointPrefab, hit.point, Quaternion.identity);
                    isPlacingStart = false;
                    placeStartButton.interactable = true;

                }
                else
                {
                    if (waypointEnd != null)
                    {
                        Destroy(waypointEnd);
                    }
                    waypointEnd = Instantiate(flagPrefab, hit.point, Quaternion.identity);
                    isPlacingEnd = false;
                    placeEndButton.interactable = true;
                }
            }
        }
    }

    private void UpdateLine()
    {
        if (!startAddedLine)
        {
            waypoints.Add(waypointStart.transform.position);
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints.ToArray());
            startAddedLine = true;
        }

        if (canDraw && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            isDrawing = true;
        }
        if (isDrawing && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], hit.point) > minDistance)
                {
                    waypoints.Add(hit.point);
                    lineRenderer.positionCount = waypoints.Count;
                    lineRenderer.SetPositions(waypoints.ToArray());
                }
            }
        }
        if (isDrawing && Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    public void updateToggleInput()
    {
        canDraw = !canDraw;
        Debug.Log("Can draw: " + canDraw);
        if (!canDraw)
        {
            waypoints.Add(waypointEnd.transform.position);
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints.ToArray());
        }
    }

    public void resetLine()
    {
        waypoints.Clear();
        lineRenderer.positionCount = 0;
        startAddedLine = false;
    }

    private void DrawBFSPath(List<Vector3> bfsPath)
    {
        bfsLineRenderer = new GameObject("BFSPathLine");
        LineRenderer lineRenderer = bfsLineRenderer.AddComponent<LineRenderer>();
        lineRenderer.positionCount = bfsPath.Count;
        lineRenderer.startWidth = 50.0f;
        lineRenderer.endWidth = 50.0f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.SetPositions(bfsPath.ToArray());
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void OnDestroy()
    {
        bfsCancellationTokenSource?.Cancel();
        bfsCancellationTokenSource?.Dispose();
    }

    async UniTaskVoid RunBFSPathFIndingAsync()
    {
        if (waypointStart == null || waypointEnd == null || computedBFS) return;

        bfsCancellationTokenSource?.Cancel();
        bfsCancellationTokenSource?.Dispose();
        bfsCancellationTokenSource = new CancellationTokenSource();

        computedBFS = true;
        try
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            TerrainLoader terrainLoader = GetComponent<TerrainLoader>();
            pathFinder = new PathFinder(terrain, terrainLoader);
            Debug.Log("Starting BFS pathfinding");
            Vector2Int startGrid = pathFinder.WorldToGrid(waypointStart.transform.position);
            Vector2Int endGrid = pathFinder.WorldToGrid(waypointEnd.transform.position);
            Vector3 startWorld = waypointStart.transform.position;
            Vector3 endWorld = waypointEnd.transform.position;

            stopwatch.Start();
            Dictionary<Vector2Int, Vector2Int> bfsPathDict = await pathFinder.FindPathThreadedAsync(startWorld, endWorld, bfsCancellationTokenSource.Token);
            List<Vector3> bfsPath = pathFinder.ConvertBFSPathToPoints(bfsPathDict, startGrid, endGrid);
            Debug.Log("BFS PATH COST: " + MetricsCalculation.getMetabolicPathCostFromArray(bfsPath));
            stopwatch.Stop();
            Debug.Log($"BFS pathfinding completed in {stopwatch.ElapsedMilliseconds} ms");

            if (bfsPath != null && bfsPath.Count > 0)
            {
                DrawBFSPath(bfsPath);
                Debug.Log("BFS path found");
            }
            else
            {
                Debug.Log("No BFS path found");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Pathfinding was canceled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pathfinding error: {ex.Message}");
        }
    }

    void Update()
    {
        UpdatePoints();
        if (waypointEnd != null && waypointStart != null)
        {
            if (!computedBFS)
            {
                RunBFSPathFIndingAsync().Forget();
            }
            UpdateLine();
        } 
    }
}
