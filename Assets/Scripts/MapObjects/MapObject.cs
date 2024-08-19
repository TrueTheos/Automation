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
        public Chunk Chunk;

        public int X, Y;

        public void Spawn(Chunk chunk, int x, int y)
        {
            Chunk = chunk;
            
            X = x; 
            Y = y;

            Chunk.SpawnObject(this);
        }

        public virtual void Break()
        {

            Destroy(gameObject);
        }
    }
}
