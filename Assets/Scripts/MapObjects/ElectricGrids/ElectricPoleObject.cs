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

        PowerState IPowerGridUser.PowerState { get; set; }

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

        public bool HasPower(IPowerGridUser questingUser, List<IPowerGridUser> checkedUsers)
        {
            if (checkedUsers.Contains(this))
            {
                return false;
            }
            
            checkedUsers.Add(this);
            
            return ConnectedGridUsers.Any(x => x != null && x != questingUser 
                                                         && x.HasPower(questingUser, checkedUsers));
        }

        public bool IsConnected()
        {
            return ConnectedGridUsers.Any();
        }

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
        
            ConnectedGridUsers = new List<IPowerGridUser>();
            ConnectedPowerCables = new List<LineRenderer>();
        }
    }
}
