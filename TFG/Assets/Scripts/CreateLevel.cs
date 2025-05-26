using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;


[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}

[System.Serializable]
public class LevelRequest
{
    public string name;
    public string description;
    public float start_X;
    public float start_Y;
    public float start_Z;
    public float end_X;
    public float end_Y;
    public float end_Z;
    public string creator;
    public List<Vector3Data> path;
    public float optimal_total3D_distance;
    public float optimal_total2D_distance;
    public float optimal_total_slope;
    public float optimal_total_positive_slope;
    public float optimal_total_negative_slope;
    public float optimal_metabolic_cost;
    public int optimal_total_avalanches;
}

public class CreateLevel : MonoBehaviour
{
    public GameObject waypointPrefab;
    public GameObject flagPrefab;
    public Button placeStartButton;
    public Button placeEndButton;
    public Button createLevelButton;
    public InputField levelName;
    public InputField levelDescription;

    public Text levelResponseText;
    public Button returnToLevelMenu;
    public Button returnToMainMenu;
    public Button returnToTerrainMenu;
    public GameObject optimalPathPanel;
    public GameObject responsePanel;


    private GameObject waypointStart;
    private GameObject waypointEnd;

    private bool isPlacingStart = false;
    private bool isPlacingEnd = false;
    private bool computedBFS = false;
    private bool bfsReturned = false;
    private bool sendPost = false;
    private bool creatingLevel = false;

    public Terrain terrain;

    private Requests requestHandler;
    private CancellationTokenSource bfsCancellationTokenSource;
    private PathFinder pathFinder;
    private List<Vector3> bfsPath;

    void Start()
    {
        requestHandler = new Requests();
    }

    public void PlaceStart()
    {
        isPlacingStart = true;
        isPlacingEnd = false;
        placeStartButton.interactable = false;
        placeEndButton.interactable = true;
    }

    public void PlaceEnd()
    {
        isPlacingEnd = true;
        isPlacingStart = false;
        placeStartButton.interactable = true;
        placeEndButton.interactable = false;
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
            Dictionary<Vector2Int, Vector2Int> bfsPathDict = await pathFinder.FindPathThreadedAsync(startGrid, endGrid, bfsCancellationTokenSource.Token);
            bfsPath = pathFinder.ConvertBFSPathToPoints(bfsPathDict, startGrid, endGrid);
            Debug.Log("BFS PATH COST: " + MetricsCalculation.getMetabolicPathCostFromArray(bfsPath));
            stopwatch.Stop();
            Debug.Log($"BFS pathfinding completed in {stopwatch.ElapsedMilliseconds} ms");

            if (bfsPath != null && bfsPath.Count > 0)
            {
                Debug.Log("BFS path found");
            }
            else
            {
                bfsPath = new List<Vector3>();
                Debug.Log("No BFS path found");
            }
            bfsReturned = true;
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

    public void onCreateLevelClick()
    {
        creatingLevel = true;
        placeStartButton.interactable = false;
        placeEndButton.interactable = false;
        createLevelButton.interactable = false;
        optimalPathPanel.SetActive(true);
        if (!computedBFS)
        {
            RunBFSPathFIndingAsync().Forget();
        }
    }


    private void sendNewLevelPost()
    {
        sendPost = true;
        List<Vector3Data> pathData = new List<Vector3Data>();
        MetricsCalculation.Metrics metrics = MetricsCalculation.getAllMetricsFromArray(bfsPath);
        TerrainLoader terrainLoader = GetComponent<TerrainLoader>();
        if (terrainLoader != null)
        {
            int[] avalancheValues = terrainLoader.GetAvalancheValues();
            int mapWidth = terrainLoader.GetMapWidth();
            float metersPerCell = terrainLoader.getMetersPerCell();
            Vector3 terrainPos = terrainLoader.GetTerrainPosition();
            metrics.accumulatedAvalancheValue = PlayerPrefs.GetInt("hasAvalancheFile", 0) == 1 ? MetricsCalculation.getAccumulateAvalancheValueFromArrayStatic(bfsPath, avalancheValues, terrainPos, mapWidth, metersPerCell) : 0;
        }
        foreach (var point in bfsPath)
        {
            pathData.Add(new Vector3Data(point));
        }

        LevelRequest levelRequest = new LevelRequest
        {
            name = levelName.text,
            description = levelDescription.text,
            start_X = waypointStart.transform.position.x,
            start_Y = waypointStart.transform.position.y,
            start_Z = waypointStart.transform.position.z,
            end_X = waypointEnd.transform.position.x,
            end_Y = waypointEnd.transform.position.y,
            end_Z = waypointEnd.transform.position.z,
            creator = PlayerPrefs.GetString("username", "Guest"),
            path = pathData,
            optimal_total3D_distance = metrics.distance3D,
            optimal_total2D_distance = metrics.distance2D,
            optimal_total_slope = metrics.totalSlope,
            optimal_total_positive_slope = metrics.positiveSlope,
            optimal_total_negative_slope = metrics.negativeSlope,
            optimal_metabolic_cost = metrics.metabolicPathCost,
            optimal_total_avalanches = metrics.accumulatedAvalancheValue
        };
        Debug.Log("LevelRequest: " + levelRequest.name + " " + levelRequest.description + " " + levelRequest.start_X + " " + levelRequest.start_Y + " " + levelRequest.start_Z + " " + levelRequest.end_X + " " + levelRequest.end_Y + " " + levelRequest.end_Z + " " + levelRequest.path);
        string json = JsonConvert.SerializeObject(levelRequest);
        StartCoroutine(requestHandler.PostRequest("/create-level/" + PlayerPrefs.GetString("TerrainUUID"), json, OnCreateLevelResponse));
    }

    public void onBackButtonClick()
    {
        SceneManager.LoadScene("LevelSelector");
    }

    private void OnCreateLevelResponse(string response)
    {
        optimalPathPanel.SetActive(false);
        responsePanel.SetActive(true);
        if (response.Contains("ERROR"))
        {
            levelResponseText.text = "Error creating level";
            levelResponseText.color = Color.red;
            return;
        }
        levelResponseText.text = "Level created successfully!";
        levelResponseText.color = Color.green;
    }

    public void onTerrainMenuClick()
    {
        responsePanel.SetActive(false);
        SceneManager.LoadScene("TerrainSelector");
    }

    public void onMainMenuClick()
    {
        responsePanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void onLevelMenuClick()
    {
        responsePanel.SetActive(false);
        SceneManager.LoadScene("LevelSelector");
    }

    void Update()
    {
        if (waypointStart != null && waypointEnd != null && levelName.text != "" && levelDescription.text != "" && !creatingLevel)
        {
            createLevelButton.interactable = true;
        }
        else
        {
            createLevelButton.interactable = false;
        }
        if (bfsReturned && !sendPost)
        {
            sendNewLevelPost();
        }
        UpdatePoints();
    }
}
