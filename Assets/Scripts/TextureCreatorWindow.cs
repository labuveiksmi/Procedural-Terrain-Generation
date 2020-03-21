using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class TextureCreatorWindow : EditorWindow
{
    private string filename = "MyProceduralTexture";
    private float perlinXScale;
    private float perlinYScale;
    private int perlinOctaves;
    private float perlinPersistance;
    private float perlinHeightScale;
    private int perlinOffsetX;
    private int perlinOffsetY;
    private bool alphaToggle = false;
    private bool seamlessToggle = false;
    private bool mapToggle = false;

    private Texture2D pTexture2D;
    
    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        pTexture2D = new Texture2D(513,513,TextureFormat.ARGB32,false);
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);

        int wSize = (int) (EditorGUIUtility.currentViewWidth - 100);
        
        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance, 1, 10);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffsetX = EditorGUILayout.IntSlider("Offset X", perlinOffsetX, 0, 10000);
        perlinOffsetY = EditorGUILayout.IntSlider("Offset Y", perlinOffsetY, 0, 10000);
        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        mapToggle = EditorGUILayout.Toggle("Map?", mapToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Generate", GUILayout.Width(wSize)))
        {
            int size = 513;
            float pValue;
            Color pixColor = Color.white;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pValue = Utils.FractalBrownianMotion((x + perlinOffsetX) * perlinXScale,
                        (y + perlinOffsetY) * perlinYScale, perlinOctaves,
                        perlinPersistance) * perlinHeightScale;
                    float colValue = pValue;
                    pixColor = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture2D.SetPixel(x,y,pixColor);
                }
            }
            pTexture2D.Apply(false,false);
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture2D, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(wSize)))
        {
            
        }
    }
}
