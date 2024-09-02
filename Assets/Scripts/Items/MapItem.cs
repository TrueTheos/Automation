using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu(fileName = "MapItem", menuName = "Item/Map Item")]
    public class MapItem : Item
    {
        public MapObject Prefab;
        public int MaxDurability;
        public bool CanBeRotated;
        public Direction DefaultDirection = Direction.Down;
        public SerializableDictionary<Direction, Sprite> DirectionSprites;

    }
}
