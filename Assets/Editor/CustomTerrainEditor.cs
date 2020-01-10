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

    private bool showRandom = false;
    private bool showLoadHeights = false;
    private bool showMultiplePerlin = false;
    private bool showVoronoi = false;

    private void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heighMapScale = serializedObject.FindProperty("heighMapScale");
        heighMapImage = serializedObject.FindProperty("heighMapImage");
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

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        EditorGUILayout.PropertyField(resetTerrain);
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
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi Tessellation");
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate pike with voronoi tessellation", EditorStyles.boldLabel);

            if (GUILayout.Button("Voronoi"))
            {
                terrain.VoronoiTessellation();
            }
        }
        if (GUILayout.Button("Reset"))
        {
            terrain.ResetTerrain();
        }
        serializedObject.ApplyModifiedProperties();
    }
}