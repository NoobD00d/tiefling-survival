using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class FpsMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -12f; //9.8 too floaty
    [SerializeField] private float initialFallVelocity = -2f;

    [Header("Crouching")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float cameraOffset = 0.4f; // Adjust this value to position the camera correctly when crouching

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference runAction;

    private CharacterController characterController;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isCrouching;
    private bool isRunning;
    private float verticalVelocity;
    private float targetHeight;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        targetHeight = standingHeight;
    }

    // Subscribe to input events when the script is enabled, and unsubscribe when disabled
    private void OnEnable() {
        moveAction.action.performed += StoreMovementInput;
        moveAction.action.canceled += StoreMovementInput;
        jumpAction.action.performed += Jump;
        runAction.action.performed += Run;
        runAction.action.canceled += Run;
        crouchAction.action.canceled += Crouch;
    }

    private void OnDisable() {
        moveAction.action.performed -= StoreMovementInput;
        moveAction.action.canceled -= StoreMovementInput;
        jumpAction.action.performed += Jump;
        runAction.action.performed -= Run;
        runAction.action.canceled -= Run;
        crouchAction.action.canceled -= Crouch;
    }

    private void Update() {
        isGrounded = characterController.isGrounded;
        HandleGravity(); //Gravity first to ensure it applies before movement, allowing for proper jumping and falling behavior
        HandleMovement();
        HandleCrouchTransition();
    }

    private void StoreMovementInput(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Jump(InputAction.CallbackContext context) {
        if (isGrounded) {
            verticalVelocity = jumpForce;
        }
    }

    private void Crouch(InputAction.CallbackContext context) {
        if (isCrouching) {
            targetHeight = standingHeight;
        } else {
            targetHeight = crouchHeight;
        }

        isCrouching = !isCrouching;
    }

    private void Run(InputAction.CallbackContext context) {
        isRunning = context.performed;
    }

    private void HandleGravity() {
        if (isGrounded && verticalVelocity < 0) {
            verticalVelocity = initialFallVelocity; // Small negative value to keep the character grounded
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleMovement() {
        //Allows movement to be relative to the camera's orientation
        var move = cameraTransform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized;
        var currentSpeed = isCrouching ? crouchSpeed : isRunning ? runSpeed : walkSpeed; // Check for move state. Default to walk speed
        var finalMove = move * currentSpeed;
        finalMove.y = verticalVelocity; // Apply vertical velocity for jumping and falling

        var collisions = characterController.Move(finalMove * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0) {
            verticalVelocity = initialFallVelocity; // Reset vertical velocity when hitting roof to prevent sticking to the ceiling
        }
    }

    private void HandleCrouchTransition()
    {
        var currentHeight = characterController.height;
        if (Mathf.Abs(currentHeight - targetHeight) < 0.01f) {
            characterController.height = targetHeight; // Snap to target height when close enough
            return;
        }

        var newHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        characterController.height = newHeight;
        characterController.center = Vector3.up * (newHeight * 0.5f);

        var cameraTargetPosition = cameraTransform.localPosition;
        cameraTargetPosition.y = targetHeight - cameraOffset; // Adjust camera position based on target height and offset
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition, 
            cameraTargetPosition, 
            crouchTransitionSpeed * Time.deltaTime);
    }
}
