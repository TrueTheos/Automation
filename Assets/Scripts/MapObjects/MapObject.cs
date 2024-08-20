using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    public abstract class MapObject : MonoBehaviour
    {
        public string Name;

        public Chunk Chunk;

        public int MaxDurability;
        private int _currentDurability;

        private int _x, _y;

        public int X => _x;
        public int Y => _y;

        public void Spawn(Chunk chunk, int x, int y)
        {
            Chunk = chunk;
            
            _x = x; 
            _y = y;

            _currentDurability = MaxDurability;

            Chunk.SpawnObject(this);
        }

        public void Hit(int power)
        {
            _currentDurability -= power;

            if(_currentDurability <= 0)
            {
                Break();
            }
        }

        public virtual void Break()
        {
            Chunk.DestroyObject(this);
            Destroy(gameObject);
        }
    }
}
