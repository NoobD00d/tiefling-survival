using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private CharacterController characterController;
    private Vector2 moveInput;
    private bool isGrounded;
    private float verticalVelocity;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
    }

    // Subscribe to input events when the script is enabled, and unsubscribe when disabled
    private void OnEnable() {
        moveAction.action.performed += StoreMovementInput;
        moveAction.action.canceled += StoreMovementInput;
        jumpAction.action.performed += Jump;
    }

    private void OnDisable() {
        moveAction.action.performed -= StoreMovementInput;
        moveAction.action.canceled -= StoreMovementInput;
        jumpAction.action.performed += Jump;
    }

    private void Update() {
        isGrounded = characterController.isGrounded;
        HandleGravity(); //Gravity first to ensure it applies before movement, allowing for proper jumping and falling behavior
        HandleMovement();
    }

    private void StoreMovementInput(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Jump(InputAction.CallbackContext context) {
        if (isGrounded) {
            verticalVelocity = jumpForce;
        }
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
        var currentSpeed = walkSpeed; // Default to walk speed
        var finalMove = move * currentSpeed;
        finalMove.y = verticalVelocity; // Apply vertical velocity for jumping and falling

        characterController.Move(finalMove * Time.deltaTime);
    }
}
