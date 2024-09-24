using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IItemMover
    {
        public void ReceiveItem(ItemObject item);
        public bool CanReceive(ItemObject item, IItemMover sender);
        public Item GetOutputData();
        public Item TakeOutItem();
        public GameObject GetGameObject();
    }
}
