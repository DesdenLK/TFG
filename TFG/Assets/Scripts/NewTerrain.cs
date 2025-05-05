using NUnit.Framework;
using SFB;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Bson;


public class TextureFile
{
    public string textureFileName;
    public byte[] textureFileBytes;
}

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
    public string rawFileName;
    public byte[] rawFileBytes;
    public List<TextureFile> textureFiles;
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
    public Transform textureListContent;
    public GameObject textureLabelPrefab;

    public Text rawFileText;

    public Button createTerrainButton;
    public Button backButton;
    private Requests requestHandler;
    private byte[] rawFileBytes;
    private List<TextureFile> textureFiles;

    private bool creatingTerrain = false;

    public void AddTextureFileToList(TextureFile textureFile)
    {
        GameObject item = Instantiate(textureLabelPrefab, textureListContent);

        
        Text fileNameText = item.transform.Find("FileNameText").GetComponent<Text>();
        Button actionButton = item.transform.Find("ActionButton").GetComponent<Button>();

        fileNameText.text = textureFile.textureFileName;

        actionButton.onClick.AddListener(() => {
            Debug.Log("Botón pulsado para " + textureFile.textureFileName);
            textureFiles.Remove(textureFile);

            foreach (Transform child in textureListContent)
            {
                if (child.transform.Find("FileNameText").GetComponent<Text>().text == textureFile.textureFileName)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        });
    }


    private void Start()
    {
        requestHandler = new Requests();
        terrainRequest = new TerrainRequest();
    }

    private void Update()
    {
        if (name.text.Length <= 0 || description.text.Length <= 0 || widthResolution.text.Length <= 0 || heightResolution.text.Length <= 0 || 
            size_x.text.Length <= 0 || size_y.text.Length <= 0 || size_z.text.Length <= 0 || rawFileText.text == "No file selected")
        {
            createTerrainButton.interactable = false;
        }
        else
        {
            if (!creatingTerrain) createTerrainButton.interactable = true;
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
            terrainRequest.rawFileName = rawFileText.text;
            terrainRequest.rawFileBytes = rawFileBytes;

        }
    }

    private void clearFields()
    {
        name.text = "";
        description.text = "";
        widthResolution.text = "";
        heightResolution.text = "";
        size_x.text = "";
        size_y.text = "";
        size_z.text = "";
        rawFileText.text = "No file selected";
        textureFiles.Clear();
        foreach (Transform child in textureListContent)
        {
            Destroy(child.gameObject);
        }
    }
    public void OnCreateTerrainButtonClick()
    {
        creatingTerrain = true;
        backButton.interactable = false;
        createTerrainButton.interactable = false;
        string json = JsonConvert.SerializeObject(terrainRequest);
        StartCoroutine(requestHandler.PostRequest(createTerrainUrl, json, OnCreateTerrainResponse));
        
    }
    private void OnCreateTerrainResponse(string response)
    {
        backButton.interactable = true;
        createTerrainButton.interactable = true;

        if (response.Contains("ERROR"))
        {
            Debug.Log("Error on the request " + response);
        }
        else
        {
            clearFields();
            creatingTerrain = false;
            Debug.Log("Terrain created successfully: " + response);
        }
    }

    public void OpenFileExplorerTexture()
    {
        var extensions = new[] {
            new ExtensionFilter("Texture Files", "png")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", extensions, true);
        if (textureFiles == null) textureFiles = new List<TextureFile>();

        foreach (var path in paths)
        {
            TextureFile textureFile = new TextureFile
            {
                textureFileName = Path.GetFileName(path),
                textureFileBytes = File.ReadAllBytes(path)
            };
            textureFiles.Add(textureFile);
            AddTextureFileToList(textureFile);
        }
        terrainRequest.textureFiles = textureFiles;
    }
    public void OpenFileExplorerRaw()
    {
        // Filtro opcional (ej: solo imágenes)
        var extensions = new[] {
            new ExtensionFilter("Raw Files", "raw"),
            //new ExtensionFilter("All Files", "*"),
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];
            rawFileText.text = Path.GetFileName(filePath);

            rawFileBytes = File.ReadAllBytes(filePath);
        }
    }

    public void onBackButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
