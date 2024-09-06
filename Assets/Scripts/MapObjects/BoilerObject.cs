using Assets.Scripts.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoilerObject : FluidUserObject
    {
        public override Connection OutputDirection { get; set; } = Connection.Right;
        public override Connection InputDirection { get; set; } = Connection.Left;

        public override FluidType InputFluidType { get; set; } = FluidType.Water;
        public override FluidType OutputFluidType { get; set; } = FluidType.Steam;

        public override float Capacity
        {
            get => _waterCapacity + _steamCapacity;
            set { /* Capacity is fixed for boiler */ }
        }

        public override float CurrentFill
        {
            get => _currentWater + _currentSteam;
            set
            {
                float totalFluid = value;
                _currentWater = Mathf.Min(totalFluid, _waterCapacity);
                _currentSteam = Mathf.Max(0, totalFluid - _currentWater);
            }
        }

        [SerializeField] private float _waterCapacity = 100f;
        [SerializeField] private float _steamCapacity = 100f;
        [SerializeField] private float _conversionRate = 1f;
        [SerializeField] private float _waterPullRate = 10f;

        private float _currentWater = 0f;
        private float _currentSteam = 0f;

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            Place();
            FluidManager.Instance.RegisterContainer(this);
        }

        public bool Place()
        {
            ConnectToAdjacentPipes();
            return true;
        }

        private void Update()
        {
            PullWaterFromInput();
            ProcessWaterToSteam();
            PushSteamToOutput();
        }

        private void PullWaterFromInput()
        {
            if (ConnectedObjects.TryGetValue(InputDirection, out FluidUserObject inputObject))
            {
                float waterToPull = Mathf.Min(_waterPullRate * Time.deltaTime, _waterCapacity - _currentWater, inputObject.CurrentFill);
                if (waterToPull > 0)
                {
                    _currentWater += waterToPull;
                    inputObject.CurrentFill -= waterToPull;
                    FluidManager.Instance.UpdateNetworks();
                }
            }
        }

        private void ProcessWaterToSteam()
        {
            if (_currentWater > 0)
            {
                float amountToConvert = Mathf.Min(_currentWater, (_steamCapacity - _currentSteam) / _conversionRate);
                _currentWater -= amountToConvert;
                _currentSteam += amountToConvert * _conversionRate;
            }
        }

        private void PushSteamToOutput()
        {
            if (_currentSteam > 0 && ConnectedObjects.TryGetValue(OutputDirection, out FluidUserObject outputObject))
            {
                float steamToPush = Mathf.Min(_currentSteam, outputObject.Capacity - outputObject.CurrentFill);
                if (steamToPush > 0)
                {
                    _currentSteam -= steamToPush;
                    outputObject.CurrentFill += steamToPush;
                    FluidManager.Instance.UpdateNetworks();
                }
            }
        }

        private void ConnectToAdjacentPipes()
        {
            foreach (var con in InputDirection.GetFlags().Cast<Connection>())
            {
                if (con == Connection.None) continue;
                CheckAndConnectPipe(ConnectionToVector(con), con);
            }
            foreach (var con in OutputDirection.GetFlags().Cast<Connection>())
            {
                if (con == Connection.None) continue;
                CheckAndConnectPipe(ConnectionToVector(con), con);
            }
        }

        private void CheckAndConnectPipe(Vector2 adjacentPosition, Connection con)
        {
            Vector2 pos = (Vector2)transform.position + adjacentPosition;
            Collider2D[] colliders = Physics2D.OverlapPointAll(pos);
            foreach (Collider2D collider in colliders)
            {
                PipeObject receiver = collider.GetComponent<PipeObject>();
                if (receiver != null)
                {
                    Connect(receiver, con);
                }
            }
        }


        public override void Connect(FluidUserObject other, Connection con)
        {
            if (!ConnectedObjects.Values.Contains(other) && other.CanConnect(this, GetOppositeConnection(con)))
            {
                ConnectedObjects[con] = other;
                other.Connect(this, GetOppositeConnection(con));

                UpdateConnections(FluidType.Steam);
                other.UpdateConnections(FluidType.Steam);
            }
        }

        public override void UpdateConnections(FluidType fluidType)
        {
            CurrentConnections = Connection.None;

            foreach (var kvp in ConnectedObjects)
            {
                CurrentConnections |= kvp.Key;
            }
        }

        public override bool IsConnectedTo(FluidUserObject other)
        {
            return ConnectedObjects.ContainsValue(other);
        }

        public override bool CanConnect(FluidUserObject other, Connection comingFrom)
        {
            if (other is not PipeObject pipe) return false;
            if (ConnectedObjects.ContainsKey(comingFrom) && ConnectedObjects[comingFrom] != null) return false;
            if (comingFrom == InputDirection)
            {
                return pipe.FluidType == FluidType.None || pipe.FluidType == InputFluidType;
            }
            if (comingFrom == OutputDirection)
            {

                return pipe.FluidType == FluidType.None || pipe.FluidType == OutputFluidType;
            }

            return false;
        }
    }
}
