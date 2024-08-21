using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CraftingOption : MonoBehaviour
    {
        public GameObject RequirementsParent;
        public Image Icon;
        public GameObject RequirementPrefab;

        public CraftRecipe Recipe;

        private Color _normalColor;

        public Image Background;
        public RectTransform Rect;
        public Button Button;

        public bool IsSelected;

        private Vector3 _initialScale;

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
    }
}
