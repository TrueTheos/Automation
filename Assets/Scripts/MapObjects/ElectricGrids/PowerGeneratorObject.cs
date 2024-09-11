using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;

using Managers;

using UnityEngine;
using static Assets.Scripts.WattsUtils;

namespace MapObjects.ElectricGrids
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PowerGeneratorObject : MapObject, IPowerGridUser
    {
        public PowerGrid PowerGrid { get; set; }
        [SerializeField] private Transform _connectionPoint;
        public Transform ConnectionPoint
        {
            get => _connectionPoint;
            set => _connectionPoint = value;
        }

        public Watt ConsumedPower { get; set; } = null;

        [SerializeField] private Watt producedPower;
        public Watt ProducedPower => producedPower;

        public PowerGridUserType PowerGridUserType => PowerGridUserType.Producer;

        private IPowerGridUser _iPowerGridUser;

        private void Start()
        {
            _iPowerGridUser = this;
        }

        public void OnClick(Player player)
        {
            var selectedItemItem = player.PlayeMovement.SelectedItem.Item;

            if (selectedItemItem != null && selectedItemItem.ItemType == ItemType.Wire)
            {
                _iPowerGridUser.OnPowerGridUserClick(player);
            }

            //Tu Logika na ewentualne dorzucanie do pieca? drewno żeby była para? coś takiego
        }
    }
}