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
        Debug.Log("Ваше имя: "+PhotonNetwork.LocalPlayer.NickName);
    }
    //Открыть окно создания сервера
    public void OpenWindowCreateRoom()
    {
        windowCreateRoom.SetActive(true);
        createServerWindow.placeholder.text = "Room " + Random.Range(1000, 9999);
    }
    //Закрыть окно создания сервера
    public void CloseWindowCreateServer()
    {
        createServerWindow.nameServer.text = "";
        createServerWindow.maxCountPlayer.captionText.text = "4";
        windowCreateRoom.SetActive(false);
    }
    //Создание сервера
    public void CreateRoom()
    {
        string nameServer;
        if (createServerWindow.nameServer.text == "") nameServer = createServerWindow.placeholder.text;
        else nameServer = createServerWindow.nameServer.text;
        // Проверка уникальности имени комнаты
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
    //Поодключиться к случайной комнате
    public void JoinRoom()
    {
        Debug.Log("Joining a random room");
        PhotonNetwork.JoinRandomRoom();
    }
    //Подключиться к выбранному серверу
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

    //После подключения перейти на сцену "Game"
    public override void OnJoinedRoom()
    {
        Debug.Log("Управление Лобби: Вы вошли в комнату");
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.LoadLevel("Game");
    }
    //Вывести сообщение в консоль событий
    void Log(string message)
    {
        Debug.Log(message);
        LogText.text += "\n";
        LogText.text += message;
    }
    //Если вы вошли в лобби
    public override void OnJoinedLobby()
    {
        // Клиент присоединился к лобби
        // Теперь вы можете присоединиться к комнате
        Debug.Log("Управление Лобби: Вы вошли в лобби");
        cachedRoomList.Clear();
    }
    //Если вы вышли из лобби
    public override void OnLeftLobby()
    {
        Debug.Log("Вы вышли из лобби");
        cachedRoomList.Clear();
    }
    //Очистить кеш при отключении 
    public override void OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
    }
    //Обновить список комнат
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
    //Обновить кэш 
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
