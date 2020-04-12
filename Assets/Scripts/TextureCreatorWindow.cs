using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TextureCreatorWindow : EditorWindow
{
    private bool alphaToggle;

    private float brightness = 0.5f;
    private float contrast = 0.5f;
    private string filename = "MyProceduralTexture";
    private bool mapToggle;
    private float perlinHeightScale;
    private int perlinOctaves;
    private int perlinOffsetX;
    private int perlinOffsetY;
    private float perlinPersistance;
    private float perlinXScale;
    private float perlinYScale;

    private Texture2D pTexture2D;
    private bool seamlessToggle;

    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        pTexture2D = new Texture2D(513, 513, TextureFormat.ARGB32, false);
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
        brightness = EditorGUILayout.Slider("Brightness", brightness, 0, 2);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0, 2);
        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        mapToggle = EditorGUILayout.Toggle("Map?", mapToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        float minColor = 1;
        float maxColor = 0;
        if (GUILayout.Button("Generate", GUILayout.Width(wSize)))
        {
            int size = 513;
            float pValue;
            Color pixColor = Color.white;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (seamlessToggle)
                    {
                        float u = x / (float) size;
                        float v = y / (float) size;

                        float noise00 = Utils.FractalBrownianMotion((x + perlinOffsetX) * perlinXScale,
                            (y + perlinOffsetY) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                        float noise01 = Utils.FractalBrownianMotion((x + perlinOffsetX) * perlinXScale,
                            (y + perlinOffsetY + size) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                        float noise10 = Utils.FractalBrownianMotion((x + perlinOffsetX + size) * perlinXScale,
                            (y + perlinOffsetY) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                        float noise11 = Utils.FractalBrownianMotion((x + perlinOffsetX + size) * perlinXScale,
                            (y + perlinOffsetY + size) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;

                        float noiseTotal = u * v * noise00 + u * (1 - v) * noise01 +
                                           (1 - u) * v * noise10 +
                                           (1 - u) * (1 - v) * noise11;

                        int offset = 50;
                        float value = (int) (256 * noiseTotal) + offset;
                        float r = Mathf.Clamp((int) noise00, 0, 255);
                        float g = Mathf.Clamp(value, 0, 255);
                        float b = Mathf.Clamp(value + offset, 0, 255);
                        float a = Mathf.Clamp(value + offset * 2, 0, 255);

                        pValue = (r + g + b) / (3 * 255.0f);
                    }
                    else
                    {
                        pValue = Utils.FractalBrownianMotion((x + perlinOffsetX) * perlinXScale,
                            (y + perlinOffsetY) * perlinYScale, perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                    }


                    float colValue = contrast * (pValue - 0.5f) + 0.5f * brightness;
                    if (minColor > colValue)
                    {
                        minColor = colValue;
                    }

                    if (maxColor < colValue)
                    {
                        maxColor = colValue;
                    }

                    pixColor = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture2D.SetPixel(x, y, pixColor);
                }
            }

            if (mapToggle)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        pixColor = pTexture2D.GetPixel(x, y);
                        float colValue = pixColor.r;
                        colValue = Utils.Map(colValue, minColor, maxColor, 0, 1);
                        pixColor.r = colValue;
                        pixColor.g = colValue;
                        pixColor.b = colValue;
                        pTexture2D.SetPixel(x, y, pixColor);
                    }
                }
            }

            pTexture2D.Apply(false, false);
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
            byte[] bytes = pTexture2D.EncodeToPNG();
            Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
            File.WriteAllBytes(Application.dataPath + "/SavedTextures/" + filename + ".png", bytes);
        }
    }
}