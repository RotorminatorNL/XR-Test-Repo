using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float playerHeight;
    [SerializeField] private Transform orientation;

    [Header("Walk")]
    [SerializeField] private float moveSpeed;

    [Header("Ground")]
    [SerializeField] private float groundDrag;
    [SerializeField] private LayerMask groundLayer;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private bool grounded;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        PlayerInput();

        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
    }
}
