using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ItemSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Image Background;
        [SerializeField] private Color HighlightColor;
        [HideInInspector] public DraggableItem CurrentItem;
        public DraggableItem DraggableItemPrefab;

        public event Action OnItemChangeEvent;

        public int Amount
        {
            get
            {
                if(CurrentItem != null)
                {
                    return CurrentItem.ItemData.Amount;
                }
                else
                {
                    return 0;
                }
            }
        }
        public Item Item 
        { 
            get 
            { 
                if(CurrentItem != null)
                {
                    return CurrentItem.ItemData.Item;
                }
                else
                {
                    return null;
                }
            } 
        }

        private Color _defaultColor;

        private void Start()
        {
            _defaultColor = Background.color;
            UpdateUI();
        }

        public void Init(ItemAmount itemAmount)
        {
            if (CurrentItem != null && CurrentItem.ItemData.Item == itemAmount.Item)
            {
                UpdateItemData(itemAmount);
            }
            else
            {
                CurrentItem = Instantiate(DraggableItemPrefab, transform).GetComponent<DraggableItem>();
                CurrentItem.Init(this, itemAmount);
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            if(CurrentItem != null)
            {
                if (CurrentItem.ItemData.Item == null || CurrentItem.ItemData.Amount <= 0)
                {
                    Destroy(CurrentItem.gameObject);
                }
                else
                {
                    CurrentItem.UpdateUI();
                }
            }
        }

        public void UpdateItemData(ItemAmount itemAmount)
        {
            if (CurrentItem == null) return;
            CurrentItem.ItemData = itemAmount;
            UpdateUI();
        }

        public void Highlight()
        {
            Background.color = HighlightColor;
        }

        public void DeHighlight()
        {
            Background.color = _defaultColor;
        }

        public bool IsEmpty()
        {
            return CurrentItem == null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

            if (transform.childCount != 0)
            {
                GameObject current = transform.GetChild(0).gameObject;
                DraggableItem currentDraggable = current.GetComponent<DraggableItem>();

                if(currentDraggable.ItemData.Item == draggableItem.ItemData.Item) 
                {
                    currentDraggable.ItemData.Amount += draggableItem.ItemData.Amount;
                    UpdateUI();
                    Destroy(draggableItem.gameObject);
                    return;
                }

                currentDraggable.transform.SetParent(draggableItem.ParentAfterDrag);
                ItemSlot slot = currentDraggable.transform.GetComponentInParent<ItemSlot>();
                slot.CurrentItem = currentDraggable;
                slot.OnItemChangeEvent?.Invoke();
            }
            draggableItem.ParentAfterDrag = transform;
            CurrentItem = draggableItem;
            OnItemChangeEvent?.Invoke();
            UpdateUI();
        }
    }
}
