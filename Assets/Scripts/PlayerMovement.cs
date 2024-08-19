using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;
    public float SprintSpeed;
    public KeyCode SprintKey;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        _moveInput.Normalize();

        if(Input.GetKey(SprintKey))
        {
            _rb.velocity = _moveInput * SprintSpeed;
        }
        else
        {
            _rb.velocity = _moveInput * MoveSpeed;
        }
    }
}
