using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Utilities;

namespace Assets.Scripts.MapObjects
{
    public class SplitterObject : MapObject, IItemReceiver
    {
        public ConveyorBeltObject InputA, InputB;
        public ConveyorBeltObject OutputA, OutputB;

        public Item Filter;

        private bool _useInputA = true;
        private bool _useOutputA = true;

        [SerializeField] private SpriteRenderer _secondArtRend;
        [SerializeField] private SerializableDictionary<Direction, Sprite[]> _artDict = new();

        private Dictionary<Direction, Vector2Int[]> _connectionOffsets = new()
        {
            // inputA, inputB, outputA, outputB
            {Direction.Right, new Vector2Int[] {new(-1,0), new(-1,1), new(1,0), new(1,1)}},
            {Direction.Left, new Vector2Int[] {new(1,0), new(1,1), new(-1,0), new(-1,1)}},
            {Direction.Up, new Vector2Int[] {new(0,-1), new(1,-1), new(0,1), new(1,1)}},
            {Direction.Down, new Vector2Int[] {new(0,1), new(1,1), new(0,-1), new(1,-1)}}
        };

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            SpriteRend.sprite = _artDict[direction][0];
            _secondArtRend.sprite = _artDict[direction][1];
            //if rotation update collider and second art
        }

        public void UpdateConnections(ConveyorBeltObject belt, bool input)
        {
            Vector2Int beltPos = new(belt.X, belt.Y);
            Vector2Int myPos = new(X, Y);

            if (input)
            {
                if (InputA == null && myPos + _connectionOffsets[Direction][0] == beltPos) InputA = belt;
                if (InputB == null && myPos + _connectionOffsets[Direction][1] == beltPos) InputB = belt;
            }
            else //output
            {
                if (OutputA == null && myPos + _connectionOffsets[Direction][2] == beltPos) OutputA = belt;
                if (OutputB == null && myPos + _connectionOffsets[Direction][3] == beltPos) OutputB = belt;
            }
        }

        private bool IsInput(Direction direction)
        {
            // Assuming inputs are always on the left or top of the splitter
            return direction == Direction.Right || direction == Direction.Down;
        }

        private ConveyorBeltObject GetConveyorAtPosition(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(position);
            foreach (Collider2D collider in colliders)
            {
                ConveyorBeltObject conveyor = collider.GetComponent<ConveyorBeltObject>();
                if (conveyor != null) return conveyor;
            }
            return null;
        }

        private Direction GetDirectionFromOffset(Vector2Int offset)
        {
            if (offset.x > 0) return Direction.Left;
            if (offset.x < 0) return Direction.Right;
            if (offset.y > 0) return Direction.Down;
            if (offset.y < 0) return Direction.Up;
            return Direction.None;
        }

        public bool CanReceive(ItemObject item)
        {
            return (InputA != null || InputB != null) && (OutputA != null || OutputB != null);
        }

        public void ReceiveItem(ItemObject item)
        {
            ConveyorBeltObject outputConveyor = DetermineOutputConveyor();
            if (outputConveyor != null && outputConveyor.CanReceive(item))
            {
                outputConveyor.ReceiveItem(item);
                ToggleOutput();
            }
        }

        private ConveyorBeltObject DetermineOutputConveyor()
        {
            if (OutputA != null && OutputB != null)
            {
                return _useOutputA ? OutputA : OutputB;
            }
            return OutputA ?? OutputB;
        }

        private void ToggleOutput()
        {
            if (OutputA != null && OutputB != null)
            {
                _useOutputA = !_useOutputA;
            }
        }

        public Item TakeOutItem()
        {
            ConveyorBeltObject inputConveyor = DetermineInputConveyor();
            if (inputConveyor != null)
            {
                Item item = inputConveyor.TakeOutItem();
                ToggleInput();
                return item;
            }
            return null;
        }

        private ConveyorBeltObject DetermineInputConveyor()
        {
            if (InputA != null && InputB != null)
            {
                return _useInputA ? InputA : InputB;
            }
            return InputA ?? InputB;
        }

        private void ToggleInput()
        {
            if (InputA != null && InputB != null)
            {
                _useInputA = !_useInputA;
            }
        }

        public Item GetOutputData()
        {
            ConveyorBeltObject outputConveyor = DetermineOutputConveyor();
            return outputConveyor?.GetOutputData();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        private Direction GetNextDirection(Direction current)
        {
            return current switch
            {
                Direction.Right => Direction.Down,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,
                Direction.Up => Direction.Right,
                _ => Direction.Right,
            };
        }
    }
}
