using UnityEngine;
using Unity.Netcode;

public class GameHUD : MonoBehaviour
{
    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }
        ClientSingleton.Instance.GameManager.Disconnect();

    }
}
