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
    //Мосты Лесопилки ... ... ... ... ... ... ...
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
        // Инстанциируем игрового игрока
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
    /////////////////////Консоль событий///////////////////////////////////////////////////
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
    /////////////////////Лобби///////////////////////////////////////////////////////////////
    //Выйти из сервера
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }
    //Выход в лобби для тeкущего игрока
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    //Если какой-то игрок зашел
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("Игрок: {0} теперь с вами", newPlayer.NickName);
        Log($"Player {newPlayer.NickName} теперь с вами");
        Log("Теперь игроков на сервере: " + PhotonNetwork.PlayerList.Length);
        readyPlayers[newPlayer.NickName] = false;

        Invoke("UpdateName", 1f);
    }
    //Если какой игрок вышел
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateDirectoryReadyPlayers();
        Debug.LogFormat("Игрок {0} вышел", otherPlayer.NickName);
        Log($"Игрок: {otherPlayer.NickName} вышел");
        Log("Теперь игроков на сервере:" + PhotonNetwork.PlayerList.Length);
        Invoke("UpdateName", 1f);
    }
    void UpdateName()
    {
        interfaceControl.UpdateAllNames();
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////Начало игры///////////////////////////////////////////////////
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
        //Загрузим информацию
        LoadPlayerInfo();
        MapControl.Instance.SendTextNameCities();
        startGameAlready = true;
        PlayerControl tempPlayerControl = thisPlayer.GetComponent<PlayerControl>();

        //Назначить начальную точку и параметры игроку.
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
                //Определяем кто начинает ходить
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
        //Начать игру игроку.
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
        takenInfo?.Invoke($"Циклов шагов: {countSteps}");
    }
    public void GetCurrentPlayerTurn()
    {
        takenInfo?.Invoke($"Очередь игрока: {currentPlayerIndex}");
    }
    public void GetCountReadyPl()
    {
        takenInfo?.Invoke($"Готовых игроков: {countReadyPl}/{PhotonNetwork.PlayerList.Length}");
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////Дейсвия с всеми игрокам///////////////////////////////////////////////////
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
                Log($"Вы не нашли игрока: {tempName}");
            }
        }
    }

    //Обозначить переменые в словаря листа для всех игроков
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
    //Обновлять владения игроков.
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
            Log($"У игрока: {players[i].name} Мостов: {possessionsPlayer[i]["Bridges"]} Лесопилок: {possessionsPlayer[i]["Sawmills"]} Буров:  {possessionsPlayer[i]["Drills"]}");
        }
        
    }
    //Дать текущему игроку заработанные ресурсы
    void GiveResource(int currentPlayerIndex)
    {
        //Дать дерево за лесопилки
        int tempValue;

        if(possessionsPlayer[currentPlayerIndex]["Sawmills"]>0)
        {
            tempValue = possessionsPlayer[currentPlayerIndex]["Sawmills"] * 5;
            players[currentPlayerIndex].GetComponent<PlayerControl>().wood += tempValue; ;
            Log($"Игрок: {players[currentPlayerIndex].name} получает {tempValue} древесины");
        }

        if (possessionsPlayer[currentPlayerIndex]["Drills"] > 0)
        {
            tempValue = possessionsPlayer[currentPlayerIndex]["Drills"] * 2;
            players[currentPlayerIndex].GetComponent<PlayerControl>().ore += tempValue;
            Log($"Игрок: {players[currentPlayerIndex].name} получает {tempValue} руды");
        }

    }
    //Смещение (пока не работает)
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
        // Вычисляем количество объектов, которые нужно создать
        int countBuiltPoint = (int)System.Math.Floor(distanceBetweenPoints / range);
        // Вычисляем шаг между объектами
        float step = 1f / countBuiltPoint;
        GameObject tempObj;
        GameObject[] Bridge = new GameObject[countBuiltPoint - 1];//(Создаем мост)
        for (int i = 1; i < countBuiltPoint; i++)
        {
            Vector3 spawnPosition = Vector3.Lerp(pointStart, pointEnd, i * step);
            tempObj = Instantiate(pointPrefab, spawnPosition, Quaternion.identity);
            tempObj.transform.SetParent(parentFromPoint.transform);
            tempObj.name = $"Point: ({(i - 1)}) |Bridge: ({MapControl.Instance.BridgesPoints.Count})";
            tempObj.GetComponent<PointSettings>().typePoint = TypePoint.Bridge;
            Bridge[i - 1] = tempObj;//(Создаем мост точек)
        }
        Bridge newBridge = new Bridge(IDStartPoint, IDEndPoint, Bridge, ownerName, typeBuild);

        MapControl.Instance.BridgesPoints.Add(newBridge);//Добавляем мост в список мостов
        MapControl.Instance.BuildsList.Add(newBridge);//Добавляем мост в список построек
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
        //Формула найти = шанс игрока + неудачные попытки * 10
        int chanceFind = plCon.chanceFindGold + plCon.streakNotFound * 10;
        Log($"{plCon.name} ищет сокровище. Шанс составил: {chanceFind} Стрик неудач: {plCon.streakNotFound}");
        if (i <= chanceFind)
        {
            plCon.streakNotFound = 0;
            FindTreasure(holeObj);
        }
        else
        {
            DisplayInfoMessage("Вы ничего не нашли ", 1f);
            plCon.streakNotFound++;
        }
    }
    private void FindTreasure(GameObject holeObj)
    {
        holeObj.transform.GetChild(0).gameObject.SetActive(true);
        int gold = Random.Range(20, 100);
        players[currentPlayerIndex].GetComponentInChildren<PlayerControl>().gold += gold;
        DisplayInfoMessage($"Вы нашли: {gold} золота", 1f, new Color32(255, 255, 0, 255));
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
            // Получаем PhotonView объекта игрока
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
    //Обычная точка
    [PunRPC]
    private void RPC_OffsetPlayer(int playerViewID, int cellPoint, bool removeOrAdd)
    {
        // Получаем игровой объект по идентификатору PhotonView
        GameObject player = PhotonView.Find(playerViewID)?.gameObject;
        // Получаем PointSettings
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
    //Точка на мосту
    [PunRPC]
    private void RPC_OffsetBridgePlayer(int playerViewID, int numbBr, int cellPoint, bool removeOrAdd)
    {
        // Получаем игровой объект по идентификатору PhotonView
        GameObject player = PhotonView.Find(playerViewID)?.gameObject;
        // Получаем PointSettings
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
    /////////////////////Пошаговая система///////////////////////////////////////////////////
    private int currentPlayerIndex = 0;
    public int countSteps = 0;
    public bool startGame = false;
    public void EndTurn()
    {
        
        // Логика завершения хода
        //на конкретном игроке
        if (PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        {
            // Логика для текущего локального игрока
            
            interfaceControl.SetOnEndTurnButtonClickActive(false);
            this.GetComponent<MapControl>().HideAllPoints();
        }
        photonView.RPC("RPC_EndTurn", RpcTarget.All);
    }
    [PunRPC]
    private void RPC_EndTurn()
    {
        players[currentPlayerIndex].GetComponent<PlayerControl>().prefabRing.SetActive(false);
        // Логика завершения хода
        currentPlayerIndex = (currentPlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
        //Логика тольког на сервере
        if (currentPlayerIndex == 0) countSteps++;
        // Дополнительные действия, если необходимо

        //Обновить владения игроков
        UpdatePossessionPlayer();

        // Начать ход следующего игрока
        StartNextTurn();
    }
    private void StartNextTurn()
    {
        //StartPlayerTurn();
        // Дополнительные действия при начале хода, если необходимо
        //на конкретном игроке
        players[currentPlayerIndex].GetComponent<PlayerControl>().prefabRing.SetActive(true);
        if (PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        {
            // Логика для текущего локального игрока
            GiveResource(currentPlayerIndex);

            // Включаем кнопку перед следующим ходом
            interfaceControl.SetOnEndTurnButtonClickActive(true);
            Log("Твоя очередь ходить!");



            //Ход игрока
            thisPlayer.GetComponent<PlayerControl>().UpdateCountSteps();
            this.GetComponent<MapControl>().ShowPoints();
        }

    }



    //////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////Готовность//////////////////////////////////////////////
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
            Debug.Log("Стартуем ");
            Invoke("StartGame", 1f);
        }
        else
        {
            Debug.Log("Не готовы");
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
            Debug.Log("Готовых: " + countReadyPl + " |Всего: " + PhotonNetwork.PlayerList.Length);
            return true;
        }
        else
        {
            Debug.Log("Готовых: " + countReadyPl + " |Всего: " + PhotonNetwork.PlayerList.Length);
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
        
        ////Логика тольког на сервере
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    //Но на конкретном игроке
        //    if (PhotonNetwork.PlayerList[currentPlayerIndex].IsLocal)
        //    {
        //        // Логика для текущего локального игрока
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