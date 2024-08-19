using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapGenerator))]
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Map Settings")]
    public int Width;
    public int Height;

    private void Awake()
    {
        Instance = this;
    }
}
