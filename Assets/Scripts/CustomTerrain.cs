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

    private float[,] GetHeights()
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
        float[,] heightMap = GetHeights();
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
        float[,] heightMap = GetHeights();
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
        float[,] heightMap = GetHeights();
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
        float[,] heightMap = GetHeights();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void VoronoiTessellation()
    {
        float[,] heightMap = GetHeights();

        for (int i = 0; i < voronoiPeaksCount; i++)
        {
            float peakHeight = Random.Range(voronoiMinHeight, voronoiMaxHeight);
            int randomLocationX = Random.Range(0, terrainData.heightmapWidth);
            int randomLocationZ = Random.Range(0, terrainData.heightmapHeight);
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