using System.Collections.Generic;
using EditorGUITable;
using UnityEditor;
using UnityEngine;

public class CustomColumnsWindow : EditorWindow
{
    private SerializedObject serializedObject;

    private GUITableState tableState;


    private void OnEnable()
    {
        tableState = new GUITableState("tableState3");
    }

    private void OnGUI()
    {
        GUILayout.Label("Customize the columns (right-click to hide optional columns)", EditorStyles.boldLabel);

        DrawCustomColumns();
    }

    private void DrawCustomColumns()
    {
        var serializedObject = new SerializedObject(SimpleExample.Instance);
        var propertyColumns = new List<SelectorColumn>
        {
            new SelectFromPropertyNameColumn("stringProperty", "String", TableColumn.Width(60f)),
            new SelectFromPropertyNameColumn("floatProperty", "Float", TableColumn.Width(50f),
                TableColumn.Optional(true)),
            new SelectFromPropertyNameColumn("objectProperty", "Object", TableColumn.Width(50f),
                TableColumn.EnabledTitle(false), TableColumn.Optional(true))
        };

        tableState =
            GUITableLayout.DrawTable(tableState, serializedObject.FindProperty("simpleObjects"), propertyColumns);
    }
}