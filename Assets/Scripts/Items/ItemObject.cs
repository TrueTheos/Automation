using Assets.Scripts.Managers;
using Assets.Scripts.MapObjects;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Items
{
    public class ItemObject : MonoBehaviour, IRightClick
    {
        [HideInInspector] public Item ItemData;
        [HideInInspector] public int Amount;
        [SerializeField] private GameObject _shadow;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("Throwing")]
        [SerializeField] private float _gravity;
        [SerializeField] private float _throwDuration;
        [SerializeField] private float _maxArcHeight;
        [SerializeField] private float _minArcHeight;
        [SerializeField] private float _maxThrowDistance;
        private float _throwTimer;
        private Vector3 _startPos;
        private Vector3 _targetPosition;
        public UnityEvent _onGroundHitEvent;
        private bool _isGrounded;

        public enum ItemState { OnBelt, OnGround, PulledToPlayer, MidAir}
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

        private void Update()
        {
            if (State == ItemState.MidAir)
            {
                UpdatePosition();
                CheckGroundHit();
            }
        }

        private void UpdatePosition()
        {
            _throwTimer += Time.deltaTime;
            float t = _throwTimer / _throwDuration;
            Vector3 currentPos = Vector3.Lerp(_startPos, _targetPosition, t);
            float yOffset = _gravity * t * (t - 1);
            currentPos.y += yOffset;
            transform.position = currentPos;
            _sprite.transform.position = currentPos;

            if (t >= 1f)
            {
                State = ItemState.OnGround;
                _isGrounded = true;
                GroundHit();
            }
        }

        private void MoveShadowToTarget()
        {
            if (_shadow != null)
            {
                _shadow.transform.DOMove(new Vector3(_targetPosition.x, _targetPosition.y, _shadow.transform.position.z), _throwDuration)
                    .SetEase(Ease.Linear);
            }
        }

        private void CheckGroundHit()
        {
            if (_sprite.transform.position.y < transform.position.y && !_isGrounded)
            {
                _sprite.transform.position = transform.position;
                _isGrounded = true;
                GroundHit();
            }
        }

        void GroundHit()
        {
            _onGroundHitEvent.Invoke();
            if (_shadow != null)
            {
                _shadow.transform.position = new Vector3(_targetPosition.x, _targetPosition.y, _shadow.transform.position.z);
            }
        }

        public void Stick()
        {
            StartCoroutine(ChangeToOnGround());
        }

        private IEnumerator ChangeToOnGround()
        {
            yield return new WaitForSeconds(1f);
            State = ItemState.OnGround;
        }

        public void Bounce(float divisionFactor)
        {
            //Throw(_groundVelocity, _lastIntialVerticalVelocity / divisionFactor);
        }

        public void Throw(Vector2 startPos, Vector2 endPos)
        {
            _startPos = startPos;
            _targetPosition = new Vector3(endPos.x, endPos.y, transform.position.z);
            _throwTimer = 0f;
            _isGrounded = false;
            MoveShadowToTarget();
            State = ItemState.MidAir;
        }

        private void ChangeState(ItemState newState)
        {
            if(newState == ItemState.OnGround)
            {
                _startPos = transform.position;
                if(_state != ItemState.OnGround)
                {
                    _shadow.SetActive(true);
                    transform.DOMoveY(_startPos.y + .3f, 2)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
            else if(newState == ItemState.MidAir)
            {
                _shadow.SetActive(true);
            }
            else
            {
                if(_state == ItemState.OnGround)
                {
                    if(newState == ItemState.OnBelt) _shadow.SetActive(false);
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
            _isGrounded = false;
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
            _sprite.sprite = item.Icon;
        }

        public void OnClick(Player player)
        {
            if(State == ItemState.OnBelt)
            {
                player.Inventory.PickupItem(this);
                AudioManager.Instance.PickupItem();
            }
        }
    }
}
