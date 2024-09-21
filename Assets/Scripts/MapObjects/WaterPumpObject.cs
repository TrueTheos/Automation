using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Managers;
using static Assets.Scripts.Utilities;

namespace Assets.Scripts.MapObjects
{
    public class WaterPumpObject : FluidUserObject
    {
        public float PumpRate;
        public ParticleSystem ParticleSystem;
        private ParticleSystem.EmissionModule _emissionModule;

        public override Connection OutputDirection { get; set; } = Connection.Up;

        private MapGenerator _mapGenerator;

        private void Awake()
        {
            _outputFluidType = FluidType.Water;
            _emissionModule = ParticleSystem.emission;
        }


        protected override void OnPlace(Direction direction)
        {
            _mapGenerator = MapGenerator.Instance;
            base.OnPlace(direction);
            Place();
            FluidManager.Instance.RegisterContainer(this);     
        }

        private void Update()
        {
            if (_mapGenerator.GetTileTypeAtPos(X, Y - 1) != TileType.WATER &&
                _mapGenerator.GetTileTypeAtPos(X + 1, Y - 1) != TileType.WATER) return;
            FluidManager.Instance.SimulatePumpFlow(this, Time.deltaTime);

            if(ConnectedObjects.Values.Where(x => x != null).Count() > 0)
            {
                _emissionModule.enabled = true;
            }
            else
            {
                _emissionModule.enabled = false;
            }
        }

        public override bool IsConnectedTo(FluidUserObject other)
        {
            return ConnectedObjects.ContainsValue(other);
        }

        public bool Place()
        {
            ConnectToAdjacentPipes();

            return true;
        }

        private void ConnectToAdjacentPipes()
        {
            foreach (var con in InputDirection.GetFlags().Cast<Connection>())
            {
                if (con == Connection.None) continue;
                CheckAndConnectPipe(con);
            }
            foreach (var con in OutputDirection.GetFlags().Cast<Connection>())
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
            if (!ConnectedObjects.Values.Contains(other) && other.CanConnect(this, GetOppositeConnection(con)))
            {
                ConnectedObjects[con] = other;
                other.Connect(this, GetOppositeConnection(con));

                UpdateConnections(FluidType.Water);
                other.UpdateConnections(FluidType.Water);
                FluidManager.Instance.UpdateNetworks();
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

        public override bool CanConnect(FluidUserObject other, Connection comingFrom)
        {
            if (other is not PipeObject pipe) return false;
            if (comingFrom != OutputDirection) return false;
            if (ConnectedObjects.ContainsKey(comingFrom) && ConnectedObjects[comingFrom] != null) return false;
            return true;
        }

        public void UpdateSprite()
        {
            //spriteRenderer.sprite = pumpSprite;
        }

        
    }
}
