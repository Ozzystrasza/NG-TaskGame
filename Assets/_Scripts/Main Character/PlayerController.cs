using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Input (Input System)")]
    public InputActionReference moveAction;
    public InputActionReference runAction;
    public InputActionReference jumpAction;
    public InputActionReference collectAction;

    [Header("Camera")]
    public Transform cameraTransform;

    CharacterController controller;
    Animator animator;

    Vector3 velocity;
    bool isRunning;
    bool isGrounded;
    Vector2 moveInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (runAction != null) runAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (collectAction != null) collectAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (runAction != null) runAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (collectAction != null) collectAction.action.Disable();
    }

    void Update()
    {
        ReadInput();
        HandleMovement();
        ApplyGravity();
        UpdateAnimator();
    }

    void ReadInput()
    {
        moveInput = Vector2.zero;
        if (moveAction != null)
            moveInput = moveAction.action.ReadValue<Vector2>();

        if (runAction != null)
            isRunning = runAction.action.ReadValue<float>() > 0.5f;
        else
            isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void HandleMovement()
    {
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
        if (moveAction == null)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            input = new Vector3(h, 0f, v);
        }

        Vector3 move = Vector3.zero;
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = Vector3.forward;
            Vector3 camRight = Vector3.right;
            if (cameraTransform != null)
            {
                camForward = cameraTransform.forward;
                camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
            }

            move = camForward * input.z + camRight * input.x;
            move = move.normalized;

            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        float speed = isRunning ? runSpeed : walkSpeed;

        if (move.sqrMagnitude > 0.0001f)
        {
            Vector3 horizontalVelocity = move * speed;
            controller.Move(horizontalVelocity * Time.deltaTime);
        }

        isGrounded = controller.isGrounded;

        bool jumpTriggered = false;
        if (jumpAction != null)
            jumpTriggered = jumpAction.action.triggered;
        else
            jumpTriggered = Input.GetButtonDown("Jump");

        if (isGrounded && jumpTriggered)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float velocitySpeed = horizontalVel.magnitude;
        float targetSpeed = 0f;
        if (moveInput.sqrMagnitude > 0.0001f)
        {
            float inputMag = moveInput.magnitude;
            float baseSpeed = isRunning ? runSpeed : walkSpeed;
            targetSpeed = baseSpeed * inputMag;
        }
        float animSpeed = (targetSpeed > 0f) ? targetSpeed : velocitySpeed;
        animator.SetFloat("Speed", animSpeed);
        animator.SetBool("IsRunning", isRunning);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsJumping", !isGrounded && velocity.y > 0.1f);
    }

    public void TriggerCollect()
    {
        if (animator != null)
        {
            animator.SetTrigger("Collect");
        }
        // TODO: implement item pickup handling
    }
}
