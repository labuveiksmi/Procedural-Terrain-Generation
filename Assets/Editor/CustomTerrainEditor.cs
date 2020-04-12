using EditorGUITable;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    private SerializedProperty blendingNoiseMultiplier;
    private SerializedProperty blendingNoiseParams;
    private SerializedProperty blendingOffset;
    private SerializedProperty heighMapImage;
    private SerializedProperty heighMapScale;

    private Texture2D heightmapTexture;
    private SerializedProperty maxTrees;
    private bool midpointDisplacement;
    private SerializedProperty mpDampererPower;
    private SerializedProperty mpMaxHeight;
    private SerializedProperty mpMinHeight;

    private SerializedProperty mpRoughtness;
    private SerializedProperty pelinOctaves;
    private SerializedProperty perlinHeightScale;
    private SerializedProperty perlinParametrs;

    private GUITableState perlinParametrsTable;
    private SerializedProperty perlinPersistance;
    private SerializedProperty perlinXOffset;
    private SerializedProperty perlinXScale;
    private SerializedProperty perlinYOffset;
    private SerializedProperty perlinYScale;
    private SerializedProperty randomHeightRange;
    private SerializedProperty resetTerrain;

    private Vector2 scrollPos;
    private bool showHeights;
    private bool showLoadHeights;
    private bool showMultiplePerlin;

    private bool showRandom;
    private bool showSplatMaps;
    private bool showVegetation;
    private bool showVoronoi;
    private SerializedProperty smoothAmount;

    private SerializedProperty splatHeights;
    private GUITableState splatMapTable;
    private SerializedProperty treeSpacing;


    //Vegetation
    private GUITableState vegMapTable;
    private SerializedProperty voronoiDropOff;

    private SerializedProperty voronoiFallOff;
    private SerializedProperty voronoiFunction;
    private SerializedProperty voronoiMaxHeight;
    private SerializedProperty voronoiMinHeight;
    private SerializedProperty voronoiPeaksCount;

    private void OnEnable()
    {
        RandomTerrainProperties();
        PerlinProperties();
        VoronoiProperties();
        MidpointDisplacementProperties();
        SplatMapProperties();
        VegetationProperties();
        heightmapTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
        smoothAmount = serializedObject.FindProperty("smoothAmount");
    }

    private void MidpointDisplacementProperties()
    {
        mpRoughtness = serializedObject.FindProperty("mpRoughtness");
        mpMinHeight = serializedObject.FindProperty("mpMinHeight");
        mpMaxHeight = serializedObject.FindProperty("mpMaxHeight");
        mpDampererPower = serializedObject.FindProperty("mpDampererPower");
    }

    private void RandomTerrainProperties()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heighMapScale = serializedObject.FindProperty("heighMapScale");
        heighMapImage = serializedObject.FindProperty("heighMapImage");
    }

    private void PerlinProperties()
    {
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        perlinYOffset = serializedObject.FindProperty("perlinYOffset");
        pelinOctaves = serializedObject.FindProperty("pelinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParametrsTable = new GUITableState("perlinParametrsTable");
        perlinParametrs = serializedObject.FindProperty("perlinParamentrs");
    }

    private void VoronoiProperties()
    {
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiPeaksCount = serializedObject.FindProperty("voronoiPeaksCount");
        voronoiFunction = serializedObject.FindProperty("voronoiFunction");
    }

    private void SplatMapProperties()
    {
        splatMapTable = new GUITableState("perlinParametrsTable");
        splatHeights = serializedObject.FindProperty("perlinParamentrs");
        blendingOffset = serializedObject.FindProperty("blendingOffset");
        blendingNoiseParams = serializedObject.FindProperty("blendingNoiseParams");
        blendingNoiseMultiplier = serializedObject.FindProperty("blendingNoiseMultiplier");
    }

    private void VegetationProperties()
    {
        vegMapTable = new GUITableState("vegMapTable");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var terrain = (CustomTerrain) target;

        //Rect r = EditorGUILayout.BeginVertical();
        //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        //EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(resetTerrain);
        RandomTerrainGUI(terrain);
        MultiplePerlinGUI(terrain);
        LoadingHeightGUI(terrain);
        VoronoiTessellationGUI(terrain);
        MidpointDisplacementGUI(terrain);
        SplatMapsGUI(terrain);
        VegetationGUI(terrain);
        EditorGUILayout.IntSlider(smoothAmount, 1, 20, new GUIContent("Times"));
        if (GUILayout.Button("Smooth")) terrain.Smooth();

        if (GUILayout.Button("Reset")) terrain.ResetTerrain();

        GenerateTexture(terrain);


        //EditorGUILayout.EndScrollView();
        //EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

    private void GenerateTexture(CustomTerrain terrain)
    {
        showHeights = EditorGUILayout.Foldout(showHeights, "Height Map");
        if (showHeights)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int) (EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(heightmapTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth,
                    terrain.terrainData.heightmapHeight);

                for (int y = 0; y < terrain.terrainData.heightmapHeight; y++)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapWidth; x++)
                    {
                        heightmapTexture.SetPixel(x, y,
                            new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y], 1));
                    }
                }

                heightmapTexture.Apply();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    private void MidpointDisplacementGUI(CustomTerrain terrain)
    {
        midpointDisplacement = EditorGUILayout.Foldout(midpointDisplacement, "MidpointDisplacement");
        if (midpointDisplacement)
        {
            GUILayout.Label("Midpoint Displacement", EditorStyles.boldLabel);
            EditorGUILayout.Slider(mpRoughtness, 0f, 5f, new GUIContent("roughtness"));
            EditorGUILayout.PropertyField(mpMinHeight, new GUIContent("MP Min Height"));
            EditorGUILayout.PropertyField(mpMaxHeight, new GUIContent("MP Max Height"));
            EditorGUILayout.PropertyField(mpDampererPower, new GUIContent("MP Damperer Power"));

            if (GUILayout.Button("Midpoint Displacement")) terrain.MidpointDisplacement();
        }
    }

    private void LoadingHeightGUI(CustomTerrain terrain)
    {
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load heights");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load heights from texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heighMapImage);
            EditorGUILayout.PropertyField(heighMapScale);

            if (GUILayout.Button("Load Texture")) terrain.LoadTexture();
        }
    }

    private void MultiplePerlinGUI(CustomTerrain terrain)
    {
        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perline Noises");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perline Noises", EditorStyles.boldLabel);
            perlinParametrsTable = GUITableLayout.DrawTable(perlinParametrsTable,
                serializedObject.FindProperty(nameof(terrain.perlinParamentrs)));
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) terrain.AddNewPerlin();
            if (GUILayout.Button("-")) terrain.RemovePerlin();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Multiple Perlin Terrain")) terrain.MultiplePerlinTerrain();
            if (GUILayout.Button("Generate")) terrain.MultiplePerlinTerrain();
        }
    }

    private void RandomTerrainGUI(CustomTerrain terrain)
    {
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");

        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set heights between values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);

            if (GUILayout.Button("Random Heights")) terrain.RandomTerrain();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate landshaft using Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("XScale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("YScale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 10000, new GUIContent("XOffset"));
            EditorGUILayout.IntSlider(perlinYOffset, 0, 10000, new GUIContent("YOffset"));
            EditorGUILayout.IntSlider(pelinOctaves, 1, 10, new GUIContent("pelinOctaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("perlinPersistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("perlinHeightScale"));

            if (GUILayout.Button("Perlin Noise")) terrain.Perlin();
        }
    }

    private void VoronoiTessellationGUI(CustomTerrain terrain)
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi Tessellation");
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate pikes with voronoi tessellation", EditorStyles.label);

            EditorGUILayout.IntSlider(voronoiPeaksCount, 1, 20, new GUIContent("PeaksCount"));
            EditorGUILayout.PropertyField(voronoiFunction);
            EditorGUILayout.Slider(voronoiFallOff, 0f, 10f, new GUIContent("FallOff"));
            EditorGUILayout.Slider(voronoiDropOff, 0f, 10f, new GUIContent("DropOff"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0f, 1f, new GUIContent("MaxHeight"));
            EditorGUILayout.Slider(voronoiMinHeight, 0f, 1f, new GUIContent("MinHeight"));

            if (GUILayout.Button("Voronoi")) terrain.VoronoiTessellation();
        }
    }

    private void SplatMapsGUI(CustomTerrain terrain)
    {
        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);

            EditorGUILayout.Slider(blendingNoiseMultiplier, 0.1f, 1f, new GUIContent("Noise Multiplier"));
            EditorGUILayout.Slider(blendingNoiseParams, 0.001f, 0.1f, new GUIContent("Noise Parameters"));
            EditorGUILayout.Slider(blendingOffset, 0.001f, 0.1f, new GUIContent("Noise Offset"));

            splatMapTable = GUITableLayout.DrawTable(splatMapTable,
                serializedObject.FindProperty(nameof(terrain.splatHeights)));
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) terrain.AddNewSplatHeight();
            if (GUILayout.Button("-")) terrain.RemoveSplatHeight();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply")) terrain.SplatMaps();
        }
    }

    private void VegetationGUI(CustomTerrain terrain)
    {
        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxTrees, 0, 10000, new GUIContent("Max Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 2, 20, new GUIContent("Tree Spacing"));


            vegMapTable = GUITableLayout.DrawTable(vegMapTable,
                serializedObject.FindProperty(nameof(terrain.vegetations)));
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.ADdNewVegetation();
            }

            if (GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply"))
            {
                terrain.PlanVegetation();
            }

            ;
        }
    }
}