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

    [HideInInspector] public List<CraftRecipe> AvailableRecieps = new();
    [HideInInspector] public PlayerMovement PlayeMovement { get; private set; }
    [HideInInspector] public Inventory Inventory { get; private set; }
    [HideInInspector] public CableBuilder CableBuilder { get; private set; }
    [HideInInspector] public PowerGridManager PowerGridManager { get; private set; }
    private void Awake()
    {
        PlayeMovement = GetComponent<PlayerMovement>();
        Inventory = GetComponent<Inventory>();
        CableBuilder = GetComponent<CableBuilder>();
        PowerGridManager = GetComponent<PowerGridManager>();
    }
}
