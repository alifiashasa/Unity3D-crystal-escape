using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerTPS : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 3.5f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    private float velocityY;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private LayerMask groundMask;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.32f;
    [SerializeField] [Range(0f, 1f)] private float footstepVolume = 1.0f;

    private float footstepTimer;
    private AudioSource audioSource;

    private bool isGrounded;
    private bool didJump;

    private CharacterController controller;
    private TPS inputActions;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintPressed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new TPS();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Jump.performed += OnJump;

        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Jump.performed -= OnJump;

        inputActions.Player.Sprint.performed -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;

        inputActions.Disable();
    }

    private void Update()
    {
        bool wasGroundedBefore = isGrounded;
        CheckGround();

        if (!wasGroundedBefore && isGrounded)
        {
            if (didJump)
            {
                if (landingSound != null)
                {
                    AudioSource cameraAudio = Camera.main.GetComponent<AudioSource>();
                    if (cameraAudio == null)
                    {
                        cameraAudio = Camera.main.gameObject.AddComponent<AudioSource>();
                    }
                    cameraAudio.PlayOneShot(landingSound);
                }
                didJump = false;
            }
        }

        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateAnimator();
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            jumpPressed = true;
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprintPressed = context.ReadValueAsButton();
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        if (move.magnitude > 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;

            Vector3 moveDirection =
                camForward * move.z +
                camRight * move.x;

            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            float currentSpeed =
                sprintPressed ? runSpeed : moveSpeed;

            controller.Move(
                moveDirection.normalized *
                currentSpeed *
                Time.deltaTime
            );

            animator.SetBool("isWalk", true);
            animator.SetBool("isRun", sprintPressed);

            if (isGrounded)
            {
                footstepTimer += Time.deltaTime;
                float currentInterval = sprintPressed ? runStepInterval : walkStepInterval;
                if (footstepTimer >= currentInterval)
                {
                    PlayFootstepSound();
                    footstepTimer = 0f;
                }
            }
            else
            {
                footstepTimer = 0f;
            }
        }
        else
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
            footstepTimer = 0f;
        }
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded && velocityY <= 0)
        {
            velocityY = Mathf.Sqrt(
                jumpForce * -2f * gravity
            );

            animator.SetTrigger("jump");
            didJump = true;
        }

        jumpPressed = false;
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }

        velocityY += gravity * Time.deltaTime;

        Vector3 gravityMove =
            new Vector3(0, velocityY, 0);

        controller.Move(gravityMove * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", velocityY);
    }

    private void PlayFootstepSound()
    {
        if (footstepSound != null)
        {
            if (audioSource == null && Camera.main != null)
            {
                audioSource = Camera.main.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
                }
            }

            if (audioSource != null)
            {
                float volume = sprintPressed ? footstepVolume : footstepVolume * 0.75f;
                audioSource.pitch = Random.Range(0.9f, 1.15f);
                audioSource.PlayOneShot(footstepSound, volume);
            }
        }
    }
}