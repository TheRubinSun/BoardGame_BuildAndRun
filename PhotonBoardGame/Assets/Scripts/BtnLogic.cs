using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnLogic : MonoBehaviour
{
    //Выйти из игры
    public void ExitGameBtn()
    {
        Application.Quit();
    }
    //Перейти в сцену лобби
    public void ServerBtn()
    {
        PhotonNetwork.JoinLobby();
        SceneManager.LoadScene("Lobby");
    } 
    //Вернуться в меню
    public void GoToMenuBtn()
    {
        PhotonNetwork.LeaveLobby();
        SceneManager.LoadScene("MainMenu");
    }
    public void FindPoint()
    {
        MapControl.Instance.SendTextNameCities();
    }
}
