using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    public class OreObject : MapObject
    {
        public Item Drop;
        public TileType SpawnTileType;

        public override void Break()
        {
            Instantiate(Drop, transform.position, Quaternion.identity);
            base.Break();
        }
    }
}
