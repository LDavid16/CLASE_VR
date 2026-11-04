using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float runMultiplier = 3f;

    [Header("Rotación")]
    public float rotationSpeed = 120f;

    [Header("Salto por Código (Sin Gravedad)")]
    public float jumHeigth = 1f;
    public float jumpDuration = 0.4f;
    private bool isJumping = false;

    private CharacterController controller;
    private PlayerInputAction inputActions;
    private Vector2 moveInput;
    private Vector2 rotateInput;
    private bool isRunning;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputAction();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Rotate.performed += ctx => rotateInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Rotate.canceled += ctx => rotateInput = Vector2.zero;

        inputActions.Player.Run.performed += ctx => isRunning = true;
        inputActions.Player.Run.canceled += ctx => isRunning = false;

        inputActions.Player.Jump.performed += ctx => OnJump();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        // 1. Movimiento Horizontal
        float currentSpeed = isRunning ? speed * runMultiplier : speed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 2. Rotación de Cámara
        if (Mathf.Abs(rotateInput.x) > 0.01f)
        {
            transform.Rotate(Vector3.up, rotateInput.x * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnJump()
    {
        if (!isJumping)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        float timer = 0f;
        Vector3 startPosition = transform.position;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            float heightOffset = Mathf.Sin(progress * Mathf.PI) * jumHeigth;
            Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y + heightOffset, startPosition.z);
            Vector3 verticalMove = targetPosition - transform.position;

            controller.Move(new Vector3(0, verticalMove.y, 0));
            yield return null;
        }

        isJumping = false;
    }
}