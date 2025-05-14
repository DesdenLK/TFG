using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    public GameObject savedScorePanel;
    public GameObject buttonPanel;
    public GameObject scoreComparisonPanel;

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
        buttonPanel.SetActive(false);
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
        if (json.Contains("ERROR"))
        {
            savedScorePanel.SetActive(true);
            savedScorePanel.transform.Find("Text").GetComponent<Text>().text = "Error saving score";
            savedScorePanel.transform.Find("Text").GetComponent<Text>().color = Color.red;
        }
        else
        {
            float Distance3DDiff = ((float.Parse(metricViewContent.transform.Find("3D_Distance").GetComponent<Text>().text) - OptimalPathStorage.optimalTotal3DDistance) / OptimalPathStorage.optimalTotal3DDistance) * 100;
            float Distance2DDiff = ((float.Parse(metricViewContent.transform.Find("2D_Distance").GetComponent<Text>().text) - OptimalPathStorage.optimalTotal2DDistance) / OptimalPathStorage.optimalTotal2DDistance) * 100;
            float SlopeDiff = ((float.Parse(metricViewContent.transform.Find("Total_Slope").GetComponent<Text>().text) - OptimalPathStorage.optimalTotalSlope) / OptimalPathStorage.optimalTotalSlope) * 100;
            float PositiveSlopeDiff = ((float.Parse(metricViewContent.transform.Find("Total_Positive_Slope").GetComponent<Text>().text) - OptimalPathStorage.optimalTotalPositiveSlope) / OptimalPathStorage.optimalTotalPositiveSlope) * 100;
            float NegativeSlopeDiff = ((float.Parse(metricViewContent.transform.Find("Total_Negative_Slope").GetComponent<Text>().text) - OptimalPathStorage.optimalTotalNegativeSlope) / OptimalPathStorage.optimalTotalNegativeSlope) * 100;
            float MetabolicCostDiff = ((float.Parse(metricViewContent.transform.Find("Metabolic_Cost").GetComponent<Text>().text) - OptimalPathStorage.optimalMetabolicCost) / OptimalPathStorage.optimalMetabolicCost) * 100;


            scoreComparisonPanel.SetActive(true);

            scoreComparisonPanel.transform.Find("Total3DDistance").GetComponent<Text>().text = Distance3DDiff >= 0
                ? $"Your 3D distance was {Distance3DDiff}% higher than the optimal."
                : $"Your 3D distance was  {Distance3DDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("Total2DDistance").GetComponent<Text>().text = Distance2DDiff >= 0
                ? $"Your 2D distance was {Distance2DDiff}% higher than the optimal."
                : $"Your 2D distance was  {Distance2DDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalSlope").GetComponent<Text>().text = SlopeDiff >= 0
                ? $"Your slope was {SlopeDiff}% higher than the optimal."
                : $"Your slope was  {SlopeDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalPositiveSlope").GetComponent<Text>().text = PositiveSlopeDiff >= 0
                ? $"Your positive slope was {PositiveSlopeDiff}% higher than the optimal."
                : $"Your positive slope was  {PositiveSlopeDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalNegativeSlope").GetComponent<Text>().text = NegativeSlopeDiff >= 0
                ? $"Your negative slope was {NegativeSlopeDiff}% higher than the optimal."
                : $"Your negative slope was  {NegativeSlopeDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("MetabolicCost").GetComponent<Text>().text = MetabolicCostDiff >= 0
                ? $"Your metabolic cost was {MetabolicCostDiff}% higher than the optimal."
                : $"Your metabolic cost was  {MetabolicCostDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("Total3DDistance").GetComponent<Text>().color = Distance3DDiff >= 0 ? Color.red : Color.green;
            scoreComparisonPanel.transform.Find("Total2DDistance").GetComponent<Text>().color = Distance2DDiff >= 0 ? Color.red : Color.green;
            scoreComparisonPanel.transform.Find("TotalSlope").GetComponent<Text>().color = SlopeDiff >= 0 ? Color.red : Color.green;
            scoreComparisonPanel.transform.Find("TotalPositiveSlope").GetComponent<Text>().color = PositiveSlopeDiff >= 0 ? Color.red : Color.green;
            scoreComparisonPanel.transform.Find("TotalNegativeSlope").GetComponent<Text>().color = NegativeSlopeDiff >= 0 ? Color.red : Color.green;
            scoreComparisonPanel.transform.Find("MetabolicCost").GetComponent<Text>().color = MetabolicCostDiff >= 0 ? Color.red : Color.green;

        }
        resetLine();
        Debug.Log("Score response: " + json);
    }

    public void onResultsContinue()
    {
        scoreComparisonPanel.SetActive(false);
        savedScorePanel.SetActive(true);
        savedScorePanel.transform.Find("Text").GetComponent<Text>().text = "Score saved successfully!";
        savedScorePanel.transform.Find("Text").GetComponent<Text>().color = Color.green;
    }

    public void onTerrainMenuClick()
    {
        savedScorePanel.SetActive(false);
        SceneManager.LoadScene("TerrainSelector");
    }

    public void onMainMenuClick()
    {
        savedScorePanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void onLevelMenuClick()
    {
        savedScorePanel.SetActive(false);
        SceneManager.LoadScene("LevelSelector");
    }

    public void onContinueButtonClick()
    {
        buttonPanel.SetActive(true);
        savedScorePanel.SetActive(false);
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
            if (canDraw || (lineRenderer.positionCount > 0 && lineRenderer.GetPosition(lineRenderer.positionCount - 1) != waypointEnd.transform.position))
            {
                saveScore.interactable = false;
            }
            else saveScore.interactable = true;

            UpdateLine();
        }
    }
}
