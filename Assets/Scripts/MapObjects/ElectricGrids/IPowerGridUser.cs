using System;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    public interface IPowerGridUser : IRightClick
    {
        List<IPowerGridUser> ConnectedGridUsers { get; set; }
        List<LineRenderer> ConnectedPowerCables { get; set; }
        Vector3 ConnectionPoint { get; }
        PowerState PowerState { get; set; }
        
        bool DebugHasPower { get; set; }
        
        abstract bool CanConnect(IPowerGridUser requestingUser);
        bool IsConnected() => ConnectedGridUsers != null && ConnectedGridUsers.Any();

        bool HasPower()
        {
            return PowerState.HasFlag(PowerState.HasPower);
        }
        
        public void SetPowerState(PowerState newPowerState)
        {
            PowerState = newPowerState;
            DebugHasPower = newPowerState.HasFlag(PowerState.HasPower);

            if (newPowerState.HasFlag(PowerState.HasPower))
            {
                DebugHasPower = true;
                
                foreach (var connectedPowerCable in ConnectedPowerCables)
                {
                    connectedPowerCable.startColor = Color.green;
                    connectedPowerCable.endColor = Color.green;
                }
            }
            else
            {
                DebugHasPower = false;
                
                foreach (var connectedPowerCable in ConnectedPowerCables)
                {
                    connectedPowerCable.startColor = Color.grey;
                    connectedPowerCable.endColor = Color.grey;
                }
            }

            foreach (var connectedGridUser in ConnectedGridUsers)
            {
                if (connectedGridUser.PowerState != newPowerState)
                {
                    connectedGridUser.SetPowerState(newPowerState);
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

            if (HasPower())
            {
                connectingUser.SetPowerState(PowerState);
            }
            else if (connectingUser.HasPower())
            {
                SetPowerState(connectingUser.PowerState);
            }
        }

        public void DisconnectUsers(IPowerGridUser disconnectedUser)
        {
            ConnectedGridUsers.Remove(disconnectedUser);
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