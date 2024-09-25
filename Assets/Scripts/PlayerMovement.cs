using Assets.Scripts;
using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEditor;
using Assets.Scripts.Managers;
using Managers;
using static Assets.Scripts.Utilities;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;
    public float SprintSpeed;
    public int DestroyPower = 10;
    public float BreakRange;
    public float DestroyInterval;
    private float _destroyTimer;
    public float PickupRange;
    public float AttractionRange;
    public float MaxAttractionSpeed;
    public LayerMask PickupLayer;
    public float MaxThrowDistance;

    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _placeObjectPreview;
    [SerializeField] private Color _canBePlaceColor;
    [SerializeField] private Color _cantBePlacedColor;
    private ItemSlot _selectedItem;
    public ItemSlot SelectedItem => _selectedItem;
    private Direction _selectedItemDirection = Direction.Down;
    private int _selectedObjectIndex;
    public event Action OnSelectedItemChange; 

    [SerializeField] private GameObject _hand;
    private SpriteRenderer _handArt;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private MapManager _mapManager;
    private Inventory _inventory;
    private Player _player;

    private Camera _cam => _cameraManager.MainCam;

    private bool _isClicking = false;
    private float _lastClickTime = 0f;
    private float _currentMovementSpeed;
    private bool _isRunning;
    
    private CableBuilder _cableBuilder;

    private CameraManager _cameraManager;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<Inventory>();
        _player = GetComponent<Player>();
        _handArt = _hand.GetComponentInChildren<SpriteRenderer>();

        _cableBuilder = _player.CableBuilder;
    }

    private void Start()
    {
        _cameraManager = CameraManager.Instance;
        _mapManager = MapManager.Instance;
        _selectedItem = _inventory.HotbarItems[0];
        if (_selectedItem.Item is MapItem mapItem) _selectedItemDirection = mapItem.DefaultDirection;
        _destroyTimer = DestroyInterval;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _cableBuilder.IsConnecting = false;

            _cableBuilder.CancelCurrentAction();
        }

        if (_cableBuilder.IsConnecting)
        {
            return;
        }
        
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        _moveInput.Normalize();

        _currentMovementSpeed = Mathf.Clamp(_moveInput.magnitude, 0f, 1f);

        if (Input.GetKey(PlayerKeybinds.SprintKey))
        {
            _rb.velocity = _moveInput * SprintSpeed;
            _isRunning = true;
        }
        else
        {
            _rb.velocity = _moveInput * MoveSpeed;
            _isRunning = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _isClicking = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isClicking = false;
        }

        if (_isClicking || Input.GetMouseButton(0))
        {
            HandleHittingObject();
        }

        var mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridMousePos = new Vector2Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y));
        
        if(Input.GetMouseButtonDown(1))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out IRightClick rightClickable))
            {
                rightClickable.OnClick(_player);
            }
        }
        
        if (Input.GetMouseButton(1))
        {
            if (_mapManager.IsFree(gridMousePos.x, gridMousePos.y))
            {
                if (_selectedItem != null && !_selectedItem.IsEmpty() && _selectedItem.Item is MapItem buildingItem)
                {
                    if (_mapManager.CanPlaceObject(buildingItem.Prefab, gridMousePos.x, gridMousePos.y))
                    {
                        _mapManager.SpawnObject(buildingItem.Prefab, gridMousePos.x, gridMousePos.y, _selectedItemDirection, true);
                        _inventory.RemoveItemFromSlot(1, _selectedItem);
                        UpdatePreviewSprite();
                    }
                }
            }   
        }

        if (Input.GetKeyDown(PlayerKeybinds.ThrowKey)) ThrowItem ();

        #region Rotating Objects
        if (_selectedItem != null && _selectedItem.Item is MapItem mapItem)
        {
            Vector3 _placeholderPos = new Vector3(.5f + gridMousePos.x, .5f + gridMousePos.y, 0);
            _placeObjectPreview.gameObject.transform.position = _placeholderPos;

            if(Input.GetKeyDown(PlayerKeybinds.RotateKey) && mapItem.CanBeRotated)
            {
                for (int i = 1; i <= 4; i++)
                {
                    Direction nextDirection = (Direction)(((int)_selectedItemDirection + i) % 4);

                    // Check if the next direction exists in the dictionary
                    if (mapItem.DirectionSprites.ContainsKey(nextDirection))
                    {
                        _selectedItemDirection = nextDirection;
                        break;
                    }
                }     
            }
            UpdatePreviewSprite();
            if (_mapManager.CanPlaceObject(mapItem.Prefab, gridMousePos.x, gridMousePos.y))
            {
                _placeObjectPreview.color = _canBePlaceColor;
            }
            else
            {
                _placeObjectPreview.color = _cantBePlacedColor;
            }
        }
        #endregion


        PickupItems();
        ScrollThruItems();
        UpdateHand();
        Animate();
    }

    private void ThrowItem()
    {
        if(_selectedItem != null && _selectedItem.Item != null)
        {
            Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            ItemObject thrownItem = _mapManager.SpawnItem(_selectedItem.Item, _hand.transform.position.x, transform.position.y, 1, ItemObject.ItemState.MidAir);

            Vector2 throwDirection = mousePos - (Vector2)_hand.transform.position;
            float throwDistance = throwDirection.magnitude;

            // If the throw distance exceeds the max, limit it
            if (throwDistance > MaxThrowDistance)
            {
                throwDirection = throwDirection.normalized;
                mousePos = (Vector2)_hand.transform.position + throwDirection * MaxThrowDistance;
            }

            mousePos = new Vector3(mousePos.x, mousePos.y);
            thrownItem.Throw(_hand.transform.position, mousePos);
            _inventory.RemoveItemFromSlot(1, _selectedItem);
            UpdatePreviewSprite();
        }
    }

    private void FixedUpdate()
    {
        int x = Mathf.RoundToInt(transform.position.x - .5f);
        int y = Mathf.RoundToInt(transform.position.y - .75f);
        if (MapGenerator.Instance.GetObjectAtPos(x,y) is ConveyorBeltObject belt)
        {
            if (belt.Child != null && belt.Child is ConveyorBeltObject childBelt)
            {
                Vector2 childBeltCenter = new Vector2(childBelt.X + .5f, childBelt.Y + .75f);
                Vector2 toNextBelt = (childBeltCenter - (Vector2)transform.position).normalized;
                Vector2 conveyorMovement = toNextBelt * belt.ItemMoveSpeed * 100 * Time.fixedDeltaTime;
                _rb.velocity += conveyorMovement;
            }
        }
    }

    private void PickupItems()
    {
        List<ItemObject> items = Physics2D.OverlapCircleAll(transform.position, AttractionRange, PickupLayer).Select(x => x.GetComponent<ItemObject>()).Where(x => x != null).ToList();
        foreach (var item in items)
        {
            if (item.State == ItemObject.ItemState.OnBelt || item.State == ItemObject.ItemState.MidAir) continue;

            float distanceToPlayer = Vector2.Distance(item.transform.position, transform.position);
            if (distanceToPlayer <= AttractionRange && distanceToPlayer > PickupRange)
            {
                if(item.State != ItemObject.ItemState.PulledToPlayer)
                {
                    item.State = ItemObject.ItemState.PulledToPlayer;
                }

                Vector2 direction = (transform.position - item.transform.position).normalized;
                float currentSpeed = Mathf.Lerp(0f, MaxAttractionSpeed, (AttractionRange - distanceToPlayer) / AttractionRange);

                item.transform.Translate(direction * currentSpeed * Time.deltaTime);
            }
            else
            {
                if (distanceToPlayer <= PickupRange)
                {
                    _inventory.PickupItem(item);
                    AudioManager.Instance.PickupItem();
                }
            }
        }
    }

    private void Animate()
    {
        Vector2 mousePosition = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPosition = transform.position;
        Vector2 directionToMouse = (mousePosition - playerPosition).normalized;
        _animator.SetFloat("Horizontal", directionToMouse.x);
        _animator.SetFloat("Vertical", directionToMouse.y);

        if (_currentMovementSpeed > 0)
        {
            if (_isRunning) _animator.SetFloat("Speed", 2);
            else _animator.SetFloat("Speed", 1);
        }
        else _animator.SetFloat("Speed", 0);
    }

    private void ScrollThruItems()
    {
        if (Input.GetKey(PlayerKeybinds.ZoomKey)) return;

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                _selectedObjectIndex = Mathf.Clamp(i, 1, _inventory.HotbarSlots) - 1;
                OnChangeSlot();
            }
        }

        if (_inventory.CraftingViewOpen) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll != 0f)
        {
            if (scroll > 0f)
            {
                _selectedObjectIndex--;
                if (_selectedObjectIndex < 0)
                {
                    _selectedObjectIndex = _inventory.HotbarSlots - 1;
                }             
            }
            else if (scroll < 0f)
            {
                _selectedObjectIndex++;
                if (_selectedObjectIndex >= _inventory.HotbarSlots)
                {
                    _selectedObjectIndex = 0;
                }
            }

            OnChangeSlot();
        }        
    }

    private void OnChangeSlot()
    {
        if (_selectedItem != null) _selectedItem.DeHighlight();
        _selectedItem = _inventory.HotbarItems[_selectedObjectIndex];
        if (_selectedItem.Item is MapItem mapItem) _selectedItemDirection = mapItem.DefaultDirection;
        _selectedItem.Highlight();

        OnSelectedItemChange?.Invoke();

        UpdatePreviewSprite();
    }

    public void UpdatePreviewSprite()
    {
        if (_selectedItem != null && _selectedItem.Item != null)
        {
            if (_selectedItem.Item is MapItem buildingItem)
            {
                _placeObjectPreview.gameObject.SetActive(true);
                if (buildingItem.DirectionSprites == null || !buildingItem.DirectionSprites.ContainsKey(_selectedItemDirection))
                {
                    _placeObjectPreview.sprite = buildingItem.Icon;
                }
                else
                {
                    _placeObjectPreview.sprite = buildingItem.DirectionSprites[_selectedItemDirection];
                }               
            }
            else
            {
                _handArt.sprite = _selectedItem.Item.Icon;
                _placeObjectPreview.gameObject.SetActive(false);
            }
        }
        else
        {
            _handArt.sprite = null;
            _placeObjectPreview.gameObject.SetActive(false);
        }
    }

    private void UpdateHand()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        _hand.transform.right = direction;

        Vector2 scale = _hand.transform.localScale;
        if (direction.x < 0)
        {
            scale.y = -1;
        }
        else if (direction.x > 0)
        {
            scale.y = 1;
        }
        _hand.transform.localScale = scale;

        if (_hand.transform.eulerAngles.z > 0 && _hand.transform.eulerAngles.z < 180)
        {
            _handArt.sortingOrder = _player.Art.sortingOrder - 1;
        }
        else
        {
            _handArt.sortingOrder = _player.Art.sortingOrder + 1;
        }
    }

    private void HandleHittingObject()
    {
        _destroyTimer += Time.deltaTime;

        if (_isClicking)
        {
            if (Time.time - _lastClickTime >= DestroyInterval)
            {
                HitObject();
                _lastClickTime = Time.time;
            }
        }
        else if (_destroyTimer >= DestroyInterval)
        {
            HitObject();
            _destroyTimer = 0;
        }
    }

    private void HitObject()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out MapObject mapObj))
        {
            mapObj.Hit(DestroyPower);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttractionRange);
    }
}
