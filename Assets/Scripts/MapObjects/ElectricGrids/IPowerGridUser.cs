using System;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts;

namespace MapObjects.ElectricGrids
{
    public interface IPowerGridUser : IRightClick
    {
        public List<IPowerGridUser> ConnectedGridUsers { get; set; }

        public bool DebugHasPower { get; set; }
        
        public void ConnectUsers(IPowerGridUser powerGridUser)
        {
            if (powerGridUser == this)
            {
                return;
            }
            
            if (!ConnectedGridUsers.Contains(powerGridUser))
            {
                ConnectedGridUsers.Add(powerGridUser);
            }
            
            if (!powerGridUser.ConnectedGridUsers.Contains(powerGridUser))
            {
                powerGridUser.ConnectedGridUsers.Add(this);
            }
        }

        public void DisconnectUsers(IPowerGridUser powerGridUser)
        {
            ConnectedGridUsers.Remove(powerGridUser);

            powerGridUser.ConnectedGridUsers.Remove(this);
        }
        
        public abstract bool CanConnect(IPowerGridUser requestingUser);
        public abstract bool HasPower(IPowerGridUser requestingUser, List<IPowerGridUser> checkedUsers);
        public abstract bool IsConnected();

        public PowerGridUserState GetPowerState()
        {
            var state = PowerGridUserState.OffGrid;

            // if (HasPower(this))
            // {
            //     state |= PowerGridUserState.HasPower;
            // }

            if (IsConnected())
            {
                state |= PowerGridUserState.IsConnected;
            }

            return state;
        }

        public void OnPowerGridUserClick(Player player)
        {
            //TODO add building type to item? If it's just place or connect or drag, dunno
            if (player.PlayeMovement.CurrentObjectBeingConnected == null)
            {
                player.PlayeMovement.CurrentObjectBeingConnected = this;

                //TODO Spawn Wire between this and mouse
            }
            else
            {
                //TODO Spawn Wire between connected users

                var wireStartObject = player.PlayeMovement.CurrentObjectBeingConnected;

                if (wireStartObject != null)
                {
                    wireStartObject.ConnectUsers(wireStartObject);

                    List<IPowerGridUser> checkList = new();
                    wireStartObject.DebugHasPower = DebugHasPower = HasPower(this, checkList);
                }

                player.PlayeMovement.CurrentObjectBeingConnected = null;
                //player.PlayeMovement.CurrentObjectBeingConnected = this;
            }
        }
    }

    [Flags]
    public enum PowerGridUserState
    {
        OffGrid = 0,
        HasPower = 1,
        IsConnected = 2 
    }
}