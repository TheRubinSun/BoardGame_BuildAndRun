using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;
using TMPro;
using System.Drawing;
using System.Xml.Serialization;
using Photon.Realtime;

public class MapControl : MonoBehaviour
{
    public GameObject LastPoint;
    public int mapPoints = 80;

    public List<Bridge> BridgesPoints = new List<Bridge>();//Лист мостов (масивов из точек)
    public List<Build> BuildsList = new List<Build>();//Лист Построек 
    public List<Hole> HolesList = new List<Hole>();//Лист Построек 
    PhotonView photonView;


    GameObject player;
    PlayerControl playerCon;
    List<GameObject> points = new List<GameObject>();

    [SerializeField] GameObject ringCi;
    [SerializeField] TypePoint pointSelectToRingCi;
    [SerializeField] Color32 colorRingCi;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TypePoint pointSelectName;
    [SerializeField] Color32 colorNameText;

    UIController interfaceControl;
    // Start is called before the first frame update
    private static MapControl instanceMapControl;
    public static MapControl Instance
    {
        get
        {
            if (instanceMapControl == null)
            {
                instanceMapControl = FindObjectOfType<MapControl>();
            }
            return instanceMapControl;
        }
    }
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        Debug.Log($"ВОООТ: {GetLastPoints()} {GetLastPoints().GetType()}");

        mapPoints = GetLastPoints();
        LoadAllPoints();

        AddRingAroundPoint(TypePoint.Village, ringCi, colorRingCi);
        AddNameCity(TypePoint.Village, nameText, colorNameText);
    }
    int GetLastPoints()
    {
        return GetIntPoint(LastPoint.name)+1;
    }
    public void GetUIControl(UIController UICon)
    {
        interfaceControl = UICon;
    }
    public void LoadCurrentPlayerInfo()
    {
        foreach (GameObject item in GameManager.Instance.players)
        {
            if (item.name == PhotonNetwork.NickName)
            {
                player = item;
            }
        }
        playerCon = player.GetComponent<PlayerControl>();
    }
    void AddRingAroundPoint(TypePoint typePoint, GameObject prefabRing, Color32 setColor)
    {
        GameObject tempObj;
        foreach(GameObject point in points)
        {
            if(point.GetComponent<PointSettings>().typePoint == typePoint)
            {
                tempObj = Instantiate(prefabRing, point.transform);
                tempObj.GetComponent<SpriteRenderer>().color = setColor;
            }
        }
    }
    void AddNameCity(TypePoint typePoint, TextMeshProUGUI namePref, Color32 setColor)
    {
        CitiesList citiesList = this.gameObject.GetComponent<SetNames>().LoadData();
        int startName = UnityEngine.Random.Range(1, 800);
        TextMeshProUGUI tempObj;
        for(int i = 0;i<points.Count;i++)
        {
            if (citiesList != null)
            {
                if (points[i].GetComponent<PointSettings>().typePoint == typePoint)
                {
                    tempObj = Instantiate(namePref, points[i].transform.GetChild(2));
                    tempObj.color = setColor;
                    tempObj.text = citiesList.citiesNamesList[startName + i];
                }
            }
            else
            {
                if (points[i].GetComponent<PointSettings>().typePoint == typePoint)
                {
                    tempObj = Instantiate(namePref, points[i].transform.GetChild(2));
                    tempObj.color = setColor;
                }
                Debug.Log($"1111 citiesList.citiesNamesList: пустой");
            }

        }
    }
    [PunRPC]
    void TakeTextNameCities(string[] objectNames)
    {
        GameObject[] cityName = GetTagsText();
        for (int i = 0; i < cityName.Length; i++)
        {
            cityName[i].GetComponent<TextMeshProUGUI>().text = objectNames[i];
        }
    }
    public void SendTextNameCities()
    {
        if (photonView != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject[] cityName = GetTagsText();
                string[] objectNames = new string[cityName.Length];
                for (int i = 0; i < cityName.Length; i++)
                {
                    objectNames[i] = cityName[i].GetComponent<TextMeshProUGUI>().text;
                }

                photonView.RPC("TakeTextNameCities", RpcTarget.All, objectNames);
            }
        }
        else
        {
            Debug.LogError("PhotonView is not initialized!");
        }
    }
    GameObject[] GetTagsText()
    {
        return GameObject.FindGameObjectsWithTag("CityTagName");
    }
    public void LoadAllPoints()
    {
        for(int i = 0;i<mapPoints;i++)
        {
            if (GameObject.Find("Point (" + i + ")"))
            {
                points.Add(GameObject.Find("Point (" + i + ")"));
            }
        }
    }
    public GameObject FindPoint(int numberPoint)
    {
        StringBuilder namePoint = new StringBuilder("Point (" + numberPoint + ")", 12);
        foreach (GameObject point in points)
        {
            if (point.name == namePoint.ToString()) return point;
        }
        return null;
    }
    public GameObject FindPointByName(string namePoints)
    {
        foreach (GameObject point in points)
        {
            if (point.name == namePoints) return point;
        }
        return null;
    }
    public GameObject FindPointInBridge(int numbBridge, int numbPoint, Bridge br)
    {
        StringBuilder namePoint = new StringBuilder($"Point: ({numbPoint}) |Bridge: ({numbBridge})", 26);
        //Debug.Log($"Trying Find Point Number: ({numbPoint}) Bridge Number: ({numbBridge})");
        foreach (GameObject point in br.bridgePoints)
        {
            if (point.name == namePoint.ToString())
            {
                //Debug.Log($"Найдена точка моста: {numbBridge}, Точка: {numbPoint}");
                return point;
            }
                
        }
        //Debug.Log($"Not found");
        return null;
    }
    public GameObject FindPointInBridge(int numbBridge, int numbPoint)
    {
        StringBuilder namePoint = new StringBuilder($"Point: ({numbPoint}) |Bridge: ({numbBridge})", 26);
        //Debug.Log($"Trying Find Point Number: ({numbPoint}) Bridge Number: ({numbBridge})");
        foreach (Bridge br in BridgesPoints)
        {
            foreach(GameObject point in br.bridgePoints)
            {
                if (point.name == namePoint.ToString())
                {
                    Debug.Log($"Найдена точка моста: {numbBridge}, Точка: {numbPoint}");
                    return point;
                }
            }
        }
        //Debug.Log($"Not found");
        return null;
    }

    public int GetIntPoint(string namePoint)
    {
        StringBuilder namePointB = new StringBuilder(namePoint, 12);
        namePointB.Remove(0, 7);//Очистить все кроме числа
        namePointB.Replace(')', '\0');
        return int.Parse(namePointB.ToString());
    }
    // Update is called once per frame
    private void InitializationUpdate()
    {
        interfaceControl = GameManager.Instance.GetInterface();
    }
    public void StartGameForPlayer(int startCell, GameObject player)
    {
        PlayerControl plCon = player.GetComponent<PlayerControl>();
        //Задаем новую координату(точку) игроку
        plCon.cellPosition = startCell;//Конвертировать в int

        //Добавить игрока в новою точку
        RemoveOrAddPlayersInPoint(player.GetComponent<PhotonView>().ViewID, plCon.cellPosition, true);
    }
    public void PlayerMovingOnPoint()
    {
        if (interfaceControl == null) InitializationUpdate();
        GameObject currentPoint = EventSystem.current.currentSelectedGameObject;
        StringBuilder namePointB = new StringBuilder(currentPoint.name, 26);
        StringBuilder pattern = new StringBuilder(@"Point: \((\d+)\) \|Bridge: \((\d+)\)");
        if (namePointB.ToString().Contains("Point"))
        {
            LoadCurrentPlayerInfo();
            Debug.Log(namePointB.ToString());
            if (player != null)
            {
                if (namePointB.ToString().Contains("Bridge"))
                {
                    // Используем Regex.Match для поиска совпадения
                    Match match = Regex.Match(namePointB.ToString(), pattern.ToString());
                    // Печать найденных чисел
                    if (match.Success)
                    {

                        int pointNumber = int.Parse(match.Groups[1].Value);
                        int bridgeNumber = int.Parse(match.Groups[2].Value);
                        playerCon.cellInBridge = pointNumber;//Конвертировать в int
                        playerCon.onNumbBridge = bridgeNumber;

                        player.transform.position = new Vector3(currentPoint.transform.position.x, currentPoint.transform.position.y, 0);

                        //Добавить игрока в новою точку
                        RemoveOrAddPlayersInPoint(player.GetComponent<PhotonView>().ViewID, bridgeNumber, pointNumber, true);
                        //Удалим игрока с старой точки
                        RemoveOrAddPlayersInPoint(player.GetComponent<PhotonView>().ViewID, playerCon.cellPosition, false);

                        playerCon.cellPosition = -1;
                        playerCon.currentCountSteps--;
                        playerCon.whatStayPoint = currentPoint.GetComponent<PointSettings>().typePoint;
                        MapControl.Instance.ShowPoints();
                    }
                    else
                    {
                        Debug.Log($"Not found this");
                    }
                    interfaceControl.UpdateTextNameCellText($"Вы на клетке №{playerCon.onNumbBridge}|{playerCon.cellInBridge} на мосту", new Color32(139, 69, 19, 255));
                }
                else
                {
                    namePointB.Remove(0, 7);//Очистить все кроме числа
                    namePointB.Replace(')', '\0');

                    player.transform.position = new Vector3(currentPoint.transform.position.x, currentPoint.transform.position.y, 0);


                    //Удалим игрока с старой точки
                    RemoveOrAddPlayersInPoint(player.GetComponent<PhotonView>().ViewID, playerCon.cellPosition, false);

                    //Задаем новую координату(точку) игроку
                    playerCon.cellPosition = int.Parse(namePointB.ToString());//Конвертировать в int

                    //Добавить игрока в новою точку
                    RemoveOrAddPlayersInPoint(player.GetComponent<PhotonView>().ViewID, playerCon.cellPosition, true);

                    playerCon.onNumbBridge = -1;
                    playerCon.cellInBridge = -1;
                    playerCon.currentCountSteps--;
                    playerCon.whatStayPoint = currentPoint.GetComponent<PointSettings>().typePoint;

                    MapControl.Instance.ShowPoints();
                    Color32 colorText;
                    string typeText = GetTypeCell(playerCon.cellPosition, out colorText);
                    interfaceControl.UpdateTextNameCellText($"Вы на клетке №{playerCon.cellPosition} {typeText}", colorText);
                }

            }
            else Debug.Log("Объект: " + PhotonNetwork.NickName + " не найден");
        }
        else
        {
            Debug.Log("Неа, это не точка");
            PlayerMovingOnPoint();
        }
    }
    //Обычная точка
    public void RemoveOrAddPlayersInPoint(int playerViewID, int cellPosition, bool RemoveOrAdd)
    {
        //Удалить игрока с старой точки
        if (playerCon.cellPosition != 0 && RemoveOrAdd == false)
        {
            GameManager.Instance.RunOffsetGloabal(playerViewID, playerCon.cellPosition, false);
        }
        else
        {
            //Добавить игрока в новою точку
            GameManager.Instance.RunOffsetGloabal(playerViewID, playerCon.cellPosition, true);
        }
    }
    //Обычная точка на мосту
    public void RemoveOrAddPlayersInPoint(int playerViewID, int numbBr, int cellPoint, bool RemoveOrAdd)
    {
        //Удалить игрока с старой точки
        if (playerCon.cellPosition != 0 && RemoveOrAdd == false)
        {
            GameManager.Instance.RunOffsetGloabal(playerViewID, numbBr, cellPoint, false);
        }
        else
        {
            //Добавить игрока в новою точку
            GameManager.Instance.RunOffsetGloabal(playerViewID, numbBr, cellPoint, true);
        }
    }
    protected string GetTypeCell(int cellPosition, out Color32 colorCell)
    {
        TypePoint typeP = FindPoint(cellPosition).GetComponent<PointSettings>().typePoint;
        switch(typeP)
        {
            case TypePoint.Woods:
                {
                    colorCell = new Color32(0, 100, 0, 255);
                    return "Леса";
                }
            case TypePoint.Stones:
                {
                    colorCell = new Color32(90, 160, 160, 255);
                    return "Рудника";
                }
            case TypePoint.Village:
                {
                    colorCell = new Color32(90, 50, 180, 255);
                    return "Поселения";
                }
            case TypePoint.Bridge:
                {
                    colorCell = new Color32(210, 120, 0, 255);
                    return "Моста";
                }
            case TypePoint.Valley:
                {
                    colorCell = new Color32(255, 255, 0, 255);
                    return "Пустошь";
                }
            default:
                {
                    colorCell = new Color32(139, 69, 19, 255);
                    return "не известно";
                }
        }
        
    }
    public void HideAllPoints()
    {
        interfaceControl.ShowAll(false);
        foreach (GameObject point in points)
        {
            point.GetComponent<PointSettings>().SetColor(new Color32(166, 166, 166, 100));
            point.GetComponent<Button>().interactable = false;
        }
        //Пройдем по всем мостам
        for (int br = 0; br < BridgesPoints.Count; br++)
        {
            for (int poBr = 0; poBr < BridgesPoints[br].bridgePoints.Length; poBr++)
            {
                //Если найдем точку
                if (FindPointInBridge(br, poBr, BridgesPoints[br]) != null)
                {
                    //Debug.Log($"Ищем мост: {br}, Точку: {poBr}");
                    GameObject thisPoint = FindPointInBridge(br, poBr, BridgesPoints[br]);
                    thisPoint.GetComponent<PointSettings>().SetColor(new Color32(166, 166, 166, 100));
                    thisPoint.GetComponent<Button>().interactable = false;
                }

            }
        }
    }
    public void ShowPoints()
    {
        HideAllPoints();
        if (playerCon.currentCountSteps < 1)//Если этот игрок сейчас не ходит - не выделять клетки.
        {
            return;
        };
        if (playerCon.cellPosition != -1)
        {

            TypePoint currPointType = FindPoint(playerCon.cellPosition).GetComponent<PointSettings>().typePoint;
            //Если игрок на клетке "Деревня", то показывать кнопки маказин и маркет.
            if (currPointType == TypePoint.Village)
            {
                interfaceControl.ShowButWindow(interfaceControl.ButShopWin, true);
                interfaceControl.ShowButWindow(interfaceControl.ButMarketWin, true);
            }
            else if(currPointType == TypePoint.Bridge)
            {
                interfaceControl.ShowAll(false);
            }
            else
            {
                //Показывать кнопку строительство
                interfaceControl.ShowButWindow(interfaceControl.ButOpenBuildWin, true);
                //Показывать кнопку копать
                interfaceControl.ShowButWindow(interfaceControl.butDigHole, true);
            }

            for (int po = playerCon.cellPosition; po <= (playerCon.cellPosition + playerCon.lengthStep); po++)
            {
                if (po == playerCon.cellPosition) continue;
                if (FindPoint(po) != null && playerCon.onNumbBridge == -1)
                {
                    ShowPoint(po);
                }
            }
        }
        if (BridgesPoints.Count > 0) ShowBridge();
    }
    void ShowPoint(int po)
    {
        GameObject thisPoint = FindPoint(po);
        thisPoint.GetComponent<PointSettings>().SetColor(new Color32(255, 244, 158, 255));
        thisPoint.GetComponent<Button>().interactable = true;
    }
    void ShowBridge()
    {
        
        ////Пройдем по всем мостам
        for (int br = 0; br<BridgesPoints.Count;br++)
        {
            if ((BridgesPoints[br].startPoint == playerCon.cellPosition) || (playerCon.onNumbBridge > -1))
            {
                if (playerCon.onNumbBridge != (-1) && br != playerCon.onNumbBridge)
                    continue;
                //Пройдем по мосту
                for (int poBr = playerCon.cellInBridge; poBr <= (playerCon.cellInBridge + playerCon.lengthStep);poBr++)
                {
                    if (poBr == playerCon.cellInBridge) continue;
                    if(FindPointInBridge(br, poBr, BridgesPoints[br]) != null && PhotonNetwork.NickName == BridgesPoints[br].ownerName)
                    {
                        GameObject thisPoint = FindPointInBridge(br, poBr, BridgesPoints[br]);
                        thisPoint.GetComponent<PointSettings>().SetColor(new Color32(255, 244, 158, 255));
                        thisPoint.GetComponent<Button>().interactable = true;
                    }
                }
                if (playerCon.cellInBridge + playerCon.lengthStep >= BridgesPoints[br].bridgePoints.Length && br == playerCon.onNumbBridge)
                {
                    for (int i = 0; i <= (playerCon.cellInBridge + playerCon.lengthStep - BridgesPoints[br].bridgePoints.Length);i++)
                    {
                        ShowPoint(BridgesPoints[br].endPoint+i);
                    }

                }
            }
        }
    }
}
public class Build
{
    public string ownerName;
    public Builds typeBuild;
    public Color32 colorBuild;
    GameObject prefab;
    public Build(string ownerName, Builds typeBuild)
    {
        this.ownerName = ownerName;
        this.typeBuild = typeBuild;
    }
    public Build(string ownerName, GameObject prefab, Builds typeBuild)
    {
        this.ownerName = ownerName;
        this.typeBuild = typeBuild;
    }
    public Build(string ownerName,GameObject prefab, Builds typeBuild, Color32 colorBuild)
    {
        this.ownerName = ownerName;
        this.prefab = prefab;
        this.typeBuild = typeBuild;
        this.colorBuild = colorBuild;
    }
}

public class Bridge:Build
{
    public int startPoint;
    public int endPoint;


    public GameObject[] bridgePoints;
    public Bridge(int _startPoint, int _endPoint, GameObject[] _bridgePoints, string ownerName, Builds TypeBuild) :base(ownerName, TypeBuild)
    {
        startPoint = _startPoint;
        endPoint = _endPoint;
        bridgePoints = _bridgePoints;
    }
}
public class Sawmill:Build
{
    public Sawmill(Color32 colorBuild, GameObject sawmillPrefab, string ownerName, Builds TypeBuild) : base(ownerName, sawmillPrefab, TypeBuild, colorBuild)
    {
    }
}
public class Drill:Build
{
    public Drill(Color32 colorBuild, GameObject drillPrefab, string ownerName, Builds TypeBuild) : base(ownerName, drillPrefab, TypeBuild, colorBuild)
    {
    }
}
public class Hole:Build
{
    public Hole(GameObject holePrefab, string ownerName, Builds typeBuild) :base(ownerName, holePrefab, typeBuild)
    {
    }
}
