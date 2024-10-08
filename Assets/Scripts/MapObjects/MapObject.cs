﻿using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using DG.Tweening;
using Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static Assets.Scripts.Utilities;
using Random = UnityEngine.Random;
namespace Assets.Scripts.MapObjects
{
    public abstract class MapObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector] public Direction Direction;
        public ObjectSize Size;
        public SpriteRenderer SpriteRend;

        public MapItem MapItem;

        private int _currentDurability;

        private int _x, _y;

        public int X => _x;
        public int Y => _y;

        private GameManager _gameManager;
        protected MapGenerator _mapGenerator => MapGenerator.Instance;

        [Range(0f, 1f), HideInInspector] public float Speed = 1f; //How fast building will work based on the provided electricity    

        public virtual Vector2Int[] GetOccupiedPositions(int x, int y)
        {
            switch (Size)
            {
                case ObjectSize._1x1:
                    return new Vector2Int[] { new(x, y) };
                case ObjectSize._2x2:
                    return new Vector2Int[] {
                    new(x, y),
                    new(x + 1, y),
                    new(x, y + 1),
                    new(x + 1, y + 1)
                };
                case ObjectSize._2x1:
                    return new Vector2Int[] {
                    new(x, y),
                    new(x + 1, y)
                };
                case ObjectSize._1x2:
                    return new Vector2Int[] {
                    new(x, y),
                    new(x, y + 1)
                };
                case ObjectSize._3x3:
                    return new Vector2Int[] {
                    new(x, y),
                    new(x + 1, y),
                    new(x, y + 1),
                    new(x + 1, y + 1),
                    new(x + 2, y),
                    new(x + 2, y + 2),
                    new(x, y + 2),
                    new(x + 2, y + 1),
                    new(x + 1, y + 2),
                };
                default:
                    return new Vector2Int[] { new(x, y) };
            }
        }

        public void Place(int x, int y, Direction direction, bool playPlaceAnimation)
        {            
            _x = x; 
            _y = y;
            _currentDurability = MapItem.MaxDurability;

            foreach (Vector2Int pos in GetOccupiedPositions(X, Y))
            {
                Chunk chunk = MapGenerator.Instance.GetChunk(pos.x, pos.y);
                transform.SetParent(chunk.transform);
                chunk.SpawnObjectPart(this, pos.x, pos.y);
            }

            Chunk originChunk = MapGenerator.Instance.GetChunk(X, Y);
            if (this is OreObject) originChunk.OresCount++;
            else if (this is TreeObject) originChunk.TreesCount++;

            _gameManager = GameManager.Instance;

            OnPlace(direction);

            if(playPlaceAnimation)
            {
                Vector3 targetPosition = transform.position;
                transform.position += Vector3.up * .5f;
                Color startColor = SpriteRend.color;
                startColor.a = 0;
                Color endColor = startColor;
                endColor.a = 1f;
                SpriteRend.color = startColor;
                Sequence sequence = DOTween.Sequence();
                sequence.Append(transform.DOMove(targetPosition, .3f).SetEase(Ease.OutBounce));
                sequence.Join(SpriteRend.DOColor(endColor, .3f).SetEase(Ease.InOutQuad));
                sequence.Join(transform.DOPunchScale(Vector3.one * 0.2f, .3f, 1, 0.5f));
                sequence.Play();
            }
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

            if(MapItem.PickRandomSprite)
            {
                var sprites = MapItem.SpriteRandomVariants.Where(x => x != null).ToList();
                if(sprites.Count > 0) SpriteRend.sprite = sprites.GetRandom();
            }
        }

        public void Hit(int power)
        {
            _currentDurability -= power;

            
            //transform.DOShakePosition(0.5f, 0.2f, 10, 90, false, true);

            if (_currentDurability <= 0)
            {
                transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1).OnComplete(Break);
            }
            else
            {
                transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1);
            }
        }

        public virtual void Break()
        {
            OnBreak();
            foreach (Vector2Int pos in GetOccupiedPositions(X, Y))
            {
                Chunk chunk = MapGenerator.Instance.GetChunk(pos.x, pos.y);
                chunk.DestroyObject(this);
            }
            DropItem();
            DestroyImmediate(gameObject);
        }

        public virtual void OnBreak() { }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Instance.ObjectHighlight.gameObject.SetActive(true);
            GameManager.Instance.ObjectHighlight.transform.position = transform.position;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameManager.Instance.ObjectHighlight.gameObject.SetActive(false);
        }

        public virtual void DropItem()
        {
            float randomXOffset = Random.Range(-.6f, .6f);
            float randomYOffset = Random.Range(-.6f, .6f);
            MapManager.Instance.SpawnItem(MapItem,
                   transform.position.x + randomXOffset,
                   transform.position.y + randomYOffset,
                   1,
                   ItemObject.ItemState.OnGround);
        }
    }

    // Width x Height
    public enum ObjectSize
    {
        _1x1,
        _2x2,
        _2x1,
        _1x2,
        _3x3
    }
}
