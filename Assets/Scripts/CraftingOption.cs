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
        public GameObject Requirements;
        public Image Icon;
        public GameObject RequirementPrefab;

        public void Init(CraftRecipe recipe)
        {
            Icon.sprite = recipe.Result.Item.Icon;

            foreach (var requirement in recipe.Requirements)
            {
                RequirementItemUI requirementItemUI = Instantiate(RequirementPrefab, Requirements.transform).GetComponent<RequirementItemUI>();
                requirementItemUI.Icon.sprite = requirement.Item.Icon;
                requirementItemUI.AmountText.text = requirement.Amount.ToString();
            }
        }
    }
}
