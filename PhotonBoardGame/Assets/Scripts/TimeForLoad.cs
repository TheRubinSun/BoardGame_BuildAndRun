using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeForLoad : MonoBehaviour
{
    public GameObject zzz; //Сам объект (сна)
    public Transform parent; //Родитель (расположениее в иер.)
    public float timeForLoad = 1;//Время для загрузки
    GameObject tempObj;//Временный созданный объект
    private void Awake()
    {
        tempObj = Instantiate(zzz, parent);
    }
    void Update()
    {
        timeForLoad -= Time.deltaTime;
        if(timeForLoad<0)
        {
            Destroy(tempObj);
        }
    }
}
