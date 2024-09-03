using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ConveyorBeltObject : MapObject, IRightClick, IItemReceiver
    {
        public IItemReceiver Parent;
        public IItemReceiver Child;

        public ItemObject IncomingItem;
        public ItemObject Item;

        public SerializableDictionary<BeltDirection, Sprite> BeltSprites = new();
        public enum BeltDirection { NS, SN, WE, EW, SE, ES, NE, EN, NW, WN, WS, SW}

        private static Dictionary<Direction, Direction> _oppositeDirection = new()
        {
            {Direction.Left, Direction.Right },
            {Direction.Right, Direction.Left },
            {Direction.Up, Direction.Down },
            {Direction.Down, Direction.Up },
        };

        private BeltDirection _direction;
        private BeltDirection _defaultDirection;

        private Direction _inputDirection = Direction.None;
        public Direction InputConnection
        {
            get
            {
                return _inputDirection;
            }
            set
            {
                _inputDirection = value;
            }
        }
        private Direction _outputDirection = Direction.None;
        public Direction OutputConnection
        {
            get
            {
                return _outputDirection;
            }
            set
            {
                _outputDirection = value;
            }
        }

        protected override void OnPlace(Direction direction)
        {
            _defaultDirection = DirectionToBeltDirection(direction);
            InputConnection = _oppositeDirection[direction];
            OutputConnection = direction;
            UpdateSprite(Direction.None);
            ConveyorBeltManager.Instance.AddBelt(this);
        }

        private BeltDirection DirectionToBeltDirection(Direction dir)
        {
            if (dir == Direction.Left) return BeltDirection.EW;
            if (dir == Direction.Right) return BeltDirection.WE;
            if (dir == Direction.Up) return BeltDirection.SN;
            if (dir == Direction.Down) return BeltDirection.NS;
            return BeltDirection.EW;
        }

        public void MoveItems()
        {
            if(Child == null)
            {
                UpdateNeighbor(OutputConnection);
            }
            if(Parent == null)
            {
                UpdateNeighbor(InputConnection);
            }

            if(Parent != null && Parent is not ConveyorBeltObject && CanReceive(null))
            {
                Item output = Parent.TakeOutItem();
                if (output != null)
                {
                    IncomingItem = MapManager.Instance.SpawnItem(output, transform.position.x, transform.position.y, 1);
                }
            }

            if(IncomingItem != null)
            {
                if (Item != null && Child != null && Child.CanReceive(IncomingItem))
                {
                    Child.ReceiveItem(Item);
                    Item = null;
                }

                if (Item == null)
                {
                    Item = IncomingItem;
                    IncomingItem = null;
                    Item.MoveToPosition(transform.position);
                }
            }     
            else
            {
                if (Item != null && Child != null && Child.CanReceive(Item))
                {
                    Child.ReceiveItem(Item);
                    Item = null;
                }
            }
        }

        public void SpawnItem(NormalItem item)
        {
            if (CanReceive(null))
            {
                ItemObject newItemObject = MapManager.Instance.SpawnItem(item, transform.position.x, transform.position.y, 1);
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
            if (comingFrom == Direction.None || comingFrom != Direction.Down) UpdateNeighborConnection(Vector2.up, Direction.Down, Direction.Up);
            if (comingFrom == Direction.None || comingFrom != Direction.Up) UpdateNeighborConnection(Vector2.down, Direction.Up, Direction.Down);
            if (comingFrom == Direction.None || comingFrom != Direction.Right) UpdateNeighborConnection(Vector2.left, Direction.Right, Direction.Left);
            if (comingFrom == Direction.None || comingFrom != Direction.Left) UpdateNeighborConnection(Vector2.right, Direction.Left, Direction.Right);
        }

        private void UpdateNeighbor(Direction dir)
        {
            if (dir == Direction.Down) UpdateNeighborConnection(Vector2.down, Direction.Up, Direction.Down);
            if (dir == Direction.Up) UpdateNeighborConnection(Vector2.up, Direction.Down, Direction.Up);
            if (dir == Direction.Right) UpdateNeighborConnection(Vector2.right, Direction.Left, Direction.Right);
            if (dir == Direction.Left) UpdateNeighborConnection(Vector2.left, Direction.Right, Direction.Left);
        }

        private void UpdateNeighborConnection(Vector2 direction, Direction neighborInput, Direction neighborOutput)
        {
            Vector2 neighborPos = (Vector2)transform.position + direction;
            IItemReceiver neighbor = GetReceiverAtPosition(neighborPos);
            if (neighbor != null)
            {
                if(neighbor is ConveyorBeltObject belt)
                {
                    UpdateBeltConnection(belt, neighborInput, neighborOutput);
                }
                else
                {
                    UpdateConnection(neighbor, neighborInput, neighborOutput);
                }
            }
        }

        private void UpdateConnection(IItemReceiver receiver, Direction neighborInput, Direction neighborOutput)
        {
            if (Parent == null)
            {
                Parent = receiver;
                InputConnection = neighborOutput;

                if (OutputConnection == Direction.None)
                {
                    OutputConnection = _oppositeDirection[neighborOutput];
                }
            }
            else if (Child == null && Parent != receiver)
            {
                Child = receiver;
                OutputConnection = neighborOutput;

                if (OutputConnection == Direction.None)
                {
                    OutputConnection = _oppositeDirection[neighborOutput];
                }
            }

            BeltDirection dir = GetBeltDirection();
            _direction = dir;
            SpriteRend.sprite = BeltSprites[dir];
        }

        private void UpdateBeltConnection(ConveyorBeltObject belt, Direction neighborInput, Direction neighborOutput)
        {
            if (Parent == null && belt.Child == null)
            {
                Parent = belt;
                belt.Child = this;
                belt.OutputConnection = neighborInput;

                if (belt.InputConnection == Direction.None || belt.Parent == null)
                {
                    belt.InputConnection = neighborOutput;
                }
                InputConnection = neighborOutput;

                if (OutputConnection == Direction.None)
                {
                    OutputConnection = neighborInput;
                }

                belt.UpdateSprite(neighborInput);
                BeltDirection dir = GetBeltDirection();
                _direction = dir;
                SpriteRend.sprite = BeltSprites[dir];
            }
            else if (Child == null && belt.Parent == null)
            {
                Child = belt;
                belt.Parent = this;
                belt.InputConnection = neighborInput;

                if (belt.OutputConnection == Direction.None || belt.Child == null)
                {
                    belt.OutputConnection = neighborOutput;
                }

                OutputConnection = neighborOutput;

                if (InputConnection == Direction.None)
                {
                    InputConnection = neighborInput;
                }

                belt.UpdateSprite(neighborInput);
                BeltDirection dir = GetBeltDirection();
                _direction = dir;
                SpriteRend.sprite = BeltSprites[dir];
            }     
        }

        private IItemReceiver GetReceiverAtPosition(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(position);
            foreach (Collider2D collider in colliders)
            {
                IItemReceiver receiver = collider.GetComponent<IItemReceiver>();
                if (receiver != null) return receiver;
            }
            return null;
        }

        private BeltDirection GetBeltDirection()
        {
            //if (Child == null && InputConnection != Direction.None)
            //{
            //    return DirectionToBeltDirection(_oppositeDirection[InputConnection]);
            //}

            //if (Parent == null && OutputConnection != Direction.None)
            //{
            //    return DirectionToBeltDirection(OutputConnection);
            //}

            if(InputConnection == Direction.Left)
            {
                if(OutputConnection == Direction.Right) return BeltDirection.WE;
                if(OutputConnection == Direction.Up) return BeltDirection.WN;
                if(OutputConnection == Direction.Down) return BeltDirection.WS;
            }
            if(InputConnection == Direction.Right)
            {
                if (OutputConnection == Direction.Left) return BeltDirection.EW;
                if (OutputConnection == Direction.Up) return BeltDirection.EN;
                if (OutputConnection == Direction.Down) return BeltDirection.ES;
                if (OutputConnection == Direction.None) return DirectionToBeltDirection(_oppositeDirection[InputConnection]);
            }
            if(InputConnection == Direction.Down)
            {
                if (OutputConnection == Direction.Right) return BeltDirection.SE;
                if (OutputConnection == Direction.Up) return BeltDirection.SN;
                if (OutputConnection == Direction.Left) return BeltDirection.SW;
                if (OutputConnection == Direction.None) return DirectionToBeltDirection(_oppositeDirection[InputConnection]);
            }
            if (InputConnection == Direction.Up)
            {
                if (OutputConnection == Direction.Right) return BeltDirection.NE;
                if (OutputConnection == Direction.Down) return BeltDirection.NS;
                if (OutputConnection == Direction.Left) return BeltDirection.NW;
                if (OutputConnection == Direction.None) return DirectionToBeltDirection(_oppositeDirection[InputConnection]);
            }

            return _defaultDirection;
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
            IncomingItem = item;
        }

        public bool CanReceive(ItemObject item)
        {
            return Item == null || (Child != null && Child.CanReceive(Item));
        }

        public Item GetOutputData()
        {
            return Item.ItemData;
        }

        public Item TakeOutItem()
        {           
            Item = null;
            return Item.ItemData;
        }
    }

    public enum Direction
    {
        None = -1,
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
}
