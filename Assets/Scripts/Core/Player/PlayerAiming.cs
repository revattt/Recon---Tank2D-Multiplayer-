using UnityEngine;
using Unity.Netcode;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    // Add this method to reinitialize after respawn
    public void ReinitializeInput(InputReader reader)
    {
        inputReader = reader;
    }

    private void LateUpdate()
    {
        if (!IsOwner) { return; }

        // Make sure input reader is valid
        if (inputReader == null) return;

        Vector2 aimScreenPosition = inputReader.AimPosition;
        Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);
        turretTransform.up = new Vector2(
            aimWorldPosition.x - turretTransform.position.x,
            aimWorldPosition.y - turretTransform.position.y);
    }
}