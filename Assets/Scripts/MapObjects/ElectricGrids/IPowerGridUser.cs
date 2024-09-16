using System;

using Assets.Scripts;

using Managers;

using UnityEngine;
using static Assets.Scripts.WattsUtils;

namespace MapObjects.ElectricGrids
{
    public interface IPowerGridUser : IRightClick
    {
        PowerGrid PowerGrid { get; set; }
        Transform ConnectionPoint { get; set; }
        Watt ConsumedPower { get; }
        Watt ProducedPower { get; }

        PowerGridUserType PowerGridUserType { get;}

        void OnPowerGridUserClick(Player player)
        {
            if (PowerGrid == null)
            {
                PowerGrid = player.PowerGridManager.CreatePowerGrid(player.CableBuilder);
            }
            
            player.CableBuilder.HandleCableActionFor(this);
        }

        bool IsConsumingPower();
    }

    public enum PowerGridUserType
    {
        None, 
        Consumer,
        Producer,
        Battery
    }

    [Flags]
    public enum PowerState
    {
        OffGrid = 0,
        HasPower = 1,
        IsConnected = 2 
    }
}