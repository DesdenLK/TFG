using System;
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
    public Button resetButton;

    public LineRenderer lineRenderer;
    public float minDistance = 0.1f;
    private float minDistanceFirstVR = 0.1f;
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

    private float lineRendererWidth = 50.0f;
    public float minWidth = 2f;
    public float maxWidth = 100f;
    public float widthChangeSpeed = 2f;

    private void Start()
    {
        requestHandler = new Requests();
        waypointStart = Instantiate(waypointPrefab, WaypointStorage.waypointStart, Quaternion.identity);
        waypointEnd = Instantiate(flagPrefab, WaypointStorage.waypointEnd, Quaternion.identity);
        cameraModeManager = GetComponent<CameraModeManager>();
        lineRenderer.startWidth = lineRendererWidth;
        lineRenderer.endWidth = lineRendererWidth;
        minDistance = terrain.terrainData.size.x / terrain.terrainData.heightmapResolution;
    }

    // Actualitza la linea de camí en funció del mode de càmera actual
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
                    if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], currentPos) > minDistanceFirstVR)
                    {
                        waypoints.Add(currentPos - new Vector3(0, 0.7f, 0));
                        lineRenderer.positionCount = waypoints.Count;
                        lineRenderer.SetPositions(waypoints.ToArray());
                    }
                }
                break;
            case CameraModeManager.Mode.VR:
                if (canDraw && cameraModeManager.isVREnabled())
                {
                    Vector3 currentPos = Camera.main.transform.position;
                    if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], currentPos) > minDistanceFirstVR)
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
                        if (waypoints.Count == 0 || Vector3.Distance(waypoints[waypoints.Count - 1], hit.point) > minDistance && Vector3.Distance(waypoints[waypoints.Count - 1], hit.point) < 15)
                        {
                            waypoints.Add(hit.point + new Vector3(0, 0.15f, 0));
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

    // Actualitza l'estat del botó de dibuix
    public void updateToggleInput()
    {
        canDraw = !canDraw;
        resetButton.interactable = !canDraw;

        if (canDraw && cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson)
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.FirstPerson, waypoints[waypoints.Count - 1]);
        }

        if (canDraw && cameraModeManager.currentMode == CameraModeManager.Mode.VR)
        {
            cameraModeManager.SwitchMode(CameraModeManager.Mode.VR, waypoints[waypoints.Count - 1]);
        }
    }

    // Guarda la puntuació del jugador
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

    // Calcula la puntuació basada en el cost metabòlic de l'usuari i el cost òptim
    private int computeScore(float userCost, float optimalCost)
    {
        // Si el cost de l'usuari és menor o igual al cost òptim, es calcula un bonus
        if (userCost <= optimalCost)
        {
            float bonus = 10f * (1f - (userCost / optimalCost));
            float score = 100f + bonus;
            return (int)score;
        }

        // Si el cost de l'usuari és major que el cost òptim, es calcula una penalització
        float deviation = userCost - optimalCost; // Desviació del cost de l'usuari respecte al cost òptim
        float scale = optimalCost * 1f; // Escala per normalitzar la puntuació
        return (int)(100f / (1f + deviation / scale)); // Retorna la puntuació ajustada
    }

    // Gestor de la resposta del servidor després de guardar la puntuació
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
            int AvalancheCount = optimal > 0 ? Mathf.RoundToInt(((userAvalanches - optimal) / optimal) * 100f) : 0;

            scoreComparisonPanel.SetActive(true);

            scoreComparisonPanel.transform.Find("Score").GetComponent<Text>().text = $"{Score}";
            scoreComparisonPanel.transform.Find("Total3DDistance").GetComponent<Text>().text = Distance3DDiff >= 0
                ? $"Your 3D distance was {Distance3DDiff.ToString("F2")}% higher than the optimal."
                : $"Your 3D distance was {Distance3DDiff.ToString("F2")}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("Total2DDistance").GetComponent<Text>().text = Distance2DDiff >= 0
                ? $"Your 2D distance was {Distance2DDiff.ToString("F2")}% higher than the optimal."
                : $"Your 2D distance was {Distance2DDiff.ToString("F2")}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalSlope").GetComponent<Text>().text = SlopeDiff >= 0
                ? $"Your slope was {SlopeDiff.ToString("F2")}% higher than the optimal."
                : $"Your slope was {SlopeDiff.ToString("F2")}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalPositiveSlope").GetComponent<Text>().text = PositiveSlopeDiff >= 0
                ? $"Your positive slope was {PositiveSlopeDiff.ToString("F2")}% higher than the optimal."
                : $"Your positive slope was {PositiveSlopeDiff.ToString("F2")}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("TotalNegativeSlope").GetComponent<Text>().text = NegativeSlopeDiff >= 0
                ? $"Your negative slope was {NegativeSlopeDiff.ToString("F2")}% higher than the optimal."
                : $"Your negative slope was {NegativeSlopeDiff.ToString("F2")}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("MetabolicCost").GetComponent<Text>().text = MetabolicCostDiff >= 0
                ? $"Your metabolic cost was {MetabolicCostDiff.ToString("F2")}% higher than the optimal."
                : $"Your metabolic cost was {MetabolicCostDiff.ToString("F2")}% lower than the optimal.";

            scoreComparisonPanel.transform.Find("AvalancheAmount").GetComponent<Text>().text = AvalancheCount >= 0
                ? $"The risk of avalanche was {AvalancheCount.ToString("F2")}% higher than the optimal."
                : $"The risk of avalanche was {AvalancheCount.ToString("F2")}% lower than the optimal.";

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
        // Gestió de la entrada del teclat per canviar el mode de càmera
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

        // Gestió de la entrada del teclat per canviar l'amplada de la línia
        if (Input.GetKeyDown(KeyCode.LeftBracket) && currentCameraMode == 0)
        {
            float newWidth = Math.Max(minWidth, lineRenderer.startWidth - widthChangeSpeed);
            lineRenderer.startWidth = newWidth;
            lineRenderer.endWidth = newWidth;
            lineRendererWidth = newWidth;
        }

        if (Input.GetKeyDown(KeyCode.RightBracket) && currentCameraMode == 0)
        {
            float newWidth = Math.Min(maxWidth, lineRenderer.startWidth + widthChangeSpeed);
            lineRenderer.startWidth = newWidth;
            lineRenderer.endWidth = newWidth;
            lineRendererWidth = newWidth;
        }

        // Canvia la mida de la linea segons el mode de càmera actual
        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson && currentCameraMode != 1)
        {
            currentCameraMode = 1;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.ThirdPerson && currentCameraMode != 0)
        {
            currentCameraMode = 0;
            lineRenderer.startWidth = lineRendererWidth;
            lineRenderer.endWidth = lineRendererWidth;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.VR && currentCameraMode != 2)
        {
            currentCameraMode = 2;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
        }


        // Gestiona la actualització de la línia de camí
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
