using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Managers;

namespace Assets.Scripts.MapObjects
{
    public class SteamEngineObject : MapObject, IFluidReceiver
    {
        public List<IFluidReceiver> Connections { get; private set; } = new List<IFluidReceiver>();
        public PipeConnection CurrentConnections { get; set; } = PipeConnection.None;
        public Vector3 Position => transform.position;

        public float Capacity { get; set; }

        public float CurrentFluid { get; set; }

        private FluidManager waterManager;

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
            CheckAndConnectPipe(Vector2.up);
            CheckAndConnectPipe(Vector2.right);
            CheckAndConnectPipe(Vector2.down);
            CheckAndConnectPipe(Vector2.left);
        }

        private void CheckAndConnectPipe(Vector2 adjacentPosition)
        {
            Vector2 pos = (Vector2)transform.position + adjacentPosition;
            Collider2D[] colliders = Physics2D.OverlapPointAll(pos);
            foreach (Collider2D collider in colliders)
            {
                IFluidReceiver receiver = collider.GetComponent<IFluidReceiver>();
                if (receiver != null)
                {
                    Connect(receiver);
                }
            }
        }


        public void Connect(IFluidReceiver other)
        {
            if (!Connections.Contains(other))
            {
                Connections.Add(other);
                other.Connect(this);

                UpdateConnections(FluidType.Steam);
                other.UpdateConnections(FluidType.Steam);
            }
        }

        public void UpdateConnections(FluidType fluidType)
        {
            CurrentConnections = PipeConnection.None;

            foreach (var node in Connections)
            {
                Vector2 direction = node.Position - Position;
                if (direction.y > 0.5f) CurrentConnections |= PipeConnection.Up;
                else if (direction.y < -0.5f) CurrentConnections |= PipeConnection.Down;
                else if (direction.x > 0.5f) CurrentConnections |= PipeConnection.Right;
                else if (direction.x < -0.5f) CurrentConnections |= PipeConnection.Left;
            }

            UpdateSprite();
        }

        public void UpdateSprite()
        {
            // The sprite is updated in CheckWaterAvailability
        }

        private void CheckWaterAvailability()
        {
            //
        }

        public bool IsConnectedTo(IFluidReceiver other)
        {
            throw new NotImplementedException();
        }
    }
}
