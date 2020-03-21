using System;
using System.Collections.Generic;
using EditorGUITable;
using UnityEngine;

public class PropertyAttributeExample : MonoBehaviour
{
    public List<SimpleObject> simpleObjectsDefaultDisplay;

    [ReorderableTable] public List<SimpleObject> simpleObjectsUsingReorderableTableAttribute;

    [Table] public List<SimpleObject> simpleObjectsUsingTableAttribute;

    private void OnGUI()
    {
        GUILayout.Label("Select the PropertyAttribute scene object to visualize the table in the inspector");
    }

    [Serializable]
    public class SimpleObject
    {
        public float floatProperty;
        public GameObject objectProperty;
        public string stringProperty;
        public Vector2 v2Property;
    }
}