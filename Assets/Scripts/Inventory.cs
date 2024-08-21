using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Assets.Scripts
{
    public class Inventory : MonoBehaviour
    {
        public int MaxSlots;

        public InventorySlot[] Items;

        public RectTransform CraftingView;
        public RectTransform CraftingOptionsParent;
        private int _selectedCraftingOptionIndex;
        private CraftingOption _selectedCraftingOption;
        public Color SelectedCraftColor;
        public KeyCode CraftingMenuKey;
        public GameObject CraftingOptionPrefab;
        private bool _crafingViewToggle;

        private float _craftingOptionsTargetPos;

        private Player _player;

        private void Awake()
        {
            Items = new InventorySlot[MaxSlots];

            _player = GetComponent<Player>();
        }

        private void Start()
        {
            _craftingOptionsTargetPos = CraftingOptionsParent.rect.height / 10;
        }

        private void Update()
        {
            if(Input.GetKeyDown(CraftingMenuKey))
            {
                _crafingViewToggle = !_crafingViewToggle;

                CraftingView.gameObject.SetActive(_crafingViewToggle);

                if(_crafingViewToggle)
                {
                    OpenCraftingView();
                }
            }

            if(_crafingViewToggle)
            {
                if(_selectedCraftingOption == null)
                {
                    _selectedCraftingOption = CraftingOptionsParent.transform.GetChild(0).GetComponent<CraftingOption>();
                    _selectedCraftingOption.Highlight(SelectedCraftColor);
                }

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0f)
                {
                    if (scroll > 0f)
                    {
                        _selectedCraftingOptionIndex--;
                        if (_selectedCraftingOptionIndex < 0)
                        {
                            _selectedCraftingOptionIndex = 0;
                        }
                    }
                    else if (scroll < 0f)
                    {
                        _selectedCraftingOptionIndex++;
                        if (_selectedCraftingOptionIndex >= CraftingOptionsParent.transform.childCount)
                        {
                            _selectedCraftingOptionIndex = CraftingOptionsParent.transform.childCount - 1;
                        }
                    }

                    MoveTowardsCraftOption(CraftingOptionsParent.transform.GetChild(_selectedCraftingOptionIndex).GetComponent<CraftingOption>());
                }   
            }
        }

        private void OpenCraftingView()
        {
            if (_player.AvailableRecieps.Count == 0) return;

            //MainCraftingOption.GetComponent<CraftingOption>().Init(_player.AvailableRecieps[0]);

            foreach (Transform child in CraftingOptionsParent.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _player.AvailableRecieps.Count; i++)
            {
                CraftingOption newOption = Instantiate(CraftingOptionPrefab, CraftingOptionsParent.transform).GetComponent<CraftingOption>();
                newOption.RequirementsParent.SetActive(false);
                newOption.Init(_player.AvailableRecieps[i]);
                newOption.GetComponentInChildren<Button>().onClick.AddListener(delegate { CraftOptionClick(newOption); });
            }
        }

        private void MoveTowardsCraftOption(CraftingOption craft)
        {
            _selectedCraftingOption?.DeHighlight();
            _selectedCraftingOption = craft;
            _selectedCraftingOption.Highlight(SelectedCraftColor);

            float selectedChildPos = _selectedCraftingOption.Rect.position.y;
            float target = _craftingOptionsTargetPos - selectedChildPos - _selectedCraftingOption.Rect.rect.height;
            float distance = Mathf.Abs(target);
            float duration = Mathf.Clamp(1f - (distance / 10f), 0.2f, 1f);
            DOTween.Kill(CraftingOptionsParent);
            CraftingOptionsParent.DOAnchorPosY(CraftingOptionsParent.position.y + target, duration, true).onComplete = _selectedCraftingOption.Select;
        }

        public void CraftOptionClick(CraftingOption craft)
        {
            if (craft.IsSelected)
            {
                if(MatchesRequirements(craft.Recipe.Requirements))
                {
                    RemoveItems(craft.Recipe.Requirements);
                    AddItem(craft.Recipe.Result, true);
                }
            }
            else
            {
                MoveTowardsCraftOption(craft);
            }
        }

        public bool MatchesRequirements(List<ItemAmount> requirements)
        {
            foreach (var req in requirements)
            {
                if (!HasItems(req.Item, req.Amount)) return false;
            }

            return true;
        }

        public bool HasItems(Item item, int amount)
        {
            if (!Items.Any(x => x != null && x.Item != null && x.Item == item)) return false;
            return Items.First(x => x.Item == item).Amount >= amount;
        }

        public bool HasEmptySlot()
        {
            return Items.Any(x => x == null);
        }

        public int GetEmptySlotIndex()
        {
            return Array.IndexOf(Items, Items.First(x => x == null));
        }

        public InventorySlot GetNotFullSlot(Item item)
        {
            return Items.FirstOrDefault(x => x != null && x.Item == item && x.Amount < x.Item.MaxStack);
        }

        public bool CanAddItem(ItemObject itemObj)
        {
            return GetNotFullSlot(itemObj.ItemData) != null;
        }

        public void RemoveItems(List<ItemAmount> items)
        {
            foreach (var item in items)
            {
                RemoveItem(item.Item, item.Amount);
            }
        }

        public void RemoveItem(Item item, int amount)
        {
            int remainingAmount = amount;

            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null && Items[i].Item == item)
                {
                    if (Items[i].Amount >= remainingAmount)
                    {
                        Items[i].ItemAmount.Amount -= remainingAmount;
                        if (Items[i].Amount == 0)
                        {
                            Items[i] = null;
                        }
                        remainingAmount = 0;
                        break;
                    }
                    else
                    {
                        remainingAmount -= Items[i].Amount;
                        Items[i] = null;
                    }
                }
            }

            if (remainingAmount > 0)
            {
                throw new Exception($"Unable to remove {remainingAmount} of {item.Name} from the inventory.");
            }
        }

        public void AddItem(ItemAmount item, bool dropTheRest = false)
        {
             int leftOver = AddItem(item.Item, item.Amount);

            if(dropTheRest && leftOver > 0)
            {
                MapManager.Instance.SpawnItem(item.Item, 
                    transform.position.x, 
                    transform.position.y, 
                    leftOver);
            }
        }

        public void PickupItem(ItemObject itemObj, bool dropTheRest = false)
        {
            int leftOver = AddItem(itemObj.ItemData, itemObj.Stack);

            if(dropTheRest && leftOver > 0)
            {
                MapManager.Instance.SpawnItem(itemObj.ItemData, 
                    itemObj.transform.position.x, 
                    itemObj.transform.position.y, 
                    leftOver);
            }

            Destroy(itemObj.gameObject);
        }

        /// <summary>
        /// Add item to the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns>Returns how many items couldn't fit</returns>
        public int AddItem(Item item, int amount)
        {
            int toAdd = amount;

            while (toAdd > 0)
            {
                InventorySlot notFullSlot = GetNotFullSlot(item);

                if (notFullSlot != null)
                {
                    int freeAmount = notFullSlot.Item.MaxStack - notFullSlot.Amount;

                    if (toAdd > freeAmount)
                    {
                        notFullSlot.ItemAmount.Amount += freeAmount;
                        toAdd -= freeAmount;
                    }
                    else
                    {
                        notFullSlot.ItemAmount.Amount += toAdd;
                    }
                }
                else
                {
                    if (HasEmptySlot())
                    {
                        int freeSlot = GetEmptySlotIndex();

                        int amountToAdd = toAdd > item.MaxStack ? item.MaxStack : toAdd;

                        InventorySlot newSlot = new InventorySlot() { ItemAmount = new(item, amountToAdd) };
                        toAdd -= amountToAdd;
                        Items[freeSlot] = newSlot;
                    }
                    else
                    {
                        return toAdd;
                    }
                }
            }

            return toAdd;
        }
    }

    public class InventorySlot
    {
        public ItemAmount ItemAmount;

        public Item Item => ItemAmount.Item;
        public int Amount => ItemAmount.Amount;
    }

    [Serializable]
    public struct ItemAmount
    {
        public Item Item;
        public int Amount;

        public ItemAmount(Item item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}
