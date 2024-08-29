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
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image Icon;
        [HideInInspector] public Transform ParentAfterDrag;
        public ItemAmount ItemData;
        [SerializeField] private TextMeshProUGUI AmountText;
        public void OnBeginDrag(PointerEventData eventData)
        {
            ParentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            Icon.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        { 
            transform.SetParent(ParentAfterDrag);
            Icon.raycastTarget = true;
        }

        public void Init(ItemSlot parent, ItemAmount item)
        {
            transform.SetParent(parent.transform);
            Icon.raycastTarget = true;
            ItemData = item;
            UpdateUI();
        }

        public void UpdateUI()
        {
            Icon.sprite = ItemData.GetItem().Icon;
            AmountText.text = ItemData.Amount.ToString();
        }
    }
}
