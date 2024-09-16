using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;
using Assets.Scripts.UI;
using Managers;

using UnityEngine;
using static Assets.Scripts.WattsUtils;

namespace MapObjects.ElectricGrids
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ElectricPoleObject : MapObject, IPowerGridUser
    {
        public PowerGrid PowerGrid { get; set; }
        [SerializeField] private Transform _connectionPoint;
        public Transform ConnectionPoint
        {
            get => _connectionPoint;
            set => _connectionPoint = value;
        }

        public Watt ConsumedPower { get; set; } = null;
        public Watt ProducedPower { get; set; } = null;

        public PowerGridUserType PowerGridUserType => PowerGridUserType.None;

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
        }

        public void OnMouseOver()
        {
            UIManager.Instance.MultimetrView.OnElectricPoleHover(this);
        }

        public void OnMouseExit()
        {
            UIManager.Instance.MultimetrView.OnElectricPoleHover(null);
        }

        public bool IsConsumingPower() => false;
    }
}
