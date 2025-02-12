using System;
using System.IO;
using UnityEngine;

public class TerrainLoader : MonoBehaviour
{
    public string rawFilePath; // Ruta del archivo RAW
    public int rawWidth = 1024;  // Ancho del RAW
    public int rawHeight = 1024; // Alto del RAW
    public Terrain terrain;

    void Start()
    {
        float[,] heightMap = LoadRaw16(rawFilePath, rawWidth, rawHeight);
        FlipHeightMapVertically(ref heightMap); // Voltear el heightMap verticalmente
        ApplyHeightMapToTerrain(heightMap);
    }

    float[,] LoadRaw16(string path, int width, int height)
    {
        byte[] rawData = File.ReadAllBytes(path);

        if (rawData.Length != width * height * 2)
        {
            Debug.LogError("El tamaño del archivo RAW no coincide con las dimensiones esperadas.");
            return null;
        }

        float[,] heightMap = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 2; // Dos bytes por píxel (16 bits)
                ushort heightValue = BitConverter.ToUInt16(rawData, index);
                heightMap[y, x] = heightValue / 65535f; // Normalizar a [0,1]
            }
        }
        return heightMap;
    }

    // Función para voltear verticalmente el heightMap
    void FlipHeightMapVertically(ref float[,] heightMap)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);

        // Voltear las filas del heightMap
        for (int y = 0; y < height / 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Intercambiar las filas
                float temp = heightMap[y, x];
                heightMap[y, x] = heightMap[height - y - 1, x];
                heightMap[height - y - 1, x] = temp;
            }
        }
    }

    void ApplyHeightMapToTerrain(float[,] heightMap)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);
        TerrainData terrainData = terrain.terrainData;

        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width * 10, 2910 - 919, height * 10); // Ajusta según escala real

        terrainData.SetHeights(0, 0, heightMap);
    }
}
