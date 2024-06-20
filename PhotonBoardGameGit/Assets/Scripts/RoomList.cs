using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Text;
using System;
using UnityEngine.UI;

public class RoomList : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject RoomPrefab;
    [SerializeField] Transform parentRoom;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    //���������� ���� 
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

    private void Awake()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();
    }
    //����������� � �����
    public override void OnJoinedLobby()
    {
        Debug.Log("���� ������: �� ����� � �����");
        cachedRoomList.Clear();
    }
    //������ ������ 
    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();
    }
    //���������� ������ 
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();

    }
    //����� ������ ������
    void UpdateRoomListView()
    {
        if (cachedRoomList == null)
        {
            Debug.LogError("cachedRoomList is null");
            return;
        }
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            Debug.Log("������� �������: "+ info.Name);
            GameObject Room = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, parentRoom);
            Room.GetComponent<Room>().Name.text = info.Name;
            if(info.PlayerCount == info.MaxPlayers)
            {
                Room.GetComponent<Room>().PlayersCountInfo.text = info.PlayerCount + "/" + info.MaxPlayers + " max";
                Room.GetComponent<Button>().interactable = false;;
            }
            else
            {
                Room.GetComponent<Room>().PlayersCountInfo.text = info.PlayerCount + "/" + info.MaxPlayers;
            }
            roomListEntries.Add(info.Name, Room);
        }
    }

}
