using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEditor.SearchService;
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
}

public class TerrainMenu : MonoBehaviour
{
    public GameObject NotDownloadedPanel;
    public Transform panelTransform;

    public Texture2D colorTexture;
    public Texture2D blackTexture;

    private Requests requestHandler;
    private List<TerrainGet> terrainList;

    public GameObject DownloadPanel;




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
        if (username == "")
        {
            username = "Luca";
        }
        string terrainsUrl = "/terrains/" + username;
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


    private void createMenu()
    {
        foreach (Transform child in panelTransform)
        {
            Destroy(child.gameObject);
        }

        string path = Path.Combine(Application.persistentDataPath, "terrains");
        for (int i = 0; i < terrainList.Count; i++)
        {
            if (!Directory.Exists(Path.Combine(path, terrainList[i].uuid)))
            {

                GameObject newPanelButton = Instantiate(NotDownloadedPanel, panelTransform);

                newPanelButton.GetComponentInChildren<Text>().text = terrainList[i].name;

                Image imageComponent = newPanelButton.GetComponentInChildren<Image>();
                Sprite sprite = Sprite.Create(blackTexture, new Rect(0, 0, blackTexture.width, blackTexture.height), new Vector2(0.5f, 0.5f));
                imageComponent.sprite = sprite;

                int index = i;
                newPanelButton.GetComponentInChildren<Button>().onClick.AddListener(() => DownloadTerrainMenu(index));
            }
        }
    }

    public void closeDownloadPanel()
    {
        DownloadPanel.SetActive(false);
    }

    public void backButtonTerrainMenu()
    {
        SceneManager.LoadScene("TerrainSelector");
    }

    private void DownloadTerrainMenu(int index)
    {
        DownloadPanel.SetActive(true);
        DownloadPanel.transform.Find("Name").GetComponent<Text>().text = terrainList[index].name;
        DownloadPanel.transform.Find("Description").GetComponent<Text>().text = terrainList[index].description;
    }
}
