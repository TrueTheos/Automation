using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts.MapObjects;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ElectricPoleObject : MapObject, IPowerGridUser
    {
        public List<IPowerGridUser> ConnectedGridUsers { get; set; }

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

        public bool CanConnect(IPowerGridUser requestingUser)
        {
            //Check distance
            return true;
        }

        public bool HasPower(IPowerGridUser requestingUser, List<IPowerGridUser> checkedUsers)
        {
            if (checkedUsers.Contains(this))
            {
                return false;
            }
            
            checkedUsers.Add(this);
            
            return ConnectedGridUsers.Any(x => x != null && x != requestingUser && x.HasPower(requestingUser, checkedUsers));
        }

        public bool IsConnected()
        {
            return ConnectedGridUsers.Any();
        }

        protected override void OnPlace()
        {
            base.OnPlace();

            ConnectedGridUsers = new List<IPowerGridUser>();
        }

        public void OnClick(Player player)
        {
            _iPowerGridUser.OnPowerGridUserClick(player);
        }
    }
}
