using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorView : MonoBehaviour, IPunObservable
{
    public PlayerControl playerContr;
    SpriteRenderer spriteRendererHead;
    SpriteRenderer spriteRendererArms;
    SpriteRenderer spriteRendererLazer;
    SpriteRenderer spriteRendererPointer;
    Color colorPlayer;
    Color colorLazer;
    void Start()
    {
        colorPlayer = new Color(0, 0, 0);
        colorLazer = new Color(0, 0, 0);
        if (playerContr != null) 
        {
            spriteRendererHead = playerContr.head.GetComponent<SpriteRenderer>();
            spriteRendererArms = playerContr.arms.GetComponent<SpriteRenderer>();
            spriteRendererLazer = playerContr.lazer.GetComponent<SpriteRenderer>();
            spriteRendererPointer = playerContr.Pointer.GetComponent<SpriteRenderer>();
            colorPlayer = spriteRendererHead.color;
            colorLazer = spriteRendererPointer.color;
        }
        else
        {
            Debug.LogError("PlayerControl is not assigned to ColorView.");
        }

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (spriteRendererHead == null) return;
        //Отправка цветов
        if (stream.IsWriting)
        {
            stream.SendNext(colorPlayer.r);
            stream.SendNext(colorPlayer.g);
            stream.SendNext(colorPlayer.b);
            stream.SendNext(colorLazer.r);
            stream.SendNext(colorLazer.g);
            stream.SendNext(colorLazer.b);

        }
        else//Иначе получить параметры цветов
        {

            colorPlayer.r = (float)stream.ReceiveNext();
            colorPlayer.g = (float)stream.ReceiveNext();
            colorPlayer.b = (float)stream.ReceiveNext();
            colorLazer.r = (float)stream.ReceiveNext();
            colorLazer.g = (float)stream.ReceiveNext();
            colorLazer.b = (float)stream.ReceiveNext();
            spriteRendererHead.color = colorPlayer;
            spriteRendererArms.color = colorPlayer;
            spriteRendererLazer.color = colorLazer;
            spriteRendererPointer.color = colorLazer;
        }
        
    }
    
}
