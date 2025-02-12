using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static TerrainLoader;

public class TerrainLoader : MonoBehaviour
{
    [System.Serializable]
    public class TerrainInfo
    {
        public int heightmapResolution;
        public int widthmapResolution;
        public Vector3 size;
        public string rawFilePath;
    }



    public InputField folderPathInput;
    public GameObject terrainObject;
    private Terrain terrain;
    private TerrainInfo terrainInfo;

    void Start()
    {
        terrain = terrainObject.GetComponent<Terrain>();
    }

    public void loadTerrain()
    {
        if (folderPathInput != null) { 
            string folderPath = folderPathInput.text;
            terrainInfo = LoadTerrainInfo(folderPath);


            if (terrainInfo != null && terrain != null)
            {
                terrainObject.SetActive(true);
                float[,] heightMap = LoadRaw16(Path.Combine(folderPath, terrainInfo.rawFilePath), terrainInfo.widthmapResolution, terrainInfo.widthmapResolution);
                FlipHeightMapVertically(ref heightMap);
                ApplyHeightMapToTerrain(heightMap);
            }
        }
    }

    TerrainInfo LoadTerrainInfo(string path)
    {
        string infoPath = Path.Combine(path, "info.json");
        if (File.Exists(infoPath))
        {
            string jsonContent = File.ReadAllText(infoPath);
            return JsonUtility.FromJson<TerrainInfo>(jsonContent);
        }
        else
        {
            Debug.Log("No se encontró el archivo JSON.");
            return null;
        }
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
        Debug.Log("Applying heightmap to terrain: " + width + "x" + height);
        TerrainData terrainData = terrain.terrainData;

        terrainData.heightmapResolution = width + 1;
        terrainData.size = terrainData.size = new Vector3(
            width * terrainInfo.size.x,
            1 * terrainInfo.size.y,
            height * terrainInfo.size.z
        );

        terrainData.SetHeights(0, 0, heightMap);
    }
}
