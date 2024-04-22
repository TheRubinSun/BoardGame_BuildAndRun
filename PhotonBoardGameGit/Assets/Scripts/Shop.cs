using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    int rateWoodToBuy;
    int rateWoodToSell;

    int rateStoneToBuy;
    int rateStoneToSell;

    int costIncreaseLengthStep;
    int costIncreaseCountSteps;
    int costIncreaseChance;

    public bool isMarket;

    public TextMeshProUGUI textGoldRec;
    public TextMeshProUGUI textWoodRec;
    public TextMeshProUGUI textStoneRec;

    public TextMeshProUGUI TextRateWoodToBuy;
    public TextMeshProUGUI TextRateWoodToSell;
    public TextMeshProUGUI TextRateStoneToBuy;
    public TextMeshProUGUI TextRateStoneToSell;

    public TextMeshProUGUI TextCostIncreaseLengthStep;
    public TextMeshProUGUI TextCostIncreaseCountSteps;
    public TextMeshProUGUI TextCostIncreaseChance;

    GameObject thisPlayer;
    PlayerControl playerControl;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < GameManager.Instance.players.Count; i++)
        {
            if (PhotonNetwork.NickName == GameManager.Instance.players[i].name)
            {
                thisPlayer = GameManager.Instance.players[i];
                playerControl = thisPlayer.GetComponent<PlayerControl>();
                break;
            }
        }
        playerControl = thisPlayer.GetComponent<PlayerControl>();
        UpdatePrice();
    }
    void UpdatePrice()
    {
        rateWoodToBuy = 10;
        rateWoodToSell = 8;

        rateStoneToBuy = 20;
        rateStoneToSell = 16;

        costIncreaseLengthStep = 150;
        costIncreaseCountSteps = 1200;
        costIncreaseChance = 200;

        if (isMarket)//Если это рынок, то выполнять рыночные операции, иначе магазинные
        {
            TextRateWoodToBuy.text = $"-{rateWoodToBuy} золота";
            TextRateWoodToSell.text = $"+{rateWoodToSell} золота";
            TextRateStoneToBuy.text = $"-{rateStoneToBuy} золота";
            TextRateStoneToSell.text = $"+{rateStoneToSell} золота";
        }
        else
        {
            TextCostIncreaseLengthStep.text = $"-{costIncreaseLengthStep} золота";
            TextCostIncreaseCountSteps.text = $"-{costIncreaseCountSteps} золота";
            TextCostIncreaseCountSteps.text = $"-{costIncreaseChance} золота";
        }
        textGoldRec.text = $"Gold: {playerControl.gold}";
        textWoodRec.text = $"Wood: {playerControl.wood}";
        textStoneRec.text = $"Stone: {playerControl.ore}";

    }
    // Update is called once per frame
    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CloseWindow();
        }
    }
    public void SellWood()
    {
        if(playerControl.wood>=1)
        {
            playerControl.wood--;
            playerControl.gold += rateWoodToSell;
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Отсутствует древесина!",2f);
        }
        UpdatePrice();
    }
    public void BuyWood()
    {
        if (playerControl.gold >= rateWoodToBuy)
        {
            playerControl.wood++;
            playerControl.gold -= rateWoodToBuy;
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Недостаточно золота!", 2f);
        }
        UpdatePrice();
    }
    public void SellStone()
    {
        if (playerControl.ore >= 1)
        {
            playerControl.ore--;
            playerControl.gold += rateStoneToSell;
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Отсутствует руда!", 2f);
        }
        UpdatePrice();
    }
    public void BuyStone()
    {
        if (playerControl.gold >= rateStoneToBuy)
        {
            playerControl.ore++;
            playerControl.gold -= rateStoneToBuy;
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Недостаточно золота!", 1f);
        }
        UpdatePrice();
    }
    public void BuyLengthStep()
    {
        if (playerControl.gold >= costIncreaseLengthStep)
        {
            playerControl.lengthStep++;
            playerControl.gold -= costIncreaseLengthStep;
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Недостаточно золота!", 1f);
        }
        UpdatePrice();
    }
    public void BuyCountSteps()
    {
        if (playerControl.gold >= costIncreaseCountSteps)
        {
            playerControl.maxCountSteps++;
            playerControl.currentCountSteps++;
            playerControl.gold -= costIncreaseCountSteps;
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Недостаточно золота!", 1f);
        }
        UpdatePrice();
    }
    public void BuyChance()
    {
        if (playerControl.gold >= costIncreaseChance && playerControl.chanceFindGold<100)
        {
            playerControl.chanceFindGold += 5;
            playerControl.gold -= costIncreaseChance;
        }
        else if(playerControl.chanceFindGold>=100)
        {
            GameManager.Instance.DisplayInfoMessage($"У вас уже максимальный шанс!", 1f);
        }
        else
        {
            GameManager.Instance.DisplayInfoMessage($"Недостаточно золота!", 1f);
        }
        UpdatePrice();
    }

    void CloseWindow()
    {
        MapControl.Instance.ShowPoints();
        Destroy(this.gameObject);
    }
}
