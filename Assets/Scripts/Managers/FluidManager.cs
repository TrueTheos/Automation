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

        [SerializeField] private float _baseFlow;
        [SerializeField] private float _minFlow;
        [SerializeField] private float _scaleFactor;
        [SerializeField] private float _exponent;

        private void Awake()
        {
            Instance = this;
        }

        public class FluidNetwork
        {
            public List<FluidUserObject> Containers = new List<FluidUserObject>();
            public float TotalCapacity => Containers.Sum(c => c.Capacity);
            public float TotalFluid => Containers.Sum(c => c.CurrentFill);
            public float FluidLevel => TotalFluid / TotalCapacity;

            public void UpdateFluidLevels()
            {
                float level = FluidLevel;
                foreach (var container in Containers)
                {
                    container.CurrentFill = container.Capacity * level;
                }
            }

            public void AddFluid(float amount)
            {
                float newTotalFluid = Mathf.Min(TotalFluid + amount, TotalCapacity);
                float newLevel = newTotalFluid / TotalCapacity;
                foreach (var container in Containers)
                {
                    container.CurrentFill = container.Capacity * newLevel;
                }
            }
        }

        //TODO add breaking fluid objects

        private List<FluidNetwork> fluidNetworks = new List<FluidNetwork>();
        public void RegisterContainer(FluidUserObject container)
        {
            List<FluidNetwork> connectedNetworks = new List<FluidNetwork>();

            foreach (var network in fluidNetworks)
            {
                if (network.Containers.Any(c => c.IsConnectedTo(container)))
                {
                    connectedNetworks.Add(network);
                }
            }

            if (connectedNetworks.Count == 0)
            {
                FluidNetwork newNetwork = new FluidNetwork();
                newNetwork.Containers.Add(container);
                fluidNetworks.Add(newNetwork);
            }
            else if (connectedNetworks.Count == 1)
            {
                connectedNetworks[0].Containers.Add(container);
            }
            else
            {
                FluidNetwork mergedNetwork = new FluidNetwork();
                foreach (var network in connectedNetworks)
                {
                    mergedNetwork.Containers.AddRange(network.Containers);
                    fluidNetworks.Remove(network);
                }
                mergedNetwork.Containers.Add(container);
                fluidNetworks.Add(mergedNetwork);
            }

            UpdateNetworks();
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
            //if (pipeCount < 1) return 0;
            //return _baseFlow / (1 + Mathf.Pow(pipeCount / _scaleFactor, _exponent)) + _minFlow;
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
            if (network != null)
            {
                int pipeCount = network.Containers.Count(c => c is PipeObject);
                float flow = CalculateFlow(pipeCount) * deltaTime;
                network.AddFluid(flow);
            }
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
