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
        [SerializeField] private TextMeshProUGUI AmountText;
        private DraggableItem _currentItem;
        public DraggableItem DraggableItemPrefab;

        public int Amount
        {
            get
            {
                if(_currentItem != null)
                {
                    return _currentItem.ItemData.Amount;
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
                if(_currentItem != null)
                {
                    return _currentItem.ItemData.Item;
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
            _currentItem = Instantiate(DraggableItemPrefab, transform).GetComponent<DraggableItem>();
            _currentItem.Init(this, itemAmount);
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (Item == null)
            {
                Icon.enabled = false;
                AmountText.enabled = false;
            }
            else
            {
                Icon.enabled = true;
                AmountText.enabled = true;
                Icon.sprite = Item.Icon;
                AmountText.text = Amount.ToString();
            }
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
            return _currentItem == null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            draggableItem.ParentAfterDrag = transform;
            _currentItem = draggableItem;
        }
    }
}
