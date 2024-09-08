using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;

using Managers;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ElectricPoleObject : MapObject, IPowerGridUser
    {
        public PowerGrid PowerGrid { get; set; }
        public Vector3 ConnectionPoint => transform.position;
        
        [SerializeField]
        private int powerSupplied = -5;
        public int PowerAmount => powerSupplied;
        
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
    }
}
