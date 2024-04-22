using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnLogic : MonoBehaviour
{
    //����� �� ����
    public void ExitGameBtn()
    {
        Application.Quit();
    }
    //������� � ����� �����
    public void ServerBtn()
    {
        PhotonNetwork.JoinLobby();
        SceneManager.LoadScene("Lobby");
    } 
    //��������� � ����
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
