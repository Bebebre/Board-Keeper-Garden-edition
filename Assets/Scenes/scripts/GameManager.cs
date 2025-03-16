using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject QuitButton, bPiece, wPiece, 
        jpPiece, ppPiece, fbPiece, Mark_Piece, Text, 
        TextCount, TextPlane, Obvodka, ObvodkaAI, Posoh;
    [SerializeField]
    private BlackBrain B;              // То, где лежит скрипт чёрных шашек
    [SerializeField]
    private AIBlackSimple BS;
    [SerializeField]
    private AIBlackMedium BM;
    [SerializeField]
    private AIBlackHard BH;
    [SerializeField]
    private MoveBrain W;              // То, где лежит скрипт белых шашек

    [SerializeField]
    private GameObject Gazonokocilka; // Газонакосилка
    [SerializeField]
    private Animator CanvasAnimator;

    public float mouseSensitivity = 2f; // Чувствительность камеры
    public bool invertMouseInput = false; 

    public bool WoB_Move, GKill, GSpeed, GWindow;            // Информация - чей ход
    public GameObject[] Gases = new GameObject[8];             // Перечень газонокосилок
    public List<GameObject> Blacks = new();
    public Piece[,] All_Checker = new Piece[8, 8];    // МАТРИЦА - перечень всех шашек на экране
    public AudioSource audioButton;

    private Vector3 target = new(2.8f, 0f, 3.5f), DefaultPos, FastChanger, FilterObject;
    private RaycastHit hit;                 // Столкновение луча с объектом
    private Ray ray;                     // Луч
    private uint MovesCount;             // Счёт ходов
    private bool OneUse, GameStop, OnlyKill, MoveBack = false, KeyButtons = false;       // Защита от ложного переключения ходов и выключатель для завершения партии              // Cкрипт белых шашек
    private List<GameObject> Marks = new(); // Заспавненые чёрные шашки для консоли и восклицательные знаки соответственно
    private List<Vector3> FilterCoord = new(), WrongCoord = new();
    private System.Random random = new();
    private Animator Animator;
    private Quaternion DefaultRot;

    void Awake()                            // Запускается до появления чего бы то нибыло на экране
    {
        if (!PlayerPrefs.HasKey("GData"))
        {
            FilerRefriter();
        }
        else
        {
            string str = PlayerPrefs.GetString("GData");
            GKill = str[0] == 1;
            GSpeed = str[1] == 1;
            GWindow = str[2] == 1;
        }
        List<AudioSource> a = GetComponents<AudioSource>().ToList();
        for (sbyte e = 0; e < a.Count; e++) 
        if(a[e].playOnAwake == false) 
        {
            if (a[e].clip.name == "button") audioButton = a[e];
            a.RemoveAt(e);
        }
        if (1 == random.Next(1, 3)) a[0].Stop();
        else a[1].Stop();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 80;    // Фиксация FPS
        MovesCount = 1;
        WoB_Move = true;
        OneUse = GameStop = false;  
        Animator = GetComponent<Animator>();
    }
    public void Level0()
    {
        GameObject _New = null;
        for (int x = 0; x < 8; x++)         // Спавн белых и чёрных шашек 
        {
            Gases[x] = Instantiate(Gazonokocilka, new Vector3((float)-1.25, (float)-0.08, x), Quaternion.Euler(0, 180, 0));
            for (int z = 0; z < 8; z++)        // Перебор всех возможных клеток
            {
                if (x % 2 == z % 2) continue; // Если обе координаты клетки чётные или не чётные
                if (x < 3)
                {
                    _New = Instantiate(wPiece, new Vector3(x, 0f, z), Quaternion.Euler(0, 0, 0));
                    All_Checker[x, z] = _New.GetComponent<Piece>();
                    All_Checker[x, z].Manager = this;
                }
                if (x > 4)
                {
                    _New = Instantiate(bPiece, new Vector3(x, 0f, z), Quaternion.Euler(0, 180, 0));
                    All_Checker[x, z] = _New.GetComponent<Piece>();       // Заспавнить чёрную шашку и добавить в матрицу
                    All_Checker[x, z].Manager = this;
                    Blacks.Add(_New);
                }
            }
        }
        W.LightOn();
        Posoh.SetActive(true);
        W.RefreshItems();
    }
    public void LevelRemover()
    {
        for (byte x = 0; x < 8; x++)         // Спавн белых и чёрных шашек 
        {
            if (Gases[x])  Destroy(Gases[x]);
            for (byte z = 0; z < 8; z++)        // Перебор всех возможных клеток
            {
                if (All_Checker[x, z])
                {
                    Destroy(All_Checker[x, z].gameObject);
                }
            }
        }
        Gases = new GameObject[8];
        All_Checker = new Piece[8, 8];
        Blacks.Clear();
        W.LightOff();
        MovesCount = 1;
        TextCount.GetComponent<TMP_Text>().text = MovesCount.ToString();
        WoB_Move = W.WoB_Move = B.WoB_Move = true;
        OneUse = GameStop = false;
        for (int i = 0; i < Marks.Count; i++) Destroy(Marks[i]);
        Posoh.SetActive(false);
    }
    void Update()         // Срабатывает КАЖДЫЙ кадр
    {
        if (MoveBack) 
        {
            if (Vector3.Distance(transform.position, DefaultPos) == 0 && Quaternion.Angle(transform.rotation, DefaultRot) == 0) MoveBack = false;
            transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, DefaultPos, Time.deltaTime * 20), Quaternion.Lerp(transform.rotation, DefaultRot, Time.deltaTime * 5));
        }
        if (!GameStop)  // Если партия идёт
        {
            if (KeyButtons)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift)) MoveBack = true;
                if (Input.GetMouseButton(1) && (!MoveBack)) // Если зажата правая кнопка мыши
                {
                    transform.LookAt(target);
                    if (!(transform.position.y <= 2f && ((invertMouseInput && Input.GetAxis("Mouse Y") < 0) || (Input.GetAxis("Mouse Y") > 0 && !invertMouseInput)))) 
                        if (!(transform.position.y >= 9f && ((invertMouseInput && Input.GetAxis("Mouse Y") > 0) || (Input.GetAxis("Mouse Y") < 0 && !invertMouseInput))))
                            transform.RotateAround(target, transform.right, invertMouseInput? Input.GetAxis("Mouse Y") : -Input.GetAxis("Mouse Y") * mouseSensitivity);
                    transform.RotateAround(target, transform.up, invertMouseInput ? -Input.GetAxis("Mouse X") : Input.GetAxis("Mouse X") * mouseSensitivity);
                }
                if (Input.GetKeyDown(KeyCode.Tab)) CanvasAnim();
            }
            if (MovesCount == 1) //*********************************!!!!!!
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Запуск луча
                if (Physics.Raycast(ray, out hit)) //Если луч попал во что - то
                {
                    Piece shecker = hit.collider.gameObject.GetComponent<Piece>();
                    if (shecker && shecker.type > 0 && Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        FastChanger = shecker.transform.position;
                        sbyte BType = shecker.type;
                        for (sbyte i = 0; i < Blacks.Count; i++) // Перебираем перечень
                        {
                            if (Blacks[i].GetComponent<Piece>() == shecker)
                            {
                                Destroy(Blacks[i]);
                                switch (BType)
                                {
                                    case 1:
                                        Blacks[i] = Instantiate(jpPiece, FastChanger, Quaternion.Euler(0, 0, 0));
                                        break;
                                    case 2:
                                        Blacks[i] = Instantiate(ppPiece, FastChanger, Quaternion.Euler(0, 0, 0));
                                        break;
                                    case 3:
                                        Blacks[i] = Instantiate(fbPiece, FastChanger, Quaternion.Euler(0, 0, 0));
                                        break;
                                    case 8:
                                        Blacks[i] = Instantiate(bPiece, FastChanger, Quaternion.Euler(0, 0, 0));
                                        break;
                                    default:
                                        break;
                                }
                                Blacks[i].GetComponent<Piece>().Manager = this;
                                All_Checker[(sbyte)FastChanger.x, (sbyte)FastChanger.z] = Blacks[i].GetComponent<Piece>();
                            }
                        }
                    }
                }
            } //********************************************!!!!!!!
            if ((!W.WoB_Move) && !OneUse)  // Если скрипт белых закончил свой ход
            {
                if (MovesCount == 1 && ObvodkaAI) NoAI();
                WoB_Move = false;
                if (GSpeed) Filter();
                if (GKill) DrawChecker(); else OnlyKill = false;
                if (B) { B.OnlyKill = OnlyKill; B.WoB_Move = false; }
                else if (BS) BS.Move(); //!!!!!!!!!!!!!!!!!!
                else if (BM) BM.Move();
                else if (BH) BH.Move();
                OneUse = true;   // Переключить ход и включить защиту от повторного использования
                MovesCount++;     // +1 к счётчику ходов
                TextCount.GetComponent<TMP_Text>().text = MovesCount.ToString();  // Обновить счётчик на экране
            }
            if (WoB_Move && OneUse)  // Если скрипт чёрных закончил свой ход
            {
                if (GKill) DrawChecker(); else OnlyKill = false;
                W.OnlyKill = OnlyKill;
                W.WoB_Move = true; OneUse = false;   // Переключить ход и включить защиту от повторного использования
                W.LightOn();
                for (int i = 0; i < Blacks.Count; i++)
                {
                    Piece X = Blacks[i].GetComponent<Piece>();
                    X.UnTouched = true;
                    X.BannedCoords.Clear();
                    X.BannedCoords.Add(new(0, 2, 0));
                    X.Confused = false;
                    X.pryorited.Clear();
                }
                for (int i = 0; i < Marks.Count; i++) Destroy(Marks[i]);
                MovesCount++;          // +1 к счётчику ходов
                TextCount.GetComponent<TMP_Text>().text = MovesCount.ToString();    // Обновить счётчик на экране
            }
        }
    }
    public void FilerRefriter()
    {
        PlayerPrefs.SetString("GData", (GKill ? "1" : "0") + (GSpeed ? "1" : "0") + (GWindow ? "1" : "0"));
    }
    public void SetDefaultPos()
    {
        KeyButtons = true;
        DefaultPos = transform.position;
        DefaultRot = transform.rotation;
        NotAnimate();
    }
    public void NotAnimate()
    {
        Animator.enabled = false;
    }
    public void CanvasAnim()
    {
        if (CanvasAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hide"))
        {
            if (SceneManager.loadedSceneCount < 2)
            {
                if (KeyButtons || (Animator.enabled && Animator.GetCurrentAnimatorStateInfo(0).IsName("FlyCam"))) 
                CanvasAnimator.SetTrigger("Trigger");
            }
            else if(!Input.GetKeyDown(KeyCode.Tab)) CanvasAnimator.SetTrigger("Trigger");
        }
        else CanvasAnimator.SetTrigger("Trigger");
    }
    public void EndTurnButton() // ДОРАБОТАТЬ
    {
        if (SceneManager.loadedSceneCount < 2 && !Animator.enabled)
        {
            audioButton.Play();
            if (!GameStop && MovesCount != 1)  // Если партия идёт
            {
                if (W.WoB_Move == true)  W.EndTurn();   
                else if (WoB_Move == false && B)  B.EndTurn();  
            }
        }
    }
    private void DrawChecker() // Проверка на ничью
    {
        Piece I;
        bool i = OnlyKill = true; // Флаг с результатом
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // Перебор матрицы
            {
                I = All_Checker[x, z];
                if (WoB_Move)
                {
                    if (I && I.type == 0 && (I.isQween ? W.CheckMoveQween(I.gameObject, GKill) : W.CheckMoveChecker(I.gameObject, GKill)))
                    {
                        i = false; // Отменяем ничью если есть доступные ходы
                        break;
                    }
                }
                else
                {
                    if (I && I.type > 0 && (!I.Confused) && B.CheckMove(I.transform.position, I.type, GKill))
                    {
                        i = false; // Отменяем ничью если есть доступные ходы
                        break;
                    }
                }
            }
            if (!i) break; // Оптимизатор
        }
        if (i)
        if (GKill) 
        { 
            GKill = false;
            DrawChecker(); 
            OnlyKill = false; 
            GKill = true;
        }
        else Won(2);
    }
    public void Filter()
    {
        Piece I;
        List<Vector3> CurrentWay = new(), Total = new(); 
        for (sbyte i = 0; i < Blacks.Count; i++)
        {
            I = Blacks[i].GetComponent<Piece>(); 
            FilterCoord.Clear();
            if (I.speed > 1 || I.type == 3)
            {
                FilterObject = I.transform.position;
                if (I.speed > 1 && FilterCheckMove(FilterObject, OnlyKill)[0].y != -1)
                {
                    bool Kill = OnlyKill;
                    List<Vector3> Complete = new(), CompleteWays = new();
                    Vector3 X = I.transform.position;
                    bool flag3 = true, flag;
                    while (flag3)
                    {
                        List<Vector3> A = FilterCheckMove(X , Kill);
                        if (CurrentWay.Count == I.speed) 
                        {
                            CompleteWays = CompleteWays.Union(CurrentWay).ToList();
                            A.Insert(0, new(0, -1, 0));
                        }
                        if (A[0].y == -1)
                        {
                            flag = true;
                            while (CurrentWay.Count > 1)
                            {
                                WrongCoord.Add(X);
                                CurrentWay.Remove(X);
                                X = CurrentWay[^1];
                                if (CompleteWays.Contains(CurrentWay[^1])) 
                                { 
                                    flag = false; 
                                    if (!CompleteWays.Contains(WrongCoord[^1])) Total.Add(WrongCoord[^1]); 
                                }
                                if (FilterCheckMove(X, Kill)[0].y != -1)
                                {
                                    FilterObject.y = 1; break;
                                }
                                else WrongCoord.RemoveAt(WrongCoord.Count - 1);
                            }
                            if (FilterObject.y != 1)
                            {
                                if (CurrentWay.Count == 1)
                                {
                                    if (CompleteWays.Contains(CurrentWay[0])) flag = false;
                                    WrongCoord.Clear();
                                    Complete.Add(X);
                                    WrongCoord.AddRange(Complete);
                                    if (flag) Total.Add(X);
                                    X = I.transform.position;
                                    Kill = OnlyKill;
                                    CurrentWay.Clear();
                                }
                                else
                                {
                                    Complete.Add(WrongCoord[^1]);
                                }
                                if (FilterCheckMove(X, Kill)[0].y == -1) 
                                { 
                                    I.BannedCoords.AddRange(Total);
                                    Total.Clear();
                                    flag3 = false;
                                    continue;
                                }
                            }
                            else
                            {
                                if (flag) Total.Add(WrongCoord[^1]);
                                FilterObject.y = 0;
                            }
                        }
                        else if (A[0].z == -1)
                        {
                            CompleteWays = CompleteWays.Union(CurrentWay).ToList();
                            CompleteWays.Add(A[1]);
                            WrongCoord.Add(A[1]);
                        }
                        else if (A.Count > 1)
                        {
                            CurrentWay.Add(A[^1]);
                            X = A[^1];
                            A.Remove(A[^1]);
                            FilterCoord.AddRange(A);
                            Kill = false;
                        }
                        else
                        {
                            CurrentWay.Add(A[0]);
                            X = A[0];
                            Kill = false;
                        }
                    }

                }
            }
        }
    }
    private List<Vector3> FilterCheckMove(Vector3 Object, bool Kill) // Проверка хода для шашек
    {
        bool flag = false, flag2 = true;
        Piece I;
        List<Vector3> Return = new();
        sbyte x, z; // Вспомогательные счётчики
        x = (sbyte)Object.x;
        z = (sbyte)Object.z; // Оптимизируем проверку координат шашки
        if (FilterObject.y == -1) flag2 = false;
        if (FilterObject == Object && x == 0) 
        {
            if (z < 7 && (!WrongCoord.Contains(new(-1, 0, z + 1)))) 
            { Return.Add(new(0, 0, -1)); Return.Add(new(-1, 0, z + 1)); return Return; }
            if (z > 0 && (!WrongCoord.Contains(new(-1, 0, z - 1))))
            { Return.Add(new(0, 0, -1)); Return.Add(new(-1, 0, z - 1)); return Return; }
        }  // Ход возможен, так как мы впритык к дому
        if (FilterObject == Object && x == 2 && All_Checker[(sbyte)FilterObject.x, (sbyte)FilterObject.z].type == 2)
        {
            try
            {
                if (z < 5 && All_Checker[1, z + 1] && (!All_Checker[0, z + 2]) && (!WrongCoord.Contains(new(-1, 0, z + 3))))
                { Return.Add(new(0, 0, -1)); Return.Add(new(-1, 0, z + 3)); return Return; }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (z > 2 && All_Checker[1, z - 1] && (!All_Checker[0, z - 2]) && (!WrongCoord.Contains(new(-1, 0, z - 3))))
                { Return.Add(new(0, 0, -1)); Return.Add(new(-1, 0, z - 3)); return Return; }
            }
            catch (IndexOutOfRangeException) { }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                x = (sbyte)((sbyte)Object.x + w);
                z = (sbyte)((sbyte)Object.z + b); // Оптимизация проверки диоганалей
                try
                {
                    try
                    {
                        I = All_Checker[x, z];
                        if (All_Checker[(sbyte)FilterObject.x, (sbyte)FilterObject.z].type == 2 && I.transform.position != FilterObject && 
                        (!WrongCoord.Contains(new(x + w * 2, 0, z + b * 2))) && (!All_Checker[x + w, z + b]) && (!All_Checker[x + w * 2, z + b * 2]))
                        {
                            Return.Add(new(0, 0, -1)); Return.Add(new(x + w * 2, 0, z + b * 2)); return Return;
                        }
                    }
                    catch (NullReferenceException) { }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = All_Checker[x, z];
                    if (I && ((!All_Checker[x + w, z + b]) || All_Checker[x + w, z + b].transform.position == FilterObject) && !FilterCoord.Contains(I.transform.position))
                    {
                        if (All_Checker[(sbyte)FilterObject.x, (sbyte)FilterObject.z].type == 2 && I.transform.position != FilterObject)
                        {
                            if (!WrongCoord.Contains(new(x + w, 0, z + b))) { Return.Add(new(0, 0, -1)); Return.Add(new(x + w, 0, z + b)); return Return; }
                        }
                        else if (I.type == 0) 
                        { 
                            if (!WrongCoord.Contains(new(x + w, 0, z + b)))
                            {
                                FilterCoord.Add(I.transform.position);
                                FilterObject.y = -1;
                                if (FilterCheckMove(new(x + w, 0, z + b), true)[0].y == -1)
                                { Return.AddRange(FilterCoord); Return.Add(new(x + w, 0, z + b)); return Return; }
                                FilterObject.y = 0;
                                FilterCoord.Remove(I.transform.position);
                            }
                            else if (!flag2) flag = true;
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    if (((FilterCoord.Contains(new(x, 0, z)) && flag2) || !All_Checker[x, z]) && (!WrongCoord.Contains(new(x, 0, z))) && w == -1 && !Kill)
                    {
                        Return.Add(new(x, 0, z)); 
                        return Return;
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        if (flag) Return.Add(new(-1, 0, 0));
        else Return.Add(new(0, -1, 0));
        return Return; //Ход не возможен
    }
    public void Won(sbyte i)  // Вызывается по завершении партии
    {
        if (B) B.WoB_Move = true;
        W.WoB_Move = false;    // Оба скрипта переключаются на не свои ходы
        GameStop = true;      // Включение завершения партии
        TextPlane.GetComponent<MeshRenderer>().enabled = true;            // Включение белого фона
        if (i == 0) Text.GetComponent<TMP_Text>().text = "White won!";
        if (i == 1) Text.GetComponent<TMP_Text>().text = "Black won!";
        if (i == 2) Text.GetComponent<TMP_Text>().text = "Draw!";          // Вывод победителя на экран
    }
    public void Confus(Vector3 a)
    {
        GameObject b = Instantiate(Mark_Piece, new Vector3(a.x, 1f, a.z), Quaternion.Euler(0, 0, 0));
        Marks.Add(b);
    }
    public IEnumerator Quit()
    {
        KeyButtons = false;
        audioButton.Play();
        MoveBack = true;
        Obvodka.SetActive(true);
        ObvodkaAI.SetActive(true);
        while (MoveBack)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Animator.enabled = true;
        Animator.SetBool("Fly", false);
        yield return new WaitForSeconds(1f);
        CanvasAnimator.GetComponent<MenuAndPreferenses>().RunMenu = true;
    }
    public void Black()   // Кнопка сброс
    {
        Vector3 piece;
        if (MovesCount == 1)   // Если первый ход
        {
            for (int i = 0; i < Blacks.Count; i++)   // Проходимся по всем чёрным шашкам
            {
                piece = Blacks[i].transform.position;
                Destroy(Blacks[i].gameObject);
                Blacks[i] = Instantiate(bPiece, piece, Quaternion.Euler(0, 0, 0));
                All_Checker[(sbyte)piece.x, (sbyte)piece.z] = Blacks[i].GetComponent<Piece>();
            }
        }
    }
    public void AIKillBridge(GameObject Object)
    {
        if (B) B.WoB_Move = false;
        else if (BS) BS.EndMoving(Object); 
        else if (BM) BM.EndMoving(Object);
        else if (BH) BH.EndMoving(Object);
    }
    public void NoAI() 
    {
        Obvodka.SetActive(false);
        ObvodkaAI.SetActive(false);
        Destroy(BS);
        Destroy(BM);
        Destroy(BH);
    }
    public void AISimple()
    {
        BS.Starting();
        Obvodka.SetActive(false);
        ObvodkaAI.SetActive(false);
        Destroy(B);
        Destroy(BM);
        Destroy(BH);
    }
    public void AIMedium()
    {
        BM.Starting();
        Obvodka.SetActive(false);
        ObvodkaAI.SetActive(false);
        Destroy(B);
        Destroy(BS);
        Destroy(BH);
    }
    public void AIHard()
    {
        BH.Starting();
        Obvodka.SetActive(false);
        ObvodkaAI.SetActive(false);
        Destroy(B);
        Destroy(BS);
        Destroy(BM);
    }
    public IEnumerator Reborn()
    {
        audioButton.Play();
        yield return new WaitForSeconds(0.2f);
        LevelRemover();
        Level0(); // Перезапускаем игру
    }
    public void Jumper() // ДОРАБОТАТЬ
    {
        Vector3 jumper;
        if (MovesCount == 1) // Если первый ход
        {
            sbyte x = (sbyte)random.Next(0, Blacks.Count), y = 0; // Выбираем рандомную шашку
            while (Blacks[x].GetComponent<Piece>().type != 1 && y < 40) // Если 20 раз не натыкаемся на обычную шашку...
            {
                x = (sbyte)random.Next(0, Blacks.Count); // Выбираем рандомную шашку
                y++;
            }
            if (y > 39) // То берём первую встречную в перечне
            {
                for (sbyte i = 0; i < Blacks.Count; i++) // Перебираем перечень
                {
                    if(Blacks[i].GetComponent<Piece>().type == 1) // Если нашли чёрную
                    {
                        x = i; break;
                    }
                } 
            }
            jumper = Blacks[x].transform.position;
            Destroy(Blacks[x].gameObject);
            Blacks[x] = Instantiate(jpPiece, jumper, Quaternion.Euler(0, 0, 0));
            All_Checker[(sbyte)jumper.x, (sbyte)jumper.z] = Blacks[x].GetComponent<Piece>();
            Blacks[x].GetComponent<Piece>().Manager = this;
        }
    }
    public void Football() // Кнопка +1 футболист
    {
        Vector3 football;
        if (MovesCount == 1) // Если первый
        {
            sbyte x = (sbyte)random.Next(0, Blacks.Count), y = 0; // Выбираем рандомную шашку
            while (Blacks[x].GetComponent<Piece>().type != 1 && y < 40) // Если 20 раз не натыкаемся на обычную шашку...
            {
                x = (sbyte)random.Next(0, Blacks.Count); // Выбираем рандомную шашку
                y++;
            }
            if (y > 39) // То берём первую встречную в перечне
            {
                for (sbyte i = 0; i < Blacks.Count; i++) // Перебираем перечень
                {
                    if (Blacks[i].GetComponent<Piece>().type == 1) // Если нашли чёрную
                    {
                        x = i; break;
                    }
                }
            }
            football = Blacks[x].transform.position;
            Destroy(Blacks[x].gameObject);
            Blacks[x] = Instantiate(fbPiece, football, Quaternion.Euler(0, 0, 0));
            All_Checker[(sbyte)football.x, (sbyte)football.z] = Blacks[x].GetComponent<Piece>();
            Blacks[x].GetComponent<Piece>().Manager = this;
        }
    }
    public void Paper() // Кнопка 
    {
        Vector3 paper;
        if (MovesCount == 1)
        {
            sbyte x = (sbyte)random.Next(0, Blacks.Count), y = 0; // Выбираем рандомную шашку
            while (Blacks[x].GetComponent<Piece>().type != 1 && y < 40) // Если 20 раз не натыкаемся на обычную шашку...
            {
                x = (sbyte)random.Next(0, Blacks.Count); // Выбираем рандомную шашку
                y++;
            }
            if (y > 39) // То берём первую встречную в перечне
            {
                for (sbyte i = 0; i < Blacks.Count; i++) // Перебираем перечень
                {
                    if (Blacks[i].GetComponent<Piece>().type == 1) // Если нашли чёрную
                    {
                        x = i; break;
                    }
                }
            }
            paper = Blacks[x].transform.position;
            Destroy(Blacks[x].gameObject);
            Blacks[x] = Instantiate(ppPiece, paper, Quaternion.Euler(0, 0, 0));
            All_Checker[(sbyte)paper.x, (sbyte)paper.z] = Blacks[x].GetComponent<Piece>();
            Blacks[x].GetComponent<Piece>().Manager = this;
        }
    }
}
