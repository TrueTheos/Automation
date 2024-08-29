﻿using Assets.Scripts.Items;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Progress;
using Item = Assets.Scripts.Items.Item;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FurnaceObject : MapObject, IItemReceiver, IRightClick
    {
        public int InputCapacity;
        public int OutputCapacity;

        public float Speed;

        public ParticleSystem ParticleSystem;

        private ItemAmount _inputItem;
        private ItemAmount _outputItem;

        private FurnaceView _furnaceView;

        private float _cooldown;

        private void Start()
        {
            _furnaceView = UIManager.Instance.FurnaceView;
        }

        private void Update()
        {
            if (_inputItem.Amount <= 0 || 
                _inputItem.GetItem() == null ||
                (_inputItem.GetItem() is NormalItem normalItem &&
                normalItem.SmeltedResult == null) ||
                IsFull())
            {
                ParticleSystem.gameObject.SetActive(false);
                _furnaceView.ProgressBar.fillAmount = 0;
                return;
            }

            ParticleSystem.gameObject.SetActive(true);

            _cooldown += Time.deltaTime;

            if(_cooldown >= Speed)
            {
                _cooldown = 0;
                if(_outputItem.GetItem() == null)
                {
                    NormalItem item = _inputItem.GetItem() as NormalItem;
                    _outputItem.SetItem(item.SmeltedResult);
                    _outputItem.Amount = 1;
                    _inputItem.Amount--;
                    _furnaceView.UpdateSlots(_inputItem, _outputItem);
                }
                else
                {
                    _outputItem.Amount = _outputItem.Amount + 1;
                    _inputItem.Amount--;
                    _furnaceView.UpdateSlots(_inputItem, _outputItem);
                }        
            }

            if(_furnaceView.IsOpen)
            {
                _furnaceView.ProgressBar.fillAmount = _cooldown / Speed;
            }
        }

        //niezły check xd
        public bool CanReceive(ItemObject item)
        {
            if (_inputItem.Amount >= InputCapacity) return false;
            if(item.ItemData is not NormalItem normalItem) return false;
            if (!normalItem.ItemFlags.HasFlag(ItemFlags.Smeltable)) return false;
            if (_outputItem.GetItem() != null && _outputItem.GetItem() != item) return false;
            if (IsFull()) return false;

            return true;
        }

        private bool IsFull()
        {
            if ( _outputItem.Amount >= OutputCapacity) return true;
            return false;
        }

        public void UpdateItems(ItemSlot input, ItemSlot output)
        {
            _inputItem.SetItem(input.Item);
            _inputItem.Amount = input.Amount;
            _outputItem.SetItem(output.Item);
            _outputItem.Amount = output.Amount;
        }

        public void ReceiveItem(ItemObject item)
        {
            if (_inputItem.GetItem() == null) _inputItem.SetItem(item.ItemData);
            _inputItem.Amount += item.Amount;
            Destroy(item.gameObject);
        }

        public void OnClick(Player player)
        {
            _furnaceView.OpenFurnace(this);
            _furnaceView.UpdateSlots(_inputItem, _outputItem);
        }

        public Item GetOutputData()
        {
            return _outputItem.GetItem();
        }

        public Item TakeOutItem()
        {
            if(_outputItem.GetItem() == null) return null;
            _outputItem.Amount--;
            _furnaceView.UpdateSlots(_inputItem, _outputItem);
            return _outputItem.GetItem();
        }
    }
}
