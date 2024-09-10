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
    [RequireComponent(typeof(BoxCollider2D))]
    public class PipeObject : FluidUserObject
    {
        [SerializeField] private SerializableDictionary<FluidType, Color> _fluidColors;

        [SerializeField] private float _capacity;
        public override float Capacity
        {
            get => _capacity;
            set { _capacity = value; }
        }
        [SerializeField] private float _currentFluid;
        public override float CurrentFill
        {
            get => _currentFluid;
            set 
            { 
                _currentFluid = value;
                if (_fluidColors != null)
                {
                    var color = _fluidSprite.color;
                    _fluidSprite.color = new Color(color.r, color.g, color.b, value / Capacity);
                }
            }
        }

        public FluidType FluidType = FluidType.None;

        [SerializeField] private SpriteRenderer _bgSprite, _fluidSprite;

        [Serializable]
        public class PipeSprite
        {
            public Sprite Pipe, BG;
        }

        public SerializableDictionary<Connection, PipeSprite> PipeSprites;
        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            Place();
            FluidManager.Instance.RegisterContainer(this);
        }

        public bool Place()
        {
            ConnectToAdjacentFluidUsers();

            return true;
        }

        private void ConnectToAdjacentFluidUsers()
        {
            CheckAndConnectFluidUser(Vector2.up, Connection.Up);
            CheckAndConnectFluidUser(Vector2.right, Connection.Right);
            CheckAndConnectFluidUser(Vector2.down, Connection.Down);
            CheckAndConnectFluidUser(Vector2.left, Connection.Left);
        }

        private void CheckAndConnectFluidUser(Vector2 adjacentPosition, Connection con)
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

                if (other is WaterPumpObject pump)
                {
                    UpdateFluidType(FluidType.Water);
                }
                else if (other is PipeObject pipe && pipe.FluidType != FluidType.None)
                {
                    UpdateFluidType(pipe.FluidType);
                }

                UpdateConnections(FluidType);
                other.UpdateConnections(FluidType);
                FluidManager.Instance.UpdateNetworks();
            }
        }
        public void UpdateFluidType(FluidType newFluidType)
        {
            if (FluidType != FluidType.None) return;
            if (FluidType != newFluidType)
            {
                FluidType = newFluidType;
                UpdateSprite();

                // Propagate the fluid type to connected pipes
                foreach (var connection in ConnectedObjects.Values)
                {
                    if (connection is PipeObject pipe)
                    {
                        pipe.UpdateFluidType(newFluidType);
                    }
                }
            }
        }


        public override void UpdateConnections(FluidType fluidType)
        {
            UpdateFluidType(fluidType);
            CurrentConnections = Connection.None;

            foreach (var node in ConnectedObjects.Values)
            {
                Vector2 direction = node.transform.position - transform.position;
                if (direction.y > 0.5f) CurrentConnections |= Connection.Up;
                else if (direction.y < -0.5f) CurrentConnections |= Connection.Down;
                else if (direction.x > 0.5f) CurrentConnections |= Connection.Right;
                else if (direction.x < -0.5f) CurrentConnections |= Connection.Left;
            }

            UpdateSprite();
        }

        public void UpdateSprite()
        {
            PipeSprite spr;
            if (CurrentConnections != Connection.None && (CurrentConnections & (CurrentConnections - 1)) == 0)
            {
                spr = PipeSprites[CurrentConnections | GetOppositeConnection(CurrentConnections)];
            }
            else
            {
                if(PipeSprites.ContainsKey(CurrentConnections))
                {
                    spr = PipeSprites[CurrentConnections];
                }
                else
                {
                    Debug.LogWarning($"Pipe doesnt have this connection in dict: {CurrentConnections}");
                    return;
                }
                
            }
         
            SpriteRend.sprite = spr.Pipe;
            _bgSprite.sprite = spr.BG;
            _fluidSprite.sprite = spr.BG;
            _fluidSprite.color = _fluidColors[FluidType];
        }

        public override bool IsConnectedTo(FluidUserObject other)
        {
            return ConnectedObjects.ContainsValue(other);
        }

        public override bool CanConnect(FluidUserObject other, Connection comingFrom)
        {
            if(other is PipeObject pipe)
            {
                if (FluidType == FluidType.None || pipe.FluidType == FluidType.None || pipe.FluidType == FluidType) return true;
                return false;
            }
            return true;
        }
    }
}
