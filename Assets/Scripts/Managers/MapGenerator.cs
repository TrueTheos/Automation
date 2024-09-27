using Assets.Scripts;
using Assets.Scripts.Managers;
using Assets.Scripts.MapObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;
using static Assets.Scripts.Utilities;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    public int Seed;
    [SerializeField] private float NoiseScale;
    [SerializeField] private float WaterNoiseScale;

    [SerializeField] private Tilemap BaseTilemap;
    [SerializeField] private Tilemap DualTileTilemap;
    [SerializeField] private Tilemap DetailsTilemap;
    [SerializeField] private Tilemap WaterTilemap;
    [SerializeField] private Tilemap SandTilemap;
    [SerializeField] private Tilemap WaterShadowTilemap;

    [SerializeField] private Tile GreenTile;
    [SerializeField] private Tile StoneTile;
    [SerializeField] private RuleTile AnimatedWaterTile;
    [SerializeField] private RuleTile PlainWaterTile;
    [SerializeField] private RuleTile StaticWaterTile;
    [SerializeField] private List<Tile> GrassBladeTiles;
    [SerializeField][Range(0f, 100f)] private float GrassBladeSpawnChance;

    [SerializeField] private float StoneBiomeThreshold;
    [SerializeField] private float WaterThreshold;

    [Header("Chunk Settings")]
    public const int CHUNK_SIZE = 32;
    [SerializeField] private float OreGenerateTickInterval;
    [SerializeField] private float TreeGenerateTickInterval;
    [SerializeField] private int MaximumOres;
    [SerializeField] private int MaximumTrees;
    [SerializeField] private TreeObject TreeObject;
    [SerializeField] private AnimationCurve SpawnProbability; // by number of ores
    public List<OreObject> Ores;
    private Chunk[,] _chunks;
    public Chunk[,] Chunks { get => _chunks; }

    [SerializeField] private MapManager _mapManager;

    [SerializeField] private Texture2D _dualTileAtlas;

    readonly List<Vector2Int> NEIGHBOURS = new() { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };

    readonly Dictionary<(TileType, TileType, TileType, TileType), Vector2Int> _neighboursToAtlasCoors = new()
    {
        {new (TileType.GRASS, TileType.GRASS, TileType.GRASS, TileType.GRASS), new Vector2Int(2,2) },
        {new (TileType.NONE, TileType.NONE, TileType.NONE, TileType.GRASS), new Vector2Int(1,0) },
        {new (TileType.NONE, TileType.NONE, TileType.GRASS, TileType.NONE), new Vector2Int(0,3) },
        {new (TileType.NONE, TileType.GRASS, TileType.NONE, TileType.NONE), new Vector2Int(0,1) },
        {new (TileType.GRASS, TileType.NONE, TileType.NONE, TileType.NONE), new Vector2Int(3,0) },
        {new (TileType.NONE, TileType.GRASS, TileType.NONE, TileType.GRASS), new Vector2Int(1,3) },
        {new (TileType.GRASS, TileType.NONE, TileType.GRASS, TileType.NONE), new Vector2Int(3,1) },
        {new (TileType.NONE, TileType.NONE, TileType.GRASS, TileType.GRASS), new Vector2Int(3,3) },
        {new (TileType.GRASS, TileType.GRASS, TileType.NONE, TileType.NONE), new Vector2Int(1,1) },
        {new (TileType.NONE, TileType.GRASS, TileType.GRASS, TileType.GRASS), new Vector2Int(1,2) },
        {new (TileType.GRASS, TileType.NONE, TileType.GRASS, TileType.GRASS), new Vector2Int(2,3) },
        {new (TileType.GRASS, TileType.GRASS, TileType.NONE, TileType.GRASS), new Vector2Int(2,1) },
        {new (TileType.GRASS, TileType.GRASS, TileType.GRASS, TileType.NONE), new Vector2Int(3,2) },
        {new (TileType.NONE, TileType.GRASS, TileType.GRASS, TileType.NONE), new Vector2Int(2,0) },
        {new (TileType.GRASS, TileType.NONE, TileType.NONE, TileType.GRASS), new Vector2Int(0,2) },
        {new (TileType.NONE, TileType.NONE, TileType.NONE, TileType.NONE), new Vector2Int(0,0) },
    };

    [HideInInspector] public bool MapGenerated = false;

    private void Awake()
    {
        Instance = this;
        _chunks = new Chunk[Mathf.CeilToInt((float)_mapManager.Width / CHUNK_SIZE), Mathf.CeilToInt((float)_mapManager.Height / CHUNK_SIZE)];
    }

    public void Generate()
    {
        Random.InitState(Seed);

        BaseTilemap.ClearAllTiles();
        DetailsTilemap.ClearAllTiles();

        for (int xChunk = 0; xChunk < _chunks.GetLength(0); xChunk++)
        {
            for (int yChunk = 0; yChunk < _chunks.GetLength(1); yChunk++)
            {
                _chunks[xChunk, yChunk] = new Chunk(xChunk, yChunk, CHUNK_SIZE, Chunk.ChunkType.Impure);
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        BaseTilemap.SetTile(new Vector3Int(x + CHUNK_SIZE * xChunk, y + CHUNK_SIZE * yChunk, 0), GreenTile);
                        _chunks[xChunk, yChunk].SetTile(x + CHUNK_SIZE * xChunk, y + CHUNK_SIZE * yChunk, TileType.GRASS);
                    }
                }           
            }
        }    

        GenerateStoneBiome();
        GenerateWater();
        StartCoroutine(GenerateOres());
        StartCoroutine(GenerateTrees());
        GenerateDualTileTilemap();
        //_mapManager.GenerateBlendTexture();

        MapGenerated = true;
        ChunkPurityManager.Instance.SetupFog();
    }

    private Vector2Int CalculateDualTile(Vector2Int coords)
    {
        TileType botRight = GetTileTypeAtPos(coords + new Vector2Int(1,0)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;
        TileType botLeft = GetTileTypeAtPos(coords + new Vector2Int(0, 0)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;
        TileType topRight = GetTileTypeAtPos(coords + new Vector2Int(1, 1)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;
        TileType topLeft = GetTileTypeAtPos(coords + new Vector2Int(0, 1)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;

        var key = (topLeft, topRight, botLeft, botRight);

        if (_neighboursToAtlasCoors.TryGetValue(key, out Vector2Int result))
        {
            return result;
        }
        else
        {
            return new Vector2Int(0,0);
        }
    }

    private void GenerateDualTileTilemap()
    {
        DualTileTilemap.ClearAllTiles();
        for (int x = 0; x < _mapManager.Width; x++)
        {
            for (int y = 0; y < _mapManager.Height; y++)
            {
                for (int i = 0; i < NEIGHBOURS.Count; i++)
                {
                    Vector2Int newPos = new Vector2Int(x, y) - NEIGHBOURS[i];
                    Vector2Int tilePos = CalculateDualTile(newPos);
                    int pixelX = tilePos.x * 16;
                    int pixelY = tilePos.y * 16;

                    // Create a rectangle for the sprite
                    Rect spriteRect = new Rect(pixelX, pixelY, 16, 16);

                    // Create the sprite
                    Sprite sprite = Sprite.Create(_dualTileAtlas, spriteRect, new Vector2(.5f, .5f), 16);

                    // Create and return a new tile with this sprite
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    DualTileTilemap.SetTile(new Vector3Int(newPos.x, newPos.y, 0), tile);
                }
               
            }
        }
    }

    private void GenerateStoneBiome()
    {
        float randomX = Random.Range(-10000f, 10000f);
        float randomY = Random.Range(-10000f, 10000f);

        for (int x = 0; x < _mapManager.Width; x++)
        {
            for (int y = 0; y < _mapManager.Height; y++)
            {
                float xCoord = randomX + x / (float)_mapManager.Width * NoiseScale;
                float yCoord = randomY + y / (float)_mapManager.Height * NoiseScale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                if (perlin < StoneBiomeThreshold)
                {
                    Chunk currentChunk = GetChunk(x, y);
                    currentChunk.SetTile(x, y, TileType.STONE);
                    BaseTilemap.SetTile(new Vector3Int(x, y, 0), StoneTile);
                }
                else
                {
                    float r = Random.Range(0, 100f);
                    if(r <= GrassBladeSpawnChance)
                    {
                        DetailsTilemap.SetTile(new Vector3Int(x, y, 0), GrassBladeTiles.GetRandom());
                    }
                }
            }
        }
    }

    private void GenerateWater()
    {
        float randomX = Random.Range(-10000f, 10000f);
        float randomY = Random.Range(-10000f, 10000f);

        for (int x = 0; x < _mapManager.Width; x++)
        {
            for (int y = 0; y < _mapManager.Height; y++)
            {
                float xCoord = randomX + x / (float)_mapManager.Width * WaterNoiseScale;
                float yCoord = randomY + y / (float)_mapManager.Height * WaterNoiseScale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                if (perlin < WaterThreshold)
                {
                    Chunk currentChunk = GetChunk(x, y);
                    currentChunk.SetTile(x, y, TileType.WATER);
                    WaterTilemap.SetTile(new Vector3Int(x, y, 0), StaticWaterTile);
                    DetailsTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    //SandTilemap.SetTile(new Vector3Int(x, y, 0), StaticWaterTile);
                    //WaterShadowTilemap.SetTile(new Vector3Int(x, y, 0), PlainWaterTile);
                }
            }
        }
    }

    public Chunk GetChunk(int x, int y)
    {       
        return _chunks[x / CHUNK_SIZE, y / CHUNK_SIZE];
    }

    public MapObject GetObjectAtPos(int x, int y)
    {
        return GetChunk(x, y).GetObjectAtPos(x,y);
    }

    public TileType GetTileTypeAtPos(Vector2Int pos)
    {
        return GetTileTypeAtPos(pos.x, pos.y);
    }

    public TileType GetTileTypeAtPos(int x, int y)
    {
        if (x < 0 || x >= _mapManager.Width || y < 0 || y >= _mapManager.Height)
        {
            return TileType.NONE;
        }
        return GetChunk(x, y).GetTileType(x, y);
    }

    public IEnumerator GenerateOres()
    {
        if (Ores == null || Ores.Count == 0) yield break;

        while (true)
        {
            yield return new WaitForSeconds(OreGenerateTickInterval);

            for (int x = 0; x < _chunks.GetLength(0); x++)
            {
                for (int y = 0; y < _chunks.GetLength(1); y++)
                {
                    Chunk chunk = _chunks[x, y];
                    if (chunk.OresCount >= MaximumOres || chunk.OresCount >= chunk.TilesCount[TileType.STONE]) continue;
                    float spawnChance = Random.Range(0f, 1f);
                    if(spawnChance <= SpawnProbability.Evaluate(chunk.OresCount / MaximumOres))
                    {
                        OreObject ore = Ores.GetRandom();

                        if (ore == null) continue;

                        Vector2Int spawnSpace = chunk.GetFreeSpace(ore.SpawnTileType);

                        if (spawnSpace != Vector2Int.zero)
                        {
                            _mapManager.SpawnObject(ore, spawnSpace.x, spawnSpace.y, Direction.None, false);
                        }
                    }
                }
            }
        }
    }

    public IEnumerator GenerateTrees()
    {
        if (TreeObject == null) yield break;
        while(true)
        {
            yield return new WaitForSeconds(TreeGenerateTickInterval);

            for (int x = 0; x < _chunks.GetLength(0); x++)
            {
                for (int y = 0; y < _chunks.GetLength(1); y++)
                {
                    Chunk chunk = _chunks[x, y];
                    if (chunk.TreesCount >= MaximumTrees || chunk.TreesCount >= chunk.TilesCount[TileType.GRASS]) continue;
                    float spawnChance = Random.Range(0f, 1f);
                    if (spawnChance <= SpawnProbability.Evaluate(chunk.TreesCount / MaximumTrees))
                    {
                        Vector2Int spawnSpace = chunk.GetFreeSpace(TileType.GRASS);

                        if (spawnSpace != Vector2Int.zero)
                        {
                            _mapManager.SpawnObject(TreeObject, spawnSpace.x, spawnSpace.y, Direction.None, false);
                        }
                    }
                }
            }
        }
    }
}

public class Chunk
{
    public enum ChunkType { Pure, Impure}
    public ChunkType Type;
    public VisualEffect Fog;
    public int X, Y;
    public int WorldX, WorldY;
    private int _chunkSize;

    public MapObject[,] Objects;
    public int OresCount; // dziwne to ale chuj niech bedzie na razie
    public int TreesCount;
    private TileType[,] _tiles;
    public Dictionary<TileType, int> TilesCount = new();

    public Chunk(int x, int y, int chunkSize, ChunkType type)
    {
        X = x;
        Y = y;
        WorldX = X * chunkSize;
        WorldY = Y * chunkSize;
        _chunkSize = chunkSize;
        Objects = new MapObject[chunkSize, chunkSize];
        _tiles = new TileType[chunkSize, chunkSize];

        foreach (var val in Enum.GetValues(typeof(TileType)).Cast<TileType>())
        {
            TilesCount[val] = 0;
        }
        Type = type;
    }

    public bool IsFreeWorldPos(int x, int y)
    {
        return Objects[x - WorldX, y - WorldY] == null;
    }

    public void SetTile(int x, int y, TileType type)
    {
        TileType previousType = _tiles[x - WorldX, y - WorldY];
        if (previousType != TileType.NONE && previousType != type && TilesCount[type] > 0)
        {
            if (TilesCount.ContainsKey(previousType)) TilesCount[previousType]--;
        }
        _tiles[x - WorldX, y - WorldY] = type;
        if (TilesCount.ContainsKey(type))
        {
            TilesCount[type]++;
        }
        else TilesCount[type] = 1;
    }

    public TileType GetTileType(int x, int y)
    {
        return _tiles[x - WorldX, y - WorldY];
    }

    public void SpawnObjectPart(MapObject obj, int x, int y)
    {
        int localX = x - WorldX;
        int localY = y - WorldY;

        if (localX >= 0 && localX < _chunkSize && localY >= 0 && localY < _chunkSize)
        {
            Objects[localX, localY] = obj;
        }
    }

    public void DestroyObjectPart(int x, int y)
    {
        int localX = x - WorldX;
        int localY = y - WorldY;

        if (localX >= 0 && localX < _chunkSize && localY >= 0 && localY < _chunkSize)
        {
            Objects[localX, localY] = null;
        }
    }

    public MapObject GetObjectAtPos(int x, int y)
    {
        return Objects[x - WorldX, y - WorldY];
    }

    public void DestroyObject(MapObject obj)
    {
        foreach (Vector2Int pos in obj.GetOccupiedPositions(obj.X, obj.Y))
        {
            DestroyObjectPart(pos.x, pos.y);
        }

        if (obj is OreObject) OresCount--;
        else if (obj is TreeObject) TreesCount--;
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
    STONE,
    WATER
}