
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class GenerateNames : MonoBehaviour
{
    string text;
    string[] words;
    Dictionary<char, List<char>> pairOfLetters = new Dictionary<char, List<char>>();
    public static GenerateNames instance;
    public static GenerateNames Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GenerateNames>();
            }
            return instance;
        }
    }

    // Путь к файлу JSON
    private string filePath;
    void Start()
    {
        string fileName = "saveNames.json";
        filePath = Path.Combine(Application.persistentDataPath, fileName);
    }
    public void SaveWords()
    {
        string fileName = "saveNames.json";
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        text = "Atlanta Austin Boston Baltimore Chicago Charlotte Dallas Denver El Paso Eugene Fresno Fargo " +
            "Greenville Greensboro Houston Hartford Indianapolis Irvine Jacksonville Jersey City " +
            "Kansas City Knoxville Los Angeles Louisville Memphis Miami Nashville New Orleans Oakland Oklahoma " +
            "City Portland Phoenix Queens Quincy Raleigh Richmond Seattle San Francisco Tampa Tucson Utica Union " +
            "City Virginia Beach Visalia Washington Wichita Xenia Xenon Yakima Yonkers Zanesville Zion";
        words = OpenText(text);
        foreach (string word in words)
        {
            parseWords(word);
        }
        List<string> names = new List<string>();
        for (int i = 0; i < 1000; i++)
        {

            names.Add(AddWord());
            
        }
        SaveData(names);

    }
    public void SaveData(List<string> data)
    {
        // Создаем объект CitiesName и передаем в него данные
        CitiesList citiesList = new CitiesList();
        citiesList.citiesNamesList = data;

        // Сериализуем объект в формат JSON
        string json = JsonUtility.ToJson(citiesList);
        Debug.Log(json);

        // Записываем JSON в файл
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved to file: " + filePath);
    }
    string AddWord()
    {
        int lengthName = Random.Range(5,10);
        StringBuilder nameCity = new StringBuilder();
        for (int i = 0; i < lengthName; i++)
        {
            if (i == 0)
            {
                int indexLetter = Random.Range(0,pairOfLetters.Keys.Count);
                char randChar = pairOfLetters.Keys.ElementAt(indexLetter);
                nameCity.Append(randChar);
            }
            else
            {
                nameCity.Append(AddLetter(i, nameCity[i - 1], nameCity));
            }

        }
        nameCity[0] = System.Char.ToUpper(nameCity[0]);
        return nameCity.ToString();
    }
    char AddLetter(int i, char lastLetter, StringBuilder nameCity)
    {
        int indexLetter = Random.Range(0, pairOfLetters[nameCity[i - 1]].Count);
        char randChar = pairOfLetters[nameCity[i - 1]].ElementAt(indexLetter);
        if (randChar == lastLetter)
        {
            randChar = AddLetter(i, nameCity[i - 1], nameCity);
        }
        return randChar;
    }
    void SortList()
    {
        foreach (List<char> letter in pairOfLetters.Values)
        {
            List<char> list = letter.Distinct().ToList();
            letter.Clear();
            letter.AddRange(list);
        }
    }
    string[] OpenText(string text)
    {

        text = text.Replace(",", "");
        string[] words = text.Split(' ');
        return words;
    }
    void parseWords(string word)
    {
        char[] charsInWord = word.ToCharArray();

        char lastChar = ' ';
        foreach (char c in charsInWord)
        {
            if (lastChar == ' ') lastChar = c;
            else
            {
                TakeLetters(lastChar, c);
                lastChar = c;
            }
        }
    }
    void TakeLetters(char a, char b)
    {
        a = System.Char.ToLower(a);
        b = System.Char.ToLower(b);
        if (!pairOfLetters.ContainsKey(a))
        {
            pairOfLetters.Add(a, new List<char> { b });
        }
        else
        {
            pairOfLetters[a].Add(b);
        }

    }
}
