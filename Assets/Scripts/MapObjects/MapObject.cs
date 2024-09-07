using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.MapObjects
{
    public abstract class MapObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector] public Direction Direction;
        public SpriteRenderer SpriteRend;

        public Chunk Chunk;
        public MapItem MapItem;

        private int _currentDurability;

        private int _x, _y;

        public int X => _x;
        public int Y => _y;

        private GameManager _gameManager;

        public void Place(Chunk chunk, int x, int y, Direction direction)
        {
            Chunk = chunk;
            
            _x = x; 
            _y = y;

            _currentDurability = MapItem.MaxDurability;

            Chunk.SpawnObject(this);

            _gameManager = GameManager.Instance;

            OnPlace(direction);
        }

        protected virtual void OnPlace(Direction direction)
        {
            Direction = direction;
            if (MapItem.DirectionSprites == null || !MapItem.DirectionSprites.ContainsKey(Direction))
            {
                SpriteRend.sprite = MapItem.Icon;
            }
            else
            {
                SpriteRend.sprite = MapItem.DirectionSprites[Direction];
            }
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
            OnBreak();
            
            Chunk.DestroyObject(this);
            Destroy(gameObject);
        }

        public virtual void OnBreak()
        {
            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Instance.ObjectHighlight.gameObject.SetActive(true);
            GameManager.Instance.ObjectHighlight.transform.position = transform.position;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameManager.Instance.ObjectHighlight.gameObject.SetActive(false);
        }
    }
}
