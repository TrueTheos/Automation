using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;

using Unity.VisualScripting;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ElectricPoleObject : MapObject, IPowerGridUser
    {
        public List<IPowerGridUser> ConnectedGridUsers { get; set; }
        
        [SerializeField]
        [Inspectable]
        public List<LineRenderer> ConnectedPowerCables { get; set; }
        public Vector3 ConnectionPoint => transform.position;
        public PowerState PowerState { get; set; }


        public bool hasPowerDebug;
        public bool DebugHasPower
        {
            get => hasPowerDebug;
            set => hasPowerDebug = value;
        }
        
        private IPowerGridUser _iPowerGridUser;

        private void Start()
        {
            _iPowerGridUser = this;

            PowerState |= PowerState.OffGrid;
        }

        public void OnClick(Player player)
        {
            var selectedItemItem = player.PlayeMovement.SelectedItem.Item;

            if (selectedItemItem != null && selectedItemItem.ItemType == ItemType.Wire)
            {
                _iPowerGridUser.OnPowerGridUserClick(player);
            }
        }

        public bool CanConnect(IPowerGridUser requestingUser)
        {
            //Check distance
            return true;
        }

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
        
            ConnectedGridUsers = new List<IPowerGridUser>();
            ConnectedPowerCables = new List<LineRenderer>();
        }
    }
}
