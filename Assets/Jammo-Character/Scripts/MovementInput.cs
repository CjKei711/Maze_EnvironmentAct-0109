using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float jumpForce = 6f;
    public float gravity = -20f;

    public float InputX;
    public float InputZ;
    public Vector3 desiredMoveDirection;

    [Header("Rotation")]
    public bool blockRotationPlayer;
    public float desiredRotationSpeed = 0.1f;
    public float allowPlayerRotation = 0.1f;

    [Header("References")]
    public Animator anim;
    public Camera cam;
    public CharacterController controller;

    [Header("Animation Smoothing")]
    [Range(0, 1f)] public float StartAnimTime = 0.3f;
    [Range(0, 1f)] public float StopAnimTime = 0.15f;

    private float speed;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isSprinting;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        anim.SetBool("Grounded", isGrounded);

        HandleMovementInput();
        HandleJump();
        ApplyGravity();
        MoveCharacter();
        HandleAnimation();
    }


    void HandleMovementInput()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = (forward * InputZ + right * InputX).normalized;

        if (!blockRotationPlayer && desiredMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, desiredRotationSpeed);
        }
    }

    void MoveCharacter()
    {
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = desiredMoveDirection * currentSpeed;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void HandleJump()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = jumpForce;
            anim.ResetTrigger("Jump"); // prevents stacking
            anim.SetTrigger("Jump");
        }
    }


    void ApplyGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    void HandleAnimation()
    {
        speed = new Vector2(InputX, InputZ).sqrMagnitude;

        if (speed > allowPlayerRotation)
        {
            anim.SetFloat("Blend", speed * (isSprinting ? 1.5f : 1f), StartAnimTime, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Blend", speed, StopAnimTime, Time.deltaTime);
        }

        anim.SetBool("Sprint", isSprinting);
        
    }

    public void LookAt(Vector3 pos)
    {
        Quaternion lookRotation = Quaternion.LookRotation(pos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, desiredRotationSpeed);
    }
}
