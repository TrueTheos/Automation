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

        [Header("Crafting")]
        public RectTransform CraftingView;
        public RectTransform CraftingOptionsParent;
        private int _selectedCraftingOptionIndex;
        private CraftingOption _selectedCraftingOption;
        public Color SelectedCraftColor;
        public KeyCode CraftingMenuKey;
        public GameObject CraftingOptionPrefab;
        private bool _crafingViewToggle;
        public bool CraftingViewOpen => _crafingViewToggle;
        private float _craftingOptionsTargetPos;

        [Header("HotBar")]
        public ItemAmountUI[] HotbarItems;
        public int HotbarSlots;
        public RectTransform HotbarView;
        public RectTransform HotbarSlotsParent;
        public ItemAmountUI HotbarSlotPrefab;

        private Player _player;

        private void Awake()
        {
            HotbarItems = new ItemAmountUI[HotbarSlots];

            for (int i = 0; i < HotbarSlots; i++)
            {
                ItemAmountUI itemAmountUI = Instantiate(HotbarSlotPrefab, HotbarSlotsParent.transform);
                HotbarItems[i] = itemAmountUI;
            }

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
                    MoveTowardsCraftOption(CraftingOptionsParent.transform.GetChild(0).GetComponent<CraftingOption>());
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

        private void OnSelect()
        {
            _selectedCraftingOption.Select();

            _selectedCraftingOption.Button.interactable = MatchesRequirements(_selectedCraftingOption.Recipe.Requirements);
        }

        private void OpenCraftingView()
        {
            if (_player.AvailableRecieps.Count == 0) return;

            foreach (Transform child in CraftingOptionsParent.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _player.AvailableRecieps.Count; i++)
            {
                CraftingOption newOption = Instantiate(CraftingOptionPrefab, CraftingOptionsParent.transform).GetComponent<CraftingOption>();
                newOption.RequirementsParent.SetActive(false);
                newOption.Init(_player.AvailableRecieps[i]);
                newOption.Button.onClick.AddListener(delegate { CraftOptionClick(newOption); });
            }
        }

        private void MoveTowardsCraftOption(CraftingOption craft)
        {
            if(_selectedCraftingOption != null)
            {
                _selectedCraftingOption.DeHighlight();
            }
            
            _selectedCraftingOption = craft;
            _selectedCraftingOption.Highlight(SelectedCraftColor);

            float selectedChildPos = _selectedCraftingOption.Rect.position.y;
            float target = _craftingOptionsTargetPos - selectedChildPos - _selectedCraftingOption.Rect.rect.height;
            float distance = Mathf.Abs(target);
            float duration = Mathf.Clamp(1f - (distance / 10f), 0.2f, 1f);
            DOTween.Kill(CraftingOptionsParent);
            CraftingOptionsParent.DOAnchorPosY(CraftingOptionsParent.position.y + target, duration, true).onComplete = OnSelect;
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
            if (!HotbarItems.Any(x => !x.IsEmpty() && x.Item == item)) return false;
            return HotbarItems.First(x => x.Item == item).Amount >= amount;
        }

        public bool HasEmptySlot()
        {
            return HotbarItems.Any(x => x == null || x.IsEmpty());
        }

        public int GetEmptySlotIndex()
        {
            return Array.IndexOf(HotbarItems, HotbarItems.First(x => x == null || x.IsEmpty()));
        }

        public ItemAmountUI GetEmptySlot()
        {
            return HotbarItems.First(x => x.IsEmpty());
        }

        public ItemAmountUI GetNotFullSlot(Item item)
        {
            return HotbarItems.FirstOrDefault(x => x != null && x.Item == item && x.Amount < x.Item.MaxStack);
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

        public void RemoveItemFromSlot(int amount, ItemAmountUI ui)
        {
            ui.ItemAmount.Amount -= amount;

            if(ui.ItemAmount.Amount <= 0)
            {
                ui.ItemAmount = new();
            }

            UpdateHotbarUI();
        }

        public void RemoveItem(Item item, int amount)
        {
            int remainingAmount = amount;

            for (int i = 0; i < HotbarItems.Length; i++)
            {
                if (HotbarItems[i] != null && HotbarItems[i].Item == item)
                {
                    if (HotbarItems[i].Amount >= remainingAmount)
                    {
                        HotbarItems[i].ItemAmount.Amount -= remainingAmount;
                        if (HotbarItems[i].Amount == 0)
                        {
                            HotbarItems[i] = null;
                        }
                        remainingAmount = 0;
                        break;
                    }
                    else
                    {
                        remainingAmount -= HotbarItems[i].Amount;
                        HotbarItems[i].ItemAmount = new();
                    }
                }
            }

            if (remainingAmount > 0)
            {
                throw new Exception($"Unable to remove {remainingAmount} of {item.Name} from the inventory.");
            }

            UpdateHotbarUI();
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

        private void UpdateHotbarUI()
        {
            foreach (var item in HotbarItems)
            {
                item.UpdateUI();
            }
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
                ItemAmountUI notFullSlot = GetNotFullSlot(item);

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
                        toAdd -= toAdd;
                    }
                }
                else
                {
                    if (HasEmptySlot())
                    {
                        ItemAmountUI emptySlot = GetEmptySlot();
                        int amountToAdd = toAdd > item.MaxStack ? item.MaxStack : toAdd;

                        emptySlot.Init(new ItemAmount(item, amountToAdd));
                        toAdd -= amountToAdd;
                    }
                    else
                    {
                        UpdateHotbarUI();
                        return toAdd;
                    }
                    /*if (HasEmptySlot())
                    {
                        int freeSlot = GetEmptySlotIndex();

                        int amountToAdd = toAdd > item.MaxStack ? item.MaxStack : toAdd;

                        InventorySlot newSlot = new InventorySlot() { ItemAmount = new(item, amountToAdd) };
                        toAdd -= amountToAdd;
                        HotbarItems[freeSlot] = newSlot;
                    }
                    else
                    {
                        return toAdd;
                    }*/
                }
            }

            UpdateHotbarUI();
            return toAdd;
        }
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
