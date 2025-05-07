using System.Collections.Generic;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelResponse
{
    public string message;
    public int statuscode;
    public List<LevelsGet> levels;
}

public class LevelsGet
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
    public string datetime;
}

public class LevelMenu : MonoBehaviour
{
    private Requests requestHandler;

    void Start()
    {
    }

    public void onCreateLevelClick()
    {
        SceneManager.LoadScene("CreateLevel");
    }
}
