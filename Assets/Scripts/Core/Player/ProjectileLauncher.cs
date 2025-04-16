using UnityEngine;
using Unity.Netcode;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CoinWallet wallet;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;
    private bool shouldFire;
    private float timer;

    private float muzzleFlashTimer;

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
            inputReader.PrimaryFireEvent -= HandlePrimaryFire;
        }

        // Set the new InputReader
        inputReader = reader;

        // Subscribe to events
        if (IsOwner && inputReader != null)
        {
            inputReader.PrimaryFireEvent += HandlePrimaryFire;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        if (inputReader != null)
        {
            inputReader.PrimaryFireEvent += HandlePrimaryFire;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        if (inputReader != null)
        {
            inputReader.PrimaryFireEvent -= HandlePrimaryFire;
        }
    }

    private void Update()
    {
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;
        }
        if (muzzleFlashTimer <= 0f)
        {
            muzzleFlash.SetActive(false);
        }
        if (!IsOwner) { return; }
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (!shouldFire) { return; }
        if (timer > 0) { return; }
        if (wallet.TotalCoins.Value < costToFire) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        timer = 1 / fireRate;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }
    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (wallet.TotalCoins.Value < costToFire) { return; }
        wallet.SpendCoins(costToFire);
        GameObject projectileInstance = Instantiate
        (serverProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        if (projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }
    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        // Handle muzzle flash for all clients
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        // Don't spawn visual projectile for the owner (they already have one)
        if (IsOwner) { return; }

        // Spawn visual projectile for other clients
        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,  // Changed to clientProjectilePrefab
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;

        // Set up visual projectile movement
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
    }
    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate
        (clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
    }
}