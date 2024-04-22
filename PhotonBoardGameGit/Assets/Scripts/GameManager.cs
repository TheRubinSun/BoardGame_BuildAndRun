using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public GameObject InterfacePrefab;
    public GameObject SawmillPrefab;
    public GameObject DrillPrefab;
    public GameObject HolePrefab;
    public GameObject pointPrefab;
    public Transform parentFromPoint;
    public Transform parentFromMessage;

    public UIController interfaceControl;
    public static GameManager instance;
    public Dictionary<string, bool> readyPlayers = new Dictionary<string, bool>();
    public bool startGameAlready = false;

    public List<GameObject> players = new List<GameObject>(8);
    //����� ��������� ... ... ... ... ... ... ...
    public List<Dictionary<string,int>> possessionsPlayer = new List<Dictionary<string,int>>(8);

    GameObject thisPlayer;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }
    void Awake()
    {
        Vector3 pos = new Vector3(UnityEngine.Random.Range(-45, -31), UnityEngine.Random.Range(4f, -10f));
        // ������������� �������� ������
        thisPlayer = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, Quaternion.identity);
        

        interfaceControl = thisPlayer.GetComponent<PlayerControl>().AssignInterface(InterfacePrefab).GetComponent<UIController>();
        MapControl.Instance.GetUIControl(interfaceControl);

        thisPlayer.name = PhotonNetwork.NickName;
        UpdateDirectoryReadyPlayers();
    }
    public UIController GetInterface()
    {
        return interfaceControl;
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////������� �������///////////////////////////////////////////////////
    public void Log(string message)
    {
        Debug.Log(message);
        interfaceControl.WriteLog(message);
    }
    public void DisplayInfoMessage(string message, float timeLeng)
    {
        interfaceControl.DisaplyTable(message, timeLeng);
    }
    public void DisplayInfoMessage(string message, float timeLeng, Color32 colorText)
    {
        interfaceControl.DisaplyTable(message, timeLeng, colorText);
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////�����///////////////////////////////////////////////////////////////
    //����� �� �������
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }
    //����� � ����� ��� �e������ ������
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    //���� �����-�� ����� �����
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("�����: {0} ������ � ����", newPlayer.NickName);
        Log($"Player {newPlayer.NickName} ������ � ����");
        Log("������ ������� �� �������: " + PhotonNetwork.PlayerList.Length);
        readyPlayers[newPlayer.NickName] = false;

        Invoke("UpdateName", 1f);
    }
    //���� ����� ����� �����
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateDirectoryReadyPlayers();
        Debug.LogFormat("����� {0} �����", otherPlayer.NickName);
        Log($"�����: {otherPlayer.NickName} �����");
        Log("������ ������� �� �������:" + PhotonNetwork.PlayerList.Length);
        Invoke("UpdateName", 1f);
    }
    void UpdateName()
    {
        interfaceControl.UpdateAllNames();
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////������ ����///////////////////////////////////////////////////
    public void SetColor(PlayerControl playerContrl)
    {
        ColorsTeam colorT = playerContrl.colorTeam;
        switch (colorT)
        {
            case ColorsTeam.Yellow:
                {
                    playerContrl.prefabRing.GetComponent<SpriteRenderer>().sprite = playerContrl.ringYellow;
                    return;
                }
            case ColorsTeam.Red:
                {
                    playerContrl.prefabRing.GetComponent<SpriteRenderer>().sprite = playerContrl.ringRed;
                    return;
                }
            case ColorsTeam.Green:
                {
                    playerContrl.prefabRing.GetComponent<SpriteRenderer>().sprite = playerContrl.ringGreen;
                    return;
                }
            case ColorsTeam.Blue:
                {
                    playerContrl.prefabRing.GetComponent<SpriteRenderer>().sprite = playerContrl.ringBlue;
                    return;
                }
            default:
                {
                    playerContrl.prefabRing.GetComponent<SpriteRenderer>().sprite = playerContrl.ringPurple;
                    return;
                }
        }

    }
    public bool GetStartGame()
    {
        return startGameAlready;
    }
    public void StartGame()
    {
        //�������� ����������
        LoadPlayerInfo();
        MapControl.Instance.SendTextNameCities();
        startGameAlready = true;
        PlayerControl tempPlayerControl = thisPlayer.GetComponent<PlayerControl>();

        //��������� ��������� ����� � ��������� ������.
        int maxCountSteps = 1;
        int lengthStep = 2;
        int cellPosition = GetStartPosition();
        int gold = 400;
        int wood = 50;
        int ore = 8;
        int chance = 10;

        tempPlayerControl.SetStartParamToPlayer(maxCountSteps, lengthStep, cellPosition, gold, wood, ore, chance);

        this.GetComponent<MapControl>().LoadCurrentPlayerInfo();
        for (int i = 0; i < players.Count; i++)
        {
            if(i<System.Enum.GetValues(typeof(ColorsTeam)).Length)
            {
                this.GetComponent<MapControl>().StartGameForPlayer(GetStartPosition(), players[i]);
                players[i].GetComponent<PlayerControl>().colorTeam = (ColorsTeam)i;
                SetColor(players[i].GetComponent<PlayerControl>());
            }
            if(PhotonNetwork.IsMasterClient)
            {
                //���������� ��� �������� ������
                if (i == currentPlayerIndex)
                {
                    SetEndStep(PhotonNetwork.PlayerList[i].NickName, true);
                }
                else SetEndStep(PhotonNetwork.PlayerList[i].NickName, false);
            }
            if(i == currentPlayerIndex && PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
            {
                this.GetComponent<MapControl>().ShowPoints();
                //players[i].GetComponent<PlayerControl>().prefabRing.SetActive(true);
            }
            else if(!PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
            {
                this.GetComponent<MapControl>().HideAllPoints();
            }
        }
        players[currentPlayerIndex].GetComponent<PlayerControl>().prefabRing.SetActive(true);
        //������ ���� ������.
        StartPossessionPlayer();

        tempPlayerControl.StartBoardGame();


        LoadPointPlayer(players);

    }
    GetInfoDeleg takenInfo;
    public void Handler(GetInfoDeleg del)
    {
        takenInfo = del;
    }

    public string GetCurrentPlayerString()
    {
        return players[currentPlayerIndex].name;
    }
    public void GetCoutSteps()
    {
        takenInfo?.Invoke($"������ �����: {countSteps}");
    }
    public void GetCurrentPlayerTurn()
    {
        takenInfo?.Invoke($"������� ������: {currentPlayerIndex}");
    }
    public void GetCountReadyPl()
    {
        takenInfo?.Invoke($"������� �������: {countReadyPl}/{PhotonNetwork.PlayerList.Length}");
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////������� � ����� �������///////////////////////////////////////////////////
    public void LoadPlayerInfo()
    {
        players.Clear();
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
        Dictionary<string, GameObject> players_d = new Dictionary<string, GameObject>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            players_d[PhotonNetwork.PlayerList[i].NickName] = gameObjects[i];
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            string tempName = PhotonNetwork.PlayerList[i].NickName;
            if (GameObject.Find(tempName))
            {
                players.Add(GameObject.Find(tempName));
            }
            else
            {
                Log($"�� �� ����� ������: {tempName}");
            }
        }
    }

    //���������� ��������� � ������� ����� ��� ���� �������
    void StartPossessionPlayer()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Dictionary<string, int> tempPoss = new Dictionary<string, int>();
            tempPoss.Add("Bridges", 0);
            tempPoss.Add("Sawmills", 0);
            tempPoss.Add("Drills", 0);
            tempPoss.Add("Holes", 0);
            possessionsPlayer.Add(tempPoss);
        }
    }
    //��������� �������� �������.
    void UpdatePossessionPlayer()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            possessionsPlayer[i]["Bridges"] = 0;
            possessionsPlayer[i]["Sawmills"] = 0;
            possessionsPlayer[i]["Drills"] = 0;
            possessionsPlayer[i]["Holes"] = 0;

            for (int build = 0; build < MapControl.Instance.BuildsList.Count; build++)
            {
                if (players[i].name == MapControl.Instance.BuildsList[build].ownerName)
                {
                    if (MapControl.Instance.BuildsList[build].typeBuild == Builds.Bridge)
                        possessionsPlayer[i]["Bridges"]++;
                    if (MapControl.Instance.BuildsList[build].typeBuild == Builds.Sawmill)
                        possessionsPlayer[i]["Sawmills"]++;
                    if (MapControl.Instance.BuildsList[build].typeBuild == Builds.Drill)
                        possessionsPlayer[i]["Drills"]++;
                    if (MapControl.Instance.BuildsList[build].typeBuild == Builds.Hole)
                        possessionsPlayer[i]["Holes"]++;
                }
            }
            Log($"� ������: {players[i].name} ������: {possessionsPlayer[i]["Bridges"]} ���������: {possessionsPlayer[i]["Sawmills"]} �����:  {possessionsPlayer[i]["Drills"]}");
        }
        
    }
    //���� �������� ������ ������������ �������
    void GiveResource(int currentPlayerIndex)
    {
        //���� ������ �� ���������
        int tempValue;

        if(possessionsPlayer[currentPlayerIndex]["Sawmills"]>0)
        {
            tempValue = possessionsPlayer[currentPlayerIndex]["Sawmills"] * 5;
            players[currentPlayerIndex].GetComponent<PlayerControl>().wood += tempValue; ;
            Log($"�����: {players[currentPlayerIndex].name} �������� {tempValue} ���������");
        }

        if (possessionsPlayer[currentPlayerIndex]["Drills"] > 0)
        {
            tempValue = possessionsPlayer[currentPlayerIndex]["Drills"] * 2;
            players[currentPlayerIndex].GetComponent<PlayerControl>().ore += tempValue;
            Log($"�����: {players[currentPlayerIndex].name} �������� {tempValue} ����");
        }

    }
    //�������� (���� �� ��������)
    void CheckTheSamePos()
    {
        foreach(GameObject player in players)
        {
            foreach (GameObject playerAntoher in players)
            {
                if (player == playerAntoher) continue;
                if(player.GetComponent<PlayerControl>().cellPosition == playerAntoher.GetComponent<PlayerControl>().cellPosition)
                {
                    MovePlayersOffset(player, playerAntoher);
                }
            }
        }
    }
    void MovePlayersOffset(GameObject player, GameObject playerAntoher)
    {
        float offset = 0.2f;
        player.transform.position = new Vector3(player.transform.position.x + offset, player.transform.position.y + offset, player.transform.position.z);
        playerAntoher.transform.position = new Vector3(playerAntoher.transform.position.x - offset, playerAntoher.transform.position.y - offset, playerAntoher.transform.position.z);
    }


    public void SentCreateBuild(Vector3 pointStart, Vector3 pointEnd, int IDStartPoint, int IDEndPoint, float range, string ownerName, Builds typeBuild)
    {
        photonView.RPC("RPC_CreateBridge", RpcTarget.All, pointStart, pointEnd, IDStartPoint, IDEndPoint, range, ownerName, typeBuild);
    }
    public void SentCreateBuild(int pointBuild, string ownerName, ColorsTeam colorName, Builds typeBuild)
    {
        photonView.RPC("RPC_CreateBuild", RpcTarget.All, pointBuild, ownerName, colorName, typeBuild);
    }
    public void SentCreateHole(int pointBuild, string ownerName, Builds typeBuild)
    {
        photonView.RPC("RPC_CreateHole", RpcTarget.All, pointBuild, ownerName, typeBuild);
    }
    [PunRPC]
    private void RPC_CreateBridge(Vector3 pointStart, Vector3 pointEnd, int IDStartPoint, int IDEndPoint, float range, string ownerName, Builds typeBuild)
    {
        float distanceBetweenPoints = Vector3.Distance(pointStart, pointEnd);
        // ��������� ���������� ��������, ������� ����� �������
        int countBuiltPoint = (int)System.Math.Floor(distanceBetweenPoints / range);
        // ��������� ��� ����� ���������
        float step = 1f / countBuiltPoint;
        GameObject tempObj;
        GameObject[] Bridge = new GameObject[countBuiltPoint - 1];//(������� ����)
        for (int i = 1; i < countBuiltPoint; i++)
        {
            Vector3 spawnPosition = Vector3.Lerp(pointStart, pointEnd, i * step);
            tempObj = Instantiate(pointPrefab, spawnPosition, Quaternion.identity);
            tempObj.transform.SetParent(parentFromPoint.transform);
            tempObj.name = $"Point: ({(i - 1)}) |Bridge: ({MapControl.Instance.BridgesPoints.Count})";
            tempObj.GetComponent<PointSettings>().typePoint = TypePoint.Bridge;
            Bridge[i - 1] = tempObj;//(������� ���� �����)
        }
        Bridge newBridge = new Bridge(IDStartPoint, IDEndPoint, Bridge, ownerName, typeBuild);

        MapControl.Instance.BridgesPoints.Add(newBridge);//��������� ���� � ������ ������
        MapControl.Instance.BuildsList.Add(newBridge);//��������� ���� � ������ ��������
        MapControl.Instance.FindPoint(IDEndPoint).GetComponent<PointSettings>().owner = ownerName;
        MapControl.Instance.HideAllPoints();
    }
    [PunRPC]
    private void RPC_CreateBuild(int pointBuild, string ownerName, ColorsTeam colorName, Builds typeBuild)
    {
        Color32 colorBuild = GetColor(colorName);
        GameObject buildPrefab;
        switch (typeBuild)
        {
            case Builds.Sawmill:
                {
                    buildPrefab = SawmillPrefab;
                    break;
                }
            case Builds.Drill:
                {
                    buildPrefab = DrillPrefab;
                    break;
                }
            default:
                {
                    buildPrefab = null;
                    break;
                }
        }
        GameObject build = Instantiate(buildPrefab, MapControl.Instance.FindPoint(pointBuild).transform);

        for (int i = 0; i < build.transform.childCount; i++)
        {
            if (build.transform.GetChild(i).name == "Flag")
            {
                build.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colorBuild;
            }
        }
        MapControl.Instance.FindPoint(pointBuild).GetComponent<PointSettings>().owner = ownerName;
        MapControl.Instance.BuildsList.Add(new Build(ownerName, build, typeBuild, colorBuild));
    }
    [PunRPC]
    private void RPC_CreateHole(int pointHole, string ownerName, Builds typeBuild)
    {
        GameObject holeObj = Instantiate(HolePrefab, MapControl.Instance.FindPoint(pointHole).transform);
        int randomZ = Random.Range(0, 360);
        holeObj.transform.GetChild(1).Rotate(0, 0, randomZ);
        MapControl.Instance.FindPoint(pointHole).GetComponent<PointSettings>().owner = ownerName;
        MapControl.Instance.HolesList.Add(new Hole(holeObj, ownerName, typeBuild));

        if(PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        {
            SearchForTreasure(holeObj);
        }

    }
    private void SearchForTreasure(GameObject holeObj)
    {
        int i = Random.Range(0, 100);
        PlayerControl plCon = players[currentPlayerIndex].GetComponentInChildren<PlayerControl>();
        //������� ����� = ���� ������ + ��������� ������� * 10
        int chanceFind = plCon.chanceFindGold + plCon.streakNotFound * 10;
        Log($"{plCon.name} ���� ���������. ���� ��������: {chanceFind} ����� ������: {plCon.streakNotFound}");
        if (i <= chanceFind)
        {
            plCon.streakNotFound = 0;
            FindTreasure(holeObj);
        }
        else
        {
            DisplayInfoMessage("�� ������ �� ����� ", 1f);
            plCon.streakNotFound++;
        }
    }
    private void FindTreasure(GameObject holeObj)
    {
        holeObj.transform.GetChild(0).gameObject.SetActive(true);
        int gold = Random.Range(20, 100);
        players[currentPlayerIndex].GetComponentInChildren<PlayerControl>().gold += gold;
        DisplayInfoMessage($"�� �����: {gold} ������", 1f, new Color32(255, 255, 0, 255));
    }

    protected Color32 GetColor(ColorsTeam colorName)
    {
        switch (colorName)
        {
            case ColorsTeam.Blue:
                {
                    return new Color32(0, 0, 255, 255);
                }
            case ColorsTeam.Green:
                {
                    return new Color32(0, 255, 0, 255);
                }
            case ColorsTeam.Red:
                {
                    return new Color32(255, 0, 0, 255);
                }
            case ColorsTeam.Yellow:
                {
                    return new Color32(255, 255, 0, 255);
                }
            case ColorsTeam.Purple:
                {
                    return new Color32(225, 1, 255, 255);
                }
            case ColorsTeam.Orange:
                {
                    return new Color32(255, 120, 0, 255);
                }
            default:
                {
                    return new Color32(255, 255, 255, 255);
                }
        }
    }



    void LoadPointPlayer(List<GameObject> players)
    {
        foreach (GameObject player in players)
        {
            // �������� PhotonView ������� ������
            PhotonView playerPhotonView = player.GetComponent<PhotonView>();
            RunOffsetGloabal(playerPhotonView.ViewID, player.GetComponent<PlayerControl>().cellPosition, true);
        }
    }
    public void RunOffsetGloabal(int playerViewID, int cellPoint, bool removeOrAdd)
    {
        photonView.RPC("RPC_OffsetPlayer", RpcTarget.All, playerViewID, cellPoint, removeOrAdd);
    }
    public void RunOffsetGloabal(int playerViewID, int numbBr, int cellPoint, bool removeOrAdd)
    {
        photonView.RPC("RPC_OffsetBridgePlayer", RpcTarget.All, playerViewID, numbBr, cellPoint, removeOrAdd);
    }
    //������� �����
    [PunRPC]
    private void RPC_OffsetPlayer(int playerViewID, int cellPoint, bool removeOrAdd)
    {
        // �������� ������� ������ �� �������������� PhotonView
        GameObject player = PhotonView.Find(playerViewID)?.gameObject;
        // �������� PointSettings
        PointSettings point = MapControl.Instance.FindPoint(cellPoint)?.GetComponent<PointSettings>();
        if (point == null && player)
        {
            return;
        }

        if (removeOrAdd)
        {
            if (!point.playersThisPoint.Contains(player))
            {
                point.playersThisPoint.Add(player);
            }
        }
        else
        {
            point.playersThisPoint.Remove(player);
        }
        point.OffsetMovePlayers();

    }
    //����� �� �����
    [PunRPC]
    private void RPC_OffsetBridgePlayer(int playerViewID, int numbBr, int cellPoint, bool removeOrAdd)
    {
        // �������� ������� ������ �� �������������� PhotonView
        GameObject player = PhotonView.Find(playerViewID)?.gameObject;
        // �������� PointSettings
        PointSettings point = MapControl.Instance.FindPointInBridge(numbBr,cellPoint)?.GetComponent<PointSettings>();
        if (point == null && player)
        {
            return;
        }

        if (removeOrAdd)
        {
            if (!point.playersThisPoint.Contains(player))
            {
                point.playersThisPoint.Add(player);
            }
        }
        else
        {
            point.playersThisPoint.Remove(player);
        }
        point.OffsetMovePlayers();

    }
    //public void UpdateLazer()
    //{
    //    photonView.RPC("RPC_UpdateLazer", RpcTarget.All);
    //}
    //[PunRPC]
    //private void RPC_UpdateLazer()
    //{
    //    foreach(GameObject player in players)
    //    {
    //        player.GetComponent<PlayerControl>().DrawLineToPoint();
    //    }

    //}

    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////��������� �������///////////////////////////////////////////////////
    private int currentPlayerIndex = 0;
    public int countSteps = 0;
    public bool startGame = false;
    public void EndTurn()
    {
        
        // ������ ���������� ����
        //�� ���������� ������
        if (PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        {
            // ������ ��� �������� ���������� ������
            
            interfaceControl.SetOnEndTurnButtonClickActive(false);
            this.GetComponent<MapControl>().HideAllPoints();
        }
        photonView.RPC("RPC_EndTurn", RpcTarget.All);
    }
    [PunRPC]
    private void RPC_EndTurn()
    {
        players[currentPlayerIndex].GetComponent<PlayerControl>().prefabRing.SetActive(false);
        // ������ ���������� ����
        currentPlayerIndex = (currentPlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
        //������ ������� �� �������
        if (currentPlayerIndex == 0) countSteps++;
        // �������������� ��������, ���� ����������

        //�������� �������� �������
        UpdatePossessionPlayer();

        // ������ ��� ���������� ������
        StartNextTurn();
    }
    private void StartNextTurn()
    {
        //StartPlayerTurn();
        // �������������� �������� ��� ������ ����, ���� ����������
        //�� ���������� ������
        players[currentPlayerIndex].GetComponent<PlayerControl>().prefabRing.SetActive(true);
        if (PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        {
            // ������ ��� �������� ���������� ������
            GiveResource(currentPlayerIndex);

            // �������� ������ ����� ��������� �����
            interfaceControl.SetOnEndTurnButtonClickActive(true);
            Log("���� ������� ������!");



            //��� ������
            thisPlayer.GetComponent<PlayerControl>().UpdateCountSteps();
            this.GetComponent<MapControl>().ShowPoints();
        }

    }



    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////����������//////////////////////////////////////////////
    void SetEndStep(string namePlayer, bool setTurn)
    {

        thisPlayer.GetComponent<PlayerControl>().UpdateCountSteps();
        GameObject playerStartTurn = GameObject.Find(namePlayer);
        playerStartTurn.GetComponent<PlayerControl>().SetReady(setTurn);
    }
    private bool SetPlayerReadyState(string name)
    {
        return readyPlayers[name];
    }
    void UpdateDirectoryReadyPlayers()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            readyPlayers[PhotonNetwork.PlayerList[i].NickName] = false;
        }
    }
    public bool GetReadyPlayer(string nameP)
    {
        foreach (KeyValuePair<string, bool> nameItem in readyPlayers)
        {
            if (nameItem.Key == nameP) return nameItem.Value;
        }
        return false;
    }
    public void SentReady(string name, bool isReady)
    {
        photonView.RPC("RPC_PlayerReadyStateChanged", RpcTarget.All, name, isReady);
    }
    [PunRPC]
    private void RPC_PlayerReadyStateChanged(string playerName, bool isReady)
    {
        //readyPlayers.Add(playerName, isReady);
        readyPlayers[playerName] = isReady;
        foreach (KeyValuePair<string, bool> name in readyPlayers)
        {
            Debug.Log(name.Key + " " + name.Value);
        }
        if (CheckReadyPlayers())
        {
            Debug.Log("�������� ");
            Invoke("StartGame", 1f);
        }
        else
        {
            Debug.Log("�� ������");
        }
    }
    int countReadyPl = 0;
    public bool CheckReadyPlayers()
    {
        countReadyPl = 0;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (SetPlayerReadyState(PhotonNetwork.PlayerList[i].NickName)) countReadyPl++;
        }
        if (countReadyPl == PhotonNetwork.PlayerList.Length)
        {
            Debug.Log("�������: " + countReadyPl + " |�����: " + PhotonNetwork.PlayerList.Length);
            return true;
        }
        else
        {
            Debug.Log("�������: " + countReadyPl + " |�����: " + PhotonNetwork.PlayerList.Length);
            return false;
        }

    }
    int GetStartPosition()
    {
        int startPos = 1;
        return startPos;
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        
        ////������ ������� �� �������
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    //�� �� ���������� ������
        //    if (PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        //    {
        //        // ������ ��� �������� ���������� ������
        //        //interfaceControl.SetOnEndTurnButtonClickActive(true);
        //    }
        //}
    }
}
public enum ColorsTeam
{
    Yellow,
    Red,
    Green,
    Blue,
    Purple,
    Orange
}