using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.Events;

public class MultiplayerManager : NetworkBehaviour
{
    private int connectedClients = 0;
    public UnityEvent<int> OnClientCountChange;

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectedCallback;
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("OnNetworkDespawn");
        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectedCallback;
        NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
    }

    private void Singleton_OnServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host started");
        }
    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        connectedClients++;
        Debug.Log($"Client connected: {clientId}. Total clients: {connectedClients}");

        OnClientCountChange.Invoke(connectedClients);
    }

    private void Singleton_OnClientDisconnectedCallback(ulong clientId)
    {
        connectedClients--;
        Debug.Log($"Client disconnected: {clientId}. Total clients: {connectedClients}");

        OnClientCountChange.Invoke(connectedClients);
    }

    public void ChangeToGameScene()
    {
        if (NetworkManager.Singleton.IsHost && connectedClients == 2)
        {
            Debug.Log("LoadScene(Stratego)");
            NetworkManager.Singleton.SceneManager.LoadScene("Stratego", LoadSceneMode.Single);
        }
    }
}
