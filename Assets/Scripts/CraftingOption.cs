﻿using System;
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
        public GameObject RequirementsParent;
        public Image Icon;
        public GameObject RequirementPrefab;

        private float _initialCraftInterval = .5f;
        private float _minCraftInterval = 0.01f;
        private float _acceleration = 0.075f;

        private float _currentCraftInterval;
        private float _timeSinceLastCraft;

        public CraftRecipe Recipe;

        [SerializeField] public Inventory Inventory;

        private Color _normalColor;

        public Image Background;
        public RectTransform Rect;
        public Button Button;

        public bool IsSelected;

        private Vector3 _initialScale;

        private bool _isCrafting;

        private void Awake()
        {
            _normalColor = Background.color;
            _initialScale = transform.localScale;
            Button = GetComponentInChildren<Button>();
        }

        public void Init(CraftRecipe recipe)
        {
            Recipe = recipe;
            Icon.sprite = recipe.Result.Item.Icon;

            foreach (var requirement in recipe.Requirements)
            {
                ItemAmountUI requirementItemUI = Instantiate(RequirementPrefab, RequirementsParent.transform).GetComponent<ItemAmountUI>();

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

        public void Highlight(Color color)
        {
            Background.color = color;
        }

        public void DeHighlight()
        {
            Background.color = _normalColor;
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
