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
    [RequireComponent(typeof(BoxCollider2D))]
    public class DrillObject : MapObject, IItemReceiver, IRightClick, IPowerGridUser
    {
        public int InputCapacity;
        public int OutputCapacity;
        public float SmeltDuration;

        public ParticleSystem ParticleSystem;
        public Animator Animator;

        private ItemAmount _inputItem;
        private ItemAmount _outputItem;

        private ElectricFurnaceView _furnaceView;

        private float _cooldown;

        public PowerGrid PowerGrid { get; set; }
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

        private bool CanDrill()
        {
            if (PowerGrid == null) return false;
            return true;
        }

        private void Update()
        {
            if(!CanDrill() || Speed <= 0)
            {
                Animator.SetBool("isDrilling", false);
                ParticleSystem.gameObject.SetActive(false);
                return;
            }

            Animator.SetBool("isDrilling", true);
            ParticleSystem.gameObject.SetActive(true);
        }

        //niezły check xd
        public bool CanReceive(ItemObject item)
        {
            return false;
        }

        public bool IsConsumingPower() => true;
        public void OnClick(Player player)
        {
            var selectedItemItem = player.PlayeMovement.SelectedItem.Item;

            if (selectedItemItem != null && selectedItemItem.ItemType == ItemType.Wire)
            {
                _iPowerGridUser.OnPowerGridUserClick(player);
            }
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void ReceiveItem(ItemObject item)
        {
            return;
        }

        public Item GetOutputData()
        {
            return null; //todo
        }

        public Item TakeOutItem()
        {
            return null; //todo
        }
    }
}
