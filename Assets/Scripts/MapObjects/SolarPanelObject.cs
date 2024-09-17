using Assets.Scripts.Items;
using Assets.Scripts.Managers;
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
    public class SolarPanelObject : MapObject, IPowerGridUser
    {
        [SerializeField] private AnimationCurve _efficiencyCurve;

        public PowerGrid PowerGrid { get; set; }

        [SerializeField] private Watt maxPowerSupplied;
        private Watt _currentPowerSupply;
        public Watt ProducedPower => _currentPowerSupply;

        public Watt ConsumedPower { get; set; } = null;
        public PowerGridUserType PowerGridUserType => PowerGridUserType.Producer;

        [SerializeField] private Transform _connectionPoint;
        public Transform ConnectionPoint
        {
            get => _connectionPoint;
            set => _connectionPoint = value;
        }

        private DayNightCycleManager _dayNightManager;
        private IPowerGridUser _iPowerGridUser;

        private void Awake()
        {
            _iPowerGridUser = this;
        }

        private void Start()
        {
            _currentPowerSupply = new Watt(maxPowerSupplied.WattType, maxPowerSupplied.Value);
            _dayNightManager = DayNightCycleManager.Instance;
        }

        private void Update()
        {
            _currentPowerSupply.Value = maxPowerSupplied.Value * Mathf.Clamp01(_efficiencyCurve.Evaluate(_dayNightManager.PercentOfDay())); 
        }

        public bool IsConsumingPower()
        {
            return false;
        }

        public void OnClick(Player player)
        {
            var selectedItemItem = player.PlayeMovement.SelectedItem.Item;

            if (selectedItemItem != null && selectedItemItem.ItemType == ItemType.Wire)
            {
                _iPowerGridUser.OnPowerGridUserClick(player);
            }
        }
    }
}
