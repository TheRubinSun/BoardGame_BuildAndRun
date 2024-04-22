using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class PointSettings : MonoBehaviour
{

    [SerializeField] GameObject otherPoint;
    public float textureRepeat = 2f; // Количество повторений текстуры
    public TypePoint typePoint;
    public int GetCountPlayerThis { get; set; }

    public string owner;

    public List<GameObject> playersThisPoint;

    [SerializeField] Material material;
    List<GameObject> lines;

    private void Start()
    {
        owner = "";
        lines = new List<GameObject>();
        UpdateColorBGPoint();
    }
    public void SetColor(Color32 colorNew)
    {
        otherPoint.GetComponent<SpriteRenderer>().color = colorNew;
    }
    void UpdateColorBGPoint()
    {
        switch (typePoint)
        {
            case TypePoint.Woods: this.GetComponent<Image>().color = new Color32(20, 200, 20, 255); break;
            case TypePoint.Stones: this.GetComponent<Image>().color = new Color32(90, 160, 160, 255); break;
            case TypePoint.Village: this.GetComponent<Image>().color = new Color32(60, 50, 180, 255); break;
            case TypePoint.Bridge: this.GetComponent<Image>().color = new Color32(210, 120, 0, 255); break;
            case TypePoint.Valley: this.GetComponent<Image>().color = new Color32(255, 255, 0, 255); break;
            default: this.GetComponent<Image>().color = new Color32(180, 122, 40, 255); break;
        }

    }
    void UpdateLazerPlayers(bool hide)
    {

        foreach(GameObject player in playersThisPoint)
        {
            if (hide == false)
            {
                player.GetComponent<PlayerControl>().DrawLineToPoint(this.gameObject);
            }
            else
            {
                //player.GetComponent<PlayerControl>().HideLineToPoint();
            }

        }
    }
    public void OffsetMovePlayers()
    {
        switch (playersThisPoint.Count)
        {
            case 0:
                {
                    UpdateLazerPlayers(false);
                    break;
                }
            case 1:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                    UpdateLazerPlayers(false);
                    break;
                }
            case 2:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x + 1f, transform.position.y - 1f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x + 1f, transform.position.y + 1f, 0);
                    UpdateLazerPlayers(false);
                    break;
                }
            case 3:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y + 0.8f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y + 0.8f, 0);
                    playersThisPoint[2].transform.position = new Vector3(transform.position.x, transform.position.y - 0.9f, 0);
                    break;
                }
            case 4:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y + 0.8f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y + 0.8f, 0);
                    playersThisPoint[2].transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y - 0.8f, 0);
                    playersThisPoint[3].transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y - 0.8f, 0);
                    break;
                }
            case 5:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x, transform.position.y + 1.1f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x - 1.1f, transform.position.y + 0.5f, 0);
                    playersThisPoint[2].transform.position = new Vector3(transform.position.x + 1.1f, transform.position.y + 0.5f, 0);
                    playersThisPoint[3].transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y - 0.8f, 0);
                    playersThisPoint[4].transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y - 0.8f, 0);
                    break;
                }
            case 6:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x, transform.position.y + 1.1f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x - 1.1f, transform.position.y + 0.6f, 0);
                    playersThisPoint[2].transform.position = new Vector3(transform.position.x + 1.1f, transform.position.y + 0.6f, 0);
                    playersThisPoint[3].transform.position = new Vector3(transform.position.x + 1.1f, transform.position.y - 0.6f, 0);
                    playersThisPoint[4].transform.position = new Vector3(transform.position.x - 1.1f, transform.position.y - 0.6f, 0);
                    playersThisPoint[5].transform.position = new Vector3(transform.position.x, transform.position.y - 1.1f, 0);
                    break;
                }
            case 7:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 1.3f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 1.3f, 0);
                    playersThisPoint[2].transform.position = new Vector3(transform.position.x - 1.3f, transform.position.y + 0.6f, 0);
                    playersThisPoint[3].transform.position = new Vector3(transform.position.x + 1.3f, transform.position.y + 0.6f, 0);
                    playersThisPoint[4].transform.position = new Vector3(transform.position.x - 1.3f, transform.position.y - 0.6f, 0);
                    playersThisPoint[5].transform.position = new Vector3(transform.position.x + 1.3f, transform.position.y - 0.6f, 0);
                    playersThisPoint[6].transform.position = new Vector3(transform.position.x, transform.position.y - 1.1f, 0);
                    break;
                }
            case 8:
                {
                    playersThisPoint[0].transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 1.3f, 0);
                    playersThisPoint[1].transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 1.3f, 0);
                    playersThisPoint[2].transform.position = new Vector3(transform.position.x - 1.3f, transform.position.y + 0.6f, 0);
                    playersThisPoint[3].transform.position = new Vector3(transform.position.x + 1.3f, transform.position.y + 0.6f, 0);
                    playersThisPoint[4].transform.position = new Vector3(transform.position.x - 1.3f, transform.position.y - 0.6f, 0);
                    playersThisPoint[5].transform.position = new Vector3(transform.position.x + 1.3f, transform.position.y - 0.6f, 0);
                    playersThisPoint[6].transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y - 1.3f, 0);
                    playersThisPoint[7].transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y - 1.3f, 0);
                    break;
                }
            default:
                {
                    break;
                }

        }
    }
}
public enum TypePoint
{
    Woods,
    Stones,
    Village,
    Bridge,
    Valley
}
