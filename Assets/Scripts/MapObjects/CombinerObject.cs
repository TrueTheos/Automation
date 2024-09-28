using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static Assets.Scripts.Utilities;
using static UnityEditor.Rendering.CameraUI;

namespace Assets.Scripts.MapObjects
{
    public class CombinerObject : MapObject, IItemMover, IRightClick
    {
        public ConveyorBeltObject InputA, InputB;
        public ConveyorBeltObject Output;

        public enum BeltPriority { None, A, B}
        [HideInInspector] public BeltPriority InputPriority;

        [SerializeField] private SpriteRenderer _secondArtRend;
        [SerializeField] private SerializableDictionary<Direction, Sprite[]> _artDict = new();

        private Dictionary<Direction, Vector2Int[]> _connectionOffsets = new()
        {
            // inputA, inputB, outputA, outputB
            {Direction.Right, new Vector2Int[] {new(-1,0), new(-1,1), new(1,0), new(1,1)}},
            {Direction.Left, new Vector2Int[] {new(1,0), new(1,1), new(-1,0), new(-1,1)}},
            {Direction.Up, new Vector2Int[] {new(0,-1), new(1,-1), new(0,1), new(1,1)}},
            {Direction.Down, new Vector2Int[] {new(0,1), new(1,1), new(0,-1), new(1,-1)}},
        };

        [SerializeField] private float _itemMoveSpeed => Output == null ? 1f : Output.ItemMoveSpeed;
        private ItemObject _currentItem;
        private Vector3 _itemStartPosition;
        private Vector3 _itemTargetPosition;
        private float _itemProgress;

        private bool _useInputA = true;

        private CombinerView _combinerView => UIManager.Instance.CombinerView;

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            SpriteRend.sprite = _artDict[direction][0];
            _secondArtRend.sprite = _artDict[direction][1];
        }

        private void Update()
        {
            MoveItem();
        }

        private void MoveItem()
        {
            if (_currentItem == null) return;

            _itemProgress += Time.deltaTime * _itemMoveSpeed;
            if (_itemProgress >= 1f)
            {
                if (Output != null && Output.CanReceive(_currentItem, this))
                {
                    Output.ReceiveItem(_currentItem);
                    _currentItem = null;
                    _itemProgress = 0f;
                }
                else
                {
                    _itemProgress = 1f;
                }
            }
            else
            {
                _currentItem.transform.position = Vector3.Lerp(_itemStartPosition, _itemTargetPosition, _itemProgress);
            }
        }

        public override Vector2Int[] GetOccupiedPositions(int x, int y)
        {
            switch (Direction)
            {
                case Direction.Down:
                case Direction.Up:
                    return new Vector2Int[] { new(x, y), new(x + 1, y) };
                case Direction.Right:
                case Direction.Left:
                    return new Vector2Int[] { new(x, y), new(x, y + 1) };
                default: return new Vector2Int[] { new(x, y) };
            }
        }

        public void UpdateConnections(ConveyorBeltObject belt, bool input)
        {
            Vector2Int beltPos = new(belt.X, belt.Y);
            Vector2Int myPos = new(X, Y);

            if (input)
            {
                if (InputA == null && myPos + _connectionOffsets[Direction][0] == beltPos)
                {
                    InputA = belt;
                }
                if (InputB == null && myPos + _connectionOffsets[Direction][1] == beltPos)
                {
                    InputB = belt;
                }
            }
            else //output
            {
                if (Output == null)
                {
                    if (myPos + _connectionOffsets[Direction][2] == beltPos ||
                        myPos + _connectionOffsets[Direction][3] == beltPos)
                    {
                        Output = belt;
                    }
                }
            }
        }

        public bool CanReceive(ItemObject item, IItemMover sender)
        {
            var inputBelt = DetermineInputConveyor();
            return _currentItem == null && Output != null && inputBelt == sender;
        }

        public void ReceiveItem(ItemObject item)
        {
            _currentItem = item;
            _itemStartPosition = item.transform.position;
            _itemTargetPosition = transform.position;
            _itemProgress = 0f;
            ToggleInput();
        }

        private ConveyorBeltObject DetermineInputConveyor()
        {
            if(InputPriority == BeltPriority.A)
            {
                if (InputA != null && InputA.GetOutputData() != null)
                    return InputA;
            }
            if (InputPriority == BeltPriority.B)
            {
                if (InputB != null && InputB.GetOutputData() != null)
                    return InputB;
            }

            if (InputA != null && InputB != null)
            {
                var selectedInput = _useInputA ? InputA : InputB;
                if (selectedInput.GetOutputData() != null)
                {
                    return selectedInput;
                }
                var alternateInput = _useInputA ? InputB : InputA;
                return alternateInput.GetOutputData() != null ? alternateInput : null;
            }

            if (InputA != null && InputA.GetOutputData() != null)
                return InputA;
            if (InputB != null && InputB.GetOutputData() != null)
                return InputB;

            return null;
        }

        public void OnClick(Player player)
        {
            _combinerView.OpenCombiner(this);
        }

        private void ToggleInput()
        {
            if (InputA != null && InputB != null)
            {
                _useInputA = !_useInputA;
            }
        }

        public Item TakeOutItem()
        {
            if (_currentItem != null)
            {
                Item itemToReturn = _currentItem.ItemData;
                DestroyImmediate(_currentItem.gameObject);
                _currentItem = null;
                return itemToReturn;
            }
            return null;
        }

        public Item GetOutputData()
        {
            return _currentItem?.ItemData;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
