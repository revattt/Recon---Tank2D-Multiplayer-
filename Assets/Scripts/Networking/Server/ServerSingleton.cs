using System;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    private static ServerSingleton instance;
    public ServerGameManager GameManager { get; private set; }
    public static ServerSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; }
            instance = FindFirstObjectByType<ServerSingleton>();
            if (instance == null)
            {
                Debug.LogError("No ServerSingleton in the scene!");
                return null;
            }
            return instance;
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void CreateServer()

    {
        GameManager = new ServerGameManager();
    }
    private void OnDestroy()
    {
        GameManager?.Dispose();

    }
}
