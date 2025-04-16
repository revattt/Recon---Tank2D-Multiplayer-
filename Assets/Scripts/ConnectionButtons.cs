using UnityEngine;
using Unity.Netcode;

public class ConnectionButtons : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
