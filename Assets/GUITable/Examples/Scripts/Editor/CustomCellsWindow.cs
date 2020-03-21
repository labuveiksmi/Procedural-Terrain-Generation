using System.Collections.Generic;
using EditorGUITable;
using UnityEditor;
using UnityEngine;

public class CustomCellsWindow : EditorWindow
{
    private SerializedObject serializedObject;

    private GUITableState tableState;


    private void OnEnable()
    {
        tableState = new GUITableState("tableState5");
    }

    private void OnGUI()
    {
        GUILayout.Label("Customize the cells", EditorStyles.boldLabel);

        DrawCustomCells();
    }

    private void DrawCustomCells()
    {
        var serializedObject = new SerializedObject(SimpleExample.Instance);

        var columns = new List<TableColumn>
        {
            new TableColumn("String", 60f),
            new TableColumn("Float", 50f),
            new TableColumn("Object", 110f),
            new TableColumn("", TableColumn.Width(100f), TableColumn.EnabledTitle(false))
        };

        var rows = new List<List<TableCell>>();

        var targetObject = (SimpleExample) serializedObject.targetObject;

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