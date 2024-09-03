using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public enum FluidType { None, Water, Steam }

    public class FluidManager : MonoBehaviour
    {
        public static FluidManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public class FluidNetwork
        {
            public List<IFluidReceiver> Containers = new List<IFluidReceiver>();
            public float TotalCapacity => Containers.Sum(c => c.Capacity);
            public float TotalFluid => Containers.Sum(c => c.CurrentFluid);
            public float FluidLevel => TotalFluid / TotalCapacity;

            public void UpdateFluidLevels()
            {
                float level = FluidLevel;
                foreach (var container in Containers)
                {
                    container.CurrentFluid = container.Capacity * level;
                }
            }

            public void AddFluid(float amount)
            {
                float newTotalFluid = Mathf.Min(TotalFluid + amount, TotalCapacity);
                float newLevel = newTotalFluid / TotalCapacity;
                foreach (var container in Containers)
                {
                    container.CurrentFluid = container.Capacity * newLevel;
                }
            }
        }

        private List<FluidNetwork> fluidNetworks = new List<FluidNetwork>();
        public void RegisterContainer(IFluidReceiver container)
        {
            var network = fluidNetworks.FirstOrDefault(n => n.Containers.Any(c => c.IsConnectedTo(container)));
            if (network == null)
            {
                network = new FluidNetwork();
                fluidNetworks.Add(network);
            }
            network.Containers.Add(container);
            network.UpdateFluidLevels();
        }

        public void UpdateNetworks()
        {
            foreach (var network in fluidNetworks)
            {
                network.UpdateFluidLevels();
            }
        }

        public float CalculateFlow(int pipeCount)
        {
            if (pipeCount < 1) return 0;
            if (pipeCount < 197)
            {
                return 10000f / (3 * pipeCount - 1) + 1000;
            }
            else
            {
                return 240000f / (pipeCount + 39);
            }
        }

        public void SimulatePumpFlow(WaterPumpObject pump, float deltaTime)
        {
            var network = fluidNetworks.First(n => n.Containers.Contains(pump));
            int pipeCount = network.Containers.Count(c => c is PipeObject);
            float flow = CalculateFlow(pipeCount) * deltaTime;
            network.AddFluid(flow);
        }
        /*(public bool HasFluid(IFluidReceiver startNode)
        {
            HashSet<IFluidReceiver> visited = new HashSet<IFluidReceiver>();
            return DFSFindPump(startNode, visited);
        }

        private bool DFSFindPump(IFluidReceiver node, HashSet<IFluidReceiver> visited)
        {
            if (visited.Contains(node))
                return false;

            visited.Add(node);

            if (node is WaterPumpObject)
                return true;

            foreach (var connection in node.Connections)
            {
                if (DFSFindPump(connection, visited))
                    return true;
            }

            return false;
        }

        public void DistributeFluid(IFluidReceiver source, )*/
    }
}
