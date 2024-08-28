using Assets.Scripts;
using Assets.Scripts.MapObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    public int Seed;
    [SerializeField] private float Scale;

    [SerializeField] private Tilemap Layer0Tilemap;
    [SerializeField] private Tilemap Layer1Tilemap;

    [SerializeField] private Tile GreenTile;
    [SerializeField] private Tile DarkGreenTile;
    [SerializeField] private Tile StoneTile;
    [SerializeField] private List<Tile> GrassTiles;
    [SerializeField][Range(0f, 100f)] private float GrassSpawnChance;

    [SerializeField] private float StoneBiomeThreshold;

    [Header("Chunk Settings")]
    public int CHUNK_SIZE = 32;
    [SerializeField] private float OreGenerateTickInterval;
    [SerializeField] private int MaximumOres;
    [SerializeField] private AnimationCurve SpawnProbability; // by number of ores
    public List<OreObject> Ores;
    private Chunk[,] Chunks;

    [SerializeField] private MapManager _mapManager;

    private void Awake()
    {
        Instance = this;
        Chunks = new Chunk[Mathf.CeilToInt((float)_mapManager.Width / CHUNK_SIZE), Mathf.CeilToInt((float)_mapManager.Height / CHUNK_SIZE)];
    }

    public void Generate()
    {
        Random.InitState(Seed);

        for (int xChunk = 0; xChunk < Chunks.GetLength(0); xChunk++)
        {
            for (int yChunk = 0; yChunk < Chunks.GetLength(1); yChunk++)
            {
                TileType[,] chunkTiles = new TileType[CHUNK_SIZE, CHUNK_SIZE];
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        Layer0Tilemap.SetTile(new Vector3Int(x, y, 0), GreenTile);
                        chunkTiles[x,y] = TileType.GRASS;
                    }
                }

                Chunks[xChunk, yChunk] = new Chunk(xChunk, yChunk, CHUNK_SIZE, chunkTiles);
            }
        }    

        GenerateStoneBiome();
        StartCoroutine(GenerateOres());
    }

    private void GenerateStoneBiome()
    {
        float randomX = Random.Range(-10000f, 10000f);
        float randomY = Random.Range(-10000f, 10000f);

        for (int x = 0; x < _mapManager.Width; x++)
        {
            for (int y = 0; y < _mapManager.Height; y++)
            {
                float xCoord = randomX + x / (float)_mapManager.Width * Scale;
                float yCoord = randomY + y / (float)_mapManager.Height * Scale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                if (perlin < StoneBiomeThreshold)
                {
                    Chunk currentChunk = GetChunk(x, y);
                    currentChunk.SetTile(x, y, TileType.STONE);
                    currentChunk.StoneTilesCount++;
                    Layer0Tilemap.SetTile(new Vector3Int(x, y, 0), StoneTile);
                }
                else
                {
                    float r = Random.Range(0, 100f);
                    if(r <= GrassSpawnChance)
                    {
                        Layer0Tilemap.SetTile(new Vector3Int(x, y, 0), GrassTiles.GetRandom());
                    }
                }
            }
        }
    }

    public Chunk GetChunk(int x, int y)
    {       
        return Chunks[x / CHUNK_SIZE, y / CHUNK_SIZE];
    }

    public MapObject GetObjectAtPos(int x, int y)
    {
        return GetChunk(x, y).GetObjectAtPos(x,y);
    }

    public IEnumerator GenerateOres()
    {
        if (Ores == null || Ores.Count == 0) yield break;

        while (true)
        {
            yield return new WaitForSeconds(OreGenerateTickInterval);

            for (int x = 0; x < Chunks.GetLength(0); x++)
            {
                for (int y = 0; y < Chunks.GetLength(1); y++)
                {
                    Chunk chunk = Chunks[x, y];
                    if (chunk.OresCount >= MaximumOres || chunk.OresCount >= chunk.StoneTilesCount) continue;
                    float spawnChance = Random.Range(0f, 1f);
                    if(spawnChance <= SpawnProbability.Evaluate(chunk.OresCount / MaximumOres))
                    {
                        OreObject ore = Ores.GetRandom();

                        if (ore == null) continue;

                        Vector2Int spawnSpace = chunk.GetFreeSpace(ore.SpawnTileType);

                        if (spawnSpace != Vector2Int.zero)
                        {
                            _mapManager.SpawnObject(ore, spawnSpace.x, spawnSpace.y);
                        }
                    }
                }
            }
        }
    }
}

public class Chunk
{
    public int X, Y;
    public int WorldX, WorldY;
    private int _chunkSize;

    public MapObject[,] Objects;
    public int OresCount;
    private TileType[,] _tiles;
    public int StoneTilesCount;

    public Chunk(int x, int y, int chunkSize, TileType[,] tiles)
    {
        X = x;
        Y = y;
        WorldX = X * chunkSize;
        WorldY = Y * chunkSize;
        _chunkSize = chunkSize;
        Objects = new MapObject[chunkSize, chunkSize];
        _tiles = tiles;
    }

    public bool IsFreeWorldPos(int x, int y)
    {
        return Objects[x - WorldX, y - WorldY] == null;
    }

    public void SetTile(int x, int y, TileType type)
    {
        _tiles[x - WorldX, y - WorldY] = type;
    }

    public void SpawnObject(MapObject obj)
    {
        if (obj is OreObject) OresCount++;
        Objects[obj.X - WorldX, obj.Y - WorldY] = obj;
    }

    public MapObject GetObjectAtPos(int x, int y)
    {
        return Objects[x - WorldX, y - WorldY];
    }

    public void DestroyObject(MapObject obj)
    {
        if(obj is OreObject) OresCount--;
        Objects[obj.X - WorldX, obj.Y - WorldY] = null;
    }

    public Vector2Int GetFreeSpace(TileType tileType = TileType.NONE)
    {
        List<Vector2Int> availableSpaces = new List<Vector2Int>();

        for (int x = 0; x < _chunkSize; x++)
        {
            for (int y = 0; y < _chunkSize; y++)
            {
                if (Objects[x, y] == null)
                {
                    if (tileType == TileType.NONE || _tiles[x, y] == tileType)
                    {
                        availableSpaces.Add(new Vector2Int(WorldX + x, WorldY + y));
                    }
                }
            }
        }

        if (availableSpaces.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableSpaces.Count);
            return availableSpaces[randomIndex];
        }

        return Vector2Int.zero;
    }
}

public enum TileType
{
    NONE,
    GRASS,
    STONE
}