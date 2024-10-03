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

    public GameObject MainGrid;

    public Tilemap BaseTilemap;
    public Tilemap DualTileTilemap;
    public Tilemap DetailsTilemap;
    public Tilemap WaterTilemap;
    public Tilemap SandTilemap;
    public Tilemap WaterShadowTilemap;

    public int Seed;
    [SerializeField] private float NoiseScale;
    [SerializeField] private float WaterNoiseScale;

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
    public const int MAXIMUM_ORES = 30;
    public const int MAXIMUM_TREES = 25;
    [SerializeField] private float OreGenerateTickInterval;
    [SerializeField] private float TreeGenerateTickInterval;
    public TreeObject TreeObject;
    public AnimationCurve SpawnProbability; // by number of ores
    public List<OreObject> Ores;
    public Dictionary<Vector2Int, Chunk> Chunks = new();
    public Chunk ChunkPrefab;

    [SerializeField] private MapManager _mapManager;

    [SerializeField] private Texture2D _dualTileAtlas;

    private float randomX;
    private float randomY;

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

    private Tile[,] _dualTiles;

    [HideInInspector] public bool MapGenerated = false;

    private void Awake()
    {
        Instance = this;
        randomX = Random.Range(-10000f, 10000f);
        randomY = Random.Range(-10000f, 10000f);

        _dualTiles = new Tile[_dualTileAtlas.width / 16, _dualTileAtlas.height / 16];

        for (int x = 0; x < _dualTiles.GetLength(0); x++)
        {
            for (int y = 0; y < _dualTiles.GetLength(1); y++)
            {
                int pixelX = x * 16;
                int pixelY = y * 16;

                // Create a rectangle for the sprite
                Rect spriteRect = new Rect(pixelX, pixelY, 16, 16);

                // Create the sprite
                Sprite sprite = Sprite.Create(_dualTileAtlas, spriteRect, new Vector2(.5f, .5f), 16);

                // Create and return a new tile with this sprite
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                _dualTiles[x,y] = tile;
            }
        }
    }

    public void Generate()
    {
        Random.InitState(Seed);

        Chunk chunk = CreateNewChunk(0, 0);
        chunk.Purify();
        GenerateDualTileTilemap(chunk);

        StartCoroutine(GenerateOres());
        StartCoroutine(GenerateTrees());

        MapGenerated = true;
    }

    public void GenerateDualTilesButton()
    {
        foreach (var chunk in Chunks.Values.Where(x => !x.GeneratedDualTiles && x.Visible).ToList())
        {
            GenerateDualTileTilemap(chunk);
        }
    }

    public Chunk CreateNewChunk(int xChunk, int yChunk)
    {
        Vector2Int chunkPos = new Vector2Int(xChunk, yChunk);
        if (Chunks.ContainsKey(chunkPos)) return Chunks[chunkPos];

        Chunk chunk = Instantiate(ChunkPrefab, Vector2.zero, Quaternion.identity).GetComponent<Chunk>();
        chunk.transform.parent = MainGrid.transform;
        chunk.name = $"Chunk {xChunk}, {yChunk}";

        chunk.Init(xChunk, yChunk, CHUNK_SIZE, Chunk.ChunkType.Impure);
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                BaseTilemap.SetTile(new Vector3Int(x + CHUNK_SIZE * xChunk, y + CHUNK_SIZE * yChunk, 0), GreenTile);
                chunk.SetTileAtLocalPos(x, y, TileType.GRASS);
            }
        }

        GenerateStoneBiome(chunk);
        GenerateWater(chunk);

        Chunks[chunkPos] = chunk;     
        return chunk;
    }

    private Vector2Int CalculateDualTile(Vector2Int coords)
    {
        TileType botRight = GetTileTypeAtWorldPos(coords + new Vector2Int(1,0)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;
        TileType botLeft = GetTileTypeAtWorldPos(coords + new Vector2Int(0, 0)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;
        TileType topRight = GetTileTypeAtWorldPos(coords + new Vector2Int(1, 1)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;
        TileType topLeft = GetTileTypeAtWorldPos(coords + new Vector2Int(0, 1)) == TileType.GRASS ? TileType.GRASS : TileType.NONE;

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

    public void GenerateDualTileTilemap(Chunk chunk)
    {
        if (chunk.GeneratedDualTiles) return;

        chunk.GeneratedDualTiles = true;
        for (int x = chunk.WorldX; x < chunk.WorldX + CHUNK_SIZE; x++)
        {
            for (int y = chunk.WorldY; y < chunk.WorldY + CHUNK_SIZE; y++)
            {
                for (int i = 0; i < NEIGHBOURS.Count; i++)
                {
                    Vector2Int newPos = new Vector2Int(x, y) - NEIGHBOURS[i];
                    Vector2Int tilePos = CalculateDualTile(newPos);
                    /*int pixelX = tilePos.x * 16;
                    int pixelY = tilePos.y * 16;

                    // Create a rectangle for the sprite
                    Rect spriteRect = new Rect(pixelX, pixelY, 16, 16);

                    // Create the sprite
                    Sprite sprite = Sprite.Create(_dualTileAtlas, spriteRect, new Vector2(.5f, .5f), 16);

                    // Create and return a new tile with this sprite
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;*/
                    DualTileTilemap.SetTile(new Vector3Int(newPos.x, newPos.y, 0), _dualTiles[tilePos.x, tilePos.y]);
                }            
            }
        }
    }

    private void GenerateStoneBiome(Chunk chunk)
    {        
        for (int x = chunk.WorldX; x < chunk.WorldX + CHUNK_SIZE; x++)
        {
            for (int y = chunk.WorldY; y < chunk.WorldY + CHUNK_SIZE; y++)
            {
                float xCoord = randomX + x / (float)128 * NoiseScale;
                float yCoord = randomY + y / (float)128 * NoiseScale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                if (perlin < StoneBiomeThreshold)
                {
                    chunk.SetTileAtWorldPos(x, y, TileType.STONE);
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

    private void GenerateWater(Chunk chunk)
    {
        for (int x = chunk.WorldX; x < chunk.WorldX + CHUNK_SIZE; x++)
        {
            for (int y = chunk.WorldY; y < chunk.WorldY + CHUNK_SIZE; y++)
            {
                float xCoord = randomX + x / (float)128 * WaterNoiseScale;
                float yCoord = randomY + y / (float)128 * WaterNoiseScale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                if (perlin < WaterThreshold)
                {
                    chunk.SetTileAtWorldPos(x, y, TileType.WATER);
                    WaterTilemap.SetTile(new Vector3Int(x, y, 0), StaticWaterTile);
                    DetailsTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }

    public Chunk GetChunk(int x, int y, bool createIfNull = true)
    {
        int chunkX = Mathf.FloorToInt(x / (float)CHUNK_SIZE);
        int chunkY = Mathf.FloorToInt(y / (float)CHUNK_SIZE);

        // 3. Check if the chunk exists, and create it if it doesn't.
        if (!Chunks.ContainsKey(new(chunkX, chunkY)))
        {
            /*if (createIfNull)
            {
                CreateNewChunk(chunkX, chunkY);
            }
            else
            {
                return null;
            }*/
            return null;
        }
        return Chunks[new(chunkX, chunkY)];
    }

    public MapObject GetObjectAtWorldPos(int x, int y)
    {
        Chunk chunk = GetChunk(x, y);
        if (chunk == null) return null;
        return chunk.GetObjectAtPos(x,y);
    }

    public TileType GetTileTypeAtWorldPos(Vector2Int pos)
    {
        return GetTileTypeAtWorldPos(pos.x, pos.y);
    }

    public TileType GetTileTypeAtWorldPos(int x, int y)
    {
        Chunk chunk = GetChunk(x, y);
        if (chunk == null) return TileType.NONE;
        return GetChunk(x, y).GetTileTypeAtWorldPos(x, y);
    }

    public IEnumerator GenerateOres()
    {
        if (Ores == null || Ores.Count == 0) yield break;

        while (true)
        {
            yield return new WaitForSeconds(OreGenerateTickInterval);

            foreach (var chunk in Chunks.Values.ToList())
            {
                chunk.GenerateOre();
            }
        }
    }

    public IEnumerator GenerateTrees()
    {
        if (TreeObject == null) yield break;
        while(true)
        {
            yield return new WaitForSeconds(TreeGenerateTickInterval);

            foreach (var chunk in Chunks.Values.ToList())
            {
                chunk.GenerateTree();
            }
        }
    }
}

public enum TileType
{
    NONE,
    GRASS,
    STONE,
    WATER
}