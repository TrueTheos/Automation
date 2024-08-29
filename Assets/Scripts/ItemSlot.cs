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
        private DraggableItem _currentItem;
        public DraggableItem CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value;
                //OnItemChangeEvent?.Invoke();
            }
        }
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
                    return CurrentItem.ItemData.GetItem();
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
            if (CurrentItem != null && CurrentItem.ItemData.GetItem() == itemAmount.GetItem())
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
                if (CurrentItem.ItemData.GetItem() == null || CurrentItem.ItemData.Amount <= 0)
                {
                    Destroy(CurrentItem.gameObject);
                }
                else
                {
                    CurrentItem.UpdateUI();
                }
            }
        }

        public void ResetSlot()
        {
            if(CurrentItem != null)
            {
                Destroy(CurrentItem.gameObject);
                CurrentItem = null;
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

            ItemSlot draggedItemSlot = draggableItem.ParentAfterDrag.GetComponent<ItemSlot>();
            if (draggedItemSlot != null)
            {
                draggedItemSlot.CurrentItem = null;
            }

            if (transform.childCount != 0)
            {
                GameObject current = transform.GetChild(0).gameObject;
                DraggableItem currentDraggable = current.GetComponent<DraggableItem>();

                if(currentDraggable.ItemData.GetItem() == draggableItem.ItemData.GetItem()) 
                {
                    currentDraggable.ItemData.Amount += draggableItem.ItemData.Amount;
                    UpdateUI();
                    OnItemChangeEvent?.Invoke();
                    Destroy(draggableItem.gameObject);
                    return;
                }

                currentDraggable.transform.SetParent(draggableItem.ParentAfterDrag);
                ItemSlot slot = currentDraggable.transform.GetComponentInParent<ItemSlot>();
                slot.CurrentItem = currentDraggable;
            }
            draggableItem.ParentAfterDrag = transform;
            CurrentItem = draggableItem;
            OnItemChangeEvent?.Invoke();
            UpdateUI();
        }
    }
}
