﻿using Assets.Scripts.MapObjects;
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
                    if (container == null) continue;
                    container.CurrentFill = container.Capacity * level;
                }
            }

            public void AddFluid(float amount)
            {
                float newTotalFluid = Mathf.Min(TotalFluid + amount, TotalCapacity);
                float newLevel = newTotalFluid / TotalCapacity;
                foreach (var container in Containers)
                {
                    if (container == null) continue;
                    container.CurrentFill = Mathf.Min(container.CurrentFill + newLevel, container.Capacity);
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

        public void UnregisterContainer(FluidUserObject container)
        {
            FluidNetwork network = fluidNetworks.FirstOrDefault(n => n.Containers.Contains(container));
            if (network != null)
            {
                network.Containers.Remove(container);
                ReevaluateNetworkIntegrity(network);
            }
        }

        private void ReevaluateNetworkIntegrity(FluidNetwork network)
        {
            if (network.Containers.Count == 0)
            {
                fluidNetworks.Remove(network);
                return;
            }

            List<FluidUserObject> visited = new List<FluidUserObject>();
            Queue<FluidUserObject> queue = new Queue<FluidUserObject>();
            queue.Enqueue(network.Containers[0]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited.Add(current);

                foreach (var neighbor in network.Containers.Where(c => c.IsConnectedTo(current) && !visited.Contains(c)))
                {
                    queue.Enqueue(neighbor);
                }
            }

            if (visited.Count < network.Containers.Count)
            {
                // Network needs to be split
                FluidNetwork newNetwork = new FluidNetwork();
                newNetwork.Containers.AddRange(network.Containers.Except(visited));
                foreach (var container in newNetwork.Containers)
                {
                    network.Containers.Remove(container);
                }
                fluidNetworks.Add(newNetwork);

                network.UpdateFluidLevels();
                newNetwork.UpdateFluidLevels();
            }
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
                return 1000f / (3 * pipeCount - 1) + 10;
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
