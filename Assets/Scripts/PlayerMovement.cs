using Assets.Scripts;
using Assets.Scripts.Items;
using Assets.Scripts.MapObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;
    public float SprintSpeed;
    public int DestroyPower = 10;
    public KeyCode SprintKey;
    public float PickupRange;
    public float AttractionRange;
    public float MaxAttractionSpeed;
    public LayerMask PickupLayer;

    [SerializeField] private SpriteRenderer _placeObjectPreview;
    public List<MapObject> TempInventory;
    private MapObject _selectedObject;
    private int _selectedObjectIndex;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private MapManager _mapManager;
    private Inventory _inventory;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<Inventory>();
    }

    private void Start()
    {
        _mapManager = MapManager.Instance;
        _selectedObject = TempInventory[0];
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

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out MapObject mapObj))
            {
                mapObj.Hit(DestroyPower);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            _selectedObjectIndex--;
            if (_selectedObjectIndex < 0)
            {
                _selectedObjectIndex = TempInventory.Count - 1;
            }
            _selectedObject = TempInventory[_selectedObjectIndex];

            _placeObjectPreview.gameObject.SetActive(_selectedObject != null);
            if(_selectedObject != null)
            {
                _placeObjectPreview.sprite = _selectedObject.GetComponentInChildren<SpriteRenderer>().sprite;
            }
        }
        else if (scroll < 0f)
        {
            _selectedObjectIndex++;
            if (_selectedObjectIndex >= TempInventory.Count)
            {
                _selectedObjectIndex = 0;
            }
            _selectedObject = TempInventory[_selectedObjectIndex];

            _placeObjectPreview.gameObject.SetActive(_selectedObject != null);
            if (_selectedObject != null)
            {
                _placeObjectPreview.sprite = _selectedObject.GetComponentInChildren<SpriteRenderer>().sprite;
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            if (_selectedObject != null)
            {
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int gridMousePos = new Vector2Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y));

                if (_mapManager.IsFree(gridMousePos.x, gridMousePos.y))
                {
                    _mapManager.SpawnObject(_selectedObject, gridMousePos.x, gridMousePos.y);
                }
            }   
        }

        if(_selectedObject != null)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 gridMousePos = new Vector3(Mathf.FloorToInt(mousePos.x) + .5f, Mathf.FloorToInt(mousePos.y) + .5f, 0);
            _placeObjectPreview.gameObject.transform.position = gridMousePos;
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
                    _inventory.AddItem(item);
                }
            }
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttractionRange);
    }
}
