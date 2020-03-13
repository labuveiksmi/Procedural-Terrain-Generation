using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange;
    public Texture2D heighMapImage;
    public Vector3 heighMapScale = Vector3.one;
    public Terrain terrain;
    public TerrainData terrainData;

    public bool resetTerrain = false;

    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinXOffset = 0;
    public int perlinYOffset = 0;
    public int pelinOctaves = 0;
    public float perlinPersistance = 8f;
    public float perlinHeightScale = 0.09f;

    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMaxHeight = 1f;
    public float voronoiMinHeight = 0f;
    public int voronoiPeaksCount = 1;

    public float mpRoughtness = 2f;
    public float mpMinHeight = 0f;
    public float mpMaxHeight = 1f;
    public float mpDampererPower = 1f;

    public float blendingOffset = 0.01f;
    public float blendingNoiseMultiplier = 0.2f;
    public float blendingNoiseParams = 0.01f;

    public int smoothAmount = 1;
    public VoronoiFunction voronoiFunction;

    public enum VoronoiFunction
    {
        Linear,
        Power,
        Combined,
        SinPow
    }

    [System.Serializable]
    public class PerlinParametrs
    {
        public float perlinXScale = 0.01f;
        public float perlinYScale = 0.01f;
        public int perlinXOffset = 0;
        public int perlinYOffset = 0;
        public int pelinOctaves = 0;
        public float perlinPersistance = 8f;
        public float perlinHeightScale = 0.09f;
        public bool remove = false;
    }

    public List<PerlinParametrs> perlinParamentrs = new List<PerlinParametrs>()
    {
        new PerlinParametrs()
    };

    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public bool remove = false;
    }

    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();

        foreach (SplatHeights splat in splatHeights)
        {
            if (!splat.remove)
            {
                keptSplatHeights.Add(splat);
            }
        }

        if (keptSplatHeights.Count == 0)
        {
            keptSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keptSplatHeights;
    }

    public void SplatMaps()
    {
        List<TerrainLayer> newTerrainLayers = new List<TerrainLayer>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            newTerrainLayers.Add(new TerrainLayer());
            newTerrainLayers[i].diffuseTexture = splatHeights[i].texture;
            newTerrainLayers[i].tileOffset = splatHeights[i].tileOffset;
            newTerrainLayers[i].tileSize = splatHeights[i].tileSize;
            newTerrainLayers[i].diffuseTexture.Apply(true);
        }
        terrainData.terrainLayers = newTerrainLayers.ToArray();

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                        terrainData.heightmapHeight);
        float[,,] splatMapData = new float[terrainData.alphamapWidth,
            terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    float noise = Mathf.PerlinNoise(x * blendingNoiseParams,
                        y * blendingNoiseParams) * blendingNoiseMultiplier;
                    float offset = blendingOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;

                    if (heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop)
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);

                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatMapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    private void NormalizeVector(float[] splat)
    {
        float total = 0;
        foreach (var v in splat)
        {
            total += v;
        }
        for (int i = 0; i < splat.Length; i++)
        {
            splat[i] /= total;
        }
    }

    private float[,] GetHeightMap()
    {
        float[,] heightMap;
        if (resetTerrain)
        {
            heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        }
        else
        {
            heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        }
        return heightMap;
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += Utils.FractalBrownianMotion((x + perlinXOffset) * perlinXScale,
                    (y + perlinYOffset) * perlinYScale,
                    pelinOctaves, perlinPersistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                foreach (PerlinParametrs parametrs in perlinParamentrs)
                {
                    heightMap[x, y] += Utils.FractalBrownianMotion((x + parametrs.perlinXOffset) * parametrs.perlinXScale,
                        (y + parametrs.perlinYOffset) * parametrs.perlinYScale,
                        parametrs.pelinOctaves, parametrs.perlinPersistance) * parametrs.perlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParamentrs.Add(new PerlinParametrs());
    }

    public void RemovePerlin()
    {
        List<PerlinParametrs> keptPerlinParametrs = new List<PerlinParametrs>();

        for (int i = 0; i < perlinParamentrs.Count; i++)
        {
            if (!perlinParamentrs[i].remove)
            {
                keptPerlinParametrs.Add(perlinParamentrs[i]);
            }
        }
        if (keptPerlinParametrs.Count == 0)
        {
            keptPerlinParametrs.Add(perlinParamentrs[0]);
        }
        perlinParamentrs = keptPerlinParametrs;
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += heighMapImage.GetPixel((int)heighMapScale.x * x,
                                                        (int)heighMapScale.z * z)
                                                        .grayscale * heighMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void VoronoiTessellation()
    {
        float[,] heightMap = GetHeightMap();

        for (int i = 0; i < voronoiPeaksCount; i++)
        {
            float peakHeight = UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight);
            int randomLocationX = UnityEngine.Random.Range(0, terrainData.heightmapWidth);
            int randomLocationZ = UnityEngine.Random.Range(0, terrainData.heightmapHeight);
            Vector3 peak = new Vector3(randomLocationX, peakHeight, randomLocationZ);

            if (heightMap[(int)peak.x, (int)peak.z] > peak.y)
            {
                continue;
            }
            heightMap[(int)peak.x, (int)peak.z] = peak.y;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);

            float maxDistance = Vector2.Distance(Vector2.zero,
                                                    new Vector2(terrainData.heightmapWidth,
                                                                terrainData.heightmapHeight));
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float height;
                        if (voronoiFunction == VoronoiFunction.Power)
                        {
                            height = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff);
                        }
                        else if (voronoiFunction == VoronoiFunction.Combined)
                        {
                            height = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) - distanceToPeak * voronoiDropOff;
                        }
                        else if (voronoiFunction == VoronoiFunction.SinPow)
                        {
                            height = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff)
                                - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff;
                        }
                        else
                        {
                            height = peak.y - distanceToPeak * voronoiDropOff;
                        }

                        if (height > heightMap[x, y])
                        {
                            heightMap[x, y] = height;
                        }
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MidpointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth - 1;

        int cornerX, cornerY;
        int squareSize = width;
        float heightMin = mpMinHeight;
        float heightMax = mpMaxHeight;

        float heightDampener = Mathf.Pow(mpDampererPower, -1 * mpRoughtness);
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;
                    midX = (x + cornerX) / 2;
                    midY = (y + cornerY) / 2;

                    heightMap[midX, midY] = (heightMap[x, y] +
                                            heightMap[x, cornerY] +
                                            heightMap[cornerX, y] +
                                            heightMap[cornerX, cornerY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax);
                }
            }
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = x + squareSize / 2;
                    midY = y + squareSize / 2;

                    pmidXR = midX + squareSize;
                    pmidYU = midY + squareSize;
                    pmidXL = midX - squareSize;
                    pmidYD = midY - squareSize;

                    if (pmidXL <= 0 || pmidYD <= 0 ||
                        pmidXR >= width - 1 || pmidYU >= width - 1)
                    {
                        continue;
                    }

                    heightMap[midX, y] = (heightMap[x, y] +
                                            heightMap[midX, midY] +
                                            heightMap[cornerX, y] +
                                            heightMap[midX, pmidYD]) / 4f + UnityEngine.Random.Range(heightMin, heightMax);
                    heightMap[x, midY] = (heightMap[x, y] +
                                            heightMap[midX, midY] +
                                            heightMap[x, cornerY] +
                                            heightMap[pmidXL, midY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax);
                    heightMap[cornerX, midY] = (heightMap[cornerX, cornerY] +
                                            heightMap[midX, midY] +
                                            heightMap[cornerX, y] +
                                            heightMap[pmidXR, midY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax);
                    heightMap[midX, cornerY] = (heightMap[midX, pmidYU] +
                                           heightMap[cornerX, cornerY] +
                                           heightMap[midX, midY] +
                                           heightMap[x, cornerY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax);
                }
            }

            squareSize = squareSize / 2;
            heightMax *= heightDampener;
            heightMin *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        float progress = 0;
        EditorUtility.DisplayProgressBar("Smoothing terrain", "Smoothing in progress", progress);
        for (int i = 0; i <= smoothAmount; i++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float avgHeight = heightMap[x, y];

                    List<Vector2> neighboards = GenerateNeighbords(new Vector2(x, y), terrainData.heightmapHeight,
                                                                   terrainData.heightmapWidth);
                    foreach (Vector2 vector2 in neighboards)
                    {
                        avgHeight += heightMap[(int)vector2.x, (int)vector2.y];
                    }
                    heightMap[x, y] = avgHeight / (neighboards.Count + 1); ;
                }
            }
            progress++;
            EditorUtility.DisplayProgressBar("Smoothing terrain", "Smoothing in progress", progress / smoothAmount);
        }
        EditorUtility.ClearProgressBar();

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    private List<Vector2> GenerateNeighbords(Vector2 position, int width, int height)
    {
        List<Vector2> neighboards = new List<Vector2>();

        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 neighboardPosition = new Vector2(Mathf.Clamp(position.x + x, 0, width - 1),
                                                             Mathf.Clamp(position.y + y, 0, height - 1));
                    if (!neighboards.Contains(neighboardPosition))
                    {
                        neighboards.Add(neighboardPosition);
                    }
                }
            }
        }

        return neighboards;
    }

    private void OnEnable()
    {
        Debug.Log("Initializing terrain data");
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    private void Awake()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        Debug.Log(tagManager.ToString());
        AddTag(tagsProperty, "Terrain");
        AddTag(tagsProperty, "Cloud");
        AddTag(tagsProperty, "Shore");

        tagManager.ApplyModifiedProperties();

        gameObject.tag = "Terrain";
    }

    private void AddTag(SerializedProperty tagProperty, string newTag)
    {
        bool found = false;

        for (int i = 0; i < tagProperty.arraySize; i++)
        {
            SerializedProperty property = tagProperty.GetArrayElementAtIndex(i);
            if (property.stringValue.Equals(newTag))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagProperty.InsertArrayElementAtIndex(0);
            SerializedProperty property = tagProperty.GetArrayElementAtIndex(0);
            property.stringValue = newTag;
        }
    }
}