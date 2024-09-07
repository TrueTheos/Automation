using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Managers;

using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public SpriteRenderer Art;

    [SerializeField] public List<CraftRecipe> AvailableRecieps = new();
    [HideInInspector] public PlayerMovement PlayeMovement { get; private set; }
    [HideInInspector] public Inventory Inventory { get; private set; }
    [HideInInspector] public CableManager CableManager { get; private set; }
    private void Awake()
    {
        AvailableRecieps = Resources.LoadAll("Recipes", typeof(CraftRecipe)).Cast<CraftRecipe>().ToList();
        PlayeMovement = GetComponent<PlayerMovement>();
        Inventory = GetComponent<Inventory>();
        CableManager = GetComponent<CableManager>();
    }
}
