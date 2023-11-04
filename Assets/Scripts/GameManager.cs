using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    private bool isConnecting = false; // Add this variable

    private void Start()
    {
        // Ensure the GameManager is not destroyed when loading new scenes.
        DontDestroyOnLoad(gameObject);

        // Check if we are already connected or in the process of connecting
        if (PhotonNetwork.NetworkClientState == ClientState.Disconnected && !isConnecting)
        {
            isConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene and clean up network objects.
    /// </summary>
    public override void OnLeftRoom()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        List<PhotonView> viewsToDestroy = new List<PhotonView>();

        foreach (PhotonView view in photonViews)
        {
            // Check if the PhotonView is not the one on GameManager or any other essential object
            if (view.ViewID != 0)
            {
                viewsToDestroy.Add(view);
            }
        }

        foreach (PhotonView view in viewsToDestroy)
        {
            PhotonNetwork.Destroy(view);
        }

        PhotonNetwork.LoadLevel(0);
      
    }




    /// <summary>
    /// Leave the gameScene
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
