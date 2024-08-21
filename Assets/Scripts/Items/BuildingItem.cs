using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu(fileName = "Building", menuName = "Item/Building")]
    public class BuildingItem : Item
    {
        public MapObject Prefab;
        public int MaxDurability;
    }
}
