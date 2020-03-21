using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleExample : MonoBehaviour
{
    private static SimpleExample instance;

    public List<SimpleObject> simpleObjects;

    public static SimpleExample Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SimpleExample>();
            return instance;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Select the SimpleExample scene object to visualize the table in the inspector");
    }

    [Serializable]
    public class SimpleObject
    {
        public float floatProperty;
        public GameObject objectProperty;
        public string stringProperty;

        public void Reset()
        {
            stringProperty = "";
            floatProperty = 0f;
            objectProperty = null;
        }
    }
}