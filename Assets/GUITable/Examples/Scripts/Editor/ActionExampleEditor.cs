using System.Collections.Generic;
using EditorGUITable;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionExample))]
public class ActionExampleEditor : Editor
{
    private GUITableState tableState = new GUITableState();

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Default display", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("simpleObjects"), true);
        GUILayout.Space(10f);
        GUILayout.Label("Table display", EditorStyles.boldLabel);
        DrawObjectsTable();
    }

    private void DrawObjectsTable()
    {
        var columns = new List<TableColumn>
        {
            new TableColumn("String", TableColumn.Width(60f)),
            new TableColumn("Float", TableColumn.Width(50f)),
            new TableColumn("Object", TableColumn.Width(110f)),
            new TableColumn("", TableColumn.Width(50f), TableColumn.EnabledTitle(false))
        };

        var rows = new List<List<TableCell>>();

        var targetObject = (ActionExample) target;

        for (var i = 0; i < targetObject.simpleObjects.Count; i++)
        {
            var entry = targetObject.simpleObjects[i];
            rows.Add(new List<TableCell>
            {
                new LabelCell(entry.stringProperty),
                new PropertyCell(serializedObject, string.Format("simpleObjects.Array.data[{0}].floatProperty", i)),
                new PropertyCell(serializedObject, string.Format("simpleObjects.Array.data[{0}].objectProperty", i)),
                new ActionCell("Reset", () => entry.Reset())
            });
        }

        tableState = GUITableLayout.DrawTable(tableState, columns, rows);
    }
}