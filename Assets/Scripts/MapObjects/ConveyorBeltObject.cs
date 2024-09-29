using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Assets.Scripts.Utilities;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Assets.Scripts.MapObjects
{
    public class ConveyorBeltObject : MapObject, IRightClick, IItemMover
    {
        [SerializeField] private float _itemMoveSpeed;
        public float ItemMoveSpeed
        {
            get => _itemMoveSpeed;
            private set => _itemMoveSpeed = value;
        }
        //public ItemObject IncomingItem;
        public SerializableDictionary<BeltDirection, Sprite> BeltSprites = new();

        public IItemMover Parent { get; set; }
        public IItemMover Child { get; set; }
        public ItemObject Item { get; set; }
        public enum BeltDirection { NS, SN, WE, EW, SE, ES, NE, EN, NW, WN, WS, SW}

        private BeltDirection _direction;

        private Vector3 _itemStartPosition;
        private Vector3 _itemTargetPosition;
        private float _itemProgress;
        public Direction InputConnection { get; set; } = Direction.None;
        public Direction OutputConnection { get; set; } = Direction.None;

        //private Direction _inputDirection = Direction.None;
        //public Direction InputConnection
        //{
        //    get
        //    {
        //        return _inputDirection;
        //    }
        //    set
        //    {
        //        _inputDirection = value;
        //    }
        //}
        //private Direction _outputDirection = Direction.None;
        //public Direction OutputConnection
        //{
        //    get
        //    {
        //        return _outputDirection;
        //    }
        //    set
        //    {
        //        _outputDirection = value;
        //    }
        //}

        protected override void OnPlace(Direction direction)
        {
            _direction = DirectionToBeltDirection(direction);
            InputConnection = GetOppositeDirection(direction);
            OutputConnection = direction;   
            UpdateSprite(Direction.None);
        }

        private void Update()
        {
            MoveItem();
            TryReceiveFromParent();
        }

        private void FixedUpdate()
        {
            if (Child == null) UpdateNeighbor(OutputConnection);
            if (Parent == null) UpdateNeighbor(InputConnection);
        }

        private void MoveItem()
        {
            if (Item == null) return;

            _itemProgress += Time.deltaTime * ItemMoveSpeed;
            if (_itemProgress >= 1f)
            {
                if (Child != null && !ReferenceEquals(Child, null) && Child.CanReceive(Item, this))
                {
                    Child.ReceiveItem(Item);
                    Item = null;
                    _itemProgress = 0f;
                }
                else
                {
                    _itemProgress = 1f;
                }
            }
            else
            {
                Item.transform.position = Vector3.Lerp(_itemStartPosition, _itemTargetPosition, _itemProgress);
            }
        }

        private void TryReceiveFromParent()
        {
            if (Parent != null && Parent is not ConveyorBeltObject &&
                Parent is not SplitterObject && Parent is not CombinerObject &&
                CanReceive(null, Parent))
            {
                Item output = Parent.TakeOutItem();
                if (output != null)
                {
                    ReceiveItem(MapManager.Instance.SpawnItem(output, transform.position.x, transform.position.y, 1, ItemObject.ItemState.OnBelt));
                }
            }
        }

        private BeltDirection DirectionToBeltDirection(Direction dir) =>
           dir switch
           {
               Direction.Left => BeltDirection.EW,
               Direction.Right => BeltDirection.WE,
               Direction.Up => BeltDirection.SN,
               Direction.Down => BeltDirection.NS,
               _ => BeltDirection.EW
           };

        public void SpawnItem(NormalItem item)
        {
            if (CanReceive(null, null))
            {
                ItemObject newItemObject = MapManager.Instance.SpawnItem(item, transform.position.x, transform.position.y, 1, ItemObject.ItemState.OnBelt);
                ReceiveItem(newItemObject);
            }
        }

        public void UpdateSprite(Direction comingFrom)
        {
            if (comingFrom == Direction.None || InputConnection == Direction.None ||
                OutputConnection == Direction.None || _direction != GetBeltDirection())
            {
                _direction = GetBeltDirection();
                SpriteRend.sprite = BeltSprites[_direction];
                UpdateNeighbors(comingFrom);
            }
        }

        private void UpdateNeighbors(Direction comingFrom)
        {
            if(comingFrom == Direction.None)
            {
                UpdateNeighborConnection(DirectionToVector(InputConnection), GetOppositeDirection(InputConnection), InputConnection);
                UpdateNeighborConnection(DirectionToVector(OutputConnection), GetOppositeDirection(OutputConnection), OutputConnection);
            }
            if (comingFrom == Direction.None || comingFrom != Direction.Down) UpdateNeighborConnection(Vector2.up, Direction.Down, Direction.Up);
            if (comingFrom == Direction.None || comingFrom != Direction.Up) UpdateNeighborConnection(Vector2.down, Direction.Up, Direction.Down);
            if (comingFrom == Direction.None || comingFrom != Direction.Right) UpdateNeighborConnection(Vector2.left, Direction.Right, Direction.Left);
            if (comingFrom == Direction.None || comingFrom != Direction.Left) UpdateNeighborConnection(Vector2.right, Direction.Left, Direction.Right);
        }

        private void UpdateNeighbor(Direction dir)
        {
            Vector2 direction = dir switch
            {
                Direction.Down => Vector2.down,
                Direction.Up => Vector2.up,
                Direction.Right => Vector2.right,
                Direction.Left => Vector2.left,
                _ => Vector2.zero
            };
            UpdateNeighborConnection(direction, GetOppositeDirection(dir), dir);
        }

        private void UpdateNeighborConnection(Vector2 direction, Direction neighborInput, Direction neighborOutput)
        {
            //Vector2 neighborPos = (Vector2)transform.position + direction;
            //IItemReceiver neighbor = GetReceiverAtPosition(neighborPos);
            var neighbor = _mapGenerator.GetObjectAtPos(X + (int)direction.x, Y  + (int)direction.y);
            if (neighbor is IItemMover receiver)
            {
                if(neighbor is ConveyorBeltObject belt)
                {
                    UpdateBeltConnection(belt, neighborInput, neighborOutput);
                }
                else
                {
                    UpdateConnection(receiver, neighborInput, neighborOutput);
                }
            }
        }

        private void UpdateConnection(IItemMover receiver, Direction neighborInput, Direction neighborOutput)
        {
            if (Parent == null)
            {
                Parent = receiver;
                InputConnection = neighborOutput;

                if (OutputConnection == Direction.None)
                {
                    OutputConnection = GetOppositeDirection(neighborOutput);
                }

                if (receiver is SplitterObject splitter) splitter.UpdateConnections(this, false);
                if (receiver is CombinerObject combiner) combiner.UpdateConnections(this, false);
            }
            else if (Child == null && Parent != receiver)
            {
                Child = receiver;
                OutputConnection = neighborOutput;

                if (OutputConnection == Direction.None)
                {
                    OutputConnection = GetOppositeDirection(neighborOutput);
                }

                if (receiver is SplitterObject splitter) splitter.UpdateConnections(this, true);
                if (receiver is CombinerObject combiner) combiner.UpdateConnections(this, true);
            }

            UpdateSpriteDirection();
        }

        private void UpdateBeltConnection(ConveyorBeltObject belt, Direction neighborInput, Direction neighborOutput)
        {
            if (Child == belt || Parent == belt || belt.Child == this || belt.Parent == this) return;

            if (IsParallelBelt(belt, neighborInput, neighborOutput)) return;

            if (Parent == null && belt.Child == null)
            {
                Parent = belt;
                belt.Child = this;
                belt.OutputConnection = neighborInput;
                InputConnection = neighborOutput;
                if (OutputConnection == Direction.None)
                {
                    OutputConnection = neighborInput;
                }
                if (belt.InputConnection == Direction.None || belt.Parent == null)
                {
                    belt.InputConnection = neighborOutput;
                }

                belt.UpdateSpriteDirection();
                UpdateSpriteDirection();
            }
            else if (Child == null && belt.Parent == null)
            {
                Child = belt;
                belt.Parent = this;
                belt.InputConnection = neighborInput;
                OutputConnection = neighborOutput;
                if (InputConnection == Direction.None)
                {
                    InputConnection = neighborInput;
                }
                if (belt.OutputConnection == Direction.None || belt.Child == null)
                {
                    belt.OutputConnection = neighborOutput;
                }
                belt.UpdateSpriteDirection();
                UpdateSpriteDirection();
            }
            else if(Child == null && belt.Parent != null)
            {
                Child = belt;
                OutputConnection = neighborOutput;
                if (InputConnection == Direction.None)
                {
                    InputConnection = neighborInput;
                }
                UpdateSpriteDirection();
            }
        }

        private bool IsParallelBelt(ConveyorBeltObject belt, Direction neighborInput, Direction neighborOutput)
        {
            return (_direction == belt._direction) &&
                   (neighborInput != OutputConnection) &&
                   (neighborOutput != InputConnection);
        }

        private void UpdateSpriteDirection()
        {
            BeltDirection dir = GetBeltDirection();
            _direction = dir;
            SpriteRend.sprite = BeltSprites[dir];
        }


        private IItemMover GetReceiverAtPosition(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(position);
            return System.Array.Find(colliders, c => c.TryGetComponent<IItemMover>(out _))?.GetComponent<IItemMover>();
        }

        private BeltDirection GetBeltDirection()
        {
            return (InputConnection, OutputConnection) switch
            {
                (Direction.Left, Direction.Right) => BeltDirection.WE,
                (Direction.Left, Direction.Up) => BeltDirection.WN,
                (Direction.Left, Direction.Down) => BeltDirection.WS,
                (Direction.Right, Direction.Left) => BeltDirection.EW,
                (Direction.Right, Direction.Up) => BeltDirection.EN,
                (Direction.Right, Direction.Down) => BeltDirection.ES,
                (Direction.Down, Direction.Right) => BeltDirection.SE,
                (Direction.Down, Direction.Up) => BeltDirection.SN,
                (Direction.Down, Direction.Left) => BeltDirection.SW,
                (Direction.Up, Direction.Right) => BeltDirection.NE,
                (Direction.Up, Direction.Down) => BeltDirection.NS,
                (Direction.Up, Direction.Left) => BeltDirection.NW,
                (_, Direction.None) => DirectionToBeltDirection(GetOppositeDirection(InputConnection)),
                _ => _direction
            };
        }

        public void OnClick(Player player)
        {
            PlayerMovement playerMovement = player.PlayeMovement;
            if (playerMovement.SelectedItem != null && playerMovement.SelectedItem.Item is NormalItem normalItem)
            {
                player.Inventory.RemoveItemFromSlot(1, playerMovement.SelectedItem);
                SpawnItem(normalItem);
            }
        }

        public void ReceiveItem(ItemObject item)
        {
            Item = item;
            if(Parent != null)
            {
                _itemStartPosition = Parent.GetGameObject().transform.position;
            }
            else
            {
                _itemStartPosition = transform.position;
            }
            _itemTargetPosition = transform.position;
            _itemProgress = 0f;
        }

        public bool CanReceive(ItemObject item, IItemMover sender) => Item == null;

        public Item GetOutputData() => Item?.ItemData;

        public Item TakeOutItem()
        {
            if (Item != null)
            {
                Item itemToReturn = Item.ItemData;
                DestroyImmediate(Item.gameObject);
                Item = null;
                return itemToReturn;
            }
            return null;
        }

        public GameObject GetGameObject() => gameObject;

        public void OnDestroy()
        {
            if (Parent is ConveyorBeltObject parentBelt) parentBelt.Child = null;
            if (Child is ConveyorBeltObject childBelt) childBelt.Parent = null;
        }
    }
}
