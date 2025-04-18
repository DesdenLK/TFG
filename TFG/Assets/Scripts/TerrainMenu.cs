using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class TerrainMenu : MonoBehaviour
{
    public GameObject prefabButton;
    public Transform panelTransform;

    private int numSubfolders = 0;
    private string[] subfolders = new string[0];


    void Start()
    {
        createMenu();
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
        string terrrainsPath = "Assets/Terrains/";
        if (Directory.Exists(terrrainsPath))
        {
            subfolders = Directory.GetDirectories(terrrainsPath);
            numSubfolders = subfolders.Length;
        }

        for (int i = 0; i < numSubfolders; i++)
        {
            GameObject newPanelButton = Instantiate(prefabButton, panelTransform);

            newPanelButton.GetComponentInChildren<Text>().text = Path.GetFileName(subfolders[i]);

            Image imageComponent = newPanelButton.GetComponentInChildren<Button>().GetComponentInChildren<Image>();
            if (imageComponent != null)
            {
                Texture2D texture = CargarImagenDesdeArchivo(Path.Combine(subfolders[i], "preview.jpg"));
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    imageComponent.sprite = sprite;
                }
            }

            int index = i;
            newPanelButton.GetComponentInChildren<Button>().onClick.AddListener(() => SelectTerrain(index));
        }
    }

    private void SelectTerrain(int index)
    {
        PlayerPrefs.SetString("SelectedTerrain", subfolders[index]);
        PlayerPrefs.SetInt("SelectedTerrainIndex", index);

        SceneManager.LoadScene("SampleScene");
    }
}
