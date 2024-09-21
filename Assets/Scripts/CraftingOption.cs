using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CraftingOption : MonoBehaviour
    {
        [SerializeField] private Color _notSelectedColor;
        [SerializeField] private Sprite _notSelectedSprite;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Sprite _selectedSprite;

        public GameObject RequirementsParent;
        [SerializeField] private Image _icon;
        [SerializeField] private ItemAmountUI _requirementPrefab;

        private float _initialCraftInterval = .5f;
        private float _minCraftInterval = 0.01f;
        private float _acceleration = 0.075f;

        private float _currentCraftInterval;
        private float _timeSinceLastCraft;

        [HideInInspector] public CraftRecipe Recipe;

        [HideInInspector] public Inventory Inventory;

        [SerializeField] private Image _background;
        public RectTransform Rect;
        [HideInInspector] public Button Button;

        [HideInInspector] public bool IsSelected;

        private Vector3 _initialScale;

        private bool _isCrafting;

        private void Awake()
        {
            _initialScale = transform.localScale;
            Button = GetComponentInChildren<Button>();
        }

        public void Init(CraftRecipe recipe)
        {
            Recipe = recipe;
            _icon.sprite = recipe.Result.GetItem().Icon;

            foreach (var requirement in recipe.Requirements)
            {
                ItemAmountUI requirementItemUI = Instantiate(_requirementPrefab, RequirementsParent.transform).GetComponent<ItemAmountUI>();

                requirementItemUI.Init(requirement);
            }
        }

        private void Update()
        {
            if(_isCrafting)
            {
                Craft();
            }
        }

        public void Highlight()
        {
            _background.color = _selectedColor;
            _background.sprite = _selectedSprite;
        }

        public void DeHighlight()
        {
            _background.color = _notSelectedColor;
            _background.sprite = _notSelectedSprite;
            if(IsSelected)
            {
                transform.localScale = _initialScale;
                RequirementsParent.SetActive(false);
            }
            IsSelected = false;
            Button.interactable = true;
        }

        public void Select()
        {
            IsSelected = true;
            transform.localScale = Vector3.one;
            RequirementsParent.SetActive(true);
        }

        public void StartCrafting()
        {
            _isCrafting = true;
            _currentCraftInterval = _initialCraftInterval;
            _timeSinceLastCraft = _currentCraftInterval;
            Craft();
        }

        public void StopCrafting()
        {
            _isCrafting = false;
            _timeSinceLastCraft = 0f;
            _currentCraftInterval = _initialCraftInterval;
        }

        private void Craft()
        {
            _timeSinceLastCraft += Time.deltaTime;
            if (_timeSinceLastCraft >= _currentCraftInterval)
            {
                Inventory.CraftOptionClick(this);
                _timeSinceLastCraft = 0f;
                _currentCraftInterval = Mathf.Max(_minCraftInterval, _currentCraftInterval - _acceleration);
            }
        }
    }
}
