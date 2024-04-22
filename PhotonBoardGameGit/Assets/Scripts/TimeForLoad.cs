using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeForLoad : MonoBehaviour
{
    public GameObject zzz; //��� ������ (���)
    public Transform parent; //�������� (������������� � ���.)
    public float timeForLoad = 1;//����� ��� ��������
    GameObject tempObj;//��������� ��������� ������
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
