using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonConnect : MonoBehaviourPunCallbacks
{
    public MenuUI menuUI;

    // Метод, который вызывается при успешном подключении к мастер-серверу
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
    }
    private void Start()
    {
        StartGame();
        Debug.Log("Никнейм игрока -  " + PhotonNetwork.NickName);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();

    }
    public void StartGame()
    {
        PhotonNetwork.NickName = menuUI.GetName();
    }

}
