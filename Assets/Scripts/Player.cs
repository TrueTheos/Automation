using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public SpriteRenderer Art;

    public List<CraftRecipe> AvailableRecieps = new();
    private void Awake()
    {
        
    }
}
