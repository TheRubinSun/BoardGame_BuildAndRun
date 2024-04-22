using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Название текущей локации
    public string NameLocation { get; set; }



    //Скорость камеры
    public float scrollSpeed = 300.0f;

    //Размер части экрана для движения
    public float scrollZoneSizeX;
    public float scrollZoneSizeY;

    //Координаты локаций
    List<Location>locations = new List<Location>();

    //Позиция игрока
    Vector3 player_pos;
    GameObject player;

    void Start()
    {
        locations.Add(new Location("Load location", -40f, -35f, -8f, 2f));
        locations.Add(new Location("Map one location", 35f, 110f, -20f, 25f));
        
        ToPlayer();
    }
    public void ToPlayer()
    {
        player = GameObject.Find(PhotonNetwork.NickName);
        player_pos = player.transform.position;
        transform.position = new Vector3(player_pos.x, player_pos.y, transform.position.z);
    }
    public void NewLocation(string newLocation)
    {
        NameLocation = newLocation;
        ToPlayer();
    }
    // Update is called once per frame
    void Update()
    {
        MoveCameraKeyboard();
    }
    void MoveCameraKeyboard()
    {
        //Напрвление движения
        float verticalMovement = 0f;
        float horizontalMovement = 0f;

        // Проверяем, находится ли курсор в зоне прокрутки по горизонтали
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalMovement = -1f;
        }
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalMovement = 1f;
        }

        // Проверяем, находится ли курсор в зоне прокрутки по вертикали
        if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.DownArrow))
        {
            verticalMovement = -1f;
        }
        else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow))
        {
            verticalMovement = 1f;
        }
        if (Input.GetKeyDown(KeyCode.K)) ToPlayer();
        if (locations[0] != null && Application.isFocused && (verticalMovement!=0 || horizontalMovement!=0))
            MoveCamera(new Vector3(horizontalMovement, verticalMovement, 0), locations[GetIdCurLocation()]);
    }
    void MoveCameraMouse()
    {
        // Получаем положение курсора в экранных координатах
        Vector3 mousePosition = Input.mousePosition;

        // Получаем размер экрана
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        //Размер части экрана для движения
        scrollZoneSizeX = Screen.width * 0.13f;
        scrollZoneSizeY = Screen.height * 0.13f;

        //Напрвление движения
        float verticalMovement = 0f;
        float horizontalMovement = 0f;

        // Проверяем, находится ли курсор в зоне прокрутки по горизонтали
        if (mousePosition.x < scrollZoneSizeX && (0 - scrollZoneSizeX * 2 < mousePosition.x))
        {
            horizontalMovement = -1f;
        }
        else if (mousePosition.x > screenSize.x - scrollZoneSizeX && (Screen.width + scrollZoneSizeX * 2 > mousePosition.x))
        {
            horizontalMovement = 1f;
        }

        // Проверяем, находится ли курсор в зоне прокрутки по вертикали
        if (mousePosition.y < scrollZoneSizeY && (0 - scrollZoneSizeY * 2 < mousePosition.y))
        {
            verticalMovement = -1f;
        }
        else if (mousePosition.y > screenSize.y - scrollZoneSizeY && (Screen.height + scrollZoneSizeY * 2 > mousePosition.y))
        {
            verticalMovement = 1f;
        }
        if (Input.GetKeyDown(KeyCode.K)) ToPlayer();
        if (locations[0] != null && Application.isFocused)
            MoveCamera(new Vector3(horizontalMovement, verticalMovement, 0), locations[GetIdCurLocation()]);
    }
    int GetIdCurLocation()
    {
        for(int i = 0;i<locations.Count;i++)
        {
            if (locations[i].name_location == NameLocation) return i;
        }
        return 0;
    }
    void MoveCamera(Vector3 direction, Location location)
    {
        // Изменяем позицию камеры на основе заданного направления
        Vector3 newPosition = transform.position + direction * scrollSpeed * Time.deltaTime;

        // Ограничиваем движение камеры в заданном диапазоне
        newPosition.x = Mathf.Clamp(newPosition.x, location.xMin, location.xMax);
        newPosition.y = Mathf.Clamp(newPosition.y, location.yMin, location.yMax);
        transform.position = newPosition;
        //transform.Translate(direction * scrollSpeed * Time.deltaTime);
    }
}

public class Location
{
    public string name_location { get; }
    public float xMin { get; set; }
    public float xMax { get; set; }
    public float yMin { get; set; }
    public float yMax { get; set; }
    public Location(string _name_location,float _xMin, float _xMax, float _yMin, float _yMax)
    {
        name_location = _name_location;
        xMin = _xMin;
        xMax = _xMax;
        yMin = _yMin;
        yMax = _yMax;
    }
}
