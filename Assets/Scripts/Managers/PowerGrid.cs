using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.MapObjects;
using MapObjects.ElectricGrids;

using Unity.VisualScripting;

using UnityEngine;
using static Assets.Scripts.WattsUtils;

namespace Managers
{
    public class PowerGrid
    {
        public Dictionary<IPowerGridUser, List<IPowerGridUser>> PowerGridConnections = new();
        public Dictionary<(IPowerGridUser, IPowerGridUser), PowerGridCable> Cables = new();
        
        private readonly CableBuilder _playerCableBuilder;
        
        public event Action<IPowerGridUser, IPowerGridUser> OnConnected;
        public event Action<IPowerGridUser, IPowerGridUser> OnDisconnected;
        public event Action OnPowerRecalculationRequired;

        public Watt ProducedPower;
        public Watt ConsumedPower;

        public PowerGrid(CableBuilder playerCableBuilder)
        {
            _playerCableBuilder = playerCableBuilder;

            OnConnected += OnConnected_DrawCable();
            OnDisconnected += OnDisconnected_DeleteCable;
            OnPowerRecalculationRequired += OnPowerRecalculationRequired_Calculate;
        }
        
        ~PowerGrid()
        {
            Debug.Log("Instance destroyed");
            
            OnConnected -= OnConnected_DrawCable();
            OnDisconnected -= OnDisconnected_DeleteCable;
            OnPowerRecalculationRequired -= OnPowerRecalculationRequired_Calculate;
        }

        public void UpdatePower()
        {
            OnPowerRecalculationRequired?.Invoke();
        }

        public void Connect(IPowerGridUser first, IPowerGridUser second)
        {
            if (TryAddConnection(first, second) && TryAddConnection(second, first))
            {
                first.PowerGrid = this;
                second.PowerGrid = this;
                
                OnConnected?.Invoke(first, second);
                OnPowerRecalculationRequired?.Invoke();
            }
            else
            {
                Debug.Log($"Coś się zjebało i próbowało połączyć już połączonych: {first} and {second}");
            }
        }

        private bool TryAddConnection(IPowerGridUser first, IPowerGridUser second)
        {
            //Try to get connection list from dict, and if failed try to add a key-value pair
            if (!PowerGridConnections.TryGetValue(first, out var connections))
            {
                return PowerGridConnections.TryAdd(first, new List<IPowerGridUser> { second });
            }

            if (!connections.Contains(second))
            {
                connections.Add(second);
                return true;
            }

            return false;
        }
        
        public void Disconnect(IPowerGridUser first, IPowerGridUser second)
        {
            if (TryDisconnect(first, second) && TryDisconnect(second, first))
            {
                OnDisconnected?.Invoke(first, second);
                OnPowerRecalculationRequired?.Invoke();
            }
            else
            {
                Debug.Log($"Coś się zjebało i próbowało rołączyć już rozłączonych: {first} and {second}");
            }
        }

        private bool TryDisconnect(IPowerGridUser first, IPowerGridUser second)
        {
            if (!PowerGridConnections.TryGetValue(first, out var connections))
            {
                return false;
            }

            var initialCount = connections.Count;
            connections.Remove(second);

            if (initialCount > connections.Count)
            {
                return true;
            }

            if (connections.Count == 0)
            {
                PowerGridConnections.Remove(first);
            }

            return false;
        }
        
        private void OnPowerRecalculationRequired_Calculate()
        {
            var cables = Cables.Values;

            var powerConsumers = PowerGridConnections.Keys.Where(x => x.PowerGridUserType == PowerGridUserType.Consumer).ToList();
            Watt availableW = SumWatts(PowerGridConnections.Keys.Where(x => x.PowerGridUserType == PowerGridUserType.Producer).Select(x => x.ProducedPower));
            ConsumedPower = SumWatts(powerConsumers.Select(x => x.ConsumedPower));

            ProducedPower = availableW;

            float speed = 0;

            if (availableW.Value == 0)
            {
                speed = 0;
            }
            else if (CompareWatts(ConsumedPower, availableW) > 0) // requiredW is greater than availableW
            {
                WattType highestUnit = GetHighestUnit(availableW.WattType, ConsumedPower.WattType);
                double availableInHighestUnit = ConvertToUnit(availableW, highestUnit);
                double requiredInHighestUnit = ConvertToUnit(ConsumedPower, highestUnit);
                speed = (float)(availableInHighestUnit / requiredInHighestUnit);
            }
            else
            {
                speed = 1;
            }

            foreach (var consumer in powerConsumers)
            {
                if(consumer is MapObject mapObject)
                {
                    mapObject.Speed = speed;
                }
            }

            if (availableW.Value > 0)
            {
                foreach (var cable in cables)
                {
                    cable.SetState(PowerState.HasPower);
                }
            }
            else
            {
                foreach (var cable in cables)
                {
                    cable.SetState(PowerState.IsConnected);
                }
            }
        }

        private Action<IPowerGridUser, IPowerGridUser> OnConnected_DrawCable()
        {
            return delegate(IPowerGridUser user1, IPowerGridUser user2)
            {
                if (_playerCableBuilder == null)
                {
                    return;
                }

                var cable = _playerCableBuilder.DrawConnectionCable(user1, user2);
                Cables.Add((user1, user2), cable);
            };
        }

        private void OnDisconnected_DeleteCable(IPowerGridUser user1, IPowerGridUser user2)
        {
            if (Cables.TryGetValue((user1, user2), out var cable))
            {
                Cables.Remove((user1, user2));
                UnityEngine.Object.Destroy(cable.gameObject);
            }
            
            if (Cables.TryGetValue((user2, user1), out var cable2))
            {
                Cables.Remove((user2, user1));
                
                if (!cable2.gameObject.IsDestroyed())
                {
                    UnityEngine.Object.Destroy(cable2.gameObject);
                }
            }
            
            //Search for a different connection between the two
        }

        public bool HaveDirectConnection(IPowerGridUser powerGridUser, IPowerGridUser otherUser)
        {
            return PowerGridConnections.ContainsKey(powerGridUser) &&
                   PowerGridConnections[powerGridUser].Any(x => x == otherUser);
        }

        public int GetGridSize()
        {
            return PowerGridConnections.Keys.Count;
        }
        
        public void DevourGrid(PowerGrid otherGrid)
        {
            if (otherGrid == this)
            {
                return;
            }
            
            foreach (var connection in otherGrid.PowerGridConnections)
            {
                connection.Key.PowerGrid = this;
                PowerGridConnections.TryAdd(connection.Key, connection.Value);
            }
            
            foreach (var cables in otherGrid.Cables)
            {
                Cables.TryAdd(cables.Key, cables.Value);
            }
            
            otherGrid.PowerGridConnections.Clear();
            otherGrid.Cables.Clear();
        }
    }
    
}