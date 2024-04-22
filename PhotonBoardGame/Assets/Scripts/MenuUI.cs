using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
   public TMP_InputField inFil;
   public TextMeshProUGUI textPlaceHold;
   private string PlayerName;
    void Start()
    {
        PlayerName = "Player " + Random.Range(1000, 9999);
        textPlaceHold.text = PlayerName;
    }
    public string GetName()//Получить имя из поля
    {
        string enderedName = inFil.text.Trim();
        if(!string.IsNullOrEmpty(enderedName))
        {
            return enderedName;
        }
        else
        {
            return PlayerName;
        }
        //if (inFil.text != "") return inFil.text;
        //else return textPlaceHold.text;
    }
}
