using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class Inventory : MonoBehaviour
    {
        public int MaxSlots;

        public InventorySlot[] Items;

        private void Awake()
        {
            Items = new InventorySlot[MaxSlots];
        }

        public bool HasItems(Item item, int amount)
        {
            if (!Items.Any(x => x != null && x.Item != null && x.Item == item)) return false;
            return Items.First(x => x.Item == item).Amount >= amount;
        }

        public bool HasEmptySlot()
        {
            return Items.Any(x => x == null);
        }

        public int GetEmptySlotIndex()
        {
            return Array.IndexOf(Items, Items.First(x => x == null));
        }

        public InventorySlot GetNotFullSlot(Item item)
        {
            return Items.FirstOrDefault(x => x != null && x.Item == item && x.Amount < x.Item.MaxStack);
        }

        public bool CanAddItem(ItemObject itemObj)
        {
            return GetNotFullSlot(itemObj.ItemData) != null;
        }

        public void AddItem(ItemObject itemObj, bool dropTheRest = false)
        {
            int leftOver = AddItem(itemObj.ItemData, itemObj.Stack);

            if(dropTheRest && leftOver > 0)
            {
                MapManager.Instance.SpawnItem(itemObj.ItemData, 
                    itemObj.transform.position.x, 
                    itemObj.transform.position.y, 
                    leftOver);
            }

            Destroy(itemObj.gameObject);
        }

        /// <summary>
        /// Add item to the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns>Returns how many items couldn't fit</returns>
        public int AddItem(Item item, int amount)
        {
            int toAdd = amount;

            while (toAdd > 0)
            {
                InventorySlot notFullSlot = GetNotFullSlot(item);

                if (notFullSlot != null)
                {
                    int freeAmount = notFullSlot.Item.MaxStack - notFullSlot.Amount;

                    if (toAdd > freeAmount)
                    {
                        notFullSlot.Amount += freeAmount;
                        toAdd -= freeAmount;
                    }
                    else
                    {
                        notFullSlot.Amount += toAdd;
                    }
                }
                else
                {
                    if (HasEmptySlot())
                    {
                        int freeSlot = GetEmptySlotIndex();

                        int amountToAdd = toAdd > item.MaxStack ? item.MaxStack : toAdd;

                        InventorySlot newSlot = new InventorySlot() { Item = item, Amount = amountToAdd };
                        toAdd -= amountToAdd;
                        Items[freeSlot] = newSlot;
                    }
                    else
                    {
                        return toAdd;
                    }
                }
            }

            return toAdd;
        }
    }

    public class InventorySlot
    {
        public Item Item;
        public int Amount;
    }
}
