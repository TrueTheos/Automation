using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public SpriteRenderer Art;

    [SerializeField] public List<CraftRecipe> AvailableRecieps = new();
    private void Awake()
    {
        AvailableRecieps = Resources.LoadAll("Recipes", typeof(CraftRecipe)).Cast<CraftRecipe>().ToList();
    }
}
