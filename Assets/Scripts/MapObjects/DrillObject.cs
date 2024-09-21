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
    public class DrillObject : MapObject, IItemReceiver, IRightClick, IPowerGridUser
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private Animator _animator;
        [SerializeField] private RandomDropTable _dropTable;

        [SerializeField] private float _cooldown;
        private float _currentCooldown;
        private Item _currentItem;

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
                _animator.SetBool("isDrilling", false);
                _particleSystem.gameObject.SetActive(false);
                return;
            }

            _animator.SetBool("isDrilling", true);
            _particleSystem.gameObject.SetActive(true);

            _currentCooldown += Time.deltaTime;

            if (_currentCooldown >= _cooldown)
            {
                _currentCooldown = 0;
                _currentItem = _dropTable.GetRandomDrop().Item;
            }
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
            var ret = Instantiate(_currentItem);
            _currentItem = null;
            return ret;
        }
    }
}
