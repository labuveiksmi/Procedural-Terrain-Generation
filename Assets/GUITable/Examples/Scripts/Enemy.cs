using System;
using UnityEditor;
using UnityEngine;

public enum EnemyType
{
    Normal,
    Flock,
    Boss
}

[Serializable]
public class Enemy : MonoBehaviour
{
    public bool canSwim;
    public Color color;
    public int health;
    public int spawnersMask;
    public float speed;
    public EnemyType type;

    public void Instantiate()
    {
        PrefabUtility.InstantiatePrefab(this);
    }
}