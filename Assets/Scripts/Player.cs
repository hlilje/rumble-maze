using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotateSpeed = 0.1f;

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
        var look = playerInput.Player.Look.ReadValue<Vector2>();

        var velocity = moveSpeed * move;
        var rotation = -(look.x * rotateSpeed);

        body.velocity = velocity;
        body.transform.Rotate(0.0f, 0.0f, rotation);

        Debug.Log("Velocity " + velocity);
        Debug.Log("Rotation " + rotation);
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
