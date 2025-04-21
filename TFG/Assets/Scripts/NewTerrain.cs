using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class TerrainRequest
{
    public string name;
    public string description;
    public int widthmapResolution;
    public int heightmapResolution;
    public int size_X;
    public int size_Y;
    public int size_Z;
    public bool isPublic;
    public string creator;
}
public class NewTerrain : MonoBehaviour
{
    private string createTerrainUrl = "/new-terrain";
    private TerrainRequest terrainRequest;
    public new InputField name;
    public InputField description;
    public InputField widthResolution;
    public InputField heightResolution;
    public InputField size_x;
    public InputField size_y;
    public InputField size_z;
    public Toggle isPublic;

    public Button createTerrainButton;
    private Requests requestHandler;
    private void Start()
    {
        requestHandler = new Requests();
        terrainRequest = new TerrainRequest();
    }

    private void Update()
    {
        if (name.text.Length <= 0 || description.text.Length <= 0 || widthResolution.text.Length <= 0 || heightResolution.text.Length <= 0 || size_x.text.Length <= 0 || size_y.text.Length <= 0 || size_z.text.Length <= 0)
        {
            createTerrainButton.interactable = false;
        }
        else
        {
            createTerrainButton.interactable = true;
            terrainRequest.name = name.text;
            terrainRequest.description = description.text;
            terrainRequest.widthmapResolution = int.Parse(widthResolution.text); // Fixed property name  
            terrainRequest.heightmapResolution = int.Parse(heightResolution.text); // Fixed property name  
            terrainRequest.size_X = int.Parse(size_x.text);
            terrainRequest.size_Y = int.Parse(size_y.text);
            terrainRequest.size_Z = int.Parse(size_z.text);
            terrainRequest.isPublic = isPublic.isOn;
            terrainRequest.creator = PlayerPrefs.GetString("username", "Luca");
            if (terrainRequest.creator == "")
            {
                terrainRequest.creator = "Luca";
            }
        }
    }
    public void OnCreateTerrainButtonClick()
    {
        string json = JsonUtility.ToJson(terrainRequest);
        StartCoroutine(requestHandler.PostRequest(createTerrainUrl, json, OnCreateTerrainResponse));
    }
    private void OnCreateTerrainResponse(string response)
    {
        if (response.Contains("ERROR"))
        {
            Debug.Log("Error on the request " + response);
        }
        else
        {
            Debug.Log("Terrain created successfully: " + response);
        }
    }
}
