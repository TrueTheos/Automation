using Assets.Scripts.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ConveyorBeltObject : MapObject, IRightClick, IItemReceiver
    {
        public IItemReceiver Parent;
        public IItemReceiver Child;

        public SpriteRenderer Sprite;

        public ItemObject IncomingItem;
        public ItemObject Item;

        public SerializableDictionary<BeltDirection, Sprite> BeltSprites = new();
        public enum BeltDirection { NS, SN, WE, EW, SE, ES, NE, EN, NW, WN, WS, SW}

        private static Dictionary<ConnectionType, ConnectionType> _oppositeDirection = new()
        {
            {ConnectionType.Left, ConnectionType.Right },
            {ConnectionType.Right, ConnectionType.Left },
            {ConnectionType.Up, ConnectionType.Down },
            {ConnectionType.Down, ConnectionType.Up },
        };

        public enum ConnectionType
        {
            None,
            Left,
            Right,
            Up,
            Down
        }

        private BeltDirection _direction;

        public ConnectionType InputConnection = ConnectionType.None;
        public ConnectionType OutputConnection = ConnectionType.None;

        protected override void OnPlace()
        {
            UpdateSprite(ConnectionType.None);
            GameManager.Instance.AddBelt(this);
        }

        public void MoveItems()
        {
            if(Child == null || Parent == null)
            {
                UpdateNeighbors(ConnectionType.None);
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

        public void UpdateSprite(ConnectionType comingFrom)
        {
            if(comingFrom == ConnectionType.None || _direction != GetBeltDirection())
            {
                _direction = GetBeltDirection();
                Sprite.sprite = BeltSprites[_direction];
                UpdateNeighbors(comingFrom);
            }
        }

        private void UpdateNeighbors(ConnectionType comingFrom)
        {
            if (comingFrom == ConnectionType.None || comingFrom != ConnectionType.Down) UpdateNeighborConnection(Vector2.up, ConnectionType.Down, ConnectionType.Up);
            if (comingFrom == ConnectionType.None || comingFrom != ConnectionType.Up) UpdateNeighborConnection(Vector2.down, ConnectionType.Up, ConnectionType.Down);
            if (comingFrom == ConnectionType.None || comingFrom != ConnectionType.Right) UpdateNeighborConnection(Vector2.left, ConnectionType.Right, ConnectionType.Left);
            if (comingFrom == ConnectionType.None || comingFrom != ConnectionType.Left) UpdateNeighborConnection(Vector2.right, ConnectionType.Left, ConnectionType.Right);
        }

        private void UpdateNeighborConnection(Vector2 direction, ConnectionType neighborInput, ConnectionType neighborOutput)
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

        private void UpdateConnection(IItemReceiver receiver, ConnectionType neighborInput, ConnectionType neighborOutput)
        {
            if (Parent == null)
            {
                Parent = receiver;
                InputConnection = neighborOutput;

                if (OutputConnection == ConnectionType.None)
                {
                    OutputConnection = _oppositeDirection[neighborOutput];
                }
            }
            else if (Child == null && Parent != receiver)
            {
                Child = receiver;
                OutputConnection = neighborOutput;

                if (OutputConnection == ConnectionType.None)
                {
                    OutputConnection = _oppositeDirection[neighborOutput];
                }
            }

            BeltDirection dir = GetBeltDirection();
            Sprite.sprite = BeltSprites[dir];
        }

        private void UpdateBeltConnection(ConveyorBeltObject belt, ConnectionType neighborInput, ConnectionType neighborOutput)
        {
            if (Parent == null && belt.Child == null)
            {
                Parent = belt;
                belt.Child = this;
                belt.OutputConnection = neighborInput;

                if (belt.InputConnection == ConnectionType.None)
                {
                    belt.InputConnection = _oppositeDirection[neighborInput];
                }
                InputConnection = neighborOutput;

                if (OutputConnection == ConnectionType.None)
                {
                    OutputConnection = _oppositeDirection[neighborOutput];
                }
            }
            else if (Child == null && belt.Parent == null)
            {
                Child = belt;
                belt.Parent = this;
                belt.InputConnection = neighborInput;

                if (belt.InputConnection == ConnectionType.None)
                {
                    belt.InputConnection = _oppositeDirection[neighborInput];
                }

                OutputConnection = neighborOutput;

                if (OutputConnection == ConnectionType.None)
                {
                    OutputConnection = _oppositeDirection[neighborOutput];
                }
            }

            belt.UpdateSprite(neighborOutput);
            BeltDirection dir = GetBeltDirection();
            Sprite.sprite = BeltSprites[dir];
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
            if (Child == null && InputConnection != ConnectionType.None)
            {
                if (InputConnection == ConnectionType.Left) return BeltDirection.WE;
                if (InputConnection == ConnectionType.Up) return BeltDirection.NS;
                if (InputConnection == ConnectionType.Right) return BeltDirection.EW;
                if (InputConnection == ConnectionType.Down) return BeltDirection.SN;
            }

            if (Parent == null && OutputConnection != ConnectionType.None)
            {
                if (OutputConnection == ConnectionType.Left) return BeltDirection.EW;
                if (OutputConnection == ConnectionType.Up) return BeltDirection.SN;
                if (OutputConnection == ConnectionType.Right) return BeltDirection.WE;
                if (OutputConnection == ConnectionType.Down) return BeltDirection.NS;
            }

            if(InputConnection == ConnectionType.Left)
            {
                if(OutputConnection == ConnectionType.Right) return BeltDirection.WE;
                if(OutputConnection == ConnectionType.Up) return BeltDirection.WN;
                if(OutputConnection == ConnectionType.Down) return BeltDirection.WS;
            }
            if(InputConnection == ConnectionType.Right)
            {
                if (OutputConnection == ConnectionType.Left) return BeltDirection.EW;
                if (OutputConnection == ConnectionType.Up) return BeltDirection.EN;
                if (OutputConnection == ConnectionType.Down) return BeltDirection.ES;
            }
            if(InputConnection == ConnectionType.Down)
            {
                if (OutputConnection == ConnectionType.Right) return BeltDirection.SE;
                if (OutputConnection == ConnectionType.Up) return BeltDirection.SN;
                if (OutputConnection == ConnectionType.Left) return BeltDirection.SW;
            }
            if (InputConnection == ConnectionType.Up)
            {
                if (OutputConnection == ConnectionType.Right) return BeltDirection.NE;
                if (OutputConnection == ConnectionType.Down) return BeltDirection.NS;
                if (OutputConnection == ConnectionType.Left) return BeltDirection.NW;
            }

            return BeltDirection.WE;
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

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey,TValue>
    {
        [System.Serializable]
        public class OBJ
        {
            public TKey Key;
            public TValue Value;
        }

        public List<OBJ> dictionary = new List<OBJ>();

        public ICollection<TKey> Keys => dictionary.Select(x => x.Key).ToList();
        public ICollection<TValue> Values => dictionary.Select(x => x.Value).ToList();
        public int Count => dictionary.Count;
        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                OBJ item = dictionary.Find(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
                if (item == null)
                    throw new KeyNotFoundException();
                return item.Value;
            }
            set
            {
                OBJ item = dictionary.Find(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
                if (item == null)
                    dictionary.Add(new OBJ { Key = key, Value = value });
                else
                    item.Value = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            dictionary.Add(new OBJ { Key = key, Value = value });
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.Exists(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
        }

        public bool Remove(TKey key)
        {
            return dictionary.RemoveAll(x => EqualityComparer<TKey>.Default.Equals(x.Key, key)) > 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            OBJ item = dictionary.Find(x => EqualityComparer<TKey>.Default.Equals(x.Key, key));
            if (item != null)
            {
                value = item.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Exists(x =>
                EqualityComparer<TKey>.Default.Equals(x.Key, item.Key) &&
                EqualityComparer<TValue>.Default.Equals(x.Value, item.Value));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("The number of elements in the source dictionary is greater than the available space from arrayIndex to the end of the destination array.");

            foreach (var item in dictionary)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.RemoveAll(x =>
                EqualityComparer<TKey>.Default.Equals(x.Key, item.Key) &&
                EqualityComparer<TValue>.Default.Equals(x.Value, item.Value)) > 0;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
