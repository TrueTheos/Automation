using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Managers;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PipeObject : MapObject, IFluidReceiver
    {
        [SerializeField] private SerializableDictionary<FluidType, Color> _fluidColors;

        [SerializeField] private float _capacity;
        public float Capacity
        {
            get => _capacity;
            set { _capacity = value; }
        }
        [SerializeField] private float _currentFluid;
        public float CurrentFluid
        {
            get => _currentFluid;
            set 
            { 
                _currentFluid = value;
                var color = _fluidSprite.color;
                _fluidSprite.color = new Color(color.r, color.g, color.b, value / Capacity);
            }
        }

        private FluidType _fluidType = FluidType.None;

        public List<IFluidReceiver> Connections { get; private set; } = new List<IFluidReceiver>();
        public PipeConnection CurrentConnections { get; set; } = PipeConnection.None;

        private Dictionary<PipeConnection, PipeConnection> _oppositeConnections = new Dictionary<PipeConnection, PipeConnection>()
        {
            {PipeConnection.Left, PipeConnection.Right},
            {PipeConnection.Right, PipeConnection.Left},
            {PipeConnection.Up, PipeConnection.Down},
            {PipeConnection.Down, PipeConnection.Up }
        };
        public Vector3 Position => transform.position;

        [SerializeField] private SpriteRenderer _bgSprite, _fluidSprite;


        [Serializable]
        public class PipeSprite
        {
            public Sprite Pipe, BG;
        }

        public SerializableDictionary<PipeConnection, PipeSprite> PipeSprites;
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

                if (other is WaterPumpObject pump)
                {
                    UpdateFluidType(FluidType.Water);
                }
                else if (other is PipeObject pipe && pipe._fluidType != FluidType.None)
                {
                    UpdateFluidType(pipe._fluidType);
                }

                UpdateConnections(_fluidType);
                other.UpdateConnections(_fluidType);
                FluidManager.Instance.UpdateNetworks();
            }
        }
        public void UpdateFluidType(FluidType newFluidType)
        {
            if (_fluidType != newFluidType)
            {
                _fluidType = newFluidType;
                UpdateSprite();

                // Propagate the fluid type to connected pipes
                foreach (var connection in Connections)
                {
                    if (connection is PipeObject pipe)
                    {
                        pipe.UpdateFluidType(newFluidType);
                    }
                }
            }
        }


        public void UpdateConnections(FluidType fluidType)
        {
            UpdateFluidType(fluidType);
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
            PipeSprite spr;
            if (CurrentConnections != PipeConnection.None && (CurrentConnections & (CurrentConnections - 1)) == 0)
            {
                spr = PipeSprites[CurrentConnections | _oppositeConnections[CurrentConnections]];
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
            _fluidSprite.color = _fluidColors[_fluidType];
        }

        public bool IsConnectedTo(IFluidReceiver other)
        {
            return Connections.Contains(other);
        }
    }
}
