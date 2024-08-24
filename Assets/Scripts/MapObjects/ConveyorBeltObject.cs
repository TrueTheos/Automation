using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ConveyorBeltObject : MapObject
    {
        public ConveyorBeltObject Parent;
        public ConveyorBeltObject Child;

        public SpriteRenderer Sprite;

        public SerializableDictionary<BeltDirection, Sprite> BeltSprites = new();
        public enum BeltDirection { NS, SN, WE, EW, SE, ES, NE, EN, NW, WN, WS, SW}

        private static Queue<ConveyorBeltObject> _updateQueue = new Queue<ConveyorBeltObject>();
        private static HashSet<ConveyorBeltObject> _enqueuedBelts = new HashSet<ConveyorBeltObject>();

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

        // N
        //W E
        // S

        private BeltDirection GetBeltDirection()
        {
            if(Child == null && InputConnection != ConnectionType.None)
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

            if (InputConnection == ConnectionType.Left && OutputConnection == ConnectionType.Right)
                return BeltDirection.WE;
            if (InputConnection == ConnectionType.Right && OutputConnection == ConnectionType.Left)
                return BeltDirection.EW;
            if (InputConnection == ConnectionType.Down && OutputConnection == ConnectionType.Up)
                return BeltDirection.SN;
            if (InputConnection == ConnectionType.Up && OutputConnection == ConnectionType.Down)
                return BeltDirection.NS;

            if (InputConnection == ConnectionType.Left && OutputConnection == ConnectionType.Up)
                return BeltDirection.WN;
            if (InputConnection == ConnectionType.Up && OutputConnection == ConnectionType.Left)
                return BeltDirection.NW;
            if (InputConnection == ConnectionType.Right && OutputConnection == ConnectionType.Up)
                return BeltDirection.EN;
            if (InputConnection == ConnectionType.Up && OutputConnection == ConnectionType.Right)
                return BeltDirection.NE;
            if (InputConnection == ConnectionType.Left && OutputConnection == ConnectionType.Down)
                return BeltDirection.WS;
            if (InputConnection == ConnectionType.Down && OutputConnection == ConnectionType.Left)
                return BeltDirection.SW;
            if (InputConnection == ConnectionType.Right && OutputConnection == ConnectionType.Down)
                return BeltDirection.ES;
            if (InputConnection == ConnectionType.Down && OutputConnection == ConnectionType.Right)
                return BeltDirection.SE;

            return BeltDirection.WE;
        }


        private void UpdateNeighborConnection(Vector2 direction, ConnectionType neighborInput, ConnectionType neighborOutput)
        {
            Vector2 neighborPos = (Vector2)transform.position + direction;
            ConveyorBeltObject neighbor = GetBeltAtPosition(neighborPos);
            if (neighbor != null)
            {
                if(Parent == null && neighbor.Child == null)
                {
                    Parent = neighbor;
                    neighbor.Child = this;
                    neighbor.OutputConnection = neighborInput;

                    if(neighbor.InputConnection == ConnectionType.None)
                    {
                        if (neighborInput == ConnectionType.Left) neighbor.InputConnection = ConnectionType.Right;
                        if (neighborInput == ConnectionType.Up) neighbor.InputConnection = ConnectionType.Down;
                        if (neighborInput == ConnectionType.Right) neighbor.InputConnection = ConnectionType.Left;
                        if (neighborInput == ConnectionType.Down) neighbor.InputConnection = ConnectionType.Up;
                    }
                    InputConnection = neighborOutput;

                    if(OutputConnection == ConnectionType.None) 
                    {
                        if (neighborOutput == ConnectionType.Left) OutputConnection = ConnectionType.Right;
                        if (neighborOutput == ConnectionType.Up) OutputConnection = ConnectionType.Down;
                        if (neighborOutput == ConnectionType.Right) OutputConnection = ConnectionType.Left;
                        if (neighborOutput == ConnectionType.Down) OutputConnection = ConnectionType.Up;
                    }
                }
                else if(Child == null && neighbor.Parent == null)
                {
                    Child = neighbor;
                    neighbor.Parent = this;
                    neighbor.InputConnection = neighborInput;

                    if (neighbor.InputConnection == ConnectionType.None)
                    {
                        if (neighborInput == ConnectionType.Left) neighbor.InputConnection = ConnectionType.Right;
                        if (neighborInput == ConnectionType.Up) neighbor.InputConnection = ConnectionType.Down;
                        if (neighborInput == ConnectionType.Right) neighbor.InputConnection = ConnectionType.Left;
                        if (neighborInput == ConnectionType.Down) neighbor.InputConnection = ConnectionType.Up;
                    }
                    OutputConnection = neighborOutput;

                    if (OutputConnection == ConnectionType.None)
                    {
                        if (neighborOutput == ConnectionType.Left) OutputConnection = ConnectionType.Right;
                        if (neighborOutput == ConnectionType.Up) OutputConnection = ConnectionType.Down;
                        if (neighborOutput == ConnectionType.Right) OutputConnection = ConnectionType.Left;
                        if (neighborOutput == ConnectionType.Down) OutputConnection = ConnectionType.Up;
                    }
                }

                neighbor.UpdateSprite(neighborOutput);
                BeltDirection dir = GetBeltDirection();
                Sprite.sprite = BeltSprites[dir];
            }
        }

        private ConveyorBeltObject GetBeltAtPosition(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(position);
            foreach (Collider2D collider in colliders)
            {
                ConveyorBeltObject belt = collider.GetComponent<ConveyorBeltObject>();
                if (belt != null) return belt;
            }
            return null;
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
