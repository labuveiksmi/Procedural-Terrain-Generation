using System.Collections.Generic;
using EditorGUITable;
using UnityEditor;
using UnityEngine;

public class CustomPropertiesWindow : EditorWindow
{
    private SerializedObject serializedObject;

    private GUITableState tableState;


    private void OnEnable()
    {
        tableState = new GUITableState("tableState2");
    }

    private void OnGUI()
    {
        GUILayout.Label("Customize the properties to display", EditorStyles.boldLabel);

        DrawCustomProperties();
    }

    private void DrawCustomProperties()
    {
        var serializedObject = new SerializedObject(SimpleExample.Instance);
        tableState = GUITableLayout.DrawTable(tableState, serializedObject.FindProperty("simpleObjects"),
            new List<string> {"floatProperty", "objectProperty"});
    }
}