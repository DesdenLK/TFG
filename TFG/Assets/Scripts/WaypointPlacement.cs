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
    public Button computeOptimalButton;
    public Button finishDrawing;


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
    private bool executingBFS = false;


    private PathFinder pathFinder;
    private GameObject bfsLineRenderer;
    public Terrain terrain;
    private CancellationTokenSource bfsCancellationTokenSource;

    private CameraModeManager cameraModeManager;
    private int currentCameraMode = 0;

    private void Start()
    {
        cameraModeManager = GetComponent<CameraModeManager>();
    }

    public void PlaceStart()
    {
        WaypointStorage.waypointStart = Vector3.negativeInfinity;
        isPlacingStart = true;
        isPlacingEnd = false;
        placeStartButton.interactable = false;
        placeEndButton.interactable = true;
        waypoints.Clear();
        computedBFS = false;
        if (bfsLineRenderer != null) Destroy(bfsLineRenderer);
        computeOptimalButton.interactable = true;
        resetLine();
    }

    public void PlaceEnd()
    {
        WaypointStorage.waypointEnd = Vector3.negativeInfinity;
        isPlacingEnd = true;
        isPlacingStart = false;
        placeStartButton.interactable = true;
        placeEndButton.interactable = false;
        waypoints.Clear();
        computedBFS = false;
        if (bfsLineRenderer != null) Destroy(bfsLineRenderer);
        computeOptimalButton.interactable = true;
        resetLine();
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
                    WaypointStorage.waypointStart = waypointStart.transform.position;
                    isPlacingStart = false;
                    placeStartButton.interactable = true;
                    resetLine();
                    Debug.Log("Waypoint start placed at: " + waypointStart.transform.position);

                }
                else
                {
                    if (waypointEnd != null)
                    {
                        Destroy(waypointEnd);
                    }
                    waypointEnd = Instantiate(flagPrefab, hit.point, Quaternion.identity);
                    WaypointStorage.waypointEnd = waypointEnd.transform.position;
                    isPlacingEnd = false;
                    placeEndButton.interactable = true;
                    resetLine();
                    Debug.Log("Waypoint end placed at: " + waypointEnd.transform.position);
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

        switch (cameraModeManager.currentMode)
        {
            case CameraModeManager.Mode.FirstPerson:
                if (canDraw)
                {
                    Vector3 currentPos = Camera.main.transform.position;
                    if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], currentPos) > minDistance)
                    {
                        waypoints.Add(currentPos - new Vector3(0, 0.7f, 0));
                        lineRenderer.positionCount = waypoints.Count;
                        lineRenderer.SetPositions(waypoints.ToArray());
                    }
                }
                break;
            case CameraModeManager.Mode.ThirdPerson:
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
                            waypoints.Add(hit.point + new Vector3(0, 0.1f,0));
                            lineRenderer.positionCount = waypoints.Count;
                            lineRenderer.SetPositions(waypoints.ToArray());
                            Debug.Log("Adding point: " + hit.point);
                        }
                    }
                }
                if (isDrawing && Input.GetMouseButtonUp(0))
                {
                    isDrawing = false;
                }
                break;
        }
    }

    public void updateToggleInput()
    {
        canDraw = !canDraw;

        if (canDraw && cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson)
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.FirstPerson, waypoints[waypoints.Count - 1]);
        }
    }


    public void onFinishDrawing()
    {
        if (!canDraw && waypoints[waypoints.Count - 1] != waypointEnd.transform.position)
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
        if (currentCameraMode == 0)
        {
            lineRenderer.startWidth = 50.0f;
            lineRenderer.endWidth = 50.0f;
        }
        else
        {
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
        }
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
            Debug.Log("Starting BFS pathfinding 1");
            pathFinder = new PathFinder(terrain, terrainLoader);
            Debug.Log("Starting BFS pathfinding");
            Vector2Int startGrid = pathFinder.WorldToGrid(waypointStart.transform.position);
            Vector2Int endGrid = pathFinder.WorldToGrid(waypointEnd.transform.position);
            Vector3 startWorld = waypointStart.transform.position;
            Vector3 endWorld = waypointEnd.transform.position;

            stopwatch.Start();
            executingBFS = true;
            Dictionary<Vector2Int, Vector2Int> bfsPathDict = await pathFinder.FindPathThreadedAsync(startWorld, endWorld, bfsCancellationTokenSource.Token);
            Debug.Log("BFS pathfinding completed in " + stopwatch.ElapsedMilliseconds + " ms");
            List<Vector3> bfsPath = pathFinder.ConvertBFSPathToPoints(bfsPathDict, startGrid, endGrid);
            Debug.Log("Start: " + bfsPath[0]);
            Debug.Log("BFS PATH COST: " + MetricsCalculation.getMetabolicPathCostFromArray(bfsPath));
            stopwatch.Stop();
            executingBFS = false;

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

    public void onComputeOptimalClick()
    {
        if (!computedBFS)
        {
            computeOptimalButton.interactable = false;
            RunBFSPathFIndingAsync().Forget();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.ThirdPerson, Vector3.zero);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && waypointStart != null && waypointEnd != null)
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.FirstPerson, waypoints[waypoints.Count - 1]);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson && currentCameraMode != 1)
        {
            currentCameraMode = 1;
            placeStartButton.interactable = false;
            placeEndButton.interactable = false;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;

            if (bfsLineRenderer != null)
            {
                LineRenderer bfsLineRendererComponent = bfsLineRenderer.GetComponent<LineRenderer>();
                if (bfsLineRendererComponent != null)
                {
                    bfsLineRendererComponent.startWidth = 0.5f;
                    bfsLineRendererComponent.endWidth = 0.5f;
                }

            }
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.ThirdPerson && currentCameraMode != 0)
        {
            currentCameraMode = 0;
            placeStartButton.interactable = true;
            placeEndButton.interactable = true;
            lineRenderer.startWidth = 50.0f;
            lineRenderer.endWidth = 50.0f;

            if (bfsLineRenderer != null)
            {
                LineRenderer bfsLineRendererComponent = bfsLineRenderer.GetComponent<LineRenderer>();
                if (bfsLineRendererComponent != null)
                {
                    bfsLineRendererComponent.startWidth = 50.0f;
                    bfsLineRendererComponent.endWidth = 50.0f;
                }
            }
        }

        if (executingBFS)
        {
            placeStartButton.interactable = false;
            placeEndButton.interactable = false;
        }
        else
        {
            if (!isPlacingStart && currentCameraMode != 1) placeStartButton.interactable = true;
            if (!isPlacingEnd && currentCameraMode != 1) placeEndButton.interactable = true;
        }
        UpdatePoints();
        if (waypointEnd != null && waypointStart != null)
        {
            if (!canDraw) finishDrawing.interactable = true;
            else finishDrawing.interactable = false;
            UpdateLine();
        }
        else
        {
            finishDrawing.interactable = false;
        }
    }
}
