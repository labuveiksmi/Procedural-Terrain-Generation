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

    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;

    public void Perlin()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] = Mathf.PerlinNoise(x * perlinXScale, y * perlinYScale);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] = heighMapImage.GetPixel((int)heighMapScale.x * x,
                                                        (int)heighMapScale.z * z)
                                                        .grayscale * heighMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += Random.Range(randomHeightRange.x, randomHeightRange.y);
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