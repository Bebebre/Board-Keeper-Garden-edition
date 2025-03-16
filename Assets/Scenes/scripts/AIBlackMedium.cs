using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBlackMedium : MonoBehaviour
{
    [SerializeField]
    private GameManager Gamemanager; // Обьект с гейм менеджером
    [SerializeField]
    private GameObject Gazon;

    private GameObject[] Blacks = new GameObject[12]; // Заспавненые чёрные шашки для консоли и восклицательные знаки соответственно
    private int TotalPrior, Value;
    private sbyte Counter, Speed, WhitePrior, KillCount;
    private List<sbyte> MultyKill = new();
    private Piece MultyKiller;
    // Category1: победные ходы
    // Category2: мультиубийства и газонокосилки
    // Category3: особые ходы и убийства.
    // Category4: обычный ход, размен.
    private System.Random Random = new();
    private bool Category1, Category2, Category3, Jump, MultyKilling;
    public void Starting()
    {
        Speed = KillCount = 0;
        for (sbyte i = 0; i < 12; i++)
        {
            Blacks[i] = Gamemanager.Blacks[i];
        }
    }
    public void Move() 
    {
        Counter = 0;
        TotalPrior = 0;
        WhitePrior = 12;
        MultyKill.Clear();
        MultyKill.Add(0);
        MultyKiller = null;
        MultyKilling = false;
        Category1 = Category2 = Category3 = true;
        for (int x = 0; x < 8; x++)         // Спавн белых и чёрных шашек 
        {
            for (int z = 0; z < 8; z++)        // Перебор всех возможных клеток
            {
                if (Gamemanager.All_Checker[x, z] && Gamemanager.All_Checker[x, z].type == 0) WhitePrior--;
            }
        }
        for (sbyte i = 0; i < 12; i++)
        {
            if (Blacks[i])
            {
                PrioritedChangerCategory1(Blacks[i]);
            }
        }
        if (Category1) for (sbyte i = 0; i < 12; i++)
        {
            if (Blacks[i])
            {
                PrioritedChangerCategory2(Blacks[i]);
                if (MultyKiller != null)
                {
                    if (Blacks[i].GetComponent<Piece>().type == 2 && Jump)
                    {
                        int t = MultyKill.Count * 12 + WhitePrior * 3 + 3;
                        if (t > 120) t = 120;
                        MultyKiller.pryorited.Add(MultyKill[0], (sbyte)t);
                        TotalPrior += (sbyte)t;
                    }
                    else
                    {
                        MultyKiller.pryorited.Add(MultyKill[0], (sbyte)(MultyKill.Count * 10 + WhitePrior * 3));
                        TotalPrior += (sbyte)(MultyKill.Count * 10 + WhitePrior * 3);
                    }
                    MultyKill.Clear();
                    MultyKill.Add(0);
                    MultyKiller = null;
                }
            }
        }
        if (Category1 && Category2) for (sbyte i = 0; i < 12; i++)
        {
            if (Blacks[i])
            {
                PrioritedChangerCategory3(Blacks[i]);
            }
        }
        if (Category1 && Category2 && Category3) for (sbyte i = 0; i < 12; i++)
        {
            if (Blacks[i])
            {
                PrioritedChangerCategory4(Blacks[i]);
            }
        }
        if (TotalPrior == 0)
        {
            Gamemanager.WoB_Move = true;
        }
        else
        {
            Value = Random.Next(1, TotalPrior + 1);
            for (sbyte i = 0; i < 12; i++)
            {
                if (Blacks[i])
                {
                    for (sbyte j = 1; j < 5; j++)
                    {
                        Counter += Blacks[i].GetComponent<Piece>().pryorited.GetValueOrDefault(j, (sbyte)0);
                        if (Counter >= Value)
                        {
                            MovePiece(Blacks[i], j);
                            TotalPrior = 0;
                            break;
                        }
                    }
                }
                if (TotalPrior == 0) break;
            }
        }
    }
    public void EndMoving(GameObject Object)
    {
        if (Object)
        {
            KillCount++;
            Piece Piece = Object.GetComponent<Piece>();
            if (MultyKilling && KillCount < MultyKill.Count)
            {
                Vector3 Position = Object.transform.position;
                sbyte i = 0;
                for (sbyte w = -1; w < 2; w += 2)
                {
                    for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
                    {
                        i++;
                        if (MultyKill[KillCount] == i)
                        {
                            Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                            if (Gamemanager.All_Checker[(sbyte)Position.x + w * 2, (sbyte)Position.z + b * 2]) Non();
                            else Gamemanager.All_Checker[(sbyte)Position.x + w * 2, (sbyte)Position.z + b * 2] = Piece;
                            Piece.PointToMove = new Vector3(Position.x + w * 2, 0, (sbyte)Position.z + b * 2);
                            Piece.AIKill = true;
                        }
                    }
                }
            }
            else if (Piece.speed > Speed)
            {
                Counter = KillCount = 0;
                TotalPrior = 0;
                WhitePrior = 12;
                MultyKill.Clear();
                MultyKill.Add(0);
                MultyKiller = null;
                MultyKilling = false;
                Category1 = Category2 = Category3 = true;
                for (int x = 0; x < 8; x++)         // Спавн белых и чёрных шашек 
                {
                    for (int z = 0; z < 8; z++)        // Перебор всех возможных клеток
                    {
                        if (Gamemanager.All_Checker[x, z] && Gamemanager.All_Checker[x, z].type == 0) WhitePrior--;
                    }
                }
                if (WhitePrior == 12) Gamemanager.Won(1);
                PrioritedChangerCategory1(Object);
                if (Category1) PrioritedChangerCategory2(Object);
                if (!Category2)
                {
                    if (Object.GetComponent<Piece>().type == 2 && Jump)
                    {
                        int t = MultyKill.Count * 12 + WhitePrior * 3 + 3;
                        if (t > 120) t = 120;
                        MultyKiller.pryorited.Add(MultyKill[0], (sbyte)t);
                        TotalPrior += (sbyte)t;
                    }
                    else
                    {
                        MultyKiller.pryorited.Add(MultyKill[0], (sbyte)(MultyKill.Count * 10 + WhitePrior * 3));
                        TotalPrior += (sbyte)(MultyKill.Count * 10 + WhitePrior * 3);
                    }
                    MultyKill.Clear();
                    MultyKill.Add(0);
                    MultyKiller = null;
                }
                if (Category1 && Category2) PrioritedChangerCategory3(Object);
                if (Category1 && Category2 && Category3) PrioritedChangerCategory4(Object);
                if (TotalPrior > 0)
                {
                    Value = Random.Next(1, TotalPrior + 1);
                    for (sbyte j = 1; j < 5; j++)
                    {
                        Counter += Piece.pryorited.GetValueOrDefault(j, (sbyte)0);
                        if (Counter >= Value)
                        {
                            Debug.Log("Double Move");
                            MovePiece(Object, j);
                            break;
                        }
                    }
                }
                else Non();
            }
            else Non();
        }
        else Non();
    }
    private void Non()
    {
        Debug.Log("White Move");
        Gamemanager.WoB_Move = true;
        Speed = KillCount = 0;
    }
    private void PrioritedChangerCategory1(GameObject Object) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        Object.GetComponent<Piece>().pryorited.Clear();
        switch (Type)  // Запускаем нужный спавнер
        {
            case 2: PrioritedChangerJumperCategory1(Object); break;
            case 3: if (!Object.GetComponent<Piece>().Confused) PrioritedChangerCheckerCategory1(Object); break;
            default: PrioritedChangerCheckerCategory1(Object); break;
        }
    }
    private void PrioritedChangerCategory2(GameObject Object) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        Object.GetComponent<Piece>().pryorited.Clear();
        switch (Type)  // Запускаем нужный спавнер
        {
            case 2: PrioritedChangerJumperCategory2(Object); break;
            case 3: if (!Object.GetComponent<Piece>().Confused) PrioritedChangerCheckerCategory2(Object); break;
            default: PrioritedChangerCheckerCategory2(Object); break;
        }
    }
    private void PrioritedChangerCategory3(GameObject Object) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        Object.GetComponent<Piece>().pryorited.Clear();
        switch (Type)  // Запускаем нужный спавнер
        {
            case 2: PrioritedChangerJumperCategory3(Object); break;
            case 3: if (!Object.GetComponent<Piece>().Confused) PrioritedChangerCheckerCategory3(Object); break;
            default: PrioritedChangerCheckerCategory3(Object); break;
        }
    }
    private void PrioritedChangerCategory4(GameObject Object) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        Object.GetComponent<Piece>().pryorited.Clear();
        switch (Type)  // Запускаем нужный спавнер
        {
            case 2: PrioritedChangerJumperCategory4(Object); break;
            case 3: if (!Object.GetComponent<Piece>().Confused) PrioritedChangerCheckerCategory4(Object); break;
            default: PrioritedChangerCheckerCategory4(Object); break;
        }
    }
    private void MovePiece(GameObject Object, sbyte j) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        switch (Type)  // Запускаем нужный спавнер
        {
            case 2: MovePieceJumper(Object, j); break;
            default: MovePieceCheckerHighSpeed(Object, j); break;
        }
    }
    private void PrioritedChangerCheckerCategory1(GameObject Object)
    {
        sbyte x, z; // Вспомогательные счётчики
        x = (sbyte)Object.transform.position.x;
        z = (sbyte)Object.transform.position.z; // Оптимизация проверки диоганалей
        if (x == 0)
        {
            try
            {
                if (!Gamemanager.Gases[z + 1])
                {
                    Category1 = false;
                    Object.GetComponent<Piece>().pryorited.Add(2, 127);
                    TotalPrior += 127;
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (!Gamemanager.Gases[z - 1])
                {
                    Category1 = false;
                    Object.GetComponent<Piece>().pryorited.Add(1, 127);
                    TotalPrior += 127;
                }
            }
            catch (IndexOutOfRangeException) { }
        }
    }
    private void PrioritedChangerCheckerCategory2(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0, CurrentedVector, WrongCurVector, Counter; // Вспомогательные счётчики
        bool Switch, Detector, Debugs;
        Vector3 Current, Checker, Container, ObjectPos = Object.transform.position;
        Dictionary<Vector3, sbyte> Crossroads = new();
        List<Vector3> MoveCombo = new();
        List<sbyte> MoveList = new();
        x = (sbyte)ObjectPos.x;
        z = (sbyte)ObjectPos.z; // Оптимизация проверки диоганалей
        if (x == 0 && !MultyKilling)
        {
            try
            {
                if (Gamemanager.Gases[z + 1])
                {
                    Category1 = false;
                    Counter = 25;
                    for (int i = 0; i < 8; i++) if (Gamemanager.All_Checker[i, z + 1] && Gamemanager.All_Checker[i, z + 1].type > 0) Counter -= 5;
                    Object.GetComponent<Piece>().pryorited.Add(2, Counter);
                    TotalPrior += Counter;
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (Gamemanager.Gases[z - 1])
                {
                    Category1 = false;
                    Counter = 25;
                    for (int i = 0; i < 8; i++) if (Gamemanager.All_Checker[i, z - 1] && Gamemanager.All_Checker[i, z - 1].type > 0) Counter -= 5;
                    Object.GetComponent<Piece>().pryorited.Add(1, Counter);
                    TotalPrior += Counter;
                }
            }
            catch (IndexOutOfRangeException) { }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                Switch = true;
                Vector++;
                x = (sbyte)((sbyte)ObjectPos.x + w);
                z = (sbyte)((sbyte)ObjectPos.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && !Gamemanager.All_Checker[x + w, z + b])
                    {
                        Current = new Vector3(x + w, 0, z + b);
                        CurrentedVector = Vector;
                        WrongCurVector = 0;
                        if (SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos) != new Vector3(-1, 0, 0)) MoveCombo.Add(new Vector3(x, CurrentedVector, z));
                        while (Switch)
                        {
                            if (SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos) != new Vector3(-1, 0, 0))
                            {
                                Checker = SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos);
                                Detector = false;
                                for (int i = 0; i < MoveCombo.Count; i++)
                                {
                                    if (Checker.x == MoveCombo[i].x && Checker.y == MoveCombo[i].y) Detector = true;
                                }
                                if (Detector)
                                {
                                    WrongCurVector = (sbyte)Checker.y;
                                }
                                else
                                {
                                    MoveCombo.Add(Checker);
                                    if (SecondMoveChecker(Current, CurrentedVector, (sbyte)Checker.y, ObjectPos) != new Vector3(-1, 0, 0))
                                    {
                                        Crossroads.Add(new Vector3(Current.x, Checker.y, Current.z), CurrentedVector);
                                    }
                                    CurrentedVector = (sbyte)Checker.y;
                                    Current = new Vector3(Checker.y < 3 ? Checker.x - 1 : Checker.x + 1, 0, Checker.y % 2 == 0 ? Checker.z + 1 : Checker.z - 1);
                                }
                            }
                            else
                            {
                                for (int o = 0; o < MoveCombo.Count; o++)
                                {
                                    MoveList.Add((sbyte)MoveCombo[o].y);
                                }
                                if (MoveList.Count > MultyKill.Count)
                                {
                                    Category2 = false;
                                    MultyKill.Clear();
                                    MultyKill.AddRange(MoveList);
                                    MultyKiller = Object.GetComponent<Piece>();
                                    MoveList.Clear();
                                }
                                if (Crossroads.Count != 0)
                                {
                                    Debugs = true;
                                    for (int i = MoveCombo.Count - 1; i > -1; i--)
                                    {
                                        Container = MoveCombo[i];
                                        if (Container.y < 3) Container.x += 1; else Container.x -= 1;
                                        if (Container.y % 2 == 0) Container.z -= 1; else Container.z += 1;
                                        if (Crossroads.ContainsKey(Container))
                                        {
                                            Debugs = false;
                                            CurrentedVector = Crossroads[Container];
                                            Current = new Vector3(Container.x, 0, Container.z);
                                            WrongCurVector = (sbyte)Container.y;
                                            Crossroads.Remove(Container);
                                            MoveCombo.RemoveRange(i, MoveCombo.Count - i);
                                            break;
                                        }
                                    }
                                    if (Debugs) Debug.Log("Error: Crossroads don't contains MoveCombo[i]; cs: 305; script: AiBlackHard");
                                }
                                else Switch = false;
                            }
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void PrioritedChangerCheckerCategory3(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0, y; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                Vector++;
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && !Gamemanager.All_Checker[x + w, z + b])
                    {
                        y = FirstMoveChecker(Object, new Vector3(x + w, 0, z + b), Vector, true);
                        if (y != 0)
                        {
                            Category3 = false;
                            y += (sbyte)(12 + WhitePrior);
                            Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                            TotalPrior += y;
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void PrioritedChangerCheckerCategory4(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0, y; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                Vector++;
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && !Gamemanager.All_Checker[x + w, z + b])
                    {
                        y = (sbyte)(2 + WhitePrior / 3);
                        if (y > 3) y++;
                        Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                        TotalPrior += y;
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!I) && w == -1)
                    {
                        y = FirstMoveChecker(Object, new Vector3(x, 0, z), Vector, false);
                        Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                        TotalPrior += y;
                    } // Обычный ход возможен если убийство не важно
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private sbyte FirstMoveChecker(GameObject Object, Vector3 Position, sbyte Vector, bool Check)
    {
        Piece I;
        sbyte x, z, WrongVector = 5;
        bool Danger = false, Save = false, Union = false;
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                WrongVector--;
                if (WrongVector == Vector) continue;
                x = (sbyte)((sbyte)Position.x + w);
                z = (sbyte)((sbyte)Position.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && (5 - WrongVector == Vector || !Gamemanager.All_Checker[x - w * 2, z - b * 2]))
                    {
                        if (Object.GetComponent<Piece>().speed == Speed + 1 || SecondMoveChecker(Position, Vector, 0, new Vector3()) == new Vector3(-1, 0, 0))
                            Danger = true;
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type > 0)
                    {
                        if (Gamemanager.All_Checker[2 * w + x, 2 * b + z] && Gamemanager.All_Checker[2 * w + x, 2 * b + z].type == 0) Save = true;
                        else Union = true;
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        if (Check && Danger) return 0;
        if (Danger) return 1;
        else if (Save) return 10;
        else if (Union) return 7;
        else return 4;
    }
    private Vector3 SecondMoveChecker(Vector3 Position, sbyte Vector, sbyte Wrong, Vector3 Object)
    {
        Piece I;
        sbyte WrongVector = 0, x, z;
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                WrongVector++;
                if (5 - WrongVector == Vector) continue;
                x = (sbyte)((sbyte)Position.x + w);
                z = (sbyte)((sbyte)Position.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && WrongVector > Wrong)
                    {
                        if ((!Gamemanager.All_Checker[x + w, z + b]) || (Gamemanager.All_Checker[x + w, z + b] && Gamemanager.All_Checker[x + w, z + b].transform.position == Object))
                            return new Vector3(x, WrongVector, z);
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        return new Vector3(-1, 0, 0);
    }
    private void PrioritedChangerJumperCategory1(GameObject Object)
    {
        sbyte x, z;
        x = (sbyte)Object.transform.position.x;
        z = (sbyte)Object.transform.position.z; // Оптимизируем проверку координат шашки
        if (x == 0)
        {
            try
            {
                if (!Gamemanager.Gases[z + 1])
                {
                    Category1 = false;
                    Object.GetComponent<Piece>().pryorited.Add(2, 126);
                    TotalPrior += 126;
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (!Gamemanager.Gases[z - 1])
                {
                    Category1 = false;
                    Object.GetComponent<Piece>().pryorited.Add(1, 126);
                    TotalPrior += 126;
                }
            }
            catch (IndexOutOfRangeException) { }
        }
        if (x == 2 && !Object.GetComponent<Piece>().FirstKill)
        {
            try
            {
                if (Gamemanager.All_Checker[(sbyte)Object.transform.position.x - 1, (sbyte)Object.transform.position.z + 1])
                {
                    if (!Gamemanager.Gases[z + 3])
                    {
                        Category1 = false;
                        Object.GetComponent<Piece>().pryorited.Add(2, 127);
                        TotalPrior += 127;
                    }
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (Gamemanager.All_Checker[(sbyte)Object.transform.position.x - 1, (sbyte)Object.transform.position.z - 1])
                {
                    if (!Gamemanager.Gases[z - 3])
                    {
                        Category1 = false;
                        Object.GetComponent<Piece>().pryorited.Add(1, 127);
                        TotalPrior += 127;
                    }
                }
            }
            catch (IndexOutOfRangeException) { }
        }
    }
    private void PrioritedChangerJumperCategory2(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0, CurrentedVector = 0, WrongCurVector = 0, Counter; // Вспомогательные счётчики
        bool Switch, Detector, Debugs;
        Vector3 Current = new(), Checker, Container, ObjectPos = Object.transform.position;
        Dictionary<Vector3, sbyte> Crossroads = new();
        List<Vector3> MoveCombo = new();
        List<sbyte> MoveList = new();
        x = (sbyte)ObjectPos.x;
        z = (sbyte)ObjectPos.z; // Оптимизация проверки диоганалей
        if (x == 0 && !MultyKilling)
        {
            try
            {
                if (Gamemanager.Gases[z + 1])
                {
                    Category2 = false;
                    Counter = 25;
                    for (int i = 0; i < 8; i++) if (Gamemanager.All_Checker[i, z + 1] && Gamemanager.All_Checker[i, z + 1].type > 0) Counter -= 5;
                    Object.GetComponent<Piece>().pryorited.Add(2, Counter);
                    TotalPrior += Counter;
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (Gamemanager.Gases[z - 1])
                {
                    Category2 = false;
                    Counter = 25;
                    for (int i = 0; i < 8; i++) if (Gamemanager.All_Checker[i, z - 1] && Gamemanager.All_Checker[i, z - 1].type > 0) Counter -= 5;
                    Object.GetComponent<Piece>().pryorited.Add(1, Counter);
                    TotalPrior += Counter;
                }
            }
            catch (IndexOutOfRangeException) { }
        }
        if (x == 2 && !Object.GetComponent<Piece>().FirstKill && !MultyKilling)
        {
            try
            {
                if (Gamemanager.All_Checker[1, (sbyte)Object.transform.position.z + 1] && !Gamemanager.All_Checker[0, (sbyte)Object.transform.position.z + 2])
                {
                    if (Gamemanager.Gases[z + 3])
                    {
                        Category2 = false;
                        Counter = 35;
                        for (int i = 0; i < 8; i++) if (Gamemanager.All_Checker[i, z + 3] && Gamemanager.All_Checker[i, z + 3].type > 0) Counter -= 5;
                        Object.GetComponent<Piece>().pryorited.Add(2, Counter);
                        TotalPrior += Counter;
                    }
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (Gamemanager.All_Checker[1, (sbyte)Object.transform.position.z - 1] && !Gamemanager.All_Checker[0, (sbyte)Object.transform.position.z - 2])
                {
                    if (Gamemanager.Gases[z - 3])
                    {
                        Category2 = false;
                        Counter = 35;
                        for (int i = 0; i < 8; i++) if (Gamemanager.All_Checker[i, z - 3] && Gamemanager.All_Checker[i, z - 3].type > 0) Counter -= 5;
                        Object.GetComponent<Piece>().pryorited.Add(1, Counter);
                        TotalPrior += Counter;
                    }
                }
            }
            catch (IndexOutOfRangeException) { }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                Vector++;
                x = (sbyte)((sbyte)ObjectPos.x + w);
                z = (sbyte)((sbyte)ObjectPos.z + b); // Оптимизация проверки диоганалей
                for (sbyte h = 1; h < 3; h++) // Перебор 4 диагоналей
                {
                    Switch = false;
                    try
                    {
                        I = Gamemanager.All_Checker[x, z];
                        if (h == 1 && I && I.type == 0 && !Gamemanager.All_Checker[x + w, z + b])
                        {
                            Switch = true;
                            Current = new Vector3(x + w, 0, z + b);
                            CurrentedVector = Vector;
                            WrongCurVector = 0;
                            if (SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos) != new Vector3(-1, 0, 0)) MoveCombo.Add(new Vector3(x, CurrentedVector, z));
                        }
                    }
                    catch (IndexOutOfRangeException) { }
                    try
                    {
                        I = Gamemanager.All_Checker[x, z];
                        if (h == 2 && (!Object.GetComponent<Piece>().FirstKill) && I && (!Gamemanager.All_Checker[x + w, z + b]) && (!Gamemanager.All_Checker[x + w * 2, z + b * 2]))
                        {
                            Switch = true;
                            Current = new Vector3(x + w * 2, 0, z + b * 2);
                            CurrentedVector = Vector;
                            WrongCurVector = 0;
                            if (SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos) != new Vector3(-1, 0, 0)) MoveCombo.Add(new Vector3(x, CurrentedVector, z));
                        }
                    }
                    catch (IndexOutOfRangeException) { }
                    while (Switch)
                    {
                        if (SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos) != new Vector3(-1, 0, 0))
                        {
                            Checker = SecondMoveChecker(Current, CurrentedVector, WrongCurVector, ObjectPos);
                            Detector = false;
                            for (int i = 0; i < MoveCombo.Count; i++)
                            {
                                if (Checker.x == MoveCombo[i].x && Checker.y == MoveCombo[i].y) Detector = true;
                            }
                            if (Detector)
                            {
                                WrongCurVector = (sbyte)Checker.y;
                            }
                            else
                            {
                                MoveCombo.Add(Checker);
                                if (SecondMoveChecker(Current, CurrentedVector, (sbyte)Checker.y, ObjectPos) != new Vector3(-1, 0, 0))
                                {
                                    Crossroads.Add(new Vector3(Current.x, Checker.y, Current.z), CurrentedVector);
                                }
                                CurrentedVector = (sbyte)Checker.y;
                                Current = new Vector3(Checker.y < 3 ? Checker.x - 1 : Checker.x + 1, 0, Checker.y % 2 == 0 ? Checker.z + 1 : Checker.z - 1);
                            }
                        }
                        else
                        {
                            for (int o = 0; o < MoveCombo.Count; o++)
                            {
                                MoveList.Add((sbyte)MoveCombo[o].y);
                            }
                            if (MoveList.Count > MultyKill.Count)
                            {
                                Category2 = false;
                                if (h == 1) Jump = false; else Jump = true;
                                MultyKill.Clear();
                                MultyKill.AddRange(MoveList);
                                MultyKiller = Object.GetComponent<Piece>();
                                MoveList.Clear();
                            }
                            if (Crossroads.Count != 0)
                            {
                                Debugs = true;
                                for (int i = MoveCombo.Count - 1; i > -1; i--)
                                {
                                    Container = MoveCombo[i];
                                    if (Container.y < 3) Container.x += 1; else Container.x -= 1;
                                    if (Container.y % 2 == 0) Container.z -= 1; else Container.z += 1;
                                    if (Crossroads.ContainsKey(Container))
                                    {
                                        Debugs = false;
                                        CurrentedVector = Crossroads[Container];
                                        Current = new Vector3(Container.x, 0, Container.z);
                                        WrongCurVector = (sbyte)Container.y;
                                        Crossroads.Remove(Container);
                                        MoveCombo.RemoveRange(i, MoveCombo.Count - i);
                                        break;
                                    }
                                }
                                if (Debugs) Debug.Log("Error: Crossroads don't contains MoveCombo[i]; cs: 305; script: AiBlackHard");
                            }
                            else Switch = false;
                        }
                    }
                }
            }
        }
    }
    private void PrioritedChangerJumperCategory3(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0, y; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                Jump = false;
                Vector++;
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!Object.GetComponent<Piece>().FirstKill) && I && (!Gamemanager.All_Checker[x + w, z + b]) && (!Gamemanager.All_Checker[x + w * 2, z + b * 2]))
                    {
                        y = FirstMoveChecker(Object, new Vector3(x + w * 2, 0, z + b * 2), Vector, true);
                        if (y != 0)
                        {
                            Category3 = false;
                            Jump = true;
                            y += 18;
                            if (I.type == 0) y += (sbyte)(WhitePrior / 3);
                            Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                            TotalPrior += y;
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && (!Gamemanager.All_Checker[x + w, z + b]) && (!Jump))
                    {
                        y = FirstMoveChecker(Object, new Vector3(x + w, 0, z + b), Vector, true);
                        if (y != 0)
                        {
                            Category3 = false;
                            y += (sbyte)(6 + WhitePrior / 2);
                            Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                            TotalPrior += y;
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void PrioritedChangerJumperCategory4(GameObject Object)
    {
        Piece I, Piece = Object.GetComponent<Piece>();
        sbyte x, z, Vector = 0, y; // Вспомогательные счётчики
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                Jump = false;
                Vector++;
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизация проверки диоганалей
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!Piece.FirstKill) && I && (!Gamemanager.All_Checker[x + w, z + b]) && (!Gamemanager.All_Checker[x + w * 2, z + b * 2]))
                    {
                        if (I.type == 0)
                        {
                            Jump = true;
                            y = 5;
                            if (WhitePrior > 6) y = 8;
                            Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                            TotalPrior += y;
                        }
                        else
                        {
                            Jump = true;
                            Object.GetComponent<Piece>().pryorited.Add(Vector, 2);
                            TotalPrior += 2;
                        }
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && (!Gamemanager.All_Checker[x + w, z + b]) && (!Jump))
                    {
                        y = (sbyte)(3 + (WhitePrior / 3) * 3);
                        Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                        TotalPrior += y;
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!I) && w == -1 && (!Jump))
                    {
                        y = FirstMoveChecker(Object, new Vector3(x, 0, z), Vector, false);
                        Object.GetComponent<Piece>().pryorited.Add(Vector, y);
                        TotalPrior += y;
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void MovePieceJumper(GameObject Object, sbyte j)
    {
        sbyte i = 1, y;
        Piece Shecker = Object.GetComponent<Piece>();
        Vector3 Position = Shecker.transform.position;
        Speed++;
        MultyKilling = true;
        if (!Category2) PrioritedChangerJumperCategory2(Object);
        if (MultyKill[0] != j) MultyKilling = false;
        else if (Jump) i = 3;
        else i = 2;
        switch (j)
        {
            case 1:
                Shecker.pryorited.TryGetValue(1, out y);
                if (y == 2 || y == 5 || y == 8 || (y > 21 && !Category3) || (y == 127 && !Category1)) i = 3;
                if (y == 3 || y == 6 || y == 9 || (y < 22 && !Category3)) i = 2;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z - i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (!Category2)
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z - i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z - i), Quaternion.Euler(0, 0, 0));
                    }
                    else Gamemanager.Won(1);
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z - i);
                break;
            case 2:
                Shecker.pryorited.TryGetValue(2, out y);
                if (y == 2 || y == 5 || y == 8 || (y > 21 && !Category3) || (y == 127 && !Category1)) i = 3;
                if (y == 3 || y == 6 || y == 9 || (y < 22 && !Category3)) i = 2;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z + i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (!Category2)
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z + i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z + i), Quaternion.Euler(0, 0, 0));
                    }
                    else Gamemanager.Won(1);
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z + i);
                break;
            case 3:
                i = 2;
                Shecker.pryorited.TryGetValue(3, out y);
                if (y == 2 || y == 5 || y == 8 || (y > 21 && !Category3)) i = 3;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z - i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z - i);
                break;
            case 4:
                i = 2;
                Shecker.pryorited.TryGetValue(4, out y);
                if (y == 2 || y == 5 || y == 8 || (y > 21 && !Category3)) i = 3;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z + i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z + i);
                break;
            default: break;
        }
        Shecker.AIKill = true;
    }
    private void MovePieceCheckerHighSpeed(GameObject Object, sbyte j)
    {
        sbyte i = 1, y;
        Piece Shecker = Object.GetComponent<Piece>();
        Vector3 Position = Shecker.transform.position;
        Speed++;
        MultyKilling = true;
        PrioritedChangerCheckerCategory2(Object);
        if (MultyKill[0] != j) MultyKilling = false;
        switch (j)
        {
            case 1:
                Shecker.pryorited.TryGetValue(1, out y);
                Debug.Log(y);
                if ((Category1 && y > 10) || y == 2 || y == 3 || y == 5 || y == 6 || !Category3) i = 2;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z - i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (y == 127) Gamemanager.Won(1);
                    else
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z - i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z - i), Quaternion.Euler(0, 0, 0));
                    }
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z - i);
                break;
            case 2:
                Shecker.pryorited.TryGetValue(2, out y);
                Debug.Log(y);
                if ((Category1 && y > 10) || y == 2 || y == 3 || y == 5 || y == 6 || !Category3) i = 2;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z + i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (y == 127) Gamemanager.Won(1);
                    else
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z + i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z + i), Quaternion.Euler(0, 0, 0));
                    }
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z + i);
                break;
            case 3:
                i = 2;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z - i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z - i);
                break;
            case 4:
                i = 2;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z + i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z + i);
                break;
            default: break;
        }
        Shecker.AIKill = true;
    }

}
