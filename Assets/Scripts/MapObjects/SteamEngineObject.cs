using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Managers;

namespace Assets.Scripts.MapObjects
{
    public class SteamEngineObject : FluidUserObject
    {
        private FluidManager waterManager;
        public override Connection InputDirection { get; set; } = Connection.Left;

        public override FluidType InputFluidType { get; set; } = FluidType.Steam;

        private void Start()
        {
            waterManager = FluidManager.Instance;
        }

        private void Update()
        {
            CheckWaterAvailability();
        }

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            Place();
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
                FluidUserObject receiver = collider.GetComponent<FluidUserObject>();
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

            foreach (var node in ConnectedObjects.Values)
            {
                Vector2 direction = node.transform.position - transform.position;
                if (direction.y > 0.5f) CurrentConnections |= Connection.Up;
                else if (direction.y < -0.5f) CurrentConnections |= Connection.Down;
                else if (direction.x > 0.5f) CurrentConnections |= Connection.Right;
                else if (direction.x < -0.5f) CurrentConnections |= Connection.Left;
            }
        }

        private void CheckWaterAvailability()
        {
            //todo
        }

        public override bool IsConnectedTo(FluidUserObject other)
        {
            return ConnectedObjects.ContainsValue(other);
        }

        public override bool CanConnect(FluidUserObject other, Connection comingFrom)
        {
            if (other is not PipeObject pipe) return false;
            if (comingFrom != InputDirection) return false;
            if (ConnectedObjects.ContainsKey(comingFrom) && ConnectedObjects[comingFrom] != null) return false;
            if (pipe.FluidType == FluidType.None|| pipe.FluidType == InputFluidType) return true;
            return false;
        }

    }
}
