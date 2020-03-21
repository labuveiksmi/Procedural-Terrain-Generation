using System.Collections.Generic;
using EditorGUITable;
using UnityEditor;

[CustomEditor(typeof(LogoTable))]
public class LogoTableEditor : Editor
{
    private GUITableState tableState;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var serializedObject = new SerializedObject(LogoTable.Instance);

        var columns = new List<TableColumn>
        {
            new TableColumn("G", 28f),
            new TableColumn("U", 22f),
            new TableColumn("I     ", 35f)
        };

        var rows = new List<List<TableCell>>();

        var targetObject = LogoTable.Instance;

        for (var i = 0; i < targetObject.logoLines.Count; i++)
            rows.Add(new List<TableCell>
            {
                new PropertyCell(serializedObject, string.Format("logoLines.Array.data[{0}].letter1", i)),
                new PropertyCell(serializedObject, string.Format("logoLines.Array.data[{0}].letter2", i)),
                new PropertyCell(serializedObject, string.Format("logoLines.Array.data[{0}].color", i))
            });

        tableState = GUITableLayout.DrawTable(tableState, columns, rows);
    }
}