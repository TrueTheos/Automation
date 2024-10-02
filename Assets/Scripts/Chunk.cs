using Assets.Scripts.Managers;
using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;
using static Assets.Scripts.Utilities;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Chunk : MonoBehaviour
    {
        public bool Visible;

        [HideInInspector] public bool GeneratedDualTiles;

        public enum ChunkType { Pure, Impure }
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

        public void ChangeVisibility(bool visible)
        {
            Visible = visible;
            Fog.gameObject.SetActive(Visible);
        }

        public void Init(int x, int y, int chunkSize, ChunkType type)
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

            Fog.transform.position = new Vector2((X * _chunkSize) + (_chunkSize / 2), (Y * _chunkSize) + (_chunkSize / 2));
        }

        public void GenerateOre()
        {
            if (OresCount >= MapGenerator.MAXIMUM_ORES || OresCount >= TilesCount[TileType.STONE]) return;
            float spawnChance = Random.Range(0f, 1f);
            if (spawnChance <= MapGenerator.Instance.SpawnProbability.Evaluate(OresCount / MapGenerator.MAXIMUM_ORES))
            {
                OreObject ore = MapGenerator.Instance.Ores.GetRandom();

                if (ore == null) return;

                Vector2Int spawnSpace = GetFreeSpaceWorld(ore.SpawnTileType);

                if (spawnSpace != Vector2Int.zero)
                {
                    MapManager.Instance.SpawnObject(ore, spawnSpace.x, spawnSpace.y, Direction.None, false);
                }
            }
        }

        public void GenerateTree()
        {
            if (TreesCount >= MapGenerator.MAXIMUM_TREES || TreesCount >= TilesCount[TileType.GRASS]) return;
            float spawnChance = Random.Range(0f, 1f);
            if (spawnChance <= MapGenerator.Instance.SpawnProbability.Evaluate(TreesCount / MapGenerator.MAXIMUM_TREES))
            {
                Vector2Int spawnSpace = GetFreeSpaceWorld(TileType.GRASS);

                if (spawnSpace != Vector2Int.zero)
                {
                    MapManager.Instance.SpawnObject(MapGenerator.Instance.TreeObject, spawnSpace.x, spawnSpace.y, Direction.None, false);
                }
            }
        }

        public bool IsFreeWorldPos(int x, int y)
        {
            return Objects[x - WorldX, y - WorldY] == null;
        }

        public void SetTileAtLocalPos(int x, int y, TileType type)
        {
            TileType previousType = _tiles[x, y];
            if (previousType != TileType.NONE && previousType != type && TilesCount[type] > 0)
            {
                if (TilesCount.ContainsKey(previousType)) TilesCount[previousType]--;
            }
            _tiles[x, y] = type;
            if (TilesCount.ContainsKey(type))
            {
                TilesCount[type]++;
            }
            else TilesCount[type] = 1;
        }

        public void SetTileAtWorldPos(int x, int y, TileType type)
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

        public TileType GetTileTypeAtWorldPos(int x, int y)
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

        public Vector2Int GetFreeSpaceLocal(TileType tileType = TileType.NONE)
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
                            availableSpaces.Add(new Vector2Int(x, y));
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

        public Vector2Int GetFreeSpaceWorld(TileType tileType = TileType.NONE)
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
                            availableSpaces.Add(new Vector2Int(x + WorldX, y + WorldY));
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
}
