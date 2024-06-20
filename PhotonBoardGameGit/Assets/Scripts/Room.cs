using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Room : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI PlayersCountInfo;
    GameObject LBManage;
    public void JoinRoom()
    {
        LBManage = GameObject.Find("Lobby Manager");
        JoinRoomInList(Name.text);
    }
    public void JoinRoomInList(string RoomName)
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRoom(RoomName);
        }
        else
        {
            Debug.LogWarning("Not connected to the Master Server yet. Wait for OnJoinedLobby or OnConnectedToMaster callback.");

        }
    }
}