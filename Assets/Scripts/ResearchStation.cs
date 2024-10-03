using Assets.Scripts;
using Assets.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResearchStation : MonoBehaviour
{
    public SerializableDictionary<int, List<CraftRecipe>> ResearchTiers = new();

    public int CurrentResearchTier;

    public void Start()
    {
        UnlockRecipes(1);
    }

    private void UnlockRecipes(int newLevel)
    {
        if (CurrentResearchTier == newLevel) return;
        CurrentResearchTier = newLevel;

        GameManager.Instance.CurrentPlayer.AvailableRecieps.AddRange(ResearchTiers[CurrentResearchTier]);
    }
}
