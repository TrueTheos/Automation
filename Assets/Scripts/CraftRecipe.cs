using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Crafting Recipe")]
    public class CraftRecipe : ScriptableObject
    {
        public List<ItemAmount> Requirements;
        public ItemAmount Result;
    }
}
