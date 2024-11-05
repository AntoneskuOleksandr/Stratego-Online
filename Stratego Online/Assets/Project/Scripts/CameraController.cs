using UnityEngine;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    public Transform playerOneCameraPosition;
    public Transform playerTwoCameraPosition;
    private bool isPlayerOneCam;

    public void Initialize()
    {
        SetCameraPosition();
    }

    private void SetCameraPosition()
    {
        if (IsHost)
        {
            Camera.main.transform.position = playerOneCameraPosition.position;
            Camera.main.transform.rotation = playerOneCameraPosition.rotation;
        }
        else
        {
            Camera.main.transform.position = playerTwoCameraPosition.position;
            Camera.main.transform.rotation = playerTwoCameraPosition.rotation;
        }
    }
}
