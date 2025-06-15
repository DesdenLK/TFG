using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

[System.Serializable]
public class TerrainsResponse
{
    public string message;
    public int statuscode;
    public List<TerrainGet> terrains;
}
public class TerrainGet
{
    public string name;
    public string description;
    public string uuid;
    public int heightmapResolution;
    public int widthmapResolution;
    public int size_X;
    public int size_Y;
    public int size_Z;
}

public class TerrainFilesResponse
{
    public string message;
    public int statuscode;
    public TerrainFilesGet terrain;
}

public class TextureFileGet
{
    public string textureFileName;
    public byte[] textureFileBytes;
}

public class TerrainFilesGet
{
    public string name;
    public string uuid;
    public string rawFileName;
    public byte[] rawFileBytes;
    public List<TextureFileGet> textureFiles;
    public string avalancheFileName;
    public byte[] avalancheFileBytes;
}

public class TerrainJsonData
{
    public int heightmapResolution;
    public int widthmapResolution;
    public Size size;
    public string rawFilePath;
    public List<string> textureFiles;
    public string avalancheFilePath;

    [System.Serializable]
    public class Size
    {
        public int x;
        public int y;
        public int z;
    }
}

public class TerrainMenu : MonoBehaviour
{
    public GameObject NotDownloadedPanel;
    public GameObject DownloadedPanel;
    public Transform panelTransform;

    public Texture2D colorTexture;
    public Texture2D blackTexture;

    private Requests requestHandler;
    private List<TerrainGet> terrainList;

    public GameObject DownloadPanel;
    private int selectedIndex = -1;

    public GameObject mainBackButton;




    void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "terrains");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        requestHandler = new Requests();
        terrainList = new List<TerrainGet>();
        string username = PlayerPrefs.GetString("username", "Luca");
        int online = PlayerPrefs.GetInt("isOnline", 0);
        if (username == "")
        {
            username = "Luca";
        }
        string terrainsUrl = "/terrains/" + username + "/" + online;
        Debug.Log("Terrains URL: " + terrainsUrl);
        StartCoroutine(requestHandler.GetRequest(terrainsUrl, OnGetTerrains));
    }

    private void OnGetTerrains(string response)
    {
        if (response.Contains("ERROR"))
        {
            Debug.Log("Error on the request " + response);
        }
        else
        {
            TerrainsResponse terrainsResponse = JsonConvert.DeserializeObject<TerrainsResponse>(response);
            Debug.Log("Response: " + response);
            Debug.Log("TerrainsResponse: " + terrainsResponse);
            terrainList = terrainsResponse.terrains;
            Debug.Log($"Terrains recibidos: {terrainList.Count}");
            createMenu();
        }
    }

    Texture2D CargarImagenDesdeArchivo(string ruta)
    {
        if (File.Exists(ruta))
        {
            byte[] datos = File.ReadAllBytes(ruta);
            Texture2D textura = new Texture2D(2, 2);
            textura.LoadImage(datos);
            return textura;
        }
        else
        {
            Debug.LogError("No se encontró la imagen: " + ruta);
            return null;
        }
    }

    private void LoadTerrainOrLevels(int index)
    {
        PlayerPrefs.SetString("SelectedTerrain", Path.Combine(Application.persistentDataPath, "terrains", terrainList[index].uuid));
        PlayerPrefs.SetString("TerrainUUID", terrainList[index].uuid);
        PlayerPrefs.SetString("PreviousScene", "TerrainSelector");
        if (PlayerPrefs.GetInt("isOnline") == 1)
        {
            SceneManager.LoadScene("LevelSelector");
        }
        else SceneManager.LoadScene("SampleScene");
    }

    private void createTerrainList()
    {
        string path = Path.Combine(Application.persistentDataPath, "terrains");
        for (int i = 0; i < terrainList.Count; i++)
        {
            if (!Directory.Exists(Path.Combine(path, terrainList[i].uuid)))
            {

                GameObject newPanelButton = Instantiate(NotDownloadedPanel, panelTransform);

                newPanelButton.GetComponentInChildren<Text>().text = terrainList[i].name;

                int index = i;
                newPanelButton.GetComponentInChildren<Button>().onClick.AddListener(() => DownloadTerrainMenu(index));
            }
            else
            {
                GameObject newPanelButton = Instantiate(DownloadedPanel, panelTransform);

                newPanelButton.GetComponentInChildren<Text>().text = terrainList[i].name;

                int index = i;
                newPanelButton.transform.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadTerrainOrLevels(index));
                newPanelButton.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(() =>
                {
                    string terrainPath = Path.Combine(path, terrainList[index].uuid);
                    if (Directory.Exists(terrainPath))
                    {
                        Directory.Delete(terrainPath, true);
                        createMenu();
                    }
                });
            }
        }
    }

    private void createMenu()
    {
        foreach (Transform child in panelTransform)
        {
            Destroy(child.gameObject);
        }
        createTerrainList();

    }

    public void closeDownloadPanel()
    {
        DownloadPanel.SetActive(false);
        mainBackButton.SetActive(true);
    }

    public void backButtonTerrainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void DownloadTerrainMenu(int index)
    {
        mainBackButton.SetActive(false);
        selectedIndex = index;
        DownloadPanel.SetActive(true);
        DownloadPanel.transform.Find("Name").GetComponent<Text>().text = terrainList[index].name;
        DownloadPanel.transform.Find("Description").GetComponent<Text>().text = terrainList[index].description;
        DownloadPanel.transform.Find("HeightmapResolution").GetComponent<Text>().text = terrainList[index].widthmapResolution + " x " + terrainList[index].heightmapResolution;
        DownloadPanel.transform.Find("GridSize").GetComponent<Text>().text = terrainList[index].size_X + " x " + terrainList[index].size_Y + " x " + terrainList[index].size_Z;
    }

    public void onDownloadClick()
    {
        DownloadPanel.transform.Find("DownloadButton").GetComponent<Button>().GetComponentInChildren<Text>().text = "Downloading...";
        DownloadPanel.transform.Find("DownloadButton").GetComponent<Button>().interactable = false;
        DownloadPanel.transform.Find("Back Button").GetComponent<Button>().interactable = false;
        string path = "/download-terrain/" + terrainList[selectedIndex].uuid;
        StartCoroutine(requestHandler.GetRequest(path, OnDownloadTerrain));
        Debug.Log("Download URL: " + path);
    }

    private string createInfoJSON(TerrainFilesGet terainInfo)
    {
        TerrainGet terrainGet = terrainList[selectedIndex];
        TerrainJsonData terrainJsonData = new TerrainJsonData();
        terrainJsonData.heightmapResolution = terrainGet.heightmapResolution;
        terrainJsonData.widthmapResolution = terrainGet.widthmapResolution;
        terrainJsonData.size = new TerrainJsonData.Size();
        terrainJsonData.size.x = terrainGet.size_X;
        terrainJsonData.size.y = terrainGet.size_Y;
        terrainJsonData.size.z = terrainGet.size_Z;
        terrainJsonData.rawFilePath = terainInfo.rawFileName;
        terrainJsonData.textureFiles = new List<string>();
        if (terainInfo.avalancheFileName != null && terainInfo.avalancheFileBytes != null)
        {
            terrainJsonData.avalancheFilePath = terainInfo.avalancheFileName;
        }
        else
        {
            terrainJsonData.avalancheFilePath = null;
        }
        foreach (TextureFileGet textureFile in terainInfo.textureFiles)
        {
            terrainJsonData.textureFiles.Add(textureFile.textureFileName);
        }
        return JsonConvert.SerializeObject(terrainJsonData, Formatting.Indented);
    }

    private void OnDownloadTerrain(string response)
    {
        if (response.Contains("ERROR"))
        {
            Debug.Log("Error on the request " + response);
        }
        else
        {
            TerrainFilesResponse terrainResponse = JsonConvert.DeserializeObject<TerrainFilesResponse>(response);
            TerrainFilesGet terrainFilesGet = terrainResponse.terrain;
            string path = Path.Combine(Application.persistentDataPath, "terrains", terrainFilesGet.uuid);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(Path.Combine(path, terrainFilesGet.rawFileName), terrainFilesGet.rawFileBytes);

            if (terrainFilesGet.avalancheFileBytes != null && terrainFilesGet.avalancheFileName != null)
            {
                File.WriteAllBytes(Path.Combine(path, terrainFilesGet.avalancheFileName), terrainFilesGet.avalancheFileBytes);
            }
            foreach (TextureFileGet textureFile in terrainFilesGet.textureFiles)
            {
                File.WriteAllBytes(Path.Combine(path, textureFile.textureFileName), textureFile.textureFileBytes);
            }

            foreach (Transform child in panelTransform)
            {
                Destroy(child.gameObject);
            }

            string infoJson = createInfoJSON(terrainFilesGet);
            File.WriteAllText(Path.Combine(path, "info.json"), infoJson);

            createTerrainList();
            DownloadPanel.transform.Find("DownloadButton").GetComponent<Button>().GetComponentInChildren<Text>().text = "Download";
            DownloadPanel.transform.Find("DownloadButton").GetComponent<Button>().interactable = true;
            DownloadPanel.transform.Find("Back Button").GetComponent<Button>().interactable = true;
            DownloadPanel.SetActive(false);
            mainBackButton.SetActive(true);
        }
    }
}
