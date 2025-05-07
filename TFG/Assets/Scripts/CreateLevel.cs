using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class LevelRequest
{
    public string name;
    public string description;
    public string terrain_uuid;
    public float start_X;
    public float start_Y;
    public float start_Z;
    public float end_X;
    public float end_Y;
    public float end_Z;
    public string creator;
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
    public GameObject responsePanel;


    private GameObject waypointStart;
    private GameObject waypointEnd;

    private bool isPlacingStart = false;
    private bool isPlacingEnd = false;

    public Terrain terrain;

    private Requests requestHandler;

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
        placeStartButton.interactable = false;
        placeEndButton.interactable = false;
        createLevelButton.interactable = false;
        LevelRequest levelRequest = new LevelRequest
        {
            name = levelName.text,
            description = levelDescription.text,
            terrain_uuid = PlayerPrefs.GetString("TerrainUUID"),
            start_X = waypointStart.transform.position.x,
            start_Y = waypointStart.transform.position.y,
            start_Z = waypointStart.transform.position.z,
            end_X = waypointEnd.transform.position.x,
            end_Y = waypointEnd.transform.position.y,
            end_Z = waypointEnd.transform.position.z,
            creator = PlayerPrefs.GetString("username", "Guest")
        };
        string json = JsonConvert.SerializeObject(levelRequest);
        StartCoroutine(requestHandler.PostRequest("/create-level/" + PlayerPrefs.GetString("TerrainUUID"), json, OnCreateLevelResponse));
    }

    public void onBackButtonClick()
    {
        SceneManager.LoadScene("LevelSelector");
    }

    private void OnCreateLevelResponse(string response)
    {
        responsePanel.SetActive(true);
        if (response.Contains("ERROR"))
        {
            levelResponseText.text = "Error creating level";
            levelResponseText.color = Color.red;
            return;
        }
        levelResponseText.text = "Level created successfully!";
        levelResponseText.color = Color.green;
        Debug.Log("Level created: " + response);
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
        if (waypointStart != null && waypointEnd != null && levelName.text != "" && levelDescription.text != "")
        {
            createLevelButton.interactable = true;
        }
        else
        {
            createLevelButton.interactable = false;
        }
        UpdatePoints();
    }
}
