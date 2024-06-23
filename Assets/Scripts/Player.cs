using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    private PlayerInput playerInput;
    private Rigidbody2D body;

    void Awake()
    {
        playerInput = new PlayerInput();
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var move = playerInput.Player.Move.ReadValue<Vector2>();
        var velocity = speed * move;
        body.velocity = velocity;
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
    }
}
