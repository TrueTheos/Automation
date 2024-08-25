using Assets.Scripts.MapObjects;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Items
{
    public class ItemObject : MonoBehaviour
    {
        public Item ItemData;
        public int Stack;

        public void MoveToPosition(Vector3 newPosition)
        {
            transform.DOMove(newPosition, .5f).SetEase(Ease.Linear);
        }

        public void Init(Item item, int count)
        {
            ItemData = item;
            if(count > item.MaxStack) 
            {
                Stack = count;
                MapManager.Instance.SpawnItem(item, transform.position.x, transform.position.y, count - Stack);
            }
            else
            {
                Stack = count;
            }
            GetComponent<SpriteRenderer>().sprite = item.Icon;
        }
    }
}
