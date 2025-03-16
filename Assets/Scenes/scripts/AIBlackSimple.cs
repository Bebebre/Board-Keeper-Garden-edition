using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBlackSimple : MonoBehaviour
{
    [SerializeField]
    private GameManager Gamemanager; // Обьект с гейм менеджером
    [SerializeField]
    private GameObject Gazon;

    private GameObject[] Blacks = new GameObject[12]; // Заспавненые чёрные шашки для консоли и восклицательные знаки соответственно
    private int TotalPrior, Counter, Value, Speed;
    private System.Random Random = new();
    private bool Murder, Jump = false, KillMode;
    public void Starting()
    {
        KillMode = false;
        for (sbyte i = 0; i < 12; i++)
        {
            Blacks[i] = Gamemanager.Blacks[i];
        }
    }
    public void Move()
    {
        Counter = 0;
        TotalPrior = 0;
        for (sbyte i = 0; i < 12; i++)
        {
            if (Blacks[i]) PrioritedChanger(Blacks[i]);
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
        Counter = 0;
        TotalPrior = 0;
        PrioritedChanger(Object);
        if (TotalPrior > 0 && (Murder || (!KillMode)))
        {
            Value = Random.Next(1, 8);
            if (Value > 1 || (!KillMode))
            {
                Value = Random.Next(1, TotalPrior + 1);
                for (sbyte j = 1; j < 5; j++)
                {
                    Counter += Object.GetComponent<Piece>().pryorited.GetValueOrDefault(j, (sbyte)0);
                    if (Counter >= Value)
                    {
                        MovePiece(Object, j);
                        break;
                    }
                }
            }
            else Non();
        }
        else Non();
    }
    private void Non()
    {
        KillMode = false;
        Gamemanager.WoB_Move = true;
        Speed = 0;
    }
    private void PrioritedChanger(GameObject Object) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        Object.GetComponent<Piece>().pryorited.Clear();
        switch (Type)  // Запускаем нужный спавнер
        {
            case 1: PrioritedChangerChecker(Object); break;
            case 2: PrioritedChangerJumper(Object); break;
            case 3: if (!Object.GetComponent<Piece>().Confused) PrioritedChangerChecker(Object); break;
            case 8: PrioritedChangerChecker(Object); break;
            default: break;
        }
    }
    private void MovePiece(GameObject Object, sbyte j) // Переходник для разных типов шашек
    {
        sbyte Type = Object.GetComponent<Piece>().type; // Запоминаем тип шашки
        switch (Type)  // Запускаем нужный спавнер
        {
            case 1: MovePieceCheckerHighSpeed(Object, j); break;
            case 2: MovePieceJumper(Object, j); break;
            case 3: MovePieceCheckerHighSpeed(Object, j); break;
            case 8: MovePieceCheckerHighSpeed(Object, j); break;
            default: break;
        }
    }
    private void PrioritedChangerChecker(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0; // Вспомогательные счётчики
        x = (sbyte)Object.transform.position.x;
        z = (sbyte)Object.transform.position.z; // Оптимизируем проверку координат шашки
        if (x == 0 && !KillMode)
        {
            try
            {
                if (Gamemanager.Gases[z + 1])
                {
                    Object.GetComponent<Piece>().pryorited.Add(2, 25);
                    TotalPrior += 25;
                }
                else
                {
                    Object.GetComponent<Piece>().pryorited.Add(2, 127);
                    TotalPrior += 127;
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (Gamemanager.Gases[z - 1])
                {
                    Object.GetComponent<Piece>().pryorited.Add(1, 25);
                    TotalPrior += 25;
                }
                else
                {
                    Object.GetComponent<Piece>().pryorited.Add(1, 127);
                    TotalPrior += 127;
                }
            }
            catch (IndexOutOfRangeException) { }
        }
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
                    if (I && I.type == 0 && !Gamemanager.All_Checker[x + w, z + b]) SecondMoveChecker(Object, new Vector3(x + w, 0, z + b), Vector);
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!I) && w == -1 && !KillMode)
                    {
                        FirstMoveChecker(Object, new Vector3(x, 0, z), Vector);
                    } // Обычный ход возможен если убийство не важно
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void FirstMoveChecker(GameObject Object, Vector3 Position, sbyte Vector)
    {
        Piece I;
        sbyte x, z;
        bool Danger = false;
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                x = (sbyte)((sbyte)Position.x + w);
                z = (sbyte)((sbyte)Position.z + b); // Оптимизация проверки диоганалей
                I = Gamemanager.All_Checker[x, z];
                if (I && I.type == 0 && Object.GetComponent<Piece>().speed == Speed + 1)
                {
                    if (!Gamemanager.All_Checker[-1 * w + x, -1 * b + z])
                    {
                        Danger = true;
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 1);
                        TotalPrior += 1;
                        break;
                    }
                    else
                    {
                        Danger = true;
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 12);
                        TotalPrior += 12;
                        break;
                    }
                }
                if (I && I.type > 0)
                {
                    if (Gamemanager.All_Checker[2 * w + x, 2 * b + z] && Gamemanager.All_Checker[2 * w + x, 2 * b + z].type == 0)
                    {
                        Danger = true;
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 16);
                        TotalPrior += 16;
                        break;
                    }
                    else
                    {
                        Danger = true;
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 8);
                        TotalPrior += 8;
                        break;
                    }
                }
            }
            if (Danger) break;
        }
        if (!Danger)
        {
            Object.GetComponent<Piece>().pryorited.Add(Vector, 6);
            TotalPrior += 6;
        }
    }
    private void SecondMoveChecker(GameObject Object, Vector3 Position, sbyte Vector)
    {
        Piece I;
        bool Danger = false, MoreKill = false;
        sbyte WrongVector = 5, x, z;
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                WrongVector--;
                if (WrongVector == Vector) continue;
                x = (sbyte)((sbyte)Position.x + w);
                z = (sbyte)((sbyte)Position.z + b); // Оптимизация проверки диоганалей
                I = Gamemanager.All_Checker[x, z];
                if (I && I.type == 0 && !Gamemanager.All_Checker[x + w, z + b])
                {
                    MoreKill = true;
                    if (Jump)
                    {
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 55);
                        TotalPrior += 55;
                    }
                    else
                    {
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 50);
                        TotalPrior += 50;
                    }
                    break;
                }
            }
            if (MoreKill) break;
        }
        WrongVector = 5;
        for (sbyte w = -1; w < 2; w += 2)
        {
            if (MoreKill) break;
            for (sbyte b = -1; b < 2; b += 2) // Перебор 4 диагоналей
            {
                WrongVector--;
                if (WrongVector == Vector) continue;
                x = (sbyte)((sbyte)Position.x + w);
                z = (sbyte)((sbyte)Position.z + b); // Оптимизация проверки диоганалей
                I = Gamemanager.All_Checker[x, z];
                if (I && I.type == 0 && Gamemanager.All_Checker[x + w, z + b] && !Gamemanager.All_Checker[-1 * w + x, -1 * b + z] && Object.GetComponent<Piece>().speed == Speed + 1)
                {
                    Danger = true;
                    if (Jump)
                    {
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 15);
                        TotalPrior += 15;
                    }
                    else
                    {
                        Object.GetComponent<Piece>().pryorited.Add(Vector, 10);
                        TotalPrior += 10;
                    }
                    break;
                }
            }
            if (Danger) break;
        }
        if ((!Danger) && (!MoreKill))
        {
            if (KillMode)
            {
                if (Jump)
                {
                    Object.GetComponent<Piece>().pryorited.Add(Vector, 55);
                    TotalPrior += 55;
                }
                else
                {
                    Object.GetComponent<Piece>().pryorited.Add(Vector, 50);
                    TotalPrior += 50;
                }
            }
            else
            {
                if (Jump)
                {
                    Object.GetComponent<Piece>().pryorited.Add(Vector, 35);
                    TotalPrior += 35;
                }
                else
                {
                    Object.GetComponent<Piece>().pryorited.Add(Vector, 30);
                    TotalPrior += 30;
                }
            }
        }
    }
    private void PrioritedChangerJumper(GameObject Object)
    {
        Piece I;
        sbyte x, z, Vector = 0; // Вспомогательные счётчики
        x = (sbyte)Object.transform.position.x;
        z = (sbyte)Object.transform.position.z; // Оптимизируем проверку координат шашки
        if (x == 0 && !KillMode)
        {
            try
            {
                if (Gamemanager.Gases[z + 1])
                {
                    Object.GetComponent<Piece>().pryorited.Add(2, 20);
                    TotalPrior += 20;
                }
                else
                {
                    Object.GetComponent<Piece>().pryorited.Add(2, 126);
                    TotalPrior += 126;
                }
            }
            catch (IndexOutOfRangeException) { }
            try
            {
                if (Gamemanager.Gases[z - 1])
                {
                    Object.GetComponent<Piece>().pryorited.Add(1, 20);
                    TotalPrior += 20;
                }
                else
                {
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
                if (Gamemanager.All_Checker[1, (sbyte)Object.transform.position.z + 1] && !Gamemanager.All_Checker[0, (sbyte)Object.transform.position.z + 2])
                {
                    if (Gamemanager.Gases[z + 3])
                    {
                        Object.GetComponent<Piece>().pryorited.Add(2, 40);
                        TotalPrior += 40;
                    }
                    else
                    {
                        Object.GetComponent<Piece>().pryorited.Add(2, 127);
                        TotalPrior += 127;
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
                        Object.GetComponent<Piece>().pryorited.Add(2, 40);
                        TotalPrior += 40;
                    }
                    else
                    {
                        Object.GetComponent<Piece>().pryorited.Add(2, 127);
                        TotalPrior += 127;
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
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // Оптимизация проверки диоганалей
                Jump = false;
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!Object.GetComponent<Piece>().FirstKill) && I && (!Gamemanager.All_Checker[x + w, z + b]) && (!Gamemanager.All_Checker[x + w * 2, z + b * 2]))
                    {
                        Jump = true;
                        SecondMoveChecker(Object, new Vector3(x + w * 2, 0, z + b * 2), Vector);
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if (I && I.type == 0 && (!Gamemanager.All_Checker[x + w, z + b]) && (!Jump)) SecondMoveChecker(Object, new Vector3(x + w * 2, 0, z + b * 2), Vector);
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Gamemanager.All_Checker[x, z];
                    if ((!I) && w == -1 && (!KillMode) && (!Jump))
                    {
                        FirstMoveChecker(Object, new Vector3(x, 0, z), Vector);
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void MovePieceJumper(GameObject Object, sbyte j)
    {
        Murder = false; sbyte i = 1, y;
        Piece Shecker = Object.GetComponent<Piece>();
        Vector3 Position = Shecker.transform.position;
        Speed++;
        switch (j)
        {
            case 1:
                Shecker.pryorited.TryGetValue(1, out y);
                if (y == 15 || y == 35 || y == 55 || y == 40 || y == 127)
                {
                    Murder = true;
                    i = 3;
                }
                if (y == 50 || y == 30 || y == 10)
                {
                    Murder = true;
                    i = 2;
                }
                Debug.Log(i + " " + y + " " + (sbyte)Position.x + " " + (sbyte)Position.z);
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z - i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (y == 40 || y == 20)
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z - i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z - i), Quaternion.Euler(0, 0, 0));
                    }
                    else if (y == 126 || y == 127) Gamemanager.Won(1);
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z - i);
                break;
            case 2:
                Shecker.pryorited.TryGetValue(2, out y);
                if (y == 15 || y == 35 || y == 55 || y == 40 || y == 127)
                {
                    Murder = true;
                    i = 3;
                }
                if (y == 50 || y == 30 || y == 10)
                {
                    Murder = true;
                    i = 2;
                }
                Debug.Log(i + " " + y + " " + (sbyte)Position.x + " " + (sbyte)Position.z);
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z + i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (y == 40 || y == 20)
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z + i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z + i), Quaternion.Euler(0, 0, 0));
                    }
                    else if (y == 126 || y == 127) Gamemanager.Won(1);
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z + i);
                break;
            case 3:
                i = 2;
                Murder = true;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z - i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z - i);
                break;
            case 4:
                i = 2;
                Murder = true;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z + i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z + i);
                break;
            default: break;
        }
        if (Speed == Shecker.speed) KillMode = true;
        Shecker.AIKill = true;
    }
    private void MovePieceCheckerHighSpeed(GameObject Object, sbyte j)
    {
        Murder = false; sbyte i = 1, y;
        Piece Shecker = Object.GetComponent<Piece>();
        Vector3 Position = Shecker.transform.position;
        Speed++;
        switch (j)
        {
            case 1:
                Shecker.pryorited.TryGetValue(1, out y);
                if (y == 50 || y == 30 || y == 10)
                {
                    Murder = true;
                    i = 2;
                }
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z - i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (y == 25)
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z - i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z - i), Quaternion.Euler(0, 0, 0));
                    }
                    else if (y == 127) Gamemanager.Won(1);
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z - i);
                break;
            case 2:
                Shecker.pryorited.TryGetValue(2, out y);
                if (y == 50 || y == 30 || y == 10)
                {
                    Murder = true;
                    i = 2;
                }
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                try
                {
                    Gamemanager.All_Checker[(sbyte)Position.x - i, (sbyte)Position.z + i] = Shecker;
                }
                catch (IndexOutOfRangeException)
                {
                    if (y == 25)
                    {
                        Destroy(Gamemanager.Gases[(sbyte)Position.z + i]);
                        Instantiate(Gazon, new Vector3(-1, 0, (sbyte)Position.z + i), Quaternion.Euler(0, 0, 0));
                    }
                    else if (y == 127) Gamemanager.Won(1);
                }
                Shecker.PointToMove = new Vector3(Position.x - i, 0, (sbyte)Position.z + i);
                break;
            case 3:
                i = 2;
                Murder = true;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z - i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z - i);
                break;
            case 4:
                i = 2;
                Murder = true;
                Gamemanager.All_Checker[(sbyte)Position.x, (sbyte)Position.z] = null;      // Удаляем координаты шашки из матрицы
                Gamemanager.All_Checker[(sbyte)Position.x + i, (sbyte)Position.z + i] = Shecker;
                Shecker.PointToMove = new Vector3(Position.x + i, 0, (sbyte)Position.z + i);
                break;
            default: break;
        }
        if (Speed == Shecker.speed) KillMode = true;
        Shecker.AIKill = true;
    }
}
