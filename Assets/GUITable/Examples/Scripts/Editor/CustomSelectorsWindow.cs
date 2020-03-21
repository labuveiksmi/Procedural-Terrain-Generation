using System.Collections.Generic;
using EditorGUITable;
using UnityEditor;
using UnityEngine;

public class CustomSelectorsWindow : EditorWindow
{
    private SerializedObject serializedObject;

    private GUITableState tableState;


    private void OnEnable()
    {
        tableState = new GUITableState("tableState4");
    }

    private void OnGUI()
    {
        GUILayout.Label("Customize the columns and the selector function for cells", EditorStyles.boldLabel);

        DrawCustomColumnsWithSelector();
    }

    private void DrawCustomColumnsWithSelector()
    {
        var serializedObject = new SerializedObject(SimpleExample.Instance);

        var selectorColumns = new List<SelectorColumn>
        {
            new SelectFromFunctionColumn(prop => new LabelCell(prop.FindPropertyRelative("stringProperty").stringValue),
                "String", TableColumn.Width(60f)),
            new SelectFromFunctionColumn(
                prop => new LabelCell(prop.FindPropertyRelative("floatProperty").floatValue.ToString()), "Float",
                TableColumn.Width(50f), TableColumn.Optional(true)),
            new SelectFromFunctionColumn(
                prop => new LabelCell(prop.FindPropertyRelative("objectProperty").objectReferenceValue.name), "Object",
                TableColumn.Width(110f), TableColumn.EnabledTitle(false), TableColumn.Optional(true))
        };

        tableState =
            GUITableLayout.DrawTable(tableState, serializedObject.FindProperty("simpleObjects"), selectorColumns);
    }
}