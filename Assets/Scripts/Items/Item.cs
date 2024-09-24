using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Items
{
    public abstract class Item : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
        public int MaxStack;
        public ItemFlags ItemFlags;
        public ItemType ItemType;

        public static bool operator ==(Item a, Item b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            if (a.GetType() != b.GetType())
                return false;

            return a.Name == b.Name && a.ItemType == b.ItemType;
        }

        public static bool operator !=(Item a, Item b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (ReferenceEquals(obj, null))
                return false;

            if (obj is Item other)
                return this == other;

            return false;
        }
    }

    public enum ItemType
    {
        None = 0,
        WorldObject = 1,
        Wire = 2
    }


    [Flags]
    public enum ItemFlags
    {
        None = 0,
        Smeltable = 1,
        SmeltingFuel = 2
    }
}
