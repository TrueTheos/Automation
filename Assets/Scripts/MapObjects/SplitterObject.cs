using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static Assets.Scripts.Utilities;

namespace Assets.Scripts.MapObjects
{
    public class SplitterObject : MapObject, IItemReceiver, IRightClick
    {
        public ConveyorBeltObject Input;
        public ConveyorBeltObject OutputA, OutputB;

        [HideInInspector] public Item FilterA, FilterB;

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
            {Direction.Down, new Vector2Int[] {new(0,1), new(1,1), new(0,-1), new(1,-1)}},
        };

        [SerializeField] private float _itemMoveSpeed = 1f;
        private ItemObject _currentItem;
        private Vector3 _itemStartPosition;
        private Vector3 _itemTargetPosition;
        private float _itemProgress;

        private SplitterView _splitterView => UIManager.Instance.SplitterView;

        protected override void OnPlace(Direction direction)
        {
            base.OnPlace(direction);
            SpriteRend.sprite = _artDict[direction][0];
            _secondArtRend.sprite = _artDict[direction][1];
            //if rotation update collider and second art
        }

        private void Update()
        {
            MoveItem();
            //TryReceiveFromInput();
        }

        private void MoveItem()
        {
            if (_currentItem == null) return;

            _itemProgress += Time.deltaTime * _itemMoveSpeed;
            if (_itemProgress >= 1f)
            {
                ConveyorBeltObject outputConveyor = DetermineOutputConveyor(_currentItem);
                if (outputConveyor != null && outputConveyor.CanReceive(_currentItem))
                {
                    outputConveyor.ReceiveItem(_currentItem);
                    _currentItem = null;
                    _itemProgress = 0f;
                    ToggleOutput();
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

        /*private void TryReceiveFromInput()
        {
            if (_currentItem == null)
            {
                ConveyorBeltObject inputConveyor = DetermineInputConveyor();
                if (inputConveyor != null)
                {
                    Item inputItemData = inputConveyor.GetOutputData();
                    if (inputItemData != null && CanReceiveItem(inputItemData))
                    {
                        Item takenItem = inputConveyor.TakeOutItem();
                        if (takenItem != null)
                        {
                            ItemObject newItemObject = MapManager.Instance.SpawnItem(takenItem, transform.position.x, transform.position.y, 1, ItemObject.ItemState.OnBelt);
                            ReceiveItem(newItemObject);
                            ToggleInput();
                        }
                    }
                }
            }
        }*/

        public override Vector2Int[] GetOccupiedPositions(int x, int y)
        {
            switch (Direction)
            {
                case Direction.Down:
                case Direction.Up:
                    return new Vector2Int[] { new(x, y), new(x + 1,y) };
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
                if (Input == null && myPos + _connectionOffsets[Direction][0] == beltPos)
                {
                    Input = belt;
                }
            }
            else //output
            {
                if (OutputA == null && myPos + _connectionOffsets[Direction][2] == beltPos)
                {
                    OutputA = belt;
                }
                if (OutputB == null && myPos + _connectionOffsets[Direction][3] == beltPos)
                {
                    OutputB = belt;
                }
            }
        }

        public bool CanReceive(ItemObject item)
        {
            return _currentItem == null && (OutputA != null || OutputB != null);
        }

        private bool CanReceiveItem(Item item)
        {
            return true;
            // todo return Filter == null || Filter.Equals(item);
        }

        public void ReceiveItem(ItemObject item)
        {
            _currentItem = item;
            _itemStartPosition = Input.transform.position;
            _itemTargetPosition = transform.position;
            _itemProgress = 0f;
        }

        private ConveyorBeltObject DetermineOutputConveyor(ItemObject item)
        {
            if(FilterA != null && FilterA == item.ItemData && OutputA.CanReceive(item))
            {
                return OutputA;
            }
            if (FilterB != null && FilterB == item.ItemData && OutputB.CanReceive(item))
            {
                return OutputB;
            }

            if (OutputA != null && OutputB != null)
            {
                return _useOutputA ? OutputA : OutputB;
            }
            else if (OutputA != null)
            {
                return OutputA;
            }
            else if (OutputB != null)
            {
                return OutputB;
            }
            return null;
        }

        public void OnClick(Player player)
        {
            _splitterView.OpenSplitter(this);
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
            if (_currentItem != null)
            {
                Item itemToReturn = _currentItem.ItemData;
                Destroy(_currentItem.gameObject);
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
