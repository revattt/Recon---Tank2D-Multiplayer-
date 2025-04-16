using UnityEngine;
using Unity.Netcode;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins = 50;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float coinValue = 10f; // Added missing field

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        // Fixed: Need to assign the radius
        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        RespawningCoin coinInstance = Instantiate(
            coinPrefab,
            GetSpawnPoint(),
            Quaternion.identity);

        coinInstance.SetValue(Mathf.RoundToInt(coinValue));
        coinInstance.GetComponent<NetworkObject>().Spawn();
        coinInstance.OnCollected += HandleCoinCollected;
    }

    public void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;
        while (true)
        {
            // Fixed: Incorrect Random.Range usage
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            Vector2 spawnPoint = new Vector2(x, y);

            // Fixed: Need to capture the return value
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);

            // Fixed: Assignment vs comparison operator and return point
            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}