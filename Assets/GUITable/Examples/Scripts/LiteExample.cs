using System;
using System.Collections.Generic;
using UnityEngine;

public class LiteExample : MonoBehaviour
{
    public List<SimpleObject> simpleObjects;

    private void OnGUI()
    {
        GUILayout.Label("Select the LiteExample scene object to visualize the table in the inspector");
    }

    [Serializable]
    public class SimpleObject
    {
        public float floatProperty;
        public GameObject objectProperty;
        public string stringProperty;
    }
}