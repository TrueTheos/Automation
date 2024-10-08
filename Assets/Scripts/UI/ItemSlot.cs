﻿using Assets.Scripts.Items;
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
                    DestroyImmediate(CurrentItem.gameObject);
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
                DestroyImmediate(CurrentItem.gameObject);
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
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            ItemAmount draggedItemData = new ItemAmount(draggedItem.ItemData.Item, draggedItem.ItemData.Amount);

            ItemSlot draggedItemSlot = draggedItem.ParentAfterDrag.GetComponent<ItemSlot>();
            if (draggedItemSlot != null)
            {
                draggedItemSlot.CurrentItem = null;
                draggedItemSlot.OnItemChangeEvent?.Invoke();
            }

            if (transform.childCount != 0)
            {
                DraggableItem currentItem = transform.GetChild(0).gameObject.GetComponent<DraggableItem>();

                if (currentItem.ItemData.GetItem() == draggedItemData.GetItem())
                {
                    if (currentItem.ItemData.Amount + draggedItemData.Amount > currentItem.ItemData.Item.MaxStack)
                    {
                        int amountToAdd = currentItem.ItemData.Item.MaxStack - currentItem.ItemData.Amount;
                        currentItem.ItemData.Amount += amountToAdd;
                        draggedItemData.Amount -= amountToAdd;
                        draggedItem.ItemData = draggedItemData;
                        draggedItem.UpdateUI();
                        UpdateUI();
                        OnItemChangeEvent?.Invoke();
                        return;
                    }
                    else
                    {
                        currentItem.ItemData.Amount += draggedItemData.Amount;
                        UpdateUI();
                        OnItemChangeEvent?.Invoke();
                        DestroyImmediate(draggedItem.gameObject);
                        return;
                    }
                }

                currentItem.transform.SetParent(draggedItem.ParentAfterDrag);
                ItemSlot slot = currentItem.transform.GetComponentInParent<ItemSlot>();
                slot.CurrentItem = currentItem;
            }
            draggedItem.ParentAfterDrag = transform;
            draggedItem.ItemData = draggedItemData;
            CurrentItem = draggedItem;
            OnItemChangeEvent?.Invoke();
            UpdateUI();
        }
    }
}
