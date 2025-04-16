using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;
    private NetworkList<LeaderboardEntityState> leaderboardEntities;

    private List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();
    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged += HandleLeaderBoardEntitiesChanged;
            foreach (LeaderboardEntityState entity in leaderboardEntities)
            {
                HandleLeaderBoardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }
        if (IsServer)
        {
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;


        }

    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderBoardEntitiesChanged;
        }
        if (IsServer)
        {
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;

        }

    }
    private void HandleLeaderBoardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        Debug.Log($"Leaderboard change event: {changeEvent.Type} for ClientId {changeEvent.Value.ClientId}");
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderboardEntityDisplay leaderboardEntity =
                     Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    leaderboardEntity.Initialise(
                        changeEvent.Value.ClientId,
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.Coins);
                    entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove =
                entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate =
                        entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToUpdate != null)
                {
                    Debug.Log($"Updating UI for ClientId {changeEvent.Value.ClientId} - Coins: {changeEvent.Value.Coins}");

                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                    Debug.Log("Leaderboard UI updated!");




                }
                else
                {
                    Debug.LogWarning($"No UI found for ClientId {changeEvent.Value.ClientId}");
                }
                break;

        }
        entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
        for (int i = 0; i < entityDisplays.Count; i++)
        {
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }
        LeaderboardEntityDisplay myDisplay =
        entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);
        if (myDisplay != null)
        {
            if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }



    }
    private void HandlePlayerSpawned(TankPlayer player)
    {
        Debug.Log($"Registering Player: {player.PlayerName.Value}, ClientId: {player.OwnerClientId}");
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0

        });
        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) =>
        HandleCoinsChanged(player.OwnerClientId, newCoins);
    }
    private void HandlePlayerDespawned(TankPlayer player)
    {
        if (leaderboardEntities == null) { return; }
        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if (entity.ClientId != player.OwnerClientId) { continue; }
            leaderboardEntities.Remove(entity);
            break;
        }
        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) =>
        HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    private void HandleCoinsChanged(ulong clientId, int newCoins)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId != clientId) { continue; }
            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                Coins = newCoins
            };
            Debug.Log($"Updated coins for ClientId {clientId}: {newCoins}");
            return;
        }
        Debug.LogWarning($"No leaderboard entry found for ClientId {clientId}");

    }

}

