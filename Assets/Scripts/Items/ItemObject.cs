using Assets.Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Items
{
    public class ItemObject : MonoBehaviour
    {
        [HideInInspector] public Item ItemData;
        [HideInInspector] public int Amount;
        [SerializeField] private GameObject _shadow;

        private Vector3 _startPos;

        public enum ItemState { OnBelt, OnGround}
        private ItemState _state;
        public ItemState State
        {
            get
            {
                return _state;
            }
            set
            {
                ChangeState(value);
                _state = value;             
            }
        }

        private void ChangeState(ItemState newState)
        {
            if(newState == ItemState.OnGround)
            {
                _startPos = transform.position;
                if(_state != ItemState.OnGround)
                {
                    _shadow.SetActive(true);
                    transform.DOMoveY(_startPos.y + .5f, 2)
                        .SetEase(Ease.InOutQuad)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
            else
            {
                if(_state == ItemState.OnGround)
                {
                    _shadow.SetActive(false);
                    DOTween.Kill(transform);
                }
            }
        }

        public void MoveToPosition(Vector3 newPosition)
        {
            transform.DOMove(newPosition, .5f).SetEase(Ease.Linear);
        }

        public void Init(Item item, int count, ItemState state)
        {
            State = state;
            ItemData = item;
            if(count > item.MaxStack) 
            {
                Amount = count;
                MapManager.Instance.SpawnItem(item, transform.position.x, transform.position.y, count - Amount, state);
            }
            else
            {
                Amount = count;
            }
            GetComponent<SpriteRenderer>().sprite = item.Icon;
        }
    }
}
