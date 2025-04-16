using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;

    private Vector2 previousMovementInput;
    private Controls _controls;

    // Add this method to access the InputReader
    public InputReader GetInputReader()
    {
        return inputReader;
    }

    // Add this method to reinitialize after respawn
    public void ReinitializeInput(InputReader reader)
    {
        // Unsubscribe from old events if needed
        if (inputReader != null)
        {
            inputReader.MoveEvent -= HandleMove;
        }

        // Set the new InputReader
        inputReader = reader;

        // Subscribe to events
        if (IsOwner && inputReader != null)
        {
            inputReader.MoveEvent += HandleMove;

            // Make sure controls are enabled
            if (_controls != null)
            {
                _controls.Enable();
            }
        }
    }

    private void Awake()
    {
        _controls = new Controls();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        // Make sure to properly enable here
        if (inputReader != null)
        {
            inputReader.MoveEvent += HandleMove;
            inputReader.OnEnable(); // Explicitly call OnEnable
        }
        _controls?.Enable();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        // Don't disable the InputReader here anymore since it's shared
        // Just unsubscribe from events
        if (inputReader != null)
        {
            inputReader.MoveEvent -= HandleMove;
        }

        if (_controls != null)
        {
            _controls.Disable();
            _controls.Dispose();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }

        rb.linearVelocity = (Vector2)bodyTransform.up * previousMovementInput.y * movementSpeed;

        float zRotation = previousMovementInput.x * -turningRate * Time.fixedDeltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
}