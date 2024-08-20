using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapGenerator))]
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public ItemObject ItemObject;

    private MapGenerator _generator;

    [Header("Map Settings")]
    public int Width;
    public int Height;

    private void Awake()
    {
        Instance = this;
        _generator = GetComponent<MapGenerator>();
    }

    public void SpawnObject(MapObject obj, int x, int y)
    {
        MapObject spawnedOre = Instantiate(obj, new Vector3(x + .5f, y + .5f, 0), Quaternion.identity);
        Chunk chunk = _generator.GetChunk(x, y);
        spawnedOre.Spawn(chunk, x, y);
    }

    public bool IsFree(int x, int y)
    {
        Chunk chunk = _generator.GetChunk(x, y);
        return chunk.IsFreeWorldPos(x, y);
    }

    public void SpawnItem(Item item, float x, float y, int count)
    {
        ItemObject itemObj = Instantiate(ItemObject, new Vector3(x,y,0), Quaternion.identity);
        itemObj.Init(item, count);
    }
}
