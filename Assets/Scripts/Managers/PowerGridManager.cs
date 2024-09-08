using System.Collections.Generic;

using UnityEngine;

namespace Managers
{
    public class PowerGridManager : MonoBehaviour
    {
        public List<PowerGrid> PowerGrids;

        private void Awake()
        {
            PowerGrids = new List<PowerGrid>();
        }
 
        public void AddPowerGrid(PowerGrid powerGrid)
        {
            PowerGrids.Add(powerGrid);
        }
        
        public void RemovePowerGrid(PowerGrid powerGrid)
        {
            PowerGrids.Remove(powerGrid);
        }

        public PowerGrid CreatePowerGrid(CableBuilder playerCableBuilder)
        {
            var powerGrid = new PowerGrid(playerCableBuilder);
            //AddPowerGrid(powerGrid);

            return powerGrid;
        }
    }
}