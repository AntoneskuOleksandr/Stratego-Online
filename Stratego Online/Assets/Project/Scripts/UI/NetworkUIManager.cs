using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUIManager : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button changeSceneButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        changeSceneButton.onClick.AddListener(() =>
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Stratego", LoadSceneMode.Single);
            }
        });
    }
}
