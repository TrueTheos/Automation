using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class OreObject : MapObject
    {
        public NormalItem Drop;
        public Vector2Int DropCountRange;
        public TileType SpawnTileType;

        public override void Break()
        {
            if (Drop != null)
            {
                float randomXOffset = Random.Range(-.6f, .6f);
                float randomYOffset = Random.Range(-.6f, .6f);
                MapManager.Instance.SpawnItem(Drop, 
                    transform.position.x + randomXOffset, 
                    transform.position.y + randomYOffset, 
                    Random.Range(DropCountRange.x, DropCountRange.y),
                    ItemObject.ItemState.OnGround);
            }
            base.Break();
        }
    }
}
