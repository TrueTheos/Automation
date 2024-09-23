using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using Assets.Scripts.Items;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ItemTypeSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        public Item CurrentItem;

        public event Action OnItemChangeEvent; 

        private void Start()
        {
            UpdateUI();
        }

        public void Init(Item item)
        {
            if (CurrentItem != null && CurrentItem == item)
            {
                UpdateItemData(item);
            }
            else
            {
                CurrentItem = item;
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            if (CurrentItem == null)
            {
                _icon.enabled = false;
            }
            else
            {
                _icon.enabled = true;
                _icon.sprite = CurrentItem.Icon;
            }
        }

        public void ResetSlot()
        {
            _icon.enabled = false;
            CurrentItem = null;
        }

        public void UpdateItemData(Item item)
        {
            if (CurrentItem == null) return;
            CurrentItem = item;
            UpdateUI();
        }

        public void OnDrop(PointerEventData eventData)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();

            ItemSlot draggedItemSlot = draggedItem.ParentAfterDrag.GetComponent<ItemSlot>();
           
            CurrentItem = draggedItem.ItemData.GetItem();
            OnItemChangeEvent?.Invoke();
            UpdateUI();
        }
    }
}
