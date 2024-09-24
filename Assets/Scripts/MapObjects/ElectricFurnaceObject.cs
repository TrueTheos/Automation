using Assets.Scripts.Items;
using Assets.Scripts.UI;
using Managers;
using MapObjects.ElectricGrids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.WattsUtils;

namespace Assets.Scripts.MapObjects
{
    public class ElectricFuranceObject : MapObject, IItemMover, IRightClick, IPowerGridUser
    {
        public int InputCapacity;
        public int OutputCapacity;
        public float SmeltDuration;

        public ParticleSystem ParticleSystem;

        private ItemAmount _inputItem = new(null, 0);
        private ItemAmount _outputItem = new(null, 0);

        private ElectricFurnaceView _furnaceView;

        private float _cooldown;

        public PowerGrid PowerGrid { get; set;}
        [SerializeField] private Transform _connectionPoint;
        public Transform ConnectionPoint
        {
            get => _connectionPoint;
            set => _connectionPoint = value;
        }

        [SerializeField] private Watt _consumedPower;
        public Watt ConsumedPower => _consumedPower;
        public Watt ProducedPower { get; set; } = null;
        public PowerGridUserType PowerGridUserType => PowerGridUserType.Consumer;
        private IPowerGridUser _iPowerGridUser;

        private void Awake()
        {
            _iPowerGridUser = this;
        }

        private void Start()
        {
            _furnaceView = UIManager.Instance.ElectricFurnaceView;
        }

        private bool CanSmelt()
        {
            if (_inputItem.Amount <= 0) return false; 
            if (_inputItem.GetItem() == null) return false; 
            if (_inputItem.GetItem() is NormalItem normalItem && normalItem.SmeltedResult == null) return false;
            if (IsFull()) return false;
            if (PowerGrid == null) return false;
            return true;
        }

        private void Update()
        {
            if (!CanSmelt() || Speed <= 0)
            {
                ParticleSystem.gameObject.SetActive(false);
                _furnaceView.ProgressBar.fillAmount = 0;
                return;
            }

            float currentSpeed = SmeltDuration + SmeltDuration * (1 - Speed);

            ParticleSystem.gameObject.SetActive(true);           

            _cooldown += Time.deltaTime;
            
            if (_cooldown >= currentSpeed)
            {
                _cooldown = 0;
                if (_outputItem.GetItem() == null)
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

            if (_furnaceView.IsOpen)
            {
                _furnaceView.ProgressBar.fillAmount = _cooldown / currentSpeed;
            }
        }

        //niezły check xd
        public bool CanReceive(ItemObject item, IItemMover sender)
        {
            if (_inputItem.Amount >= InputCapacity) return false;
            if (item.ItemData is not NormalItem normalItem) return false;
            if (!normalItem.ItemFlags.HasFlag(ItemFlags.Smeltable)) return false;
            if (_outputItem.GetItem() != null && _outputItem.GetItem() != normalItem.SmeltedResult) return false;
            if (IsFull()) return false;

            return true;
        }

        private bool IsFull()
        {
            if (_outputItem.Amount >= OutputCapacity) return true;
            return false;
        }

        public bool IsConsumingPower()
        {
            return CanSmelt();
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
            var selectedItemItem = player.PlayeMovement.SelectedItem.Item;

            if (selectedItemItem != null && selectedItemItem.ItemType == ItemType.Wire)
            {
                _iPowerGridUser.OnPowerGridUserClick(player);
            }
            else
            {
                _furnaceView.OpenFurnace(this);
                _furnaceView.UpdateSlots(_inputItem, _outputItem);
            }
        }

        public Item GetOutputData()
        {
            if (_outputItem.Amount <= 0) return null;
            return _outputItem.GetItem();
        }

        public Item TakeOutItem()
        {
            if (_outputItem.GetItem() == null) return null;
            _outputItem.Amount--;
            _furnaceView.UpdateSlots(_inputItem, _outputItem);
            return _outputItem.GetItem();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
