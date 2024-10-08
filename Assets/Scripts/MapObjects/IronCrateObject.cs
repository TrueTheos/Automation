﻿using Assets.Scripts.Items;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    public class IronCrateObject : MapObject, IItemMover, IRightClick
    {
        public int SlotsCount;

        private ItemAmount[] _items;
        public ItemAmount[] Items => _items;

        private IronCrateView _ironCrateView;

        private void Awake()
        {
            _items = new ItemAmount[SlotsCount];
            for (int i = 0; i < SlotsCount; i++)
            {
                _items[i] = new(null, 0);
            }
        }

        private void Start()
        {
            _ironCrateView = UIManager.Instance.IronCrateView;
        }

        public void UpdateSlot(ItemSlot slot, int index)
        {
            _items[index].Item = slot.Item;
            _items[index].Amount = slot.Amount;
        }

        public bool CanReceive(ItemObject item, IItemMover sender)
        {
            foreach (var slot in Items)
            {
                if (slot.Item == null) return true;
                else if (slot.Item == item.ItemData && slot.Amount + item.Amount < slot.Item.MaxStack) return true;
            }
            return false;
        }

        public Item GetOutputData()
        {
            return _items.Select(x => x.Item).FirstOrDefault(x => x != null);
        }

        public void OnClick(Player player)
        {
            _ironCrateView.OpenIronCrate(this);
        }

        public void ReceiveItem(ItemObject incomingItem)
        {
            int toAdd = incomingItem.Amount;

            for (int i = 0; i < SlotsCount; i++)
            {
                if(toAdd <= 0)
                {
                    break;
                }

                if (_items[i].Item == null || _items[i].Item == incomingItem.ItemData)
                {
                    if (_items[i].Item == null)
                    {
                        _items[i].Item = incomingItem.ItemData;
                        _items[i].Amount = incomingItem.Amount;
                        if (_ironCrateView.IsOpen) _ironCrateView.UpdateSlot(i, _items[i]);
                        DestroyImmediate(incomingItem.gameObject);
                        return;
                    }
                    else
                    {
                        int freeAmount = _items[i].Item.MaxStack - _items[i].Amount;
                        
                        if(toAdd > freeAmount)
                        {
                            _items[i].Amount += freeAmount;
                            toAdd -= freeAmount;
                        }
                        else
                        {
                            _items[i].Amount += toAdd;
                            toAdd -= toAdd; //xd
                        }

                        if (_ironCrateView.IsOpen) _ironCrateView.UpdateSlot(i, _items[i]);
                    }
                }
            }

            DestroyImmediate(incomingItem.gameObject);
        }

        public Items.Item TakeOutItem()
        {
            for (int i = 0; i < SlotsCount; i++)
            {
                if (Items[i].Item != null && Items[i].Amount > 0)
                {
                    Items[i].Amount--;            
                    _ironCrateView.UpdateSlot(i, Items[i]);
                    return Items[i].Item;
                }
            }

            return null;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
