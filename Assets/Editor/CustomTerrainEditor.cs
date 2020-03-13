﻿using System.Collections;
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

    private SerializedProperty splatHeights;
    private GUITableState splatMapTable;

    private SerializedProperty blendingNoiseMultiplier;
    private SerializedProperty blendingOffset;
    private SerializedProperty blendingNoiseParams;

    private bool showRandom = false;
    private bool showLoadHeights = false;
    private bool showMultiplePerlin = false;
    private bool showVoronoi = false;
    private bool midpointDisplacement = false;
    private bool showSplatMaps = false;

    private void OnEnable()
    {
        RandomTerrainProperties();
        PerlinProperties();
        VoronoiProperties();
        MidpointDisplacementProperties();
        SplatMapProperties();
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

    private Vector2 scrollPos;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

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

        EditorGUILayout.IntSlider(smoothAmount, 1, 20, new GUIContent("Times"));
        if (GUILayout.Button("Smooth"))
        {
            terrain.Smooth();
        }

        if (GUILayout.Button("Reset"))
        {
            terrain.ResetTerrain();
        }

        //EditorGUILayout.EndScrollView();
        //EditorGUILayout.EndVertical();
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
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply"))
            {
                terrain.SplatMaps();
            }
        }
    }
}