using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class SetNames : MonoBehaviour
{
    string filePath; // ���� � ������ JSON �����
    public static SetNames instance;
    public static SetNames Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SetNames>();
            }
            return instance;
        }
    }
    void Start()
    {
        // ��������� ��� �����
        string fileName = "saveNames.json";
        // ����������� ���� � ����� � ����� � ���������� ���������� ������ ����������
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(filePath))
        {
            CreateFileNames(filePath);
        }
    }
    public void CreateFileNames(string filePath)
    {
        if (!File.Exists(filePath))
        {

            GenerateNames.Instance.SaveWords();
        }
    }
    public CitiesList LoadData()
    {
        // ��������� ��� �����
        string fileName = "saveNames.json";
        // ����������� ���� � ����� � ����� � ���������� ���������� ������ ����������
        filePath = Path.Combine(Application.persistentDataPath, fileName);


        if (File.Exists(filePath))
        {
            Debug.LogWarning("�������� �������: " + filePath);
            string jsonContent = File.ReadAllText(filePath);
            CitiesList citiesList = JsonUtility.FromJson<CitiesList>(jsonContent);
            return citiesList;

        }
        else
        {
            Debug.LogWarning("File not found at path: " + filePath);
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
[Serializable]
public class CitiesList
{
    public List<string> citiesNamesList;
}
