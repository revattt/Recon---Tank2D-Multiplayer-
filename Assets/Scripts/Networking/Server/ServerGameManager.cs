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


public class ServerGameManager : MonoBehaviour
{
    public void Dispose()
    {
        Debug.Log("ServerGameManager Dispose called.");


    }
}

