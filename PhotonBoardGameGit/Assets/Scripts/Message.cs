using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Message : MonoBehaviour
{
    public TextMeshProUGUI textTable;
    Animation anim;
    private void Start()
    {
        anim = GetComponent<Animation>();
        anim.Stop();
    }
    public void DisaplyText(string message, float timeForDestroy)
    {
        textTable.text = message;
        Invoke("LeaveSlowly", timeForDestroy-0.6f);
    }
    public void DisaplyText(string message, float timeForDestroy, Color32 colorText)
    {
        textTable.color = colorText;
        textTable.text = message;
        Invoke("LeaveSlowly", timeForDestroy - 0.6f);
    }
    private void DestroyThis()
    {
        Destroy(gameObject);
    }
    private void LeaveSlowly()
    {
        anim.Play("MessageAway");
        Invoke("DestroyThis", 0.7f);
    }
}
