using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionExample : MonoBehaviour
{
    public List<SimpleObject> simpleObjects;

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