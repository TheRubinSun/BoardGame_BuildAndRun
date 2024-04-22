using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject ButtonEndTurn;
    public TextMeshProUGUI textTurnInfo;
    public TextMeshProUGUI LogText;
    public GameObject MessagePref;

    public GameObject ButtonReady;
    public Color32 colorCancel;
    public Color32 colorRdy;

    public bool playerInCity;

    public GameObject buildWindow;
    public GameObject shopWindow;
    public GameObject marketWindow;

    public GameObject ButOpenBuildWin;
    public GameObject ButShopWin;
    public GameObject ButMarketWin;

    public GameObject butDigHole;
    public TextMeshProUGUI textButHole;
    Builds SelectBuild;

    public TextMeshProUGUI namePlayerText;               
    public TextMeshProUGUI nameItemTextMouse;//Поле для имя наведенного предмета мышкой                
    public TextMeshProUGUI nameItemTextDirection; //Поле для имя наведенного предмета направления
    public TextMeshProUGUI nameCellText;
    public TextMeshProUGUI nameTextAtrb;
    public TextMeshProUGUI nameGlobalCountSteps;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    PhotonView photonView;

    GameObject isMePlayer;
    PlayerControl isMePlayerControl;
    async void Start()
    {
        isMePlayer = GameObject.Find(PhotonNetwork.NickName);
        isMePlayerControl = isMePlayer.GetComponent<PlayerControl>();
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("PhotonView not found on UIController.");
        }
        SetOnEndTurnButtonClickActive(false);
        Invoke("UpdateAllNames", 1f);
        //НЕ УДАЛЯТЬ, ЭТО МБ БУДЕТ ЕЩЕ НУЖНО
        //InvokeRepeating("UpdateAllNames", 0f, 3f);
    }

    void Update()
    {
        //Debug.Log(GameManager.Instance.GetStartGame());
        if (GameManager.Instance.GetStartGame())
        {
            if (photonView != null)
            {
                GameManager.Instance.Handler(DisaplyCurrentPlayerTurn);
                GameManager.Instance.GetCurrentPlayerTurn();
                GameManager.Instance.Handler(DisaplyGloabalCountSteps);
                GameManager.Instance.GetCoutSteps();

                isMePlayerControl.Handler(DisplayGold);
                isMePlayerControl.GetGold();
                isMePlayerControl.Handler(DisplayWood);
                isMePlayerControl.GetWood();
                isMePlayerControl.Handler(DisplayStone);
                isMePlayerControl.GetStone();

                isMePlayerControl.Handler(DisaplyTextNameAtr);
                isMePlayerControl.UpdateAtrValue();

                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
                {

                    OnEndTurnButtonClick();
                }
            }
            else
            {
                Debug.Log("Неа");
            }
        }
        else
        {
            GameManager.Instance.Handler(DisaplyReadyInfo);
            GameManager.Instance.GetCountReadyPl();
        }
    }
    public void DisaplyTable(string message, float timeLeng)
    {
        GameObject messageObj = Instantiate(MessagePref, this.transform);
        messageObj.GetComponent<Message>().DisaplyText(message, timeLeng);
    }
    public void DisaplyTable(string message, float timeLeng, Color32 colorText)
    {
        GameObject messageObj = Instantiate(MessagePref, this.transform);
        messageObj.GetComponent<Message>().DisaplyText(message, timeLeng, colorText);
    }
    void DisaplyGloabalCountSteps(string info)
    {
        nameGlobalCountSteps.text = info;
    }
    void DisaplyCurrentPlayerTurn(string info)
    {
        textTurnInfo.text = info;
    }
    void DisaplyReadyInfo(string info)
    {
        textTurnInfo.text = info;
    }
    public void DisaplyTextNameAtr(string info)
    {
        nameTextAtrb.text = info;
    }
    public void DisplayGold(string info)
    {
        goldText.text = info;
    }
    public void DisplayWood(string info)
    {
        woodText.text = info;
    }
    public void DisplayStone(string info)
    {
        stoneText.text = info;
    }

    public void ShowAll(bool showBool)
    {
        ShowButWindow(butDigHole, showBool);
        ShowButWindow(ButOpenBuildWin, showBool);
        ShowButWindow(ButShopWin, showBool);
        ShowButWindow(ButMarketWin, showBool);
    }
    public void ShowButWindow(GameObject butOpenWindow,bool showBool)
    {
        butOpenWindow.SetActive(showBool);
    }
    public void OpenBuildWindow()
    {
        ShowButWindow(butDigHole, false);
        ShowButWindow(ButOpenBuildWin,false);
        ShowButWindow(ButShopWin, false);
        ShowButWindow(ButMarketWin, false);
        Instantiate(buildWindow,this.transform);
    }
    public void OpenShopWindow()
    {
        ShowButWindow(butDigHole, false);
        ShowButWindow(ButOpenBuildWin, false);
        ShowButWindow(ButShopWin, false);
        ShowButWindow(ButMarketWin, false);
        Instantiate(shopWindow, this.transform);
    }
    public void OpenMarketWindow()
    {
        ShowButWindow(butDigHole, false);
        ShowButWindow(ButOpenBuildWin, false);
        ShowButWindow(ButShopWin, false);
        ShowButWindow(ButMarketWin, false);
        Instantiate(marketWindow, this.transform);
    }

    public void SetOnEndTurnButtonClickActive(bool isActive)
    {
        if(ButtonEndTurn!=null)
        {
            ButtonEndTurn.SetActive(isActive);
        }
    }
    public void OffButton()
    {
        ButtonReady.SetActive(false);
    }
    public void WriteLog(string info)
    {
        LogText.text += $"{info}\n";
    }
    //Завершить ход
    public void OnEndTurnButtonClick()
    {
        if (isMePlayer.name == GameManager.Instance.GetCurrentPlayerString())
        {
            GameManager.Instance.EndTurn();
        }
        else WriteLog("Не ваша очередь");
    }
    public void ReadyBut()
    {
        isMePlayer = GameObject.Find(PhotonNetwork.NickName);
        isMePlayerControl = isMePlayer.GetComponent<PlayerControl>();
        if (isMePlayerControl == null)
        {
            Debug.Log("Не найден игрок");
            return;
        }
        if (!GameManager.Instance.GetReadyPlayer(PhotonNetwork.NickName))
        {
            ButtonReady.GetComponent<Image>().color = colorRdy;
            ButtonReady.GetComponentInChildren<TextMeshProUGUI>().text = "Вы готовы!";
            GameManager.Instance.SentReady(PhotonNetwork.NickName, true);
        }
        else
        {
            ButtonReady.GetComponent<Image>().color = colorCancel;
            ButtonReady.GetComponentInChildren<TextMeshProUGUI>().text = "Вы не готовы";
            GameManager.Instance.SentReady(PhotonNetwork.NickName, false);
        }
    }
    public void UpdateTextNameItemTextMouse(string info)
    {
        nameItemTextMouse.text = info;
    }
    public void UpdateTextNameItemTextDirection(string info)
    {
        nameItemTextDirection.text = info;
    }
    public void UpdateTextNameCellText(string info, Color32 colorText)
    {
        nameCellText.color = colorText;
        nameCellText.text = info;
    }

    //Искать не обновленные имена игроков
    public void UpdateAllNames()
    {
        namePlayerText.text = "Вы: " + PhotonNetwork.NickName;
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            UpdateNickNames(GameObject.FindGameObjectsWithTag("Player"));
        }
    }
    //Обновить имя игрока 
    void UpdateNickName(GameObject player)//Обновтиь один ник
    {
        player.name = player.GetComponent<PhotonView>().Owner.NickName;
    }
     void UpdateNickNames(GameObject[] player)//Обновтиь список ников
    {
        for (int i = 0; i < player.Length; i++)
        {
            player[i].name = player[i].GetComponent<PhotonView>().Owner.NickName;
        }
    }


    public void DigHoleBuild()
    {
        SelectBuild = Builds.Hole;
        //Проверка на возможность стройки (Точка для лесопилки и без владельца );

        PointSettings paramPoint = MapControl.Instance.FindPoint(isMePlayerControl.cellPosition).GetComponent<PointSettings>();
        if (paramPoint.typePoint == TypePoint.Valley && paramPoint.owner == "")
        {
            butDigHole.SetActive(false);
        }
        else if(paramPoint.typePoint == TypePoint.Valley && paramPoint.owner != "")
        {
            GameManager.Instance.DisplayInfoMessage("Тут когда-то уже копали...", 1f);
            return;
        }
        else
        {
            butDigHole.SetActive(false);
            if (paramPoint.typePoint != TypePoint.Valley)
            {
                GameManager.Instance.DisplayInfoMessage("Вы не на клетке \"Пустошь\" ", 1f);
            }
            else
            {
                GameManager.Instance.DisplayInfoMessage("Эта клетка занята строением", 1f);
            }
            return;
        }

        DigHole(SelectBuild);
    }
    void DigHole(Builds typeBuild)
    {
        GameManager.Instance.SentCreateHole(isMePlayerControl.cellPosition, PhotonNetwork.NickName, typeBuild);
    }
}

public delegate void GetInfoDeleg(string info);
public delegate void GetAttribPlayer(int a, int b, int c);