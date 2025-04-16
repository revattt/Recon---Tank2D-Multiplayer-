using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models; // This includes the Allocation type
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;


public class HostGameManager : IDisposable
{
    private Allocation allocation; // Correct type from Relay.Models
    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";
    private string joinCode;
    private string lobbyId;
    public NetworkServer NetworkServer { get; private set; }

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay allocation failed: {e}");
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code: {joinCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Join code fetch failed: {e}");
            return;
        }


        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,value: joinCode)
                }

            };
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown User");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15));

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }
        NetworkServer = new NetworkServer(NetworkManager.Singleton);
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId

        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartHost();
        NetworkServer.OnClientLeft += HandleClientLeft;
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);


    }
    private IEnumerator HeartBeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    public void Dispose()
    {
        Shutdown();
    }
    public async void Shutdown()
    {
        if (HostSingleton.Instance != null)
        {
            HostSingleton.Instance.StopCoroutine(nameof(HeartBeatLobby));
            if (!string.IsNullOrEmpty(lobbyId))
            {
                try
                {
                    await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);

                }
                lobbyId = string.Empty;
            }
            NetworkServer.OnClientLeft -= HandleClientLeft;
            NetworkServer?.Dispose();
        }

    }
    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}

