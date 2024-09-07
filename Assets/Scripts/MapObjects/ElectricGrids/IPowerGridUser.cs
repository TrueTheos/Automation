using System;
using System.Collections.Generic;

using Assets.Scripts;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    public interface IPowerGridUser : IRightClick
    {
        public List<IPowerGridUser> ConnectedGridUsers { get; set; }
        public List<LineRenderer> ConnectedPowerCables { get; set; }
        public Vector3 ConnectionPoint { get; }
        public PowerState PowerState { get; protected set; }

        public bool DebugHasPower { get; set; }
        
        public abstract bool CanConnect(IPowerGridUser requestingUser);
        public abstract bool HasPower(IPowerGridUser questingUser, List<IPowerGridUser> checkedUsers);
        public abstract bool IsConnected();

        public void RefreshPowerState()
        {
            PowerState = PowerState.OffGrid;

            if (HasPower(this, new List<IPowerGridUser>()))
            {
                PowerState |= PowerState.HasPower;
            }

            if (IsConnected())
            {
                PowerState |= PowerState.IsConnected;
            }

            if (PowerState.HasFlag(PowerState.HasPower))
            {
                foreach (var connectedPowerCable in ConnectedPowerCables)
                {
                    connectedPowerCable.startColor = Color.green;
                    connectedPowerCable.endColor = Color.green;
                }
            }
        }

        public void OnPowerGridUserClick(Player player)
        {
            player.CableManager.HandleConnectingGridUsers(this);
        }
        
        public void ConnectUsers(IPowerGridUser connectingUser, LineRenderer lineRenderer)
        {
            if (connectingUser == this)
            {
                return;
            }
            
            if (!ConnectedGridUsers.Contains(connectingUser))
            {
                ConnectedGridUsers.Add(connectingUser);
            }
            
            if (!connectingUser.ConnectedGridUsers.Contains(connectingUser))
            {
                connectingUser.ConnectedGridUsers.Add(this);
            }
            
            ConnectedPowerCables.Add(lineRenderer);
            connectingUser.ConnectedPowerCables.Add(lineRenderer);
            
            RefreshPowerState();
            connectingUser.RefreshPowerState();
        }

        public void DisconnectUsers(IPowerGridUser powerGridUser)
        {
            ConnectedGridUsers.Remove(powerGridUser);

            powerGridUser.ConnectedGridUsers.Remove(this);
        }
    }

    [Flags]
    public enum PowerState
    {
        OffGrid = 0,
        HasPower = 1,
        IsConnected = 2 
    }
}