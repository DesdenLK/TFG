using System;
using UnityEngine;

public class TerrainLoading : MonoBehaviour
{
    public string demImagePath;
    public Terrain terrain;

    void Start()
    {
        // Desactivar temporalmente el terreno para evitar problemas mientras se actualiza
        terrain.enabled = false;

        // Cargar la textura DEM
        Texture2D demTexture = LoadDemImage(demImagePath);

        // Convertir la textura DEM a un heightmap
        float[,] heightMap = ConvertDemToHeightMap(demTexture);

        // Aplicar el heightmap al terreno
        ApplyHeightMapToTerrain(heightMap);

        // Reactivar el terreno
        terrain.enabled = true;
    }

    // Función para cargar la imagen DEM desde el disco
    Texture2D LoadDemImage(string path)
    {
        Texture2D demTexture = new Texture2D(1, 1);
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        demTexture.LoadImage(fileData);
        return demTexture;
    }

    // Función para convertir la textura DEM a un heightmap
    float[,] ConvertDemToHeightMap(Texture2D demTexture)
    {
        int width = demTexture.width;
        int height = demTexture.height;
        float[,] heightMap = new float[height, width]; // El formato es [y, x] como debe ser

        // Recorrer cada píxel de la textura y mapearlo al heightmap
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = demTexture.GetPixel(x, y);
                Debug.Log("Pixel: " + pixel);
                float normalizedHeight = pixel.r; // El valor está en el rango [0, 1]

                heightMap[y, x] = normalizedHeight; // Asignar el valor directamente
            }
        }
        return heightMap;
    }

    // Función para aplicar el heightmap al terreno
    void ApplyHeightMapToTerrain(float[,] heightMap)
    {
        int width = heightMap.GetLength(1); // Ahora la primera dimensión es Y
        int height = heightMap.GetLength(0);

        Debug.Log("Width: " + width + " Height: " + height);

        // Obtener datos del terreno y asegurar que la resolución es compatible
        TerrainData terrainData = terrain.terrainData;

        // Establecer la resolución del heightmap basada en el tamaño de la textura
        terrainData.heightmapResolution = width + 1; // Ajustar según tus necesidades

        // Ajustar el tamaño del terreno (escala en X y Z)
        terrainData.size = new Vector3(width * 10, 2910-919, height * 10); // Ajusta el factor de escala según tus necesidades

        // Aplicar el heightmap al terreno
        terrainData.SetHeights(0, 0, heightMap);
    }
}
