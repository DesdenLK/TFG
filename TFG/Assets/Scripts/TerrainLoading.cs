using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public string[] textureFiles;
        public string avalancheFilePath;
    }

    public GameObject terrainObject;
    public Dropdown dropdown;
    public CameraModeManager cameraModeManager;
    private Terrain terrain;
    private TerrainInfo terrainInfo;
    private TerrainLayer[] terrainLayers;
    private float[,] heightMap;
    private int[] avalancheValues;
    private Vector3 terrainPos;


    private TerrainLayer avalancheLayer;

    void Start()
    {
        terrain = terrainObject.GetComponent<Terrain>();
        terrainLayers = new TerrainLayer[] { };
        dropdown.ClearOptions();
        loadTerrain();
    }

    // Carrega el terreny seleccionat des de PlayerPrefs
    public void loadTerrain()
    {
        string selectedTerrain = PlayerPrefs.GetString("SelectedTerrain");
        if (selectedTerrain != null) {
            string folderPath = selectedTerrain;
            terrainInfo = LoadTerrainInfo(folderPath);


            if (terrainInfo != null && terrain != null)
            {
                terrainObject.SetActive(true);
                float[,] heightMap = LoadRaw16(Path.Combine(folderPath, terrainInfo.rawFilePath), terrainInfo.widthmapResolution, terrainInfo.widthmapResolution);
                FlipHeightMapVertically(ref heightMap);
                ApplyHeightMapToTerrain(heightMap);
                avalancheValues = LoadAvalancheMap(Path.Combine(folderPath, terrainInfo.avalancheFilePath), terrainInfo.widthmapResolution, terrainInfo.heightmapResolution);
                if (avalancheValues != null)
                {
                    FlipAvalancheValuesVertically(ref avalancheValues, terrainInfo.widthmapResolution, terrainInfo.heightmapResolution);
                    PlayerPrefs.SetInt("hasAvalancheFile", 1);

                    //Texture2D avalancheTexture = GenerateAvalancheTerrainTexture(avalancheValues, terrainInfo.widthmapResolution, terrainInfo.heightmapResolution);

                    //avalancheLayer = new TerrainLayer();
                    //avalancheLayer.diffuseTexture = avalancheTexture;
                    //avalancheLayer.smoothnessSource = TerrainLayerSmoothnessSource.Constant;
                    //avalancheLayer.tileSize = new Vector2(terrainInfo.widthmapResolution * terrainInfo.size.x - 1, terrainInfo.heightmapResolution * terrainInfo.size.z);
                }
                else PlayerPrefs.SetInt("hasAvalancheFile", 0);
                //FlipAvalancheValuesHorizontally(ref avalancheValues, terrainInfo.widthmapResolution, terrainInfo.heightmapResolution);
                addTerrainLayers(folderPath, terrainInfo);
                terrainPos = terrain.GetPosition();
            }
        }
    }

    // Carrega la informaci� del terreny des d'un fitxer JSON
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
            Debug.Log("No se encontr� el archivo JSON.");
            return null;
        }
    }

    // Carrega el heightMap des d'un fitxer RAW de 16 bits
    float[,] LoadRaw16(string path, int width, int height)
    {
        byte[] rawData = File.ReadAllBytes(path);

        if (rawData.Length != width * height * 2)
        {
            Debug.LogError("El tama�o del archivo RAW no coincide con las dimensiones esperadas.");
            return null;
        }

        heightMap = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 2; // Dos bytes por p�xel (16 bits)
                ushort heightValue = BitConverter.ToUInt16(rawData, index);
                heightMap[y, x] = heightValue / 65535f; // Normalizar a [0,1]
            }
        }
        return heightMap;
    }

    // Fa un flip vertical del heightMap
    void FlipHeightMapVertically(ref float[,] heightMap)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);

        // Fa un flip vertical del heightMap
        for (int y = 0; y < height / 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Intercanvia els valors de la fila y amb la fila height - y - 1
                float temp = heightMap[y, x];
                heightMap[y, x] = heightMap[height - y - 1, x];
                heightMap[height - y - 1, x] = temp;
            }
        }
    }

    // Aplica el heightMap al terreny
    void ApplyHeightMapToTerrain(float[,] heightMap)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);
        Debug.Log("Applying heightmap to terrain: " + width + "x" + height);
        TerrainData terrainData = terrain.terrainData;

        terrainData.heightmapResolution = width;
        terrainData.size = terrainData.size = new Vector3(
            width * terrainInfo.size.x,
            1 * terrainInfo.size.y,
            height * terrainInfo.size.z
        );

        terrainData.SetHeights(0, 0, heightMap);
    }

    // Afegeix les capes de terreny al terreny
    private void addTerrainLayers(string path, TerrainInfo tInfo)
    {
        List<TerrainLayer> validTerrainLayers = new List<TerrainLayer>();
        List<string> textureOptions = new List<string>();

        for (int i = 0; i < tInfo.textureFiles.Length; i++)
        {
            Texture2D texture = LoadTextureFromFile(Path.Combine(path, tInfo.textureFiles[i]), tInfo.widthmapResolution, tInfo.heightmapResolution);

            if (texture != null)
            {
                TerrainLayer terrainLayer = new TerrainLayer();
                terrainLayer.diffuseTexture = texture;
                terrainLayer.smoothnessSource = TerrainLayerSmoothnessSource.Constant;
                terrainLayer.tileSize = new Vector2(tInfo.widthmapResolution * tInfo.size.x - 1, tInfo.heightmapResolution * tInfo.size.z);

                validTerrainLayers.Add(terrainLayer);
                textureOptions.Add(tInfo.textureFiles[i]);
            }
        }
        //if (avalancheLayer != null)
        //{
        //    Debug.Log("Adding avalanche layer to terrain layers");
        //    validTerrainLayers.Add(avalancheLayer);
        //    textureOptions.Add("Avalanche Map");
        //}

        terrainLayers = validTerrainLayers.ToArray();
        dropdown.AddOptions(textureOptions);

        if (terrainLayers.Length > 0)
        {
            assignLayerToTerrain(0);
        }
        else
        {
            Debug.LogWarning("No valid terrain layers were created.");
        }
    }

    // Carrega el mapa d'allaus des d'un fitxer
    int[] LoadAvalancheMap(string path, int width, int height)
    {
        if (!File.Exists(path))
        {
            Debug.Log("Terrain with no avalanche file");
            return null;
        }

        string[] tokens = File.ReadAllText(path).Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != width * height)
        {
            Debug.LogError($"Expected {width * height} avalanche values, got {tokens.Length}");
            return null;
        }

        int[] avalancheMap = new int[width * height];

        for (int i = 0; i < avalancheMap.Length; i++)
        {
            if (int.TryParse(tokens[i], out int val))
                avalancheMap[i] = val;
            else
                avalancheMap[i] = 0;
        }

        return avalancheMap;
    }

    // Fa un flip vertical dels valors d'allaus
    void FlipAvalancheValuesVertically(ref int[] avalancheValues, int width, int height)
    {
        for (int y = 0; y < height / 2; y++)
        {
            int oppositeY = height - y - 1;
            for (int x = 0; x < width; x++)
            {
                int topIndex = y * width + x;
                int bottomIndex = oppositeY * width + x;

                int temp = avalancheValues[topIndex];
                avalancheValues[topIndex] = avalancheValues[bottomIndex];
                avalancheValues[bottomIndex] = temp;
            }
        }
    }

    void FlipAvalancheValuesHorizontally(ref int[] avalancheValues, int width, int height)
    {
        int[] flipped = new int[avalancheValues.Length];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = z * width + x;
                int flippedIndex = z * width + (width - 1 - x);
                flipped[flippedIndex] = avalancheValues[originalIndex];
            }
        }

        avalancheValues = flipped;
    }
    //public Texture2D GenerateAvalancheTerrainTexture(int[] values, int width, int height)
    //{
    //    Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
    //    texture.filterMode = FilterMode.Point;

    //    for (int y = 0; y < height; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            int index = y * width + x;
    //            int val = values[index];

    //            float normalized = Mathf.Clamp01(val);
    //            Color color = Color.Lerp(Color.green, Color.red, normalized);

    //            texture.SetPixel(x, y, color);
    //        }
    //    }

    //    texture.Apply();
    //    return texture;
    //}



    // Carrega una textura des d'un fitxer
    Texture2D LoadTextureFromFile(string path, int width, int height)
    {
        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(width, height);

            if (!tex.LoadImage(fileData))
            {
                return null;
            }

            if (tex.width > width || tex.height > height)
            {
                return null;
            }

            float terrainAspect = (float)width / height;
            float textureAspect = (float)tex.width / tex.height;

            if (Mathf.Abs(terrainAspect - textureAspect) > 0.01f)
            {
                return null;
            }

            return tex;
        }
        else
        {
            Debug.LogError("Could not load the texture");
            return null;
        }
    }

    // Assigna una capa de terreny al terreny actual
    public void assignLayerToTerrain(int index)
    {
        if (terrain != null)
        {
            TerrainLayer[] tLayers = new TerrainLayer[] { terrainLayers[index] };
            terrain.terrainData.terrainLayers = tLayers;
        }

        else
        {
            Debug.LogError("Terrain Component not found");
        }
    }

    public float[,] GetHeightMap()
    {
        return heightMap;
    }

    public void onBackButton()
    {
        cameraModeManager.SwitchMode(CameraModeManager.Mode.ThirdPerson, Vector3.zero);
        WaypointStorage.waypointStart = Vector3.negativeInfinity;
        WaypointStorage.waypointEnd = Vector3.negativeInfinity;
        SceneManager.LoadScene(PlayerPrefs.GetString("PreviousScene", "MainMenu"));
    }

    public int[] GetAvalancheValues()
    {
        if (avalancheValues == null || avalancheValues.Length == 0)
        {
            Debug.LogWarning("Avalanche values not set or terrain not loaded.");
            return null;
        }
        return avalancheValues;
    }

    public int GetMapWidth()
    {
        if (terrainInfo != null)
        {
            return terrainInfo.widthmapResolution;
        }
        Debug.LogWarning("Terrain info not set.");
        return 0;
    }

    public Vector3 GetTerrainPosition()
    {
        if (terrain != null)
        {
            return terrain.GetPosition();
        }
        Debug.LogWarning("Terrain not found.");
        return Vector3.zero;
    }

    public float getMetersPerCell()
    {
        if (terrain != null)
        {
            Debug.Log("Terrain info found, meters per cell: " + terrain.terrainData.size.x + " " + heightMap.GetLength(1));
            return terrain.terrainData.size.x / heightMap.GetLength(1);
        }
        Debug.LogWarning("Terrain info not set.");
        return 0f;
    }
}
