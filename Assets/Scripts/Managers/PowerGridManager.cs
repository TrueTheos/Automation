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

        private void Update()
        {
            // zobaczymy jak wolne to będzie :D ~ theos
            foreach (var powerGrid in PowerGrids) 
            {
                if (powerGrid == null) continue;
                powerGrid.UpdatePower();
            }
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
            AddPowerGrid(powerGrid);

            return powerGrid;
        }
    }
}