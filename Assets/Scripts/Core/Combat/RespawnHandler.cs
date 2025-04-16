using UnityEngine;
using Unity.Netcode;
using System.Collections;


public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private float keptcoinPercentage;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }
        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);

        foreach (TankPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }
        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }
    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }
    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }
    private void HandlePlayerDie(TankPlayer player)
    {
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptcoinPercentage / 100));
        ulong ownerClientId = player.OwnerClientId;

        // Store input reader information before despawning
        InputReader inputReader = null;

        // Find the InputReader before despawning from any component that has it
        if (player.TryGetComponent<PlayerMovement>(out var movement))
        {
            // Get the reference to the InputReader ScriptableObject
            inputReader = movement.GetInputReader();
        }
        else if (player.TryGetComponent<PlayerAiming>(out var aiming) &&
                aiming.TryGetComponent<InputReader>(out var aimingInputReader))
        {
            inputReader = aimingInputReader;
        }
        else if (player.TryGetComponent<ProjectileLauncher>(out var launcher))
        {
            inputReader = launcher.GetInputReader();
        }

        // Despawn the player after capturing the InputReader reference
        player.NetworkObject.Despawn();

        // Start respawn process with the stored InputReader
        StartCoroutine(RespawnPlayer(ownerClientId, keptCoins, inputReader));
    }
    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins, InputReader inputReader)
    {
        yield return new WaitForSeconds(1f); // Give a short delay before respawn

        NetworkObject playerInstance =
            Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(ownerClientId);

        // Set the kept coins to the new player instance
        if (playerInstance.TryGetComponent<TankPlayer>(out var tankPlayer) &&
            tankPlayer.Wallet != null)
        {
            tankPlayer.Wallet.TotalCoins.Value = keptCoins;

            // Re-enable the InputReader if it was previously disabled
            if (inputReader != null)
            {
                // Force re-enable the input system
                inputReader.OnEnable();

                // Apply the InputReader to all components that need it
                if (playerInstance.TryGetComponent<PlayerMovement>(out var movement))
                {
                    movement.ReinitializeInput(inputReader);
                }

                if (playerInstance.TryGetComponent<PlayerAiming>(out var aiming))
                {
                    aiming.ReinitializeInput(inputReader);
                }

                if (playerInstance.TryGetComponent<ProjectileLauncher>(out var launcher))
                {
                    launcher.ReinitializeInput(inputReader);
                }
            }
        }
    }
}