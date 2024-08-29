using Assets.Scripts.Items;
using DG.Tweening;
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
        public MapItem MapItem;

        private int _currentDurability;

        private int _x, _y;

        public int X => _x;
        public int Y => _y;

        private GameManager _gameManager;

        public void Place(Chunk chunk, int x, int y)
        {
            Chunk = chunk;
            
            _x = x; 
            _y = y;

            _currentDurability = MapItem.MaxDurability;

            Chunk.SpawnObject(this);

            _gameManager = GameManager.Instance;

            OnPlace();
        }

        protected virtual void OnPlace() 
        {
        }

        public void Hit(int power)
        {
            _currentDurability -= power;

            transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1);
            //transform.DOShakePosition(0.5f, 0.2f, 10, 90, false, true);

            if (_currentDurability <= 0)
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
