using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public enum TagType
    {
        Tag = 0,
        Layer = 1
    }

    public enum VoronoiFunction
    {
        Linear,
        Power,
        Combined,
        SinPow
    }

    public float blendingNoiseMultiplier = 0.2f;
    public float blendingNoiseParams = 0.01f;

    public float blendingOffset = 0.01f;
    public Texture2D heighMapImage;
    public Vector3 heighMapScale = Vector3.one;

    public int maxTrees = 5000;
    public float mpDampererPower = 1f;
    public float mpMaxHeight = 1f;
    public float mpMinHeight;

    public float mpRoughtness = 2f;
    public int pelinOctaves;
    public float perlinHeightScale = 0.09f;

    public List<PerlinParametrs> perlinParamentrs = new List<PerlinParametrs>
    {
        new PerlinParametrs()
    };

    public float perlinPersistance = 8f;
    public int perlinXOffset;

    public float perlinXScale = 0.01f;
    public int perlinYOffset;
    public float perlinYScale = 0.01f;
    public Vector2 randomHeightRange;

    public bool resetTerrain;

    public int smoothAmount = 1;

    public List<SplatHeights> splatHeights = new List<SplatHeights>
    {
        new SplatHeights()
    };

    public Terrain terrain;
    public TerrainData terrainData;

    [SerializeField] private int terrainLayer = -1;
    public int treeSpacing = 5;

    public List<Vegetation> vegetations = new List<Vegetation>
    {
        new Vegetation()
    };

    public float voronoiDropOff = 0.6f;

    public float voronoiFallOff = 0.2f;
    public VoronoiFunction voronoiFunction;
    public float voronoiMaxHeight = 1f;
    public float voronoiMinHeight;
    public int voronoiPeaksCount = 1;

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        var keptSplatHeights = new List<SplatHeights>();

        foreach (var splat in splatHeights)
            if (!splat.remove)
                keptSplatHeights.Add(splat);

        if (keptSplatHeights.Count == 0) keptSplatHeights.Add(splatHeights[0]);
        splatHeights = keptSplatHeights;
    }

    private float GetSteepness(float[,] heightMap, int x, int y, int width, int height)
    {
        var h = heightMap[x, y];
        var nx = x + 1;
        var ny = y + 1;

        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        var dx = heightMap[nx, y] - h;
        var dy = heightMap[x, ny] - h;
        var gradient = new Vector2(dx, dy);

        var steep = gradient.magnitude;
        return steep;
    }

    public void SplatMaps()
    {
        var newTerrainLayers = new List<TerrainLayer>();
        for (var i = 0; i < splatHeights.Count; i++)
        {
            newTerrainLayers.Add(new TerrainLayer());
            newTerrainLayers[i].diffuseTexture = splatHeights[i].texture;
            newTerrainLayers[i].tileOffset = splatHeights[i].tileOffset;
            newTerrainLayers[i].tileSize = splatHeights[i].tileSize;
            newTerrainLayers[i].diffuseTexture.Apply(true);
        }

        terrainData.terrainLayers = newTerrainLayers.ToArray();

        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
            terrainData.heightmapHeight);
        var splatMapData = new float[terrainData.alphamapWidth,
            terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (var y = 0; y < terrainData.alphamapHeight; y++)
        for (var x = 0; x < terrainData.alphamapWidth; x++)
        {
            var splat = new float[terrainData.alphamapLayers];
            for (var i = 0; i < terrainData.alphamapLayers; i++)
            {
                var noise = Mathf.PerlinNoise(x * blendingNoiseParams,
                    y * blendingNoiseParams) * blendingNoiseMultiplier;
                var offset = blendingOffset + noise;
                var thisHeightStart = splatHeights[i].minHeight - offset;
                var thisHeightStop = splatHeights[i].maxHeight + offset;
                //float steepness = GetSteepness(heightMap, x, y,
                //    terrainData.heightmapWidth, terrainData.heightmapHeight);
                var steepness = terrainData.GetSteepness(y / (float) terrainData.alphamapHeight,
                    x / (float) terrainData.alphamapWidth);
                if (heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop &&
                    steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope)
                    splat[i] = 1;
            }

            NormalizeVector(splat);

            for (var j = 0; j < splatHeights.Count; j++) splatMapData[x, y, j] = splat[j];
        }

        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    private void NormalizeVector(float[] splat)
    {
        float total = 0;
        foreach (var v in splat) total += v;
        for (var i = 0; i < splat.Length; i++) splat[i] /= total;
    }

    private float[,] GetHeightMap()
    {
        float[,] heightMap;
        if (resetTerrain)
            heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        else
            heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                terrainData.heightmapHeight);
        return heightMap;
    }

    public void Perlin()
    {
        var heightMap = GetHeightMap();
        for (var x = 0; x < terrainData.heightmapWidth; x++)
        for (var y = 0; y < terrainData.heightmapHeight; y++)
            heightMap[x, y] += Utils.FractalBrownianMotion((x + perlinXOffset) * perlinXScale,
                (y + perlinYOffset) * perlinYScale,
                pelinOctaves, perlinPersistance) * perlinHeightScale;
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        var heightMap = GetHeightMap();
        for (var x = 0; x < terrainData.heightmapWidth; x++)
        for (var y = 0; y < terrainData.heightmapHeight; y++)
            foreach (var parametrs in perlinParamentrs)
                heightMap[x, y] += Utils.FractalBrownianMotion((x + parametrs.perlinXOffset) * parametrs.perlinXScale,
                                       (y + parametrs.perlinYOffset) * parametrs.perlinYScale,
                                       parametrs.pelinOctaves, parametrs.perlinPersistance) *
                                   parametrs.perlinHeightScale;
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParamentrs.Add(new PerlinParametrs());
    }

    public void RemovePerlin()
    {
        var keptPerlinParametrs = new List<PerlinParametrs>();

        for (var i = 0; i < perlinParamentrs.Count; i++)
            if (!perlinParamentrs[i].remove)
                keptPerlinParametrs.Add(perlinParamentrs[i]);
        if (keptPerlinParametrs.Count == 0) keptPerlinParametrs.Add(perlinParamentrs[0]);
        perlinParamentrs = keptPerlinParametrs;
    }

    public void LoadTexture()
    {
        var heightMap = GetHeightMap();
        for (var x = 0; x < terrainData.heightmapWidth; x++)
        for (var z = 0; z < terrainData.heightmapHeight; z++)
            heightMap[x, z] += heighMapImage.GetPixel((int) heighMapScale.x * x,
                    (int) heighMapScale.z * z)
                .grayscale * heighMapScale.y;
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        var heightMap = GetHeightMap();
        for (var x = 0; x < terrainData.heightmapWidth; x++)
        for (var z = 0; z < terrainData.heightmapHeight; z++)
            heightMap[x, z] += Random.Range(randomHeightRange.x, randomHeightRange.y);
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void VoronoiTessellation()
    {
        var heightMap = GetHeightMap();

        for (var i = 0; i < voronoiPeaksCount; i++)
        {
            var peakHeight = Random.Range(voronoiMinHeight, voronoiMaxHeight);
            var randomLocationX = Random.Range(0, terrainData.heightmapWidth);
            var randomLocationZ = Random.Range(0, terrainData.heightmapHeight);
            var peak = new Vector3(randomLocationX, peakHeight, randomLocationZ);

            if (heightMap[(int) peak.x, (int) peak.z] > peak.y) continue;
            heightMap[(int) peak.x, (int) peak.z] = peak.y;

            var peakLocation = new Vector2(peak.x, peak.z);

            var maxDistance = Vector2.Distance(Vector2.zero,
                new Vector2(terrainData.heightmapWidth,
                    terrainData.heightmapHeight));
            for (var y = 0; y < terrainData.heightmapHeight; y++)
            for (var x = 0; x < terrainData.heightmapWidth; x++)
                if (!(x == peak.x && y == peak.z))
                {
                    var distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                    float height;
                    if (voronoiFunction == VoronoiFunction.Power)
                        height = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff);
                    else if (voronoiFunction == VoronoiFunction.Combined)
                        height = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) -
                                 distanceToPeak * voronoiDropOff;
                    else if (voronoiFunction == VoronoiFunction.SinPow)
                        height = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff)
                                        - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff;
                    else
                        height = peak.y - distanceToPeak * voronoiDropOff;

                    if (height > heightMap[x, y]) heightMap[x, y] = height;
                }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MidpointDisplacement()
    {
        var heightMap = GetHeightMap();
        var width = terrainData.heightmapWidth - 1;

        int cornerX, cornerY;
        var squareSize = width;
        var heightMin = mpMinHeight;
        var heightMax = mpMaxHeight;

        var heightDampener = Mathf.Pow(mpDampererPower, -1 * mpRoughtness);
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        while (squareSize > 0)
        {
            for (var x = 0; x < width; x += squareSize)
            for (var y = 0; y < width; y += squareSize)
            {
                cornerX = x + squareSize;
                cornerY = y + squareSize;
                midX = (x + cornerX) / 2;
                midY = (y + cornerY) / 2;

                heightMap[midX, midY] = (heightMap[x, y] +
                                         heightMap[x, cornerY] +
                                         heightMap[cornerX, y] +
                                         heightMap[cornerX, cornerY]) / 4f + Random.Range(heightMin, heightMax);
            }

            for (var x = 0; x < width; x += squareSize)
            for (var y = 0; y < width; y += squareSize)
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
                    continue;

                heightMap[midX, y] = (heightMap[x, y] +
                                      heightMap[midX, midY] +
                                      heightMap[cornerX, y] +
                                      heightMap[midX, pmidYD]) / 4f + Random.Range(heightMin, heightMax);
                heightMap[x, midY] = (heightMap[x, y] +
                                      heightMap[midX, midY] +
                                      heightMap[x, cornerY] +
                                      heightMap[pmidXL, midY]) / 4f + Random.Range(heightMin, heightMax);
                heightMap[cornerX, midY] = (heightMap[cornerX, cornerY] +
                                            heightMap[midX, midY] +
                                            heightMap[cornerX, y] +
                                            heightMap[pmidXR, midY]) / 4f + Random.Range(heightMin, heightMax);
                heightMap[midX, cornerY] = (heightMap[midX, pmidYU] +
                                            heightMap[cornerX, cornerY] +
                                            heightMap[midX, midY] +
                                            heightMap[x, cornerY]) / 4f + Random.Range(heightMin, heightMax);
            }

            squareSize = squareSize / 2;
            heightMax *= heightDampener;
            heightMin *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Smooth()
    {
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
            terrainData.heightmapHeight);
        float progress = 0;
        EditorUtility.DisplayProgressBar("Smoothing terrain", "Smoothing in progress", progress);
        for (var i = 0; i <= smoothAmount; i++)
        {
            for (var y = 0; y < terrainData.heightmapHeight; y++)
            for (var x = 0; x < terrainData.heightmapWidth; x++)
            {
                var avgHeight = heightMap[x, y];

                var neighboards = GenerateNeighbords(new Vector2(x, y), terrainData.heightmapHeight,
                    terrainData.heightmapWidth);
                foreach (var vector2 in neighboards) avgHeight += heightMap[(int) vector2.x, (int) vector2.y];
                heightMap[x, y] = avgHeight / (neighboards.Count + 1);
                ;
            }

            progress++;
            EditorUtility.DisplayProgressBar("Smoothing terrain", "Smoothing in progress", progress / smoothAmount);
        }

        EditorUtility.ClearProgressBar();

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void PlanVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetations.Count];
        for (int i = 0; i < newTreePrototypes.Length; i++)
        {
            newTreePrototypes[i] = new TreePrototype();
            newTreePrototypes[i].prefab = vegetations[i].prefab;
        }

        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int treeProt = 0; treeProt < terrainData.treePrototypes.Length; treeProt++)
                {
                    if (allVegetation.Count >= maxTrees)
                    {
                        break;
                    }

                    if (Random.Range(0, 1f) > vegetations[treeProt].density)
                    {
                        break;
                    }

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float startHeight = vegetations[treeProt].minHeight;
                    float endHeight = vegetations[treeProt].maxHeight;

                    float stepness = terrainData.GetSteepness(x / terrainData.size.x,
                        z / terrainData.size.z);

                    if (thisHeight < startHeight || thisHeight > endHeight ||
                        stepness < vegetations[treeProt].minSlope || stepness > vegetations[treeProt].maxSlope)
                    {
                        //This tree prototype not fit to chosen parameters
                        continue;
                    }

                    TreeInstance instance = new TreeInstance();
                    instance.position = new Vector3((x + Random.Range(-5f, 5f)) / terrainData.size.x,
                        thisHeight, (z + Random.Range(-5f, 5f)) / terrainData.size.z);

                    Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,
                        instance.position.y * terrainData.size.y,
                        instance.position.z * terrainData.size.z) + transform.position;

                    RaycastHit hit;
                    int layerMask = 1 << terrainLayer;
                    if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), Vector3.down, out hit, 100, layerMask)
                        || Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask))
                    {
                        float treeHeight = (hit.point.y - transform.position.y) / terrainData.size.y;
                        instance.position.y = treeHeight;
                        instance.rotation = Random.Range(vegetations[treeProt].minRotation,
                            vegetations[treeProt].maxRotation);
                        instance.prototypeIndex = treeProt;
                        instance.color = Color.Lerp(vegetations[treeProt].color1, vegetations[treeProt].color2,
                            Random.Range(0, 1f));
                        instance.lightmapColor = vegetations[treeProt].lightColor;

                        var scale = Random.Range(vegetations[treeProt].minScale, vegetations[treeProt].maxScale);
                        instance.heightScale = scale;
                        instance.widthScale = scale;

                        allVegetation.Add(instance);
                    }
                }
            }
        }

        terrainData.treeInstances = allVegetation.ToArray();
    }

    public void ADdNewVegetation()
    {
        vegetations.Add(new Vegetation());
    }

    //TODO make generic method of removing from table
    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetations = new List<Vegetation>();
        for (int i = 0; i < vegetations.Count; i++)
        {
            if (!vegetations[i].remove)
            {
                keptVegetations.Add(vegetations[i]);
            }
        }

        //Save at least one, if removing all
        if (keptVegetations.Count == 0)
        {
            keptVegetations.Add(vegetations[0]);
        }

        vegetations = keptVegetations;
    }

    public void ResetTerrain()
    {
        var heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (var x = 0; x < terrainData.heightmapWidth; x++)
        for (var z = 0; z < terrainData.heightmapHeight; z++)
            heightMap[x, z] = 0;
        terrainData.SetHeights(0, 0, heightMap);
    }

    private List<Vector2> GenerateNeighbords(Vector2 position, int width, int height)
    {
        var neighboards = new List<Vector2>();

        for (var y = -1; y < 2; y++)
        for (var x = -1; x < 2; x++)
            if (!(x == 0 && y == 0))
            {
                var neighboardPosition = new Vector2(Mathf.Clamp(position.x + x, 0, width - 1),
                    Mathf.Clamp(position.y + y, 0, height - 1));
                if (!neighboards.Contains(neighboardPosition)) neighboards.Add(neighboardPosition);
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
        var tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        var tagsProperty = tagManager.FindProperty("tags");

        AddTag(tagsProperty, "Terrain", TagType.Tag);
        AddTag(tagsProperty, "Cloud", TagType.Tag);
        AddTag(tagsProperty, "Shore", TagType.Tag);

        var layerProperties = tagManager.FindProperty("layers");

        terrainLayer = AddTag(layerProperties, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        gameObject.tag = "Terrain";
        gameObject.layer = terrainLayer;
    }

    private int AddTag(SerializedProperty tagProperty, string newTag, TagType tagType)
    {
        for (var i = 0; i < tagProperty.arraySize; i++)
        {
            var property = tagProperty.GetArrayElementAtIndex(i);
            if (property.stringValue.Equals(newTag))
            {
                return i;
            }
        }

        if (tagType == TagType.Tag)
        {
            tagProperty.InsertArrayElementAtIndex(0);
            var property = tagProperty.GetArrayElementAtIndex(0);
            property.stringValue = newTag;
        }
        else
        {
            for (int j = 8; j < tagProperty.arraySize; j++)
            {
                var newLayer = tagProperty.GetArrayElementAtIndex(j);
                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }

        return -1;
    }

    //Vegetation
    [Serializable]
    public class Vegetation
    {
        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public float density = 0.5f;
        public Color lightColor = Color.white;

        public float maxHeight = 0.2f;
        public float maxRotation = 360f;
        public float maxScale = 1f;
        public float maxSlope = 90;
        public float minHeight = 0.1f;
        public float minRotation;
        public float minScale = 0.5f;
        public float minSlope;
        public GameObject prefab;
        public bool remove;
    }

    [Serializable]
    public class PerlinParametrs
    {
        public int pelinOctaves;
        public float perlinHeightScale = 0.09f;
        public float perlinPersistance = 8f;
        public int perlinXOffset;
        public float perlinXScale = 0.01f;
        public int perlinYOffset;
        public float perlinYScale = 0.01f;
        public bool remove;
    }

    [Serializable]
    public class SplatHeights
    {
        public float maxHeight = 0.2f;
        public float maxSlope = 1.5f;
        public float minHeight = 0.1f;
        public float minSlope;
        public bool remove;
        public Texture2D texture;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
    }
}