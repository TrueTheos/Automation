using System;

using Assets.Scripts;

using Managers;

using UnityEngine;

namespace MapObjects.ElectricGrids
{
    public interface IPowerGridUser : IRightClick
    {
        PowerGrid PowerGrid { get; set; }
        Vector3 ConnectionPoint { get; }
        int PowerAmount { get; }
        
        void OnPowerGridUserClick(Player player)
        {
            if (PowerGrid == null)
            {
                PowerGrid = player.PowerGridManager.CreatePowerGrid(player.CableBuilder);
            }
            
            player.CableBuilder.HandleCableActionFor(this);
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