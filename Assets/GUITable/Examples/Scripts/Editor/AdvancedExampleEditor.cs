using System.Linq;
using EditorGUITable;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdvancedExample))]
public class AdvancedExampleEditor : Editor
{
    private GUITableState tableState;

    private void OnEnable()
    {
        tableState = new GUITableState("tableState_Advanced");
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Default display", EditorStyles.boldLabel);

        base.OnInspectorGUI();

//		EditorGUILayout.PropertyField (serializedObject.FindProperty("enemies"), true);
//		serializedObject.ApplyModifiedProperties();

        GUILayout.Space(20f);

        GUILayout.Label("Table display", EditorStyles.boldLabel);

        if (GUILayout.Button("Show Window")) AdvancedExampleWindow.Init();
    }
}

public class SpawnersCell : TableCell
{
    private readonly SerializedObject so;

    private readonly SerializedProperty sp;

    public SpawnersCell(SerializedObject so, string propertyName)
    {
        sp = so.FindProperty(propertyName);
        this.so = so;
    }

    public override string comparingValue => string.Empty;

    public override void DrawCellLayout(float width, float height)
    {
        sp.intValue = EditorGUILayout.MaskField(sp.intValue,
            AdvancedExample.Instance.spawners.Select(s => s.name).ToArray(), GUILayout.Width(width),
            GUILayout.Height(height));
        so.ApplyModifiedProperties();
    }

    public override void DrawCell(Rect rect)
    {
        sp.intValue = EditorGUI.MaskField(rect, sp.intValue,
            AdvancedExample.Instance.spawners.Select(s => s.name).ToArray());
        so.ApplyModifiedProperties();
    }
}