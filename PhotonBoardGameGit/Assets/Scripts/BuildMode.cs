using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class BuildMode : MonoBehaviour
{
    public Builds SelectBuild;
    public bool buildLine;
    GameObject thisPlayer;
    PlayerControl playerControl;

    public GameObject butBridge;
    public TextMeshProUGUI textButBridge;
    int costBridgeGold;
    int costBridgeWood;
    int costBridgeOre;

    public GameObject butSawmill;
    public TextMeshProUGUI textButSawmill;
    int costSawmillGold;
    int costSawmillWood;
    int costSawmillOre;

    public GameObject butDrill;
    public TextMeshProUGUI textButDrill;
    int costDrillGold;
    int costDrillWood;
    int costDrillOre;

    int smallDistance;

    void Start()
    {
        costBridgeGold = 100;
        costBridgeWood = 25;
        costBridgeOre = 0;

        costSawmillGold = 50;
        costSawmillWood = 20;
        costSawmillOre = 6;

        costDrillGold = 70;
        costDrillWood = 10;
        costDrillOre = 15;

        smallDistance = 4; //Мин знач. для стройки моста (разница межно клеткой начало моста и конца)

        for (int i = 0;i<GameManager.Instance.players.Count;i++)
        {
            if(PhotonNetwork.NickName == GameManager.Instance.players[i].name)
            {
                thisPlayer = GameManager.Instance.players[i];
                playerControl = thisPlayer.GetComponent<PlayerControl>();
                break;
            }
        }

        textButBridge.text = $"Мост G:{costBridgeGold} W:{costBridgeWood}";
        textButSawmill.text = $"Лесопилка G:{costSawmillGold} W:{costSawmillWood} S: {costSawmillOre}";
        textButDrill.text = $"Бур G:{costDrillGold} W:{costDrillWood} S: {costDrillOre}";
    }
    string pointName;
    // Update is called once per frame

    void Update()
    {
        if(Input.GetMouseButtonDown(1) && !buildLine)
        {
            CloseWindow();
        }
        if(buildLine)
        {
            pointName = playerControl.PlayerRayCastsOnDirection(ref smallDistance);
            if(Input.GetMouseButtonDown(1))
            {
                playerControl.RayClear();
                butBridge.SetActive(true);
                buildLine = false;
            }
            else if(Input.GetMouseButtonDown(0) && pointName!=null)
            {
                int startPoint = playerControl.cellPosition;
                int endPoint = MapControl.Instance.GetIntPoint(pointName);
                if (endPoint - startPoint <= smallDistance)
                {
                    GameManager.Instance.DisplayInfoMessage($"Слишком коротное расстояние. Нужно больше {smallDistance} клеток",1f);
                }
                else
                {
                    CreateBridge(startPoint, endPoint);
                    CloseWindow();
                }
            }
        }

    }

    void CloseWindow()
    {
        playerControl.RayClear();
        BuildNone();
        buildLine = false;
        //ShowButBuild(true);
        MapControl.Instance.ShowPoints();
        Destroy(this.gameObject);
    }
    public void BuildNone()
    {
        SelectBuild = Builds.None;
    }
    public void BuildBridge()
    {
        SelectBuild = Builds.Bridge;

        if (playerControl.gold >= costBridgeGold && playerControl.wood >= costBridgeWood)
        {
            buildLine = true;
            CloseAllButton();
        }
        else
        {
            if(playerControl.gold < costBridgeGold && playerControl.wood < costBridgeWood)
                GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {costBridgeGold - playerControl.gold} золота \n и {costBridgeWood - playerControl.wood} дерева",1f);
            else if(playerControl.gold < costBridgeGold)
                GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {costBridgeGold - playerControl.gold} золота", 1f);
            else if (playerControl.wood < costBridgeWood)
                GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {costBridgeWood - playerControl.wood} дерева", 1f);
            Debug.Log("Так так");
        }
    }
    public void BuildSawmill()
    {
        SelectBuild = Builds.Sawmill;
        //Проверка на возможность стройки (Точка для лесопилки и без владельца );
        PointSettings paramPoint = MapControl.Instance.FindPoint(playerControl.cellPosition).GetComponent<PointSettings>();
        if (paramPoint.typePoint == TypePoint.Woods && paramPoint.owner == "")
        {
            butSawmill.GetComponent<Button>().interactable = true;
        }
        else
        {
            butSawmill.GetComponent<Button>().interactable = false;
            if (paramPoint.typePoint != TypePoint.Woods)
            {
                GameManager.Instance.DisplayInfoMessage("Вы не на клетке с деревьями", 1f);
            }
            else
            {
                GameManager.Instance.DisplayInfoMessage("Эта клетка занята другим игроком", 1f);
            }
            CloseWindow();
            return;
        }

        //Проверка на Экономические ресурсы
        if (playerControl.gold >= costSawmillGold && playerControl.wood >= costSawmillWood && playerControl.ore >= costSawmillOre)
        {
            //CreateSawmill();
            CreateBuild(costSawmillGold,costSawmillWood,costSawmillOre, SelectBuild);
            CloseWindow();
        }
        else
        {
            DisMesHaventRec(costSawmillGold, playerControl.gold, costSawmillWood, playerControl.wood, costSawmillOre, playerControl.ore);
        }
    }
    public void BuildDrill()
    {
        SelectBuild = Builds.Drill;
        //Проверка на возможность стройки (Точка для лесопилки и без владельца );
        PointSettings paramPoint = MapControl.Instance.FindPoint(playerControl.cellPosition).GetComponent<PointSettings>();
        if (paramPoint.typePoint == TypePoint.Stones && paramPoint.owner == "")
        {
            butDrill.GetComponent<Button>().interactable = true;
        }
        else
        {
            butDrill.GetComponent<Button>().interactable = false;
            if (paramPoint.typePoint != TypePoint.Stones)
            {
                GameManager.Instance.DisplayInfoMessage("Вы не на клетке с рудой", 1f);
            }
            else
            {
                GameManager.Instance.DisplayInfoMessage("Эта клетка занята строением", 1f);
            }
            CloseWindow();
            return;
        }

        //Проверка на Эокономические ресурсы
        if (playerControl.gold >= costDrillGold && playerControl.wood >= costDrillWood && playerControl.ore >= costDrillOre)
        {
            CreateBuild(costDrillGold, costDrillWood, costDrillOre, SelectBuild);
            CloseWindow();
        }
        else
        {
            DisMesHaventRec(costDrillGold, playerControl.gold, costDrillWood, playerControl.wood, costDrillOre, playerControl.ore);
        }
    }

    public void BuildBarrier()
    {
        SelectBuild = Builds.Barrier;
    }
    void DisMesHaventRec(int needGold, int haveGold, int needWood, int haveWood, int needOre, int haveOre)
    {
        if (haveGold < needGold && haveWood < needWood && playerControl.ore < costDrillOre)
            GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {needGold - haveGold} золота \n и {needWood - haveWood} дерева\n и {needOre - haveOre} камня", 1f);
        else if (haveGold < needGold)
            GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {needGold - haveGold} золота", 1f);
        else if (haveWood < needWood)
            GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {needWood - haveWood} дерева", 1f);
        else if (haveOre < needOre)
            GameManager.Instance.DisplayInfoMessage($"Нехватает ресурсов!\nНужно еще {needOre - haveOre} камня", 1f);
        Debug.Log("Так так");
    }
    void CloseAllButton()
    {
        butBridge.SetActive(false);
        butSawmill.SetActive(false);
    }
    void CreateBridge(int IDStartPoint, int IDEndPoint)
    {
        if(MapControl.Instance.FindPoint(IDEndPoint).GetComponent<PointSettings>().owner != "")
        {
            GameManager.Instance.DisplayInfoMessage($"Эта клетка уже занята другим строением", 1f, new Color32(255,0,0,255));
            return;
        }
        playerControl.gold -= costBridgeGold;
        playerControl.wood -= costBridgeWood;
        Vector3 startPointPos = MapControl.Instance.FindPoint(IDStartPoint).transform.position;
        Vector3 endPointPos = MapControl.Instance.FindPoint(IDEndPoint).transform.position;
        float range = 2f;

        MapControl.Instance.FindPoint(IDEndPoint).GetComponent<PointSettings>().owner = PhotonNetwork.NickName;
        GameManager.Instance.SentCreateBuild(startPointPos, endPointPos, IDStartPoint, IDEndPoint, range, PhotonNetwork.NickName, Builds.Bridge);
        MapControl.Instance.ShowPoints();
    }
    void CreateBuild(int costGoldBuild, int costWoodBuild, int costOreBuild, Builds typeBuild)
    {
        playerControl.gold -= costGoldBuild;
        playerControl.wood -= costWoodBuild;
        playerControl.ore -= costOreBuild;

        GameManager.Instance.SentCreateBuild(playerControl.cellPosition, PhotonNetwork.NickName, playerControl.colorTeam, typeBuild);
    }
}
public enum Builds
{
    None,
    Hole,
    Bridge,
    Barrier,
    Sawmill,
    Drill
}