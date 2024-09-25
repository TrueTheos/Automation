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
    public class ItemAmountUI : MonoBehaviour
    {
        public Image Icon;
        public Image Background;
        public Color HighlightColor;
        public TextMeshProUGUI AmountText;

        public ItemAmount ItemAmount;

        public int Amount => ItemAmount.Amount;
        public Item Item => ItemAmount.GetItem();

        private Color _defaultColor;

        private void Start()
        {
            _defaultColor = Background.color;
            UpdateUI();
        }

        public void Init(ItemAmount itemAmount)
        {
            ItemAmount = itemAmount;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if(Item == null)
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
    }
}
