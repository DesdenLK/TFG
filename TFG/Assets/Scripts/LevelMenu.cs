using System.Collections.Generic;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using UnityEngine.SocialPlatforms.Impl;
using System.Linq;

public class LevelResponse
{
    public string message;
    public int statuscode;
    public List<LevelsGet> levels;
}

public class LeaderboardResponse
{
    public string message;
    public int statuscode;
    public List<LeaderboardGet> scores;
}

public class LeaderboardGet
{
    public string uuid;
    public string user;
    public float total2D_distance;
    public float total3D_distance;
    public float total_slope;
    public float total_positive_slope;
    public float total_negative_slope;
    public float metabolic_cost;
    public string created_at;
}

public class LevelsGet
{
    public string uuid;
    public string name;
    public string description;
    public float start_X;
    public float start_Y;
    public float start_Z;
    public float end_X;
    public float end_Y;
    public float end_Z;
    public string creator;
    public string creator_uuid;
    public string datetime;
}

public class LevelMenu : MonoBehaviour
{
    private Requests requestHandler;
    public GameObject levelButtonPrefab;
    public Transform levelListContent;
    public GameObject infoPanel;
    private LevelsGet levelSelected;
    public GameObject mainBackButton;
    public GameObject leaderboardPanel;
    public GameObject scoreText;
    public Transform scoresList;

    private List<LeaderboardGet> leadearboardList;

    void Start()
    {
        requestHandler = new Requests();
        string levelsUrl = "/levels/" + PlayerPrefs.GetString("TerrainUUID");
        StartCoroutine(requestHandler.GetRequest(levelsUrl, OnGetLevels));

    }

    private void OnGetLevels(string json)
    {
        LevelResponse levelResponse = JsonConvert.DeserializeObject<LevelResponse>(json);
        if (levelResponse.statuscode == 200)
        {
            foreach (LevelsGet level in levelResponse.levels)
            {
                GameObject levelButton = Instantiate(levelButtonPrefab, levelListContent);
                levelButton.GetComponentInChildren<Text>().text = level.name;
                levelButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    levelSelected = level;
                    infoPanel.transform.Find("Name").GetComponent<Text>().text = level.name;
                    infoPanel.transform.Find("Description").GetComponent<Text>().text = level.description;
                    infoPanel.transform.Find("Creator").GetComponent<Text>().text = level.creator;
                    infoPanel.transform.Find("WaypointStart").GetComponent<Text>().text = "Start: " + level.start_X + ", " + level.start_Y + ", " + level.start_Z;
                    infoPanel.transform.Find("WaypointEnd").GetComponent<Text>().text = "End: " + level.end_X + ", " + level.end_Y + ", " + level.end_Z;
                    infoPanel.transform.Find("PlayButton").GetComponent<Button>().onClick.AddListener(() => onPlayButton());
                    infoPanel.SetActive(true);
                    mainBackButton.SetActive(false);
                });
            }
        }
        else
        {
            Debug.LogError("Failed to load levels: " + levelResponse.message);
        }
    }

    public void onCloseInfoPanelClick()
    {
        infoPanel.SetActive(false);
        mainBackButton.SetActive(true);
    }

    public void onLeaderboardClick()
    {
        leaderboardPanel.transform.Find("Back Button").GetComponent<Button>().interactable = false;
        StartCoroutine(requestHandler.GetRequest("/level-scores/" + levelSelected.uuid, OnGetLeaderboard));
        infoPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
    }

    private void OnGetLeaderboard(string json)
    {
        LeaderboardResponse leaderboardResponse = JsonConvert.DeserializeObject<LeaderboardResponse>(json);
        leadearboardList = leaderboardResponse.scores;
        if (leaderboardResponse.statuscode == 200)
        {
            foreach (LeaderboardGet score in leaderboardResponse.scores)
            {
                GameObject scores = Instantiate(scoreText, scoresList);
                scores.transform.Find("User").GetComponent<Text>().text = score.user;
                scores.transform.Find("Metabolic_Cost").GetComponent<Text>().text = score.metabolic_cost.ToString();
                scores.transform.Find("2D_Distance").GetComponent<Text>().text = score.total2D_distance.ToString();
                scores.transform.Find("3D_Distance").GetComponent<Text>().text = score.total3D_distance.ToString();
                scores.transform.Find("Total Slope").GetComponent<Text>().text = score.total_slope.ToString();
                scores.transform.Find("Total Positive Slope").GetComponent<Text>().text = score.total_positive_slope.ToString();
                scores.transform.Find("Total Negative Slope").GetComponent<Text>().text = score.total_negative_slope.ToString();
            }
        }
        else
        {
            Debug.LogError("Failed to load leaderboard: " + leaderboardResponse.message);
        }
        leaderboardPanel.transform.Find("Back Button").GetComponent<Button>().interactable = true;
    }

    public void onMetabolicCostSort()
    {
        List<LeaderboardGet> sortedList = leadearboardList.OrderBy(x => x.metabolic_cost).ToList();
        scoresList.DestroyChildren();
        createScoreList(sortedList);
    }

    public void on2DDistanceSort()
    {
        List<LeaderboardGet> sortedList = leadearboardList.OrderBy(x => x.total2D_distance).ToList();
        scoresList.DestroyChildren();
        createScoreList(sortedList);
    }

    public void on3DDistanceSort()
    {
        List<LeaderboardGet> sortedList = leadearboardList.OrderBy(x => x.total3D_distance).ToList();
        scoresList.DestroyChildren();
        createScoreList(sortedList);
    }

    public void onTotalSlopeSort()
    {
        List<LeaderboardGet> sortedList = leadearboardList.OrderBy(x => x.total_slope).ToList();
        scoresList.DestroyChildren();
        createScoreList(sortedList);
    }

    public void onTotalPositiveSlopeSort()
    {
        List<LeaderboardGet> sortedList = leadearboardList.OrderBy(x => x.total_positive_slope).ToList();
        scoresList.DestroyChildren();
        createScoreList(sortedList);
    }

    public void onTotalNegativeSlopeSort()
    {
        List<LeaderboardGet> sortedList = leadearboardList.OrderBy(x => x.total_negative_slope).ToList();
        scoresList.DestroyChildren();
        createScoreList(sortedList);
    }

    private void createScoreList(List<LeaderboardGet> scoreList)
    {
        foreach (LeaderboardGet score in scoreList)
        {
            GameObject scores = Instantiate(scoreText, scoresList);
            scores.transform.Find("User").GetComponent<Text>().text = score.user;
            scores.transform.Find("Metabolic_Cost").GetComponent<Text>().text = score.metabolic_cost.ToString();
            scores.transform.Find("2D_Distance").GetComponent<Text>().text = score.total2D_distance.ToString();
            scores.transform.Find("3D_Distance").GetComponent<Text>().text = score.total3D_distance.ToString();
            scores.transform.Find("Total Slope").GetComponent<Text>().text = score.total_slope.ToString();
            scores.transform.Find("Total Positive Slope").GetComponent<Text>().text = score.total_positive_slope.ToString();
            scores.transform.Find("Total Negative Slope").GetComponent<Text>().text = score.total_negative_slope.ToString();
        }
    }

    public void onCloseLeaderboardClick()
    {
        scoresList.DestroyChildren();
        leaderboardPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void onPlayButton()
    {
        PlayerPrefs.SetString("PreviousScene", "LevelSelector");
        PlayerPrefs.SetString("LevelUUID", levelSelected.uuid);
        WaypointStorage.waypointStart = new Vector3(levelSelected.start_X, levelSelected.start_Y, levelSelected.start_Z);
        WaypointStorage.waypointEnd = new Vector3(levelSelected.end_X, levelSelected.end_Y, levelSelected.end_Z);
        SceneManager.LoadScene("OnlineLevel");
    }


    public void onCreateLevelClick()
    {
        SceneManager.LoadScene("CreateLevel");
    }

    public void onBackButtonClick()
    {
        SceneManager.LoadScene("TerrainSelector");
    }
}

public static class TransformExtensions
{
    public static void DestroyChildren(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}
