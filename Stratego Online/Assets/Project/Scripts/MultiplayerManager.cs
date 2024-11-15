using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MultiplayerManager : NetworkBehaviour
{
    private int connectedClients = 0;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
    }

    private void Singleton_OnServerStarted()
    {
        // Проверка состояния подключения хоста
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host started");
        }
    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        connectedClients++;
        Debug.Log($"Client connected: {clientId}. Total clients: {connectedClients}");

        // Если у нас есть как минимум один клиент и хост, мы загружаем игровую сцену
        if (NetworkManager.Singleton.IsHost && connectedClients > 1)
        {
            Debug.Log("LoadScene(Stratego)");
            NetworkManager.Singleton.SceneManager.LoadScene("Stratego", LoadSceneMode.Single);
        }
    }
}
