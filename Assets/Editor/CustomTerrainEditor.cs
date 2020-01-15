using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    private SerializedProperty randomHeightRange;
    private SerializedProperty heighMapScale;
    private SerializedProperty heighMapImage;
    private SerializedProperty perlinXScale;
    private SerializedProperty perlinYScale;
    private SerializedProperty perlinXOffset;
    private SerializedProperty perlinYOffset;
    private SerializedProperty pelinOctaves;
    private SerializedProperty perlinPersistance;
    private SerializedProperty perlinHeightScale;
    private SerializedProperty resetTerrain;

    private GUITableState perlinParametrsTable;
    private SerializedProperty perlinParametrs;

    private SerializedProperty voronoiFallOff;
    private SerializedProperty voronoiDropOff;
    private SerializedProperty voronoiMaxHeight;
    private SerializedProperty voronoiMinHeight;
    private SerializedProperty voronoiPeaksCount;
    private SerializedProperty voronoiFunction;

    private SerializedProperty mpRoughtness;
    private SerializedProperty mpMinHeight;
    private SerializedProperty mpMaxHeight;
    private SerializedProperty mpDampererPower;

    private SerializedProperty smoothAmount;

    private bool showRandom = false;
    private bool showLoadHeights = false;
    private bool showMultiplePerlin = false;
    private bool showVoronoi = false;
    private bool midpointDisplacement = false;

    private void OnEnable()
    {
        RandomTerrainProperties();
        PerlinProperties();
        VoronoiProperties();
        MidpointDisplacementProperties();
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

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        EditorGUILayout.PropertyField(resetTerrain);
        RandomTerrainGUI(terrain);
        MultiplePerlinGUI(terrain);
        LoadingHeightGUI(terrain);
        VoronoiTessellationGUI(terrain);
        MidpointDisplacementGUI(terrain);

        EditorGUILayout.IntSlider(smoothAmount, 1, 20, new GUIContent("Times"));
        if (GUILayout.Button("Smooth"))
        {
            terrain.Smooth();
        }

        if (GUILayout.Button("Reset"))
        {
            terrain.ResetTerrain();
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void MidpointDisplacementGUI(CustomTerrain terrain)
    {
        midpointDisplacement = EditorGUILayout.Foldout(midpointDisplacement, "MidpointDisplacement");
        if (midpointDisplacement)
        {
            GUILayout.Label("Midpoint Displacement", EditorStyles.boldLabel);
            EditorGUILayout.Slider(mpRoughtness, 0f, 5f, new GUIContent("roughtness"));
            EditorGUILayout.PropertyField(mpMinHeight, new GUIContent("MP Min Heigt"));
            EditorGUILayout.PropertyField(mpMaxHeight, new GUIContent("MP Max Heigt"));
            EditorGUILayout.PropertyField(mpDampererPower, new GUIContent("MP Damperer Power"));

            if (GUILayout.Button("Midpoint Displacement"))
            {
                terrain.MidpointDisplacement();
            }
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

            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
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
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Multiple Perlin Terrain"))
            {
                terrain.MultiplePerlinTerrain();
            }
            if (GUILayout.Button("Generate"))
            {
                terrain.MultiplePerlinTerrain();
            }
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

            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate landshaft using Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("XScale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("YScale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 10000, new GUIContent("XOffset"));
            EditorGUILayout.IntSlider(perlinYOffset, 0, 10000, new GUIContent("YOffset"));
            EditorGUILayout.IntSlider(pelinOctaves, 1, 10, new GUIContent("pelinOctaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("perlinPersistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("perlinHeightScale"));

            if (GUILayout.Button("Perlin Noise"))
            {
                terrain.Perlin();
            }
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

            if (GUILayout.Button("Voronoi"))
            {
                terrain.VoronoiTessellation();
            }
        }
    }
}