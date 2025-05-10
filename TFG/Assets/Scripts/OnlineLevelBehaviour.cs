using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScoreRequest
{
    public string level_uuid;
    public string user;
    public float total2D_distance;
    public float total3D_distance;
    public float total_slope;
    public float total_positive_slope;
    public float total_negative_slope;
    public float metabolic_cost;
}

public class OnlineLevelBehaviour : MonoBehaviour
{
    public GameObject waypointPrefab;
    public GameObject flagPrefab;

    public LineRenderer lineRenderer;
    public float minDistance = 0.1f;
    private List<Vector3> waypoints = new List<Vector3>();
    private bool isDrawing = false;

    private GameObject waypointStart;
    private GameObject waypointEnd;

    private bool startAddedLine = false;
    private bool canDraw = false;

    public Terrain terrain;

    public Button saveScore;
    private Requests requestHandler;

    public GameObject metricViewContent;

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
        if (!canDraw)
        {
            waypoints.Add(waypointEnd.transform.position);
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints.ToArray());
        }
    }

    public void onSaveScore()
    {
        ScoreRequest scoreRequest = new ScoreRequest
        {
            level_uuid = PlayerPrefs.GetString("LevelUUID"),
            user = PlayerPrefs.GetString("username"),
            total2D_distance = float.Parse(metricViewContent.transform.Find("2D_Distance").GetComponent<Text>().text),
            total3D_distance = float.Parse(metricViewContent.transform.Find("3D_Distance").GetComponent<Text>().text),
            total_slope = float.Parse(metricViewContent.transform.Find("Total_Slope").GetComponent<Text>().text),
            total_positive_slope = float.Parse(metricViewContent.transform.Find("Total_Positive_Slope").GetComponent<Text>().text),
            total_negative_slope = float.Parse(metricViewContent.transform.Find("Total_Negative_Slope").GetComponent<Text>().text),
            metabolic_cost = float.Parse(metricViewContent.transform.Find("Metabolic_Cost").GetComponent<Text>().text)
        };
        string json = JsonConvert.SerializeObject(scoreRequest);
        StartCoroutine(requestHandler.PostRequest("/submit-level-score", json, OnScoreResponse));
    }

    private void OnScoreResponse(string json)
    {
        Debug.Log("Score response: " + json);
    }

    public void resetLine()
    {
        waypoints.Clear();
        lineRenderer.positionCount = 0;
        startAddedLine = false;
    }

    void Start()
    {
        requestHandler = new Requests();
        waypointStart = Instantiate(waypointPrefab, WaypointStorage.waypointStart, Quaternion.identity);
        waypointEnd = Instantiate(flagPrefab, WaypointStorage.waypointEnd, Quaternion.identity);
    }

    void Update()
    {
        if (waypointEnd != null && waypointStart != null)
        {
            if (!isDrawing && lineRenderer.positionCount > 0 && lineRenderer.GetPosition(lineRenderer.positionCount - 1) != waypointEnd.transform.position)
            {
                saveScore.interactable = false;
            }
            else saveScore.interactable = true;

            UpdateLine();
        }
    }
}
