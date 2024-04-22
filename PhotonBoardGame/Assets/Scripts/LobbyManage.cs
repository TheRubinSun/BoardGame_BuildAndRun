using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManage : MonoBehaviourPunCallbacks
{
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    public TextMeshProUGUI LogText;
    public CreateServer createServerWindow;
    public GameObject windowCreateRoom;
    GameObject window;

    private void Awake()
    {
        Debug.Log("���� ���: "+PhotonNetwork.LocalPlayer.NickName);
    }
    //������� ���� �������� �������
    public void OpenWindowCreateRoom()
    {
        windowCreateRoom.SetActive(true);
        createServerWindow.placeholder.text = "Room " + Random.Range(1000, 9999);
    }
    //������� ���� �������� �������
    public void CloseWindowCreateServer()
    {
        createServerWindow.nameServer.text = "";
        createServerWindow.maxCountPlayer.captionText.text = "4";
        windowCreateRoom.SetActive(false);
    }
    //�������� �������
    public void CreateRoom()
    {
        string nameServer;
        if (createServerWindow.nameServer.text == "") nameServer = createServerWindow.placeholder.text;
        else nameServer = createServerWindow.nameServer.text;
        // �������� ������������ ����� �������
        if (IsRoomNameUnique(nameServer))
        {
            int maxCountPlayer = System.Int32.Parse(createServerWindow.maxCountPlayer.captionText.text);
            bool isVisible = createServerWindow.isVisible.isOn;
            bool isOpen = createServerWindow.isOpen.isOn;
            PhotonNetwork.CreateRoom(nameServer, new Photon.Realtime.RoomOptions { MaxPlayers = (byte)maxCountPlayer, IsVisible = isVisible, IsOpen = isOpen }, TypedLobby.Default, null);
            CloseWindowCreateServer();
        }
        else
        {
            Debug.LogWarning("Room name is not unique. Choose a different name.");
        }

        //int maxCountPlayer = System.Int32.Parse(createServerWindow.maxCountPlayer.captionText.text);
        //bool isVisible = createServerWindow.isVisible.isOn;
        //bool isOpen = createServerWindow.isOpen.isOn;
        //PhotonNetwork.CreateRoom(nameServer, new Photon.Realtime.RoomOptions { MaxPlayers = (byte)maxCountPlayer, IsVisible = isVisible, IsOpen = isOpen }, TypedLobby.Default, null);
        //CloseWindowCreateServer();
    }
    private bool IsRoomNameUnique(string roomName)
    {
        return !cachedRoomList.ContainsKey(roomName);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join random room failed: {message}");
    }
    //������������� � ��������� �������
    public void JoinRoom()
    {
        Debug.Log("Joining a random room");
        PhotonNetwork.JoinRandomRoom();
    }
    //������������ � ���������� �������
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

    //����� ����������� ������� �� ����� "Game"
    public override void OnJoinedRoom()
    {
        Debug.Log("���������� �����: �� ����� � �������");
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.LoadLevel("Game");
    }
    //������� ��������� � ������� �������
    void Log(string message)
    {
        Debug.Log(message);
        LogText.text += "\n";
        LogText.text += message;
    }
    //���� �� ����� � �����
    public override void OnJoinedLobby()
    {
        // ������ ������������� � �����
        // ������ �� ������ �������������� � �������
        Debug.Log("���������� �����: �� ����� � �����");
        cachedRoomList.Clear();
    }
    //���� �� ����� �� �����
    public override void OnLeftLobby()
    {
        Debug.Log("�� ����� �� �����");
        cachedRoomList.Clear();
    }
    //�������� ��� ��� ���������� 
    public override void OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
    }
    //�������� ������ ������
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
    //�������� ��� 
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
    }
}
