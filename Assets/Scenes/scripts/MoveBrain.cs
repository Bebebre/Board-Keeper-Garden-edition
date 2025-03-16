using System;
using System.Collections.Generic;
using UnityEngine; // Это всяческие штуки для того чтобы юнити смог всё правильно прочитать

public class MoveBrain : MonoBehaviour
{
    [SerializeField]
    private GameObject Qween; 
    [SerializeField]
    private GameObject Plane; // Плитка
    [SerializeField]
    private GameObject GrayPlane; // Серая плитка
    [SerializeField]
    private GameObject Light; // Образец подсветки
    [SerializeField]
    private GameManager Manager; // Обьект с гейм менеджером

    private System.Random random = new();
    private List<Piece> Whites = new();
    public sbyte QPosoh = 3, QPosohCD = 0;
    private GameObject Clicked;   // Оптимизатор для курсора
    private List<GameObject> Planes = new(), Lights = new();          // Перечени плиток и подсветок
    private bool Continue, Lighting, End, Blocked, Select;  // Флаг обозначяющий возможность мульти убийства, флаг разрешающий мульти убийство, флаг конца хода, флаг разрешающий выбор шашки и флаг проверки на ничью
    public bool WoB_Move, OnlyKill;                 // Локальный флаг хода
    private Piece Current, Currented;     // Подсвеченая и выбраная шашки
    private Vector3 PlaneCoordinate;           // Координаты плиты для совершения хода
    private RaycastHit hit;                 // Столкновение луча с объектом
    private Ray ray;                     // Луч

    void Awake()           // Запускается до появления чего бы то нибыло на экране
    {
        Continue = End = Blocked =  false;
        Select = Lighting = WoB_Move = true;
    }
    void Update()        // Срабатывает КАЖДЫЙ кадр  
    {
        if (WoB_Move) //Скрипт активен только в свой ход
        {
// ************************************    Механизм выделения шашки под курсором    **********************************************************
            ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Запуск луча
            if (Physics.Raycast(ray, out hit)) //Если луч попал во что - то
            {            
                Piece shecker = hit.collider.gameObject.GetComponent<Piece>(); //Запись шашки в которую попали
                if (shecker && shecker.Stat && Select) //Если попали в статичную шашку и никто ещё не двигался на этом ходу
                { 
                    if (hit.collider.gameObject.GetComponent<Piece>().type == 0 && Current != shecker) // Проверка на белых (никакого рассизма) и отличие шашки под курсором от текущей серой шашки
                    {
                        if (Current) Non(); // Вернуть серую шашку в прежнее состояние если она есть
                        Current = shecker; // Обозначаем нашу шашку как серую
                        shecker.Select(); // Перекрашиваем
                    }
                }
                else Non();  // Сброс серой шашки
            }
            else Non();  // Сброс серой шашки
// *************************   Механизм проверки конца хода и запуска мульти убийства   ********************************************
            if (Currented && Currented.Stat) // Если передвигаемая нами шашка остановилась
            {
                if (Currented.transform.position.x == 7 && !Currented.isQween) QweenMaker(false);
                if (Blocked) // Если был доступен ход с убийством
                {
                    if (Currented.Kill) // Проверяем наличие убитой шашки
                    {
                        if(Currented.isQween ? CheckMoveQween(Currented.gameObject, true) : CheckMoveChecker(Currented.gameObject, true)) // Если есть доступные ходы (убийство обязательно)
                            Selected(Currented);              // Спавним плитки для текущей шашки
                        else 
                            End = true;    // Иначе конец хода
                        Blocked = false;  // Защищаемся от повторного использования
                        Currented.Kill = false;  // Устанавливаем в текущую шашку "никого" в качестве убитого
                    }
                    else End = true; // Иначе запускаем окончание хода
                }
                if (End) EndTurn(); // Заканчиваем ход если требуется
            }
//*****************************************************   Механизм нажатия ЛКМ   *************************************************************
            if (Input.GetKeyDown(KeyCode.Mouse0) && hit.collider)  // Если мы нажали ЛКМ на что - то
            {
                Clicked = hit.collider.gameObject;     // Оптимизируем проверки нажатого объекта
                if (Clicked.CompareTag("Plane")) //Нажатие на плиту
                {
                    PlaneCoordinate = Clicked.transform.position; // Оптимизация проверки координат плитки
                    Currented.PointToMove = PlaneCoordinate; // Запускаем движение шашки, отправляя ей координаты плитки
                    Currented.Stat = false;  // Защищаемся от прежде временного запуска механизма окончания хода
                    Manager.All_Checker[(sbyte)Currented.transform.position.x, (sbyte)Currented.transform.position.z] = null; // Удаляем не актуальные координаты шашки из матрицы
                    Manager.All_Checker[(sbyte)PlaneCoordinate.x, (sbyte)PlaneCoordinate.z] = Currented; // Записываем текущие координаты шашки в матрицу
                    if (Continue) Blocked = true; else End = true; // Записываем - можем ли мы совершить мульти убийство в этом ходу
                    PlaneBust();        // Анегилируем все плитки
                    Select = false;     // Запрещаем выбор шашки курсором
                }
                if (Select) // Если выбор шашки разрешен  
                {
                    if (Clicked.name == "QPosoh" && QPosohCD == 0 && QPosoh > 0) QweenMaker(true);
                    if (Currented) //Если есть уже выбраная
                    {
                        if (Clicked && Currented.transform.position != Clicked.transform.position) // Если выбраная шашка не равна нажатой
                        {
                            if (Clicked.GetComponent<Piece>() && Clicked.GetComponent<Piece>().type == 0) // Если кликнули на шашку и она белая
                            {
                                PlaneBust();                  // Анегилируем все плитки
                                Selected(Clicked.GetComponent<Piece>());          // Спавним плитки для нашей шашки
                            }
                        }
                        else //Если навели на выделенную шашку 
                        {
                            PlaneBust();               // Анегилируем все плитки
                            if (Lights.Count == 0) LightOn(); // Если нет подсвеетки, то заспавнить её
                            Currented = null;     // Обнуляем выбраную шашку
                        }
                    }
                    else if (Current) Selected(Current);//Если навели на шашку
                }
                DebugMatrice();   // Это используется для проверки матрицы
                Clicked = null;     // Обнуляем оптимизатор
            }
//*****************************************************************************************************************************************
        }
    }
    private void GenerateWhites()
    {
        Whites.Clear();
        for (int x = 0; x < 8; x++)         // Спавн белых и чёрных шашек 
        {
            for (int z = 0; z < 8; z++)        // Перебор всех возможных клеток
            {
                if (Manager.All_Checker[x, z] && Manager.All_Checker[x, z].type == 0) Whites.Add(Manager.All_Checker[x, z]);
            }
        }
    }
    private void QweenMaker(bool Rand)
    {
        if (Rand)
        {
            LightOff();
            GenerateWhites();
            sbyte x = (sbyte)random.Next(0, Whites.Count), y = 0; // Выбираем рандомную шашку
            while (Whites[x].GetComponent<Piece>().type != 0 && y < 40) // Если 20 раз не натыкаемся на обычную шашку...
            {
                x = (sbyte)random.Next(0, Whites.Count); // Выбираем рандомную шашку
                y++;
            }
            if (y > 39) // То берём первую встречную в перечне
            {
                for (sbyte i = 0; i < Whites.Count; i++) // Перебираем перечень
                {
                    if (Whites[i].GetComponent<Piece>().type == 0) // Если нашли чёрную
                    {
                        x = i; break;
                    }
                }
            }
            Vector3 A = Whites[x].transform.position;
            Destroy(Whites[x].gameObject);
            Manager.All_Checker[(sbyte)A.x, (sbyte)A.z] = Instantiate(Qween, A, Quaternion.Euler(0, 0, 0)).GetComponent<Piece>();
            QPosohCD = 2;
            QPosoh--;
            LightOn();
        }
        Blocked = End = false;
        if (Currented.Kill) Blocked = true;
        PlaneCoordinate = new(7, 0, (sbyte)Currented.transform.position.z);
        Destroy(Manager.All_Checker[7, (sbyte)Currented.transform.position.z].gameObject);
        Currented = Instantiate(Qween, PlaneCoordinate, Quaternion.Euler(0, 0, 0)).GetComponent<Piece>();
        Manager.All_Checker[7, (sbyte)PlaneCoordinate.z] = Currented;
        if (Blocked && CheckMoveQween(Currented.gameObject, true))
            Selected(Currented);              // Спавним плитки для текущей шашки
        else
            End = true;    // Иначе конец хода
        Blocked = false;
    }
    public void RefreshItems()
    {
        QPosoh = 3;
        QPosohCD = 0;
    }
    private void DebugMatrice()   // Проверка содержания матрицы
    {
        string matrice = " "; // Текст с матрицей
        for (sbyte x = 7; x > -1; x--)
        {
            for (sbyte z = 7; z > -1; z--) // Перебираем матрицу
            {
                if (!Manager.All_Checker[x, z]) matrice += "0 "; // 0 - пусто
                else if (Manager.All_Checker[x, z].type == 0) matrice += "3 "; // 3 - белая
                else if (Manager.All_Checker[x, z].type > 0) matrice += "2 ";  // 2 - чёрная
            }
            matrice += "\n "; // Перенос строки каждые 8 символов
        }
        Debug.Log(matrice); // Вывод результата
        // Вывод:
        // 0 2 0 2 0 2 0 2
        // 2 0 2 0 2 0 2 0
        // 0 2 0 0 0 2 0 2
        // 0 0 0 0 2 0 0 0
        // 0 0 0 3 0 0 0 0
        // 3 0 0 0 3 0 3 0
        // 0 3 0 3 0 3 0 3
        // 3 0 3 0 3 0 3 0
    }
    public void EndTurn()   // Запускается для окончания хода
    {
        bool i = true;   // Флаг для проверки на победу
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // Перебор матрицы
            {
                if (Manager.All_Checker[x, z] && Manager.All_Checker[x, z].type > 0 && !Manager.All_Checker[x, z].Confused) 
                { 
                    i = false;  // Отключить объявление победы если жива хоть одна чёрная шашка
                    break;
                }
            }
            if (!i) break; // Оптимизатор
        }
        if (i) Manager.Won(0); // Обьявить победу если убиты все чёрные шашки
        PlaneBust();  // Возращаем все параметры...
        End = false;
        LightOff();
        if (Currented) Currented.Deselect();
        Currented = null;
        Continue = Blocked = WoB_Move = false;
        Lighting = Select = true;  // в начальную позицию
        if (QPosohCD > 0) QPosohCD--;
    }
    private void Non() // Запускается если не навели курсор на шашку
    {
        if (Current)
            Current.Deselect(); //Перекрашиваем шашку Current если она есть
        Current = null; // Обнуляем Current
    }
    private void PlaneBust() // Запускается для анигиляции плит 
    {
        for (byte y = 0; y < Planes.Count; y++)
        {
            Destroy(Planes[y]); // Перебор и анигиляция плит
        }
        Planes.Clear(); // Очистка перечня плит
    }
    private void Selected(Piece piece) // Запускается для спавна плит
    {
        Continue = false;
        if (piece.isQween && CheckMoveQween(piece.gameObject, OnlyKill)) // Если есть доступные ходы дамки(убийство не обязательно)
        {
            LightOff();  // Выключить подсветку
            Currented = piece;   // Делаем шашку выбранной
            if (OnlyKill) Select = false;
            PlaneSpawnQween(Currented.gameObject, new(), 0, 0); // Запускаем спавн плит
            if (OnlyKill) Select = true;
        }
        else if ((!piece.isQween) && CheckMoveChecker(piece.gameObject, OnlyKill)) // Если есть доступные ходы шашки(убийство не обязательно)
        {
            LightOff();  // Выключить подсветку
            Currented = piece;   // Делаем шашку выбранной
            if (OnlyKill) Select = false;
            PlaneSpawnChecker(Currented.gameObject, new()); // Запускаем спавн плит
            if (OnlyKill) Select = true;
        }
        else if (Lights.Count == 0) { LightOn(); Currented = null; } // Если нет ходов и подсветки, то заспавнить подсветку и обнулить выбраную шашку
    }
    public void LightOff() // Выключатель подсветки
    {
        for (byte i = 0;i < Lights.Count; i++) // Перебираем подсветку
        {
            Destroy(Lights[i]); // Уничтожаем подсветку
        }
        Lights.Clear(); // Чистим перечень
    }
    public void LightOn() // Включатель подсветки и система проверки на ничью
    {
        Piece I;
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // Перебор матрицы
            {
                I = Manager.All_Checker[x, z];
                if (I && I.type == 0 && (I.isQween ? CheckMoveQween(I.gameObject, OnlyKill) : CheckMoveChecker(I.gameObject, OnlyKill)))
                { 
                    Lights.Add(Instantiate(Light, new Vector3(x - 0.2f, 0, z), Quaternion.Euler(0, 0, 0))); // Если наткнулись на белую шашку, которая может ходить
                }
            }
        }
    }
    private bool Find(List<Vector3> A, int x, int z) // Проверка на наличие координат в перечне
    {
        Vector3 V = new(x, 0, z); // Создание координат для проверки
        for (sbyte i = 0; i < A.Count; i++)
        {
            if (A[i] == V) return true; // Перебираем перечень и возвражаем "Да" если нашли совпадение
        }
        return false; // Возвращаем "Нет"
    }
    private bool FindForDestroyPlane(int x, int z) // Проверка на наличие координат в перечне
    {
        Vector3 V = new(x, 0, z); // Создание координат для проверки
        bool flag = true;
        for (sbyte i = 0; i < Planes.Count; i++)
        {
            if (Planes[i].transform.position == V) 
            {
                if (Planes[i].CompareTag("Plane")) flag = false;
                Destroy(Planes[i]);
                Planes.Remove(Planes[i]); // Находим и уничтожаем плиту  
                break;
            } 
        }
        return flag;
    }
    public bool CheckMoveChecker(GameObject Object, bool Kill) // Проверка хода для шашки
    {
        Piece I;
        sbyte x, z; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2) 
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор диоганалей
            {
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизируем проверку диоганалей
                try
                {
                    I = Manager.All_Checker[x, z];
                    if (I && (!Manager.All_Checker[x + w, z + b]) && (I.type > 0) && I.UnTouched) return true; // Если можно кого - то убить, то ход возможен
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Manager.All_Checker[x, z];
                    if ((!I) && w == 1 && !Kill) return true; // Если убийство не обязательно и спереди пусто, то ход возможен
                }
                catch (IndexOutOfRangeException) { } // Эти штуки позволяют не ломать программу если мы выйдем за пределы перечня
            }
        }
        return false; // Ход не возможен
    }
    public bool CheckMoveQween(GameObject Object, bool Kill)  // Проверка хода для дамки
    {
        Vector3 This = Object.transform.position; // Координаты объекта
        Piece I;
        bool Old_Kill = Kill;
        sbyte breaks, x, z; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // перебор диоганалей
            { 
                breaks = 0; // Это счётчик для определения границ передвижения
                Kill = Old_Kill;
                for (sbyte p = 1; p < 9; p++) // Перебор клеток во всей диоганали
                {
                    x = (sbyte)((sbyte)This.x + (p * w));
                    z = (sbyte)((sbyte)This.z + (p * b)); // Оптимизируем проверку диоганалей
                    try
                    {
                        I = Manager.All_Checker[x, z];
                        if (!I && breaks == 1) breaks--; // Если клетка пуста то счётчик на 0
                        if (I && (I.type == 0)) breaks = 2; // Если есть белая шашка, то счётчик на 2
                        if (I && I.type > 0 && I.UnTouched) { breaks++; Kill = false; } // Если есть чёрная шашка, то счётчик +1 и выключить флаг обязательного убийства
                        if (breaks == 0 && !Kill) return true; // Если счётчик 0 и флаг обязательного убийства выключен, то ход возможен
                        if (breaks == 2) break; // Остановить проверку текущей диоганали если счётчик 2 
                    }
                    catch (IndexOutOfRangeException) { break; } // Остановить проверку текущей диоганали если вышли за пределы доски
                }
            }
        }
        return false; // Ход не возможен
    }
    // Работа спавнеров:
    // Получил шашку -> заспавнил плиты -> отправил самому себе заспавненые плиты -> заспавнил серые плиты относительно них -> отправил самому себе заспавненые серые плиты... и так по кругу
    private void PlaneSpawnQween(GameObject Object, List<Vector3> AllThings, sbyte BlockVectorX, sbyte BlockVectorZ) // Спавнер плит для дамки
    {
        Vector3 This = Object.transform.position; // Координаты объекта
        GameObject plane;  // Вспомогательный контейнер
        Piece piece = Object.GetComponent<Piece>(), I;
        bool spawn = true; // Флаг разрешающий спавн плит на диоганали
        bool killing; // Флаг отделяющий плиты обычного хода и хода с убийством
        sbyte x, z, breaks, statical = (sbyte)AllThings.Count; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор  
            {
                if (w == BlockVectorX && b == BlockVectorZ) continue; // Если это запрещённый вектор, то пропускаем его
                breaks = 0; // Счётчик на ноль
                killing = false; // Флаг убийства на ноль
                for (sbyte p = 1; p < 9; p++) // Перебор каждой клетки диоганали
                {
                    x = (sbyte)((sbyte)This.x + (p * w));
                    z = (sbyte)((sbyte)This.z + (p * b)); // Оптимизация проверки диоганалей
                    try
                    {
                        I = Manager.All_Checker[x, z];
                        if (!I) // Если клетка пуста
                        {
                            if (breaks == 1) { killing = true; breaks = 0; } // Если до этого была чёрная шашка, то счётчик ноль
                            spawn = true; // Разрешить спавн
                        }
                        if (I && (I.type == 0)) // Если белая шашка
                        { 
                            breaks = 2;  // Счётчик на два
                            spawn = false; // Запретить спавн
                        }
                        if (I && I.type > 0 && I.UnTouched) // Если чёрная шашка
                        { 
                            spawn = false; // Запретить спавн
                            if (!Find(AllThings, x, z)) // Если мы не сталкивались с этой шашкой
                            {
                                breaks++; // Счётчик +1
                                killing = true; // Включить флаг убийств
                                AllThings.Add(I.gameObject.transform.position); // Сохраняем в перечень
                            }
                            else if (breaks == 1) { breaks = 0; } // Иначе считаем клетку пустой
                        }
                        if (spawn) // Если разрешён спавн и с этой клеткой раньше не сталкивались
                        {
                            if ((piece && Select) || killing) // Если убийство или обычный ход
                            {
                                if (FindForDestroyPlane(x, z)) plane = Instantiate(piece ? Plane : GrayPlane, new Vector3(x, 0, z), Quaternion.Euler(0f, 0f, 0f)); // Спавним плиту
                                else plane = Instantiate(Plane, new Vector3(x, 0, z), Quaternion.Euler(0f, 0f, 0f));
                                Planes.Add(plane); // Добавляем плиту в перечень
                                if (!piece) Continue = true; // Если объект - плита, то включить флаг мульти убийства
                                if (killing) PlaneSpawnQween(plane, AllThings, (sbyte)(w * -1), (sbyte)(b * -1)); // Вызываем спавнер в спавнере(Да, так можно)
                            }
                        }
                        if (breaks == 2) { break;}// Если счётчик 2 - остановить проверку диоганали
                    }
                    catch (IndexOutOfRangeException) { break; } // Остановить проверку диоганали если вышли за пределы
                }
                while (statical < AllThings.Count) AllThings.RemoveAt(AllThings.Count - 1);
            }
        }
    }
    private void PlaneSpawnChecker(GameObject Object, List<Vector3> AllThings) // Спавнер плит для шашки
    {
        Vector3 This = Object.transform.position; // Координаты объекта
        Piece piece = Object.GetComponent<Piece>(), I;
        GameObject plane; // Вспомогательный контейнер
        sbyte x = (sbyte)This.x, z = (sbyte)This.z; // Вспомогательные счётчики
        if (piece && Select) // Если обычный ход
        {
            try
            {
                if (!Manager.All_Checker[x + 1, z + 1]) // Если клетка пуста
                {
                    plane = Instantiate(Plane, new Vector3(x + 1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // Заспавнить плиту
                    Planes.Add(plane); // Добавляем плиту в перечень
                }
            }catch(IndexOutOfRangeException) { } // Эти штуки позволяют не ломать программу если мы выйдем за пределы перечня
            try
            {
                if (!Manager.All_Checker[x + 1, z - 1]) // Если клетка пуста
                {
                    plane = Instantiate(Plane, new Vector3(x + 1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // Заспавнить плиту
                    Planes.Add(plane); // Добавляем плиту в перечень
                }
            }catch(IndexOutOfRangeException) { } // Эти штуки позволяют не ломать программу если мы выйдем за пределы перечня
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор диоганалей 
            {
                x = (sbyte)((sbyte)This.x + w);
                z = (sbyte)((sbyte)This.z + b); // Оптимизируем проверку диоганалей
                try 
                {
                    I = Manager.All_Checker[x, z];
                    if (I && I.type > 0 && (!Manager.All_Checker[x + w, z + b]) && (!Find(AllThings, x, z)) && I.UnTouched) // Если на клетке чёрная шашка, за ней пусто и она не в перечне
                    {
                        AllThings.Add(new(x, 0, z)); // Заносим клетки в перечень
                        if (!piece) Continue = true; // Если объект - плита, то включить флаг мульти 
                        if (FindForDestroyPlane(x + w, z + b)) plane = Instantiate(piece ? Plane : GrayPlane, new Vector3(x + w, 0, z + b ), Quaternion.Euler(0, 0, 0));
                        else plane = Instantiate(Plane, new Vector3(x + w, 0, z + b), Quaternion.Euler(0, 0, 0)); // Спавним плиту
                        Planes.Add(plane); // Заносим плиту в перечень
                        if (x + w < 7) PlaneSpawnChecker(plane, AllThings); // Вызываем спавнер в спавнере
                        AllThings.RemoveAt(AllThings.Count - 1); // Удаляем два последних элемента перед новой диоганалью
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
}