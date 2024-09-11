using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Managers;
using static Assets.Scripts.Utilities;
using MapObjects.ElectricGrids;
using Managers;
using Assets.Scripts.Items;
using static Assets.Scripts.WattsUtils;

namespace Assets.Scripts.MapObjects
{
    public class SteamEngineObject : FluidUserObject, IPowerGridUser
    {
        public override FluidType InputFluidType { get; set; } = FluidType.Steam;
        public PowerGrid PowerGrid { get; set; }

        [SerializeField] private List<ParticleSystem> Particles;

        [SerializeField] private Transform _connectionPoint;
        public Transform ConnectionPoint
        {
            get => _connectionPoint;
            set => _connectionPoint = value;
        }

        [SerializeField]
        private Watt maxPowerSupplied;
        private Watt _currentPowerSupply;

        public Watt ConsumedPower { get; set; } = null;
        public Watt ProducedPower => _currentPowerSupply;

        public PowerGridUserType PowerGridUserType => PowerGridUserType.Producer;

        [SerializeField] private float _steamPullRate = 10f;
        [SerializeField] private float _steamConsumption = 10f;
        [SerializeField] private float _steamCapacity = 100f;
        private float _currentSteam = 0f;

        private IPowerGridUser _iPowerGridUser;

        private void Start()
        {
            _iPowerGridUser = this;
            _currentPowerSupply = maxPowerSupplied;
        }

        private void Update()
        {
            PullSteamFromInput();

            if(_currentSteam > 0f && _currentSteam > _steamConsumption)
            {
                foreach (var particle in Particles)
                {
                    ParticleSystem.EmissionModule emissionModule = particle.emission;
                    emissionModule.enabled = true;
                }

                _currentPowerSupply.Value = maxPowerSupplied.Value;

                _currentSteam -= _steamConsumption * Time.deltaTime;
            }
            else
            {
                foreach (var particle in Particles)
                {
                    ParticleSystem.EmissionModule emissionModule = particle.emission;
                    emissionModule.enabled = false;
                }
                _currentPowerSupply.Value = 0;
            }
        }

        private void PullSteamFromInput()
        {
            FluidUserObject inputObject = ConnectedObjects.Values.Where(x => x != null).FirstOrDefault();
            if (inputObject != null)
            {
                float steamToPull = Mathf.Min(_steamPullRate * Time.deltaTime, _steamCapacity - _currentSteam, inputObject.CurrentFill);
                if (steamToPull > 0)
                {
                    _currentSteam += steamToPull;
                    inputObject.CurrentFill -= steamToPull;
                    FluidManager.Instance.UpdateNetworks();
                }
            }
        }


        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            Place();
        }

        public void OnClick(Player player)
        {
            var selectedItemItem = player.PlayeMovement.SelectedItem.Item;

            if (selectedItemItem != null && selectedItemItem.ItemType == ItemType.Wire)
            {
                _iPowerGridUser.OnPowerGridUserClick(player);
            }
        }

        public bool Place()
        {
            ConnectToAdjacentPipes();

            return true;
        }

        private void ConnectToAdjacentPipes()
        {
            foreach (var con in Enum.GetValues(typeof(Connection)).Cast<Connection>())
            {
                if (con == Connection.None) continue;
                CheckAndConnectPipe(con);
            }
        }

        private void CheckAndConnectPipe(Connection con)
        {
            Vector2[] checkPositions = GetCheckPositions(con);

            foreach (Vector2 pos in checkPositions)
            {
                Collider2D[] colliders = Physics2D.OverlapPointAll(pos);
                foreach (Collider2D collider in colliders)
                {
                    FluidUserObject receiver = collider.GetComponent<FluidUserObject>();
                    if (receiver != null && receiver != this)
                    {
                        Connect(receiver, con);
                        return;
                    }
                }
            }
        }

        public override void Connect(FluidUserObject other, Connection con)
        {
            if (CanConnect(other, con) && !ConnectedObjects.Values.Contains(other) && other.CanConnect(this, GetOppositeConnection(con)))
            {              
                other.Connect(this, GetOppositeConnection(con));
                ConnectedObjects[con] = other;
                UpdateConnections(FluidType.Steam);
                other.UpdateConnections(FluidType.Steam);
            }
        }

        public override void UpdateConnections(FluidType fluidType)
        {
            CurrentConnections = Connection.None;

            foreach (var node in ConnectedObjects.Values)
            {
                Vector2 direction = node.transform.position - transform.position;
                if (direction.y > 0.5f) CurrentConnections |= Connection.Up;
                else if (direction.y < -0.5f) CurrentConnections |= Connection.Down;
                else if (direction.x > 0.5f) CurrentConnections |= Connection.Right;
                else if (direction.x < -0.5f) CurrentConnections |= Connection.Left;
            }
        }

        public override bool IsConnectedTo(FluidUserObject other)
        {
            return ConnectedObjects.ContainsValue(other);
        }

        public override bool CanConnect(FluidUserObject other, Connection comingFrom)
        {
            if (other is not PipeObject pipe) return false;
            if(ConnectedObjects.Values.Where(x => x != null).Count() > 0) return false;
            if (pipe.FluidType == FluidType.None|| pipe.FluidType == InputFluidType) return true;
            return false;
        }
    }
}
