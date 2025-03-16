using System;
using System.Collections.Generic;
using UnityEngine;    // Это всяческие штуки для того чтобы юнити смог всё правильно прочитать

public class BlackBrain : MonoBehaviour
{
    [SerializeField]
    private GameObject Plane; // Плитка
    [SerializeField]
    private GameObject GrayPlane; // Серая плитка

    [SerializeField]
    private GameObject ActivGazonokosilka; // Активированая газонокосилка
    [SerializeField]
    private GameObject Gazonokocilka; // Газонакосилка
    [SerializeField]
    private GameManager Manager; // Обьект с гейм менеджером


    private GameObject Gas1, Gas2, Clicked;             // Первая и вторая газонакосилка, которые можно активировать и оптимизатор для курсора
    private List<GameObject> Planes = new();            // Перечень плиток
    private Vector3 PlaneCoordinate;             // Координаты плиты для совершения хода
    private Piece Current, Currented;             // Подсвеченая и выбраная шашки
    private sbyte CurrentSpeed = 1;             // Счётчик для системы скорости 
    private bool Continue, Blocked, End, Select, Selecting;  // Флаг обозначяющий возможность мульти убийства, флаг разрешающий мульти убийство, флаг конца хода, флаг разрешающий выбор шашки и флаг проверки на ничью
    public bool WoB_Move, OnlyKill;                 // Локальный флаг хода
    private RaycastHit hit;                 // Столкновение луча с объектом
    private Ray ray;                     // Луч

    private void Awake()
    {
        Continue = Blocked = End = false;
        WoB_Move = Select = Selecting  = true;
    }
    private void Update()         // Срабатывает КАЖДЫЙ кадр  
    {
        if (!WoB_Move) // Скрипт активен только в свой ход
        {
// ************************************    Механизм выделения шашки под курсором    **********************************************************
            ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Запуск луча
            if (Physics.Raycast(ray, out hit)) // Если луч попал во что - то
            {
                Piece shecker = hit.collider.gameObject.GetComponent<Piece>(); // Запоминаем шашку в которую попали
                if (shecker && shecker.Stat && Select) //Если попали в статичную шашку и никто ещё не двигался на этом ходу
                {
                    if (Current != shecker && (!shecker.Confused) && shecker.type > 0 ) // Проверка на белых (никакого рассизма) и отличие шашки под курсором от текущей серой шашки
                    {
                        if (Current) Non(); // Вернуть серую шашку в прежнее состояние если она есть
                        Current = shecker; // Обозначаем нашу шашку как серую
                        shecker.Select(); // Перекрашиваем
                    }
                }
                else Non(); // Сброс серой шашки
            }
            else  Non(); // Сброс серой шашки
// *************************   Механизм скорости, проверки конца хода и запуска мульти убийства   ********************************************
            if (Currented && Currented.Stat && (End || Blocked))   // Если передвигаемая нами шашка остановилась
            {
                if (CurrentSpeed < Currented.speed)
                {
                    Blocked = End = false;
                    if (CheckMove(Currented.transform.position, Currented.type, false)) 
                    { 
                        Selected(Currented);
                    }
                    else 
                        End = true;    // Иначе конец хода
                    Currented.Kill = false;  // Устанавливаем в текущую шашку "никого" в качестве убитого
                    CurrentSpeed++;
                } 
                if (Blocked)  // Продолжаем ход если требуется
                {
                    if (Currented.Kill) // Если мы кого-то убили (это защита от повторного использования)
                    {
                        if (CheckMove(Currented.transform.position, Currented.type, true)) // Если есть доступные ходы (убийство обязательно)
                            Selected(Currented);              // Спавним плитки для текущей шашки
                        else
                            End = true;    // Иначе конец хода
                        Blocked = false;   // Защита от повторного использования 
                        Currented.Kill = false;  // Устанавливаем в текущую шашку "никого" в качестве убитого
                    }
                    else End = true; // Конец хода если нет убитых
                }
                if (End) { EndTurn(); }  // Конец хода если это требуется
            }
//*****************************************************   Механизм нажатия ЛКМ   *************************************************************
            if (Input.GetKeyDown(KeyCode.Mouse0) && hit.collider) // Если мы нажали ЛКМ на что - то
            {
                Clicked = hit.collider.gameObject;     // Оптимизируем проверки нажатого объекта
                if (Clicked.GetComponent<Gazonokosilka>())    //Нажатие на газонокосилку
                {
                    PlaneCoordinate = Clicked.transform.position;    // Запоминаем координаты газонокосилки
                    Currented.PointToMove = PlaneCoordinate;      // Запускаем движение шашки к газонокосилке
                    Currented.Stat = false;                             // Защищаемся от прежде временного запуска механизма окончания хода
                    Manager.All_Checker[(sbyte)Currented.transform.position.x, (sbyte)Currented.transform.position.z] = null;      // Удаляем координаты шашки из матрицы
                    if (Clicked == Gas1) Destroy(Gas2);     // Удалеям одну лишнию из двух...
                    if (Clicked == Gas2) Destroy(Gas1);     // активированных газонокосилок
                    Destroy(Manager.Gases[(sbyte)PlaneCoordinate.z]);   // Удаляем газонокосилку из под активированной газонокосилки
                    EndTurn();  // Заканчиваем ход
                }
                if (Clicked.CompareTag("Plane")) //Нажатие на плиту
                {
                    PlaneCoordinate = Clicked.transform.position;    // Запоминаем координаты газонокосилки
                    Currented.PointToMove = PlaneCoordinate;      // Запускаем движение шашки к газонокосилке
                    Currented.Stat = false;                             // Защищаемся от прежде временного запуска механизма окончания хода
                    Manager.All_Checker[(sbyte)Currented.transform.position.x, (sbyte)Currented.transform.position.z] = null;      // Удаляем координаты шашки из матрицы
                    if (PlaneCoordinate.x != -1)  // Если это не ход с захватом дома
                        Manager.All_Checker[(sbyte)PlaneCoordinate.x, (sbyte)PlaneCoordinate.z] = Currented; // Записываем в матрицу текущие координаты нашей шашки
                    if (Continue) Blocked = true; else End = true; // Записываем - можем ли мы совершить мульти убийство
                    PlaneBust();        // Анегилируем все плитки
                    Selecting = Select = false; // Запрещаем выбор шашки курсором
                }
                if (Select) // Если выбор шашки разрешен  
                {  
                    if (Currented) //Если есть уже выбраная
                    { 
                        if (Currented.transform.position != Clicked.transform.position) // Если выбраная шашка не равна нажатой
                        {
                            Piece A = Clicked.GetComponent<Piece>();
                            if (A && A.type > 0 && !A.Confused)  // Если кликнули на шашку и она чёрная
                            {
                                PlaneBust();           // Анегилируем все плитки
                                Selected(Clicked.GetComponent<Piece>());          // Спавним плитки для нашей шашки
                            }
                        }
                        else    // Если навели на выделенную шашку
                        {
                            PlaneBust();                // Анегилируем все плитки
                            Currented = null;     // Обнуляем выбраную шашку
                        }
                    }
                    else if (Current) Selected(Current); // Если мы навели на шашку
                }
                //DebugMatrice();   // Это используется для проверки матрицы
                Clicked = null;     // Обнуляем оптимизатор
            }
//*****************************************************************************************************************************************
        }
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
    public void EndTurn()    // Запускается для окончания хода
    {
        bool i = true;  // Флаг для проверки на победу через убийство
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // Перебор матрицы
            {
                if (Manager.All_Checker[x, z] && Manager.All_Checker[x, z].type == 0) 
                { 
                    i = false; // Отключить объявление победы если есть хоть одна белая шашка
                    break; 
                }
            }
            if (!i) break; // Оптимизатор
        }
        if (i || (Currented && Currented.transform.position.x == -1))
            Manager.Won(1); // Обьявить победу если захвачен дом или все белые шашки
        if (Currented)   // Возращаем все параметры...
            Currented.Deselect();
        PlaneBust();
        Currented = null;
        End = Continue = Blocked = false;
        Manager.WoB_Move = WoB_Move = Select = Selecting = true;
        CurrentSpeed = 1; // в начальную позицию
    }
    private void Non() // Запускается если не навели курсор на шашку
    {
        if (Current) 
            Current.Deselect(); //Перекрашиваем шашку Current если она есть
        Current = null; // Обнуляем Current
    }
    private void PlaneBust() // Запускается для анигиляции плит и не только
    {
        for (byte y = 0; y < Planes.Count; y++) 
        {
            Destroy(Planes[y]); // Перебор и анигиляция плит
        }
        if (Gas1 && PlaneCoordinate != Gas1.transform.position && Gas1.transform.position.x == -1.25)
        {
            Manager.Gases[(int)Gas1.transform.position.z] = Instantiate(Gazonokocilka, new Vector3((float)-1.25, (float)-0.08, (int)Gas1.transform.position.z), Quaternion.Euler(0, 180, 0));
            Destroy(Gas1);  // Удалить не использованую активированую газонокосилку
        }
        if (Gas2 && PlaneCoordinate != Gas2.transform.position && Gas2.transform.position.x == -1.25)
        {
            Manager.Gases[(int)Gas2.transform.position.z] = Instantiate(Gazonokocilka, new Vector3((float)-1.25, (float)-0.08, (int)Gas2.transform.position.z), Quaternion.Euler(0, 180, 0));
            Destroy(Gas2);  // Удалить не использованую активированую газонокосилку
        }
        Planes.Clear(); // Очистка перечня плит
    }
    private void Selected(Piece piece) // Запускается для спавна плит
    {
        Continue = false;  // Указываем флагу мультикила значение по умолчанию
        if (CheckMove(piece.transform.position, piece.type, OnlyKill)) // Если есть доступные ходы (убийство не обязательно)
        {
            Currented = piece;   // Делаем шашку выбранной
            if (OnlyKill) Select = false;
            PlaneSpawn(Currented.gameObject); // Запускаем спавн плит
            if (OnlyKill) Select = true;
        }
        else Currented = null; // Иначе - обнулить выделенную шашку
    }
    private bool Find(List<Vector3> A, sbyte x, sbyte z) // Проверка на наличие координат в перечне
    {
        Vector3 V = new(x, 0, z); // Создание координат для проверки
        for (sbyte i = 0; i < A.Count; i++)
        {
            if (A[i] == V) return true; // Перебираем перечень и возвражаем "Да" если нашли совпадение
        }
        return false; // Возвращаем "Нет"
    }
    private void FindForDestroyPlane(sbyte x, sbyte z) // Проверка на наличие координат в перечне
    {
        Vector3 V = new(x, 0, z); // Создание координат для проверки
        for (sbyte i = 0; i < Planes.Count; i++)

        {
            if (Planes[i].transform.position == V && Planes[i].CompareTag("PlaneGray")) { Destroy(Planes[i]); Planes.Remove(Planes[i]); break; } // Находим и уничтожаем плиту
        }
    }
    public bool CheckMove(Vector3 Object, sbyte Type, bool Kill) // Переходник для разных типов шашек
    {
        switch (Type)  // Запускаем нужную проверку
        {
            case 1: return CheckMoveChecker(Object, Kill); 
            case 2: return CheckMoveJumper(Object, Kill);
            case 3: return CheckMoveChecker(Object, Kill); 
            case 4: return CheckMoveDancer(Object, Kill); 
            case 5: return CheckMoveAZP(Object, Kill); 
            case 6: return CheckMovePortal(Object, Kill); 
            case 7: return CheckMoveCave(Object, Kill); 
            case 8: return CheckMoveChecker(Object, Kill); 
            case 9: return CheckMoveSky(Object, Kill); 
            case 10: return CheckMoveRope(Object, Kill); 
            case 11: return CheckMoveCase(Object, Kill); 
            case 12: return CheckMoveGun(Object, Kill); 
            case 13: return CheckMoveGiant(Object, Kill); 
            default: break;
        }
        return false;
    }
    private void PlaneSpawn(GameObject Object) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        switch (Type)  // Запускаем нужный спавнер
        {
            case 1: PlaneSpawnChecker(Object, new()); break;
            case 2: PlaneSpawnJumper(Object, new()); break;
            case 3: PlaneSpawnChecker(Object, new()); break;
            case 4: PlaneSpawnDancer(Object); break;
            case 5: PlaneSpawnAZP(Object); break;
            case 6: PlaneSpawnPortal(Object); break;
            case 7: PlaneSpawnCave(Object); break;
            case 8: PlaneSpawnChecker(Object, new()); break;
            case 9: PlaneSpawnSky(Object); break;
            case 10: PlaneSpawnRope(Object); break;
            case 11: PlaneSpawnCase(Object); break;
            case 12: PlaneSpawnGun(Object); break;
            case 13: PlaneSpawnGiant(Object); break;
            default: break;
        }
    }
    private bool FilterAnalis(Vector3 X, Piece I, Vector3 Wrong)
    {
        Piece Y;
        bool flag2 = false;
        sbyte x, z; // Вспомогательные счётчики
        for (sbyte v = -1; v < 2; v += 2)
        {
            for (sbyte d = -1; d < 2; d += 2) // Перебор 4 диагоналей
            {
                x = (sbyte)((sbyte)X.x + v);
                z = (sbyte)((sbyte)X.z + d);
                if (x + v > 7 || x + v < 0 || z + d > 7 || z + d < 0) continue;
                Y = Manager.All_Checker[x, z]; 
                if (Y && Y.type == 0 && (!Manager.All_Checker[x + v, z + d]) && Wrong != Y.transform.position)
                {
                    flag2 = true;
                    if(!FilterAnalis(new(x + v, 0, z + d), I, Y.transform.position)) 
                    {
                        if (!I.BannedCoords.Contains(new(x + v, 0, z + d))) I.Stat = false;
                    }
                }
                if (!I.Stat) break;
            }
        }
        if ((!flag2) && Wrong.Equals(new(0, 2, 0)) && !I.BannedCoords.Contains(X)) I.Stat = false;
        return flag2;
    }
    private bool CheckMoveChecker(Vector3 Object, bool Kill) // Проверка хода для шашек
    {
        Piece I;
        sbyte x, z; // Вспомогательные счётчики
        if (Object.x == 0 && ((!Kill) || OnlyKill)) return true;  // Ход возможен, так как мы впритык к дому
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                x = (sbyte)((sbyte)Object.x + w);
                z = (sbyte)((sbyte)Object.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Manager.All_Checker[x, z];
                    if (I && I.type == 0 && !Manager.All_Checker[x + w, z + b])
                        if (Manager.All_Checker[(int)Object.x, (int)Object.z].speed > 1 && Manager.GSpeed) 
                        { 
                            FilterAnalis(new(x + w, 0, z + b), Manager.All_Checker[(int)Object.x, (int)Object.z], new(0, 2, 0)); 
                            if (!Manager.All_Checker[(int)Object.x, (int)Object.z].Stat) 
                            {
                                Manager.All_Checker[(int)Object.x, (int)Object.z].Stat = true; 
                                return true; 
                            } 
                            else return false;
                        }
                        else return true; // Если можно кого-то убить, то ход возможен
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Manager.All_Checker[(int)Object.x, (int)Object.z];
                    if ((!Manager.All_Checker[x, z]) && w == -1 && (!Kill) && !I.BannedCoords.Contains(new(x, 0, z))) return true; // Если можно кого-то убить, то ход возможен
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        return false; //Ход не возможен
    }
    private bool CheckMoveJumper(Vector3 Object, bool Kill)
    {
        Piece I;
        sbyte x, z; // Вспомогательные счётчики
        x = (sbyte)Object.x;
        z = (sbyte)Object.z; // Оптимизируем проверку координат шашки
        if (x == 0 && ((!Kill) || OnlyKill)) return true;  // Ход возможен, так как мы впритык к дому
        if (x == 2 && Manager.All_Checker[x, z].FirstKill) 
        {          // Ход возможен если у нас есть прыжок, мы в двух клетках от дома и перед нами шашка
            try {if (Manager.All_Checker[x - 1, z + 1] && (!Manager.All_Checker[x - 2, z + 2]) && z < 5) return true; }
            catch (IndexOutOfRangeException) { }
            try {if (Manager.All_Checker[x - 1, z - 1] && (!Manager.All_Checker[x - 2, z - 2]) && z > 2) return true; }
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
                    I = Manager.All_Checker[x, z]; 
                    if (I && (!Manager.All_Checker[(sbyte)Object.x, (sbyte)Object.z].FirstKill) && (!Manager.All_Checker[x + w, z + b]) && (!Manager.All_Checker[x + w * 2, z + b * 2]))
                        if (Manager.GSpeed)
                        {
                            FilterAnalis(new(x + w, 0, z + b), Manager.All_Checker[x - w, z - b], new(0, 2, 0));
                            if (!Manager.All_Checker[x - w, z - b].Stat)
                            {
                                Manager.All_Checker[x - w, z - b].Stat = true;
                                return true;
                            }
                            else return false;
                        }
                        else return true; // Если можно кого-то убить, то ход возможен// Если можно кого-то убить, то ход возможен
                }
                catch (IndexOutOfRangeException) { }// Эти штуки помогают не ловить ошибку если мы выйдем за пределы матрицы
                try
                {
                    I = Manager.All_Checker[x, z];
                    if (I && (I.type == 0 || !Manager.All_Checker[(sbyte)Object.x, (sbyte)Object.z].FirstKill) && (!Manager.All_Checker[x + w, z + b]))
                        if (Manager.GSpeed)
                        {
                            FilterAnalis(new(x + w, 0, z + b), Manager.All_Checker[x - w, z - b], new(0, 2, 0));
                            if (!Manager.All_Checker[x - w, z - b].Stat)
                            {
                                Manager.All_Checker[x - w, z - b].Stat = true;
                                return true;
                            }
                            else return false;
                        }
                        else return true; // Если можно кого-то убить, то ход возможен// Если можно кого-то убить, то ход возможен
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Manager.All_Checker[x - w, z - b];
                    if ((!Manager.All_Checker[x, z]) && w == -1 && (!Kill) && !I.BannedCoords.Contains(new(x, 0, z))) return true; // Если можно кого-то убить, то ход возможен
                }
                catch (IndexOutOfRangeException) { }// Эти штуки помогают не ловить ошибку если мы выйдем за пределы матрицы
            }
        }
        return false;
    }
    private bool CheckMovePaper(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveDancer(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveAZP(Vector3 Object, bool Kill) { return false; }
    private bool CheckMovePortal(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveCave(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveSky(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveRope(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveCase(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveGun(Vector3 Object, bool Kill) { return false; }
    private bool CheckMoveGiant(Vector3 Object, bool Kill) { return false; } // Будущие заготовки
    // Работа спавнеров:
    // Получил шашку -> заспавнил плиты -> отправил самому себе заспавненые плиты -> заспавнил серые плиты относительно них -> отправил самому себе заспавненые серые плиты... и так по кругу
    private void PlaneSpawnChecker(GameObject Object, List<Vector3> AllThings) // Спавнер плит для обычной шашки
    {
        Vector3 This = Object.transform.position; // Координаты объекта
        Piece piece = Object.GetComponent<Piece>(), I;
        GameObject plane;  // Вспомогательный контейнер
        sbyte x = (sbyte)This.x, z = (sbyte)This.z; // Оптимизируем проверку координат
        if (piece)
        {
            if (x == 0) //Если мы в притык к дому и спавнер вызван для шашки
            {
                if (z < 7) // Если координаты позволяют не выйти за пределы доски
                {
                    if (Manager.Gases[z + 1])   // Если газонокосилка стоит слева от нас
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z + 1), Quaternion.Euler(0, 180, 0)); // Спавним активированную газонокосилку
                        Destroy(Manager.Gases[z + 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // Иначе спавним обычную плитку если там пусто
                        Planes.Add(plane); // Сохраняем плиту в перечень
                    }
                }
                if (z > 0) // Если координаты позволяют не выйти за пределы доски
                {
                    if (Manager.Gases[z - 1])    // Если газонокосилка стоит справа от нас
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z - 1), Quaternion.Euler(0, 180, 0)); // Спавним активированную газонокосилку
                        Destroy(Manager.Gases[z - 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // Иначе спавним обычную плитку если там пусто
                        Planes.Add(plane); // Сохраняем плиту в перечень
                    }
                }
            }
            if(!Blocked)
            {
                try
                {
                    if ((!Manager.All_Checker[x - 1, z + 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z + 1)))// Если клетка слева пуста
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // Спавним плиту
                        Planes.Add(plane);  // Добавляем её в перечни
                    }
                }
                catch (IndexOutOfRangeException)  { }
                try
                {
                    if ((!Manager.All_Checker[x - 1, z - 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z - 1))) // Если клетка справа пуста
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // Спавним плиту
                        Planes.Add(plane);     // Добавляем плиту в перечни
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебираем диоганали
            {
                x = (sbyte)((sbyte)This.x + w);
                z = (sbyte)((sbyte)This.z + b); // Оптимизируем проверку диогпналей
                try
                {
                    I = Manager.All_Checker[x, z]; 
                    if (I && I.type == 0 && (!Manager.All_Checker[x + w, z + b]) && (!Find(AllThings, x, z))) // Если рядом белая шашка не записаная в перечень
                    {
                        if (!piece) // Если обьект является плитой, то включить флаг мульти убийства
                            Continue = true;     
                        else
                        {
                            FilterAnalis(new(x + w, 0, z + b), piece, new(0, 2, 0));
                            if (Manager.All_Checker[x - w, z - b].Stat) continue;
                            Manager.All_Checker[x - w, z - b].Stat = true;
                        }
                        FindForDestroyPlane((sbyte)(x + w), (sbyte)(z + b));
                        AllThings.Add(new(x, 0, z));
                        plane = Instantiate(piece ? Plane : GrayPlane , new Vector3(x + w , 0, z + b), Quaternion.Euler(0, 0, 0)); // Спавним плиту
                        Planes.Add(plane);  // Добавляем плиты в перечень
                        PlaneSpawnChecker(plane, AllThings);  // Вызываем спавнер внутри спавнера (да, так можно)
                        AllThings.RemoveAt(AllThings.Count - 1);
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void PlaneSpawnJumper(GameObject Object, List<Vector3> AllThings)  // Спавнер плит для прыгунов
    {
        GameObject plane;  // Вспомогательный контейнер
        Piece piece = Object.GetComponent<Piece>(), I;
        sbyte x = (sbyte)Object.transform.position.x, z = (sbyte)Object.transform.position.z; // Оптимизируем проверку координат
        if (Object.GetComponent<Piece>())
        {
            piece = Object.GetComponent<Piece>();
            if (x == 0) //Если мы в притык к дому и спавнер вызван для шашки
            {
                if (z < 7) // Если координаты позволяют не выйти за пределы доски
                {
                    if (Manager.Gases[z + 1])   // Если газонокосилка стоит слева от нас
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z + 1), Quaternion.Euler(0, 180, 0)); // Спавним активированную газонокосилку
                        Destroy(Manager.Gases[z + 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // Иначе спавним обычную плитку если там пусто
                        Planes.Add(plane); // Сохраняем плиту в перечень
                    }
                }
                if (z > 0) // Если координаты позволяют не выйти за пределы доски
                {
                    if (Manager.Gases[z - 1])    // Если газонокосилка стоит справа от нас
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z - 1), Quaternion.Euler(0, 180, 0)); // Спавним активированную газонокосилку
                        Destroy(Manager.Gases[z - 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // Иначе спавним обычную плитку если там пусто
                        Planes.Add(plane); // Сохраняем плиту в перечень
                    }
                }
            }
            if (x == 2 && !piece.FirstKill) // Если мы в двух клетках от дома, объект - шашка и у неё не использован прыжок
            {
                if (z < 5 && Manager.All_Checker[1, z + 1] && (!Manager.All_Checker[0, z + 2])) // Если координаты и доска позволяют сделать прыжок на левую газонокосилку
                {
                    AllThings.Add(new(1, 0, z + 1));
                    if (Manager.Gases[z + 3])  // Если в месте будущего левого прыжка есть газонокосилка
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z + 3), Quaternion.Euler(0, 180, 0)); // Спавним активированную газонокосилку
                        Destroy(Manager.Gases[z + 3]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z + 3), Quaternion.Euler(0, 0, 0)); // Иначе спавним плиту
                        Planes.Add(plane); // Записываем плиту в перечень
                    }
                }
                if (z > 2 && Manager.All_Checker[1, z - 1] && (!Manager.All_Checker[0, z - 2])) // Если координаты и доска позволяют сделать прыжок на правую газонокосилку
                {
                    AllThings.Add(new(1, 0, z - 1));
                    if (Manager.Gases[z - 3]) // Если в месте будущего правого прыжка есть газонокосилка
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z - 3), Quaternion.Euler(0, 180, 0)); // Спавним активированную газонокосилку
                        Destroy(Manager.Gases[z - 3]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z - 3), Quaternion.Euler(0, 0, 0)); // Иначе спавним плиту
                        Planes.Add(plane); // Записываем плиту в перечень
                    }
                }
            }
            if (!Blocked) // Если обычный ход доступен и объект - шашка
            {
                try
                {
                    if ((!Manager.All_Checker[x - 1, z + 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z + 1))) // Если слева пустая клетка
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z + 1), Quaternion.Euler(0, 0, 0));  // Спавним плиту
                        Planes.Add(plane);  // Записываем плиту в перечни
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    if ((!Manager.All_Checker[x - 1, z - 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z - 1))) // Если справа пустая клетка
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // Спавним плиту                        Planes.Add(plane);  // Записываем плиту в перечни
                        Planes.Add(plane);  // Записываем плиту в перечни
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебираем диоганали
            {
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизируем проверку диоганалей
                try
                {
                    I = Manager.All_Checker[x, z]; 
                    if (I && !Manager.All_Checker[x + w, z + b] && (!Find(AllThings, x, z))) // Если рядом есть шашка и она не в перечне
                    {
                        if (piece && !piece.FirstKill)
                        {
                            try
                            {
                                if (!Manager.All_Checker[x + (w * 2), z + (b * 2)])
                                {
                                    FindForDestroyPlane((sbyte)(x + w * 2), (sbyte)(z + b * 2));
                                    plane = Instantiate(Plane, new Vector3(x + (w * 2), 0, z + (b * 2)), Quaternion.Euler(0f, 0f, 0f)); // Спавним плиту
                                    Planes.Add(plane); // Записываем плиту в перечни
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                FindForDestroyPlane((sbyte)(x + w), (sbyte)(z + b));
                                plane = Instantiate(Plane, new Vector3(x + w, 0, z + b), Quaternion.Euler(0f, 0f, 0f)); // Спавним плиту
                                Planes.Add(plane); // Записываем плиту в перечни
                            }
                        }
                        else if (I.type == 0)
                        {
                            FindForDestroyPlane((sbyte)(x + w), (sbyte)(z + b));
                            AllThings.Add(new(x, 0, z));
                            if (!piece)   // Если обьект является плитой, то включить флаг многократного убийства
                                Continue = true;
                            plane = Instantiate(!Object.GetComponent<Piece>() ? GrayPlane : Plane, new Vector3(x + w, 0, z + b), Quaternion.Euler(0, 0, 0)); // Спавним плиту
                            Planes.Add(plane); // Записываем плиту в перечни
                            PlaneSpawnJumper(plane, AllThings);   // Вызываем спавнер внутри спавнера (да, так можно)
                            AllThings.RemoveAt(AllThings.Count - 1);
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void PlaneSpawnPaper(GameObject Object) {  }
    private void PlaneSpawnDancer(GameObject Object) { }
    private void PlaneSpawnAZP(GameObject Object) {  }
    private void PlaneSpawnPortal(GameObject Object) {  }
    private void PlaneSpawnCave(GameObject Object) {  }
    private void PlaneSpawnSky(GameObject Object) {  }
    private void PlaneSpawnRope(GameObject Object) { }
    private void PlaneSpawnCase(GameObject Object) {  }
    private void PlaneSpawnGun(GameObject Object) {  }
    private void PlaneSpawnGiant(GameObject Object) { } // Заготовки
}