using System.Collections.Generic;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class LevelResponse
{
    public string message;
    public int statuscode;
    public List<LevelsGet> levels;
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
    public string datetime;
}

public class LevelMenu : MonoBehaviour
{
    private Requests requestHandler;
    public GameObject levelButtonPrefab;
    public Transform levelListContent;

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
                    Debug.Log("Level clicked: " + level.name);
                });
            }
        }
        else
        {
            Debug.LogError("Failed to load levels: " + levelResponse.message);
        }
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
