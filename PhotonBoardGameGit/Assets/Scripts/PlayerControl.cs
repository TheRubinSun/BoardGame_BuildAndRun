using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;


public class PlayerControl : MonoBehaviour, IPunObservable
{
    private PhotonView photonView;

    //Спрайты частей игрока
    private SpriteRenderer spriteRendererHead;
    private SpriteRenderer spriteRendererArms;
    private SpriteRenderer spriteRendererLazer;
    private SpriteRenderer spriteRendererPointer;

    public GameObject prefabRing;
    public Sprite ringYellow;
    public Sprite ringRed;
    public Sprite ringGreen;
    public Sprite ringBlue;
    public Sprite ringPurple;
    public Sprite ringOrange;
    //Анимации
    Animator animWheels;
    Animator animHead;
    Animator animLazer;

    //Цвета
    private Color32 colorPlayer;
    private Color32 colorLazer;
    public ColorsTeam colorTeam;
    //Лазерная точка
    public GameObject Pointer;

    public Vector2Int Direction;

    //Вид объекты частей игрока
    public GameObject face;
    public GameObject wheels;
    public GameObject head;
    public GameObject lazer;
    public GameObject arms;

    private LineRenderer lineRenderer;
    private LineRenderer lineRendererTwo;


    //Анимация лучей
    [SerializeField] private Texture[] textures;
    [SerializeField] private Texture[] texturesForPoint;
    [SerializeField] private float fps;
    private float fpsCounter;
    private int animationStep;
    private int animationStepTwo;


    private UIController inteface;

    GameObject cameraPlayer;

    bool readyStart = false;

    public int cellPosition;
    public int onNumbBridge;
    public int cellInBridge;

    public int countSawmill;

    public int currentCountSteps;
    public int maxCountSteps;
    public int lengthStep;
    private GameObject interfacePlayer;
    public TypePoint whatStayPoint;

    static float maxLazerRange = 12f;
    public int wood { get; set; }
    public int ore { get; set; }
    public int gold { get; set; }
    public int chanceFindGold { get; set; }
    public int streakNotFound { get; set; }
    //public bool readyStart;

    public Color GetColor()
    {
        return colorPlayer;
    }
    public GameObject AssignInterface(GameObject interfacePl)
    {
        GameObject interfaceInstance = Instantiate(interfacePl, Vector3.zero, Quaternion.identity);
        interfaceInstance.transform.SetParent(transform);  // Привязать интерфейс к игроку
        return interfaceInstance;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        bool ready = readyStart;
        bool setActPointer = Pointer.active;

        //bool rtTheMove = startTheMove;
        if (stream.IsWriting)
        {
            stream.SendNext(cellPosition);
            stream.SendNext(ready);
            stream.SendNext(whatStayPoint);
            stream.SendNext(setActPointer);
            stream.SendNext(onNumbBridge);
            stream.SendNext(cellInBridge);
        }
        else
        {
            cellPosition = (int)stream.ReceiveNext();
            readyStart = (bool)stream.ReceiveNext();
            whatStayPoint = (TypePoint)stream.ReceiveNext();
            Pointer.SetActive((bool)stream.ReceiveNext());
            onNumbBridge = (int)stream.ReceiveNext();
            cellInBridge = (int)stream.ReceiveNext();
        }
    }
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        animWheels = wheels.GetComponent<Animator>();
        animHead = head.GetComponent<Animator>();
        animLazer = lazer.GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRendererTwo = face.GetComponent<LineRenderer>();

    }
    private void Start()
    {
        inteface = GameObject.FindObjectOfType<UIController>();
        cameraPlayer = GameObject.FindGameObjectWithTag("MainCamera");
        ColorPlayer();
        ColorLazer();
        FindCamera();
        SettingLineRenderTwo();
    }

    void SettingLineRenderTwo()
    {
        lineRendererTwo.startWidth = 0.3f;
        lineRendererTwo.endWidth = 0.6f;
        lineRendererTwo.startColor = GetColor();
        lineRendererTwo.endColor = GetColor();
        lineRendererTwo.sortingOrder = 10;
        lineRendererTwo.positionCount = 2;
    }


    public void DrawLineToPoint(GameObject cell)
    {
        Transform transformPoint = cell.transform;
        Vector2 startPoint = new Vector2(transformPoint.position.x, transformPoint.transform.position.y);
        Vector2 endPoint = new Vector2(this.transform.position.x, this.transform.position.y);

        lineRendererTwo.SetPosition(0, startPoint);
        lineRendererTwo.SetPosition(1, endPoint);
    }
    public void HideLineToPoint()
    {
        Vector2 startPoint = new Vector2(this.transform.position.x, this.transform.transform.position.y);
        Vector2 endPoint = new Vector2(this.transform.position.x, this.transform.position.y);

        lineRendererTwo.SetPosition(0, startPoint);
        lineRendererTwo.SetPosition(1, endPoint);

    }
    public void DrawLineToPointAlways()
    {
        if (cellPosition > 0)
        {
            Transform transformPoint = MapControl.Instance.FindPoint(cellPosition).transform;
            Vector2 startPoint = new Vector2(transformPoint.position.x, transformPoint.transform.position.y);
            Vector2 endPoint = new Vector2(this.transform.position.x, this.transform.position.y);

            lineRendererTwo.SetPosition(0, startPoint);
            lineRendererTwo.SetPosition(1, endPoint);
        }
        else if (cellPosition == -1 && onNumbBridge != -1)
        {
            Transform transformPoint = MapControl.Instance.FindPointInBridge(onNumbBridge, cellInBridge).transform;
            Vector2 startPoint = new Vector2(transformPoint.position.x, transformPoint.transform.position.y);
            Vector2 endPoint = new Vector2(this.transform.position.x, this.transform.position.y);

            lineRendererTwo.SetPosition(0, startPoint);
            lineRendererTwo.SetPosition(1, endPoint);
        }
    }
    ///////////////////////////...Настройка параметров...///////////////////////////////////
    public void SetReady(bool ready)
    {
        readyStart = ready;
    }
    public void SetStartParamToPlayer(int _maxCountSteps, int _lengthStep,int _cellPosition,int _gold, int _wood, int _ore, int _chance)
    {
        cellPosition = _cellPosition;
        maxCountSteps = _maxCountSteps;
        currentCountSteps = 0;
        lengthStep = _lengthStep;
        onNumbBridge = -1;
        cellInBridge = -1;
        gold = _gold;
        wood = _wood;
        ore = _ore;
        chanceFindGold = _chance;
    }
    public void UpdateCountSteps()
    {
        currentCountSteps = maxCountSteps;
    }
    public void StartBoardGame()
    {
        interfacePlayer = GameObject.Find("Interface(Clone)");
        interfacePlayer.GetComponent<UIController>().ButtonReady.SetActive(false);
        interfacePlayer.GetComponent<UIController>().ButtonEndTurn.SetActive(readyStart);
        cameraPlayer.GetComponent<CameraController>().NewLocation("Map one location");
    }


    //Случаный цвет RGB 
    void ColorUpdateStepTwo(ref int one,ref int two, ref int three , int min , int max)
    {
        one = 255;
        int randromTwoThree = Random.Range(1, 3);
        if (randromTwoThree == 1)
        {
            two = Random.Range(min, max);
            three = min;
        }
        else
        {
            three = Random.Range(min, max);
            two = min;
        }
    }
    GetInfoDeleg takenAttrib;
    public void Handler(GetInfoDeleg del)
    {
        takenAttrib = del;
    }
    public void UpdateAtrValue()
    {
        string info = "";
        info += "Длина шага: " + lengthStep + "\n";
        info += "Всего шагов: " + maxCountSteps + "\n";
        info += "Осталось шагов: " + currentCountSteps + "\n";
        takenAttrib?.Invoke(info);
    }
    public void GetGold()
    {
        takenAttrib?.Invoke($"Gold: {gold}");
    }
    public void GetWood()
    {
        takenAttrib?.Invoke($"Wood: {wood}");
    }
    public void GetStone()
    {
        takenAttrib?.Invoke($"Ore: {ore}");
    }
    
    //Постоянный вызыв методов на управление персонажом игроком
    void Update()
    {
        //DrawLineToPointAlways();
        fpsCounter += Time.deltaTime;
        if(fpsCounter >= 1f /fps)
        {
            animationStep++;
            if (animationStep == textures.Length)
                animationStep = 0;
            lineRenderer.material.SetTexture("_MainTex", textures[animationStep]);

            animationStepTwo++;
            if (animationStepTwo == texturesForPoint.Length)
                animationStepTwo = 0;
            lineRendererTwo.material.SetTexture("_MainTex", texturesForPoint[animationStepTwo]);

            fpsCounter = 0f;

        }

        if (prefabRing.active)
        {
            if (photonView.IsMine)
            {
                PlayerRotation();
                PlayerRayCastsOnCursor();
            }
        }
        else
        {
            if (photonView.IsMine)
            {
                PlayerRotation();
                PlayerRayCastsOnCursor();
            }
        }


    }

    //Повороты персонажа
    void PlayerRotation()
    {
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, (rotZ - 90));
    }
    //Создать линию между игроком и наведенным объектом
    void PlayerRayCastsOnCursor()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition); //точка курсора на карте
        RaycastHit2D rayHit = Physics2D.Raycast(worldPoint, Vector2.zero, 5f); //Луч к точке на карте
        if (rayHit.transform != null && rayHit.transform.name != photonView.name) //Курсор   //Если курсор касаеться объекта
        {
            Pointer.SetActive(true); 
            if (rayHit.transform!=this.transform) Pointer.transform.position = rayHit.point;
            inteface.UpdateTextNameItemTextMouse(rayHit.collider.name);
            //Debug.DrawLine(transform.position, rayHit.point, Color.red);
            animLazer.SetBool("LazerON", true);

        }
        else
        {
            inteface.UpdateTextNameItemTextMouse("");
            Pointer.SetActive(false);
            animLazer.SetBool("LazerON", false);
        }
    }

    //Создать вектор перед игроком 
    public string PlayerRayCastsOnDirection(ref int smallDistance)
    {
        //float maxLazerRange = 6f;
        //Луч в сторону направления игрока на объект

        RaycastHit2D[] rayToward = Physics2D.RaycastAll(face.transform.position, transform.up, maxLazerRange); //Луч к точке на карте

        bool empty = true;
        lineRenderer.positionCount = 0;
        float desiredWidth = 2f; // Устанавливаем желаемую ширину линии

        //nameItemTextDirection.text = "";//Вывести все объекты по пути
        for (int i = 0; i < rayToward.Length; i++)
        {
            if (rayToward[i].collider.CompareTag("Player") || rayToward[i].collider.name == $"Point ({cellPosition})" )
                continue;
            else if (rayToward[i].collider.name == $"Point ({cellPosition + 1})"|| rayToward[i].collider.name == $"Point ({cellPosition - 1})")
                continue;
            else if(rayToward[i].collider.name.Contains("Bridge") || !rayToward[i].collider.name.Contains("Point") || rayToward[i].collider.CompareTag("PlayerParts"))
                continue;
            else if (MapControl.Instance.FindPointByName(rayToward[i].collider.name).GetComponent<PointSettings>().owner != "")
                continue;
            Debug.Log(rayToward[i].collider.name);
            if (rayToward[i].collider.name == this.name )
            {
                empty = true;
            }
            else
            { 
                inteface.UpdateTextNameItemTextDirection(rayToward[i].collider.name);
                if (MapControl.Instance.GetIntPoint(rayToward[i].collider.name)-cellPosition <= smallDistance)
                {
                    animHead.SetBool("RedOn", false);
                    Debug.DrawLine(face.transform.position, rayToward[i].transform.position, Color.red);
                    return rayToward[i].collider.name;
                }
                Debug.DrawLine(face.transform.position, rayToward[i].transform.position, Color.green);
                animHead.SetBool("RedOn", true);

                // Настройка LineRenderer
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = lineRenderer.endWidth = desiredWidth;
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                lineRenderer.SetPosition(0, face.transform.position);

                //Способ фиксация на объекте (как магнит)
                Vector2 pointCord = new Vector2(rayToward[i].transform.position.x, rayToward[i].transform.position.y);
                lineRenderer.SetPosition(1, pointCord);

                //Способ свободый режим (без намагничивания)
                //float rangeToPoint = Vector2.Distance(face.transform.position, rayToward[i].transform.position);
                //lineRenderer.SetPosition(1, face.transform.position + transform.up * rangeToPoint);

                empty = false;
                return rayToward[i].collider.name;
            }

        }

        if (empty)
        {
            // Настройка LineRenderer
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = lineRenderer.endWidth = desiredWidth;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.up * maxLazerRange);

            inteface.UpdateTextNameItemTextDirection("");
            //nameItemTextDirection.text = " ";
            Debug.DrawRay(face.transform.position, transform.up * maxLazerRange, Color.red);
            animHead.SetBool("RedOn", false);
            return null;
        }
        return null;
    }
    public void RayClear()
    {
        lineRenderer.positionCount = 0;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; // Цвет гизмо (вашего круга)
        Gizmos.DrawWireSphere(face.transform.position, maxLazerRange);
    }
    void FindCamera()
    {
        cameraPlayer = GameObject.Find("Main Camera");
    }
    //Случайный цвет персонажа
    void ColorPlayer()
    {
        spriteRendererHead = head.GetComponent<SpriteRenderer>();
        spriteRendererArms = arms.GetComponent<SpriteRenderer>();
        spriteRendererLazer = lazer.GetComponent<SpriteRenderer>();
        spriteRendererPointer = Pointer.GetComponent<SpriteRenderer>();

        int randromOneThree = Random.Range(1, 4);
        int a = 0;
        int b = 0;
        int c = 0;
        if (randromOneThree == 1) ColorUpdateStepTwo(ref a, ref b, ref c, 155, 255);
        else if (randromOneThree == 2) ColorUpdateStepTwo(ref b, ref a, ref c, 155, 255);
        else if (randromOneThree == 3) ColorUpdateStepTwo(ref c, ref b, ref a, 155, 255);
        colorPlayer = new Color32((byte)a, (byte)b, (byte)c, 255);

        spriteRendererHead.color = colorPlayer;
        spriteRendererArms.color = colorPlayer;
    }
    //Случайный цвет лазера персонажа
    void ColorLazer()
    {
        int randromOneThree = Random.Range(1, 4);

        int a = 0;
        int b = 0;
        int c = 0;
        if (randromOneThree == 1) ColorUpdateStepTwo(ref a, ref b, ref c, 0, 255);
        else if (randromOneThree == 2) ColorUpdateStepTwo(ref b, ref a, ref c, 0, 255);
        else if (randromOneThree == 3) ColorUpdateStepTwo(ref c, ref b, ref a, 0, 255);
        colorLazer = new Color32((byte)a, (byte)b, (byte)c, 255);
        spriteRendererLazer.color = colorLazer;
        spriteRendererPointer.color = colorLazer;
    }
}
