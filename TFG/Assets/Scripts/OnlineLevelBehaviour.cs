using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ScoreRequest
{
    public string level_uuid;
    public string user;
    public int score;
    public float total2D_distance;
    public float total3D_distance;
    public float total_slope;
    public float total_positive_slope;
    public float total_negative_slope;
    public float metabolic_cost;
    public int number_avalanche;
}

public class OnlineLevelBehaviour : MonoBehaviour
{
    public GameObject waypointPrefab;
    public GameObject flagPrefab;
    public Button finishDrawing;

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

    private CameraModeManager cameraModeManager;
    private int currentCameraMode = 0;

    private void Start()
    {
        requestHandler = new Requests();
        waypointStart = Instantiate(waypointPrefab, WaypointStorage.waypointStart, Quaternion.identity);
        waypointEnd = Instantiate(flagPrefab, WaypointStorage.waypointEnd, Quaternion.identity);
        cameraModeManager = GetComponent<CameraModeManager>();
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
            case CameraModeManager.Mode.VR:
                if (canDraw)
                {
                    Vector3 currentPos = Camera.main.transform.position;
                    if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], currentPos) > minDistance)
                    {
                        waypoints.Add(currentPos - new Vector3(0, 1.4f, 0));
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
                        if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], hit.point) > minDistance && Vector3.Distance(waypoints[waypoints.Count - 1], hit.point) < 50)
                        {
                            waypoints.Add(hit.point + new Vector3(0, 0.1f, 0));
                            lineRenderer.positionCount = waypoints.Count;
                            lineRenderer.SetPositions(waypoints.ToArray());
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

        if (canDraw && cameraModeManager.currentMode == CameraModeManager.Mode.VR)
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.VR, waypoints[waypoints.Count - 1]);
        }
    }

    public void onSaveScore()
    {
        buttonPanel.SetActive(false);
        ScoreRequest scoreRequest = new ScoreRequest
        {
            level_uuid = PlayerPrefs.GetString("LevelUUID"),
            user = PlayerPrefs.GetString("username"),
            score = computeScore(float.Parse(metricViewContent.transform.Find("Metabolic_Cost").GetComponent<Text>().text), OptimalPathStorage.optimalMetabolicCost),
            total2D_distance = float.Parse(metricViewContent.transform.Find("2D_Distance").GetComponent<Text>().text),
            total3D_distance = float.Parse(metricViewContent.transform.Find("3D_Distance").GetComponent<Text>().text),
            total_slope = float.Parse(metricViewContent.transform.Find("Total_Slope").GetComponent<Text>().text),
            total_positive_slope = float.Parse(metricViewContent.transform.Find("Total_Positive_Slope").GetComponent<Text>().text),
            total_negative_slope = float.Parse(metricViewContent.transform.Find("Total_Negative_Slope").GetComponent<Text>().text),
            metabolic_cost = float.Parse(metricViewContent.transform.Find("Metabolic_Cost").GetComponent<Text>().text),
            number_avalanche = int.Parse(metricViewContent.transform.Find("Number_Avalanche").GetComponent<Text>().text)
        };
        string json = JsonConvert.SerializeObject(scoreRequest);
        StartCoroutine(requestHandler.PostRequest("/submit-level-score", json, OnScoreResponse));
    }

    private int computeScore(float userCost, float optimalCost)
    {
        if (userCost <= optimalCost)
        {
            float bonus = 10f * (1f - (userCost / optimalCost));
            float score = 100f + bonus;
            return (int)score;
        }

        float deviation = userCost - optimalCost;
        float scale = optimalCost * 1f;
        return (int)(100f / (1f + deviation / scale));
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
            int Score = computeScore((float.Parse(metricViewContent.transform.Find("Metabolic_Cost").GetComponent<Text>().text)), OptimalPathStorage.optimalMetabolicCost);
            float Distance3DDiff = ((float.Parse(metricViewContent.transform.Find("3D_Distance").GetComponent<Text>().text) - OptimalPathStorage.optimalTotal3DDistance) / OptimalPathStorage.optimalTotal3DDistance) * 100;
            float Distance2DDiff = ((float.Parse(metricViewContent.transform.Find("2D_Distance").GetComponent<Text>().text) - OptimalPathStorage.optimalTotal2DDistance) / OptimalPathStorage.optimalTotal2DDistance) * 100;
            float SlopeDiff = ((float.Parse(metricViewContent.transform.Find("Total_Slope").GetComponent<Text>().text) - OptimalPathStorage.optimalTotalSlope) / OptimalPathStorage.optimalTotalSlope) * 100;
            float PositiveSlopeDiff = ((float.Parse(metricViewContent.transform.Find("Total_Positive_Slope").GetComponent<Text>().text) - OptimalPathStorage.optimalTotalPositiveSlope) / OptimalPathStorage.optimalTotalPositiveSlope) * 100;
            float NegativeSlopeDiff = ((float.Parse(metricViewContent.transform.Find("Total_Negative_Slope").GetComponent<Text>().text) - OptimalPathStorage.optimalTotalNegativeSlope) / OptimalPathStorage.optimalTotalNegativeSlope) * 100;
            float MetabolicCostDiff = ((float.Parse(metricViewContent.transform.Find("Metabolic_Cost").GetComponent<Text>().text) - OptimalPathStorage.optimalMetabolicCost) / OptimalPathStorage.optimalMetabolicCost) * 100;
            float userAvalanches = float.Parse(metricViewContent.transform.Find("Number_Avalanche").GetComponent<Text>().text);
            float optimal = OptimalPathStorage.optimalAvalanches;
            int AvalancheCount = Mathf.RoundToInt(((userAvalanches - optimal) / optimal) * 100f);

            scoreComparisonPanel.SetActive(true);

            scoreComparisonPanel.transform.Find("Score").GetComponent<Text>().text = $"{Score}";
            scoreComparisonPanel.transform.Find("Total3DDistance").GetComponent<Text>().text = Distance3DDiff >= 0
                ? $"Your 3D distance was {Distance3DDiff}% higher than the optimal."
                : $"Your 3D distance was {Distance3DDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("Total2DDistance").GetComponent<Text>().text = Distance2DDiff >= 0
                ? $"Your 2D distance was {Distance2DDiff}% higher than the optimal."
                : $"Your 2D distance was {Distance2DDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalSlope").GetComponent<Text>().text = SlopeDiff >= 0
                ? $"Your slope was {SlopeDiff}% higher than the optimal."
                : $"Your slope was {SlopeDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalPositiveSlope").GetComponent<Text>().text = PositiveSlopeDiff >= 0
                ? $"Your positive slope was {PositiveSlopeDiff}% higher than the optimal."
                : $"Your positive slope was {PositiveSlopeDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalNegativeSlope").GetComponent<Text>().text = NegativeSlopeDiff >= 0
                ? $"Your negative slope was {NegativeSlopeDiff}% higher than the optimal."
                : $"Your negative slope was {NegativeSlopeDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("MetabolicCost").GetComponent<Text>().text = MetabolicCostDiff >= 0
                ? $"Your metabolic cost was {MetabolicCostDiff}% higher than the optimal."
                : $"Your metabolic cost was {MetabolicCostDiff}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("AvalancheAmount").GetComponent<Text>().text = AvalancheCount >= 0
                ? $"The risk of avalanche was {AvalancheCount}% higher than the optimal."
                : $"The risk of avalanche was {AvalancheCount}% lower than the optimal.";

            if (PlayerPrefs.GetInt("hasAvalancheFile") == 0) scoreComparisonPanel.transform.Find("AvalancheAmount").gameObject.SetActive(false);
            else scoreComparisonPanel.transform.Find("AvalancheAmount").gameObject.SetActive(true);

            scoreComparisonPanel.transform.Find("Total3DDistance").GetComponent<Text>().color = Distance3DDiff >= 0 ? new Vector4(0.75f,0,0,1) : new Vector4(0,0.75f,0,1);
            scoreComparisonPanel.transform.Find("Total2DDistance").GetComponent<Text>().color = Distance2DDiff >= 0 ? new Vector4(0.75f,0,0,1) : new Vector4(0,0.75f,0,1);
            scoreComparisonPanel.transform.Find("TotalSlope").GetComponent<Text>().color = SlopeDiff >= 0 ? new Vector4(0.75f, 0, 0, 1) : new Vector4(0, 0.75f, 0, 1);
            scoreComparisonPanel.transform.Find("TotalPositiveSlope").GetComponent<Text>().color = PositiveSlopeDiff >= 0 ? new Vector4(0.75f,0,0,1) : new Vector4(0,0.75f,0,1);
            scoreComparisonPanel.transform.Find("TotalNegativeSlope").GetComponent<Text>().color = NegativeSlopeDiff >= 0 ? new Vector4(0.75f,0,0,1) : new Vector4(0,0.75f,0,1);
            scoreComparisonPanel.transform.Find("MetabolicCost").GetComponent<Text>().color = MetabolicCostDiff >= 0 ? new Vector4(0.75f,0,0,1) : new Vector4(0,0.75f,0,1);
            scoreComparisonPanel.transform.Find("AvalancheAmount").GetComponent<Text>().color = AvalancheCount >= 0 ? new Vector4(0.75f,0,0,1) : new Vector4(0,0.75f,0,1);

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
        cameraModeManager.SwitchMode(CameraModeManager.Mode.ThirdPerson, Vector3.zero);
        savedScorePanel.SetActive(false);
        SceneManager.LoadScene("TerrainSelector");
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

    public void onMainMenuClick()
    {
        cameraModeManager.SwitchMode(CameraModeManager.Mode.ThirdPerson, Vector3.zero);
        savedScorePanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void onLevelMenuClick()
    {
        cameraModeManager.SwitchMode(CameraModeManager.Mode.ThirdPerson, Vector3.zero);
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
        if (Input.GetKeyDown(KeyCode.Alpha3) && waypointStart != null && waypointEnd != null)
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.VR, waypoints[waypoints.Count - 1]);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson && currentCameraMode != 1)
        {
            currentCameraMode = 1;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.ThirdPerson && currentCameraMode != 0)
        {
            currentCameraMode = 0;
            lineRenderer.startWidth = 50.0f;
            lineRenderer.endWidth = 50.0f;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.VR && currentCameraMode != 2)
        {
            currentCameraMode = 2;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
        }



        if (waypointEnd != null && waypointStart != null)
        {
            if (!canDraw && waypoints.Count > 0 && Vector3.Distance(waypointEnd.transform.position, waypoints[waypoints.Count - 1]) < 200) finishDrawing.interactable = true;
            else finishDrawing.interactable = false;

            if (canDraw || (lineRenderer.positionCount > 0 && lineRenderer.GetPosition(lineRenderer.positionCount - 1) != waypointEnd.transform.position))
            {
                saveScore.interactable = false;
            }
            else saveScore.interactable = true;

            UpdateLine();
        }
    }
}
