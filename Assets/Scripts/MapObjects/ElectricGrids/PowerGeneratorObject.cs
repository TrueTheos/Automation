using System;
using System.Collections.Generic;

using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;

using Unity.VisualScripting;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PowerGeneratorObject : MapObject, IPowerGridUser
    {
        public List<IPowerGridUser> ConnectedGridUsers { get; set; }
        
        [SerializeField]
        [Inspectable]
        public List<LineRenderer> ConnectedPowerCables { get; set; }
        public Vector3 ConnectionPoint => transform.position;
        public PowerState PowerState { get; set; }

        public bool debugGowno;
        public bool DebugHasPower
        {
            get => debugGowno;
            set => debugGowno = value;
        }

        private IPowerGridUser _iPowerGridUser;

        private void Start()
        {
            _iPowerGridUser = this;

            PowerState |= PowerState.HasPower;
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
        
        public bool CanConnect(IPowerGridUser requestingUser)
        {
            return true;
        }

        public bool HasPower()
        {
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