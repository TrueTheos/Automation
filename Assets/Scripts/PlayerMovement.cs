using Assets.Scripts;
using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.XR;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;
    public float SprintSpeed;
    public int DestroyPower = 10;
    public float DestroyInterval;
    private float _destroyTimer;
    public KeyCode SprintKey;
    public float PickupRange;
    public float AttractionRange;
    public float MaxAttractionSpeed;
    public LayerMask PickupLayer;

    [SerializeField] private SpriteRenderer _placeObjectPreview;
    private ItemAmountUI _selectedItem;
    private int _selectedObjectIndex;

    [SerializeField] private GameObject _hand;
    private SpriteRenderer _handArt;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private MapManager _mapManager;
    private Inventory _inventory;
    private Player _player;

    private Camera _cam;

    private bool _isClicking = false;
    private float _lastClickTime = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<Inventory>();
        _cam = Camera.main;
        _player = GetComponent<Player>();
        _handArt = _hand.GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        _mapManager = MapManager.Instance;
        _selectedItem = _inventory.HotbarItems[0];
        _destroyTimer = DestroyInterval;
    }

    private void Update()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        _moveInput.Normalize();

        if(Input.GetKey(SprintKey))
        {
            _rb.velocity = _moveInput * SprintSpeed;
        }
        else
        {
            _rb.velocity = _moveInput * MoveSpeed;
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


        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridMousePos = new Vector2Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y));
        if (Input.GetMouseButton(1))
        {
            if (_mapManager.IsFree(gridMousePos.x, gridMousePos.y))
            {
                if (_selectedItem != null && !_selectedItem.IsEmpty() && _selectedItem.Item is MapItem buildingItem)
                {
                    _mapManager.SpawnObject(buildingItem.Prefab, gridMousePos.x, gridMousePos.y);
                    _inventory.RemoveItemFromSlot(1, _selectedItem);
                }
            }         
        }
        if(Input.GetMouseButtonDown(1))
        {
            if (!_mapManager.IsFree(gridMousePos.x, gridMousePos.y))
            {
                if (MapGenerator.Instance.GetObjectAtPos(gridMousePos.x, gridMousePos.y) is ConveyorBeltObject conveyorBelt)
                {
                    if (_selectedItem != null && _selectedItem.Item is NormalItem normalItem)
                    {
                        _inventory.RemoveItemFromSlot(1, _selectedItem);
                        conveyorBelt.SpawnItem(normalItem);
                    }
                }
            }
        }
        if(_selectedItem != null && _selectedItem.Item is MapItem)
        {
            Vector3 _placeholderPos = new Vector3(.5f + gridMousePos.x, .5f + gridMousePos.y, 0);
            _placeObjectPreview.gameObject.transform.position = _placeholderPos;
        }

        List<ItemObject> items = Physics2D.OverlapCircleAll(transform.position, AttractionRange, PickupLayer).Select(x => x.GetComponent<ItemObject>()).ToList();

        foreach (var item in items)
        {
            float distanceToPlayer = Vector2.Distance(item.transform.position, transform.position);
            if (distanceToPlayer <= AttractionRange && distanceToPlayer > PickupRange)
            {
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

        ScrollThruItems();
        UpdateHand();
    }

    private void ScrollThruItems()
    {
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
        _selectedItem.Highlight();

        if (_selectedItem != null && _selectedItem.Item != null)
        {
            if (_selectedItem.Item is MapItem buildingItem)
            {
                _placeObjectPreview.gameObject.SetActive(true);
                _placeObjectPreview.sprite = _selectedItem.Item.Icon;           
            }
            else
            {
                _handArt.sprite = _selectedItem.Item.Icon;
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
