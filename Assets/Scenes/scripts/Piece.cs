using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
     // Объект с гейм менеджером нельзя поместить в скрипт, так как шашки спавнятся, а не стоят на сцене
    [SerializeField]
    private sbyte HP; // Тип, скорость и здоровье

    private bool Moving = false, OneUse = true, ArmorDefuse = false; // Съёмщик брони и защитные флаги от повторного использования
    private Color defult;  // Цвет шашки
    private Material material;
    
    public GameManager Manager;
    public sbyte speed, type; // Тип, скорость и здоровье
    public Dictionary<sbyte, sbyte> pryorited = new();
    // 1 = -z; -x;
    // 2 = z; -x;
    // 3 = -z; x;
    // 4 = x; z;
    // 5 >= special;
    
    public bool FirstKill = false, Armor = false, Confused = false, AIKill = false, Kill = false, UnTouched = true, isQween , Stat; // Сама броня и флаг первого убийства
    public Vector3 PointToMove;     // Точка в которую безпрерывно движется шашка если она не стоит в ней (Сюда передаём координаты плиток)
    public List<Vector3> BannedCoords = new();
    void Awake() // Запускается в начале (Если написать Awake, то будет пипец, так как большая часть шашек не заспавнится к этому моменту)
    {
        PointToMove = transform.position; // Заставляем шашку стоять на месте
        BannedCoords.Add(new(0, 2, 0));
        material = transform.GetChild(0).gameObject.GetComponent<Renderer>().material;
        defult = material.color;
    }
    void Update() 
    {
        if (PointToMove != transform.position && !Moving) // Если шашка движется, то флаг статичности на ноль
        { Moving = true; Stat = false; Kill = false; }
        if (PointToMove == transform.position && Moving)  // Если шашка не движется, то включить флаг статичности 
        { 
            Moving = false; Stat = true;
            if (AIKill) {  AIKill = false; Manager.AIKillBridge(gameObject);}
        }
        transform.position = Vector3.MoveTowards(transform.position, PointToMove, 0.1f); // Вечный двигатель двигатель
    }
    public void Select() // Перекрас в серый
    {
        if (OneUse) 
        {
            material.color = Color.gray;
            OneUse = false;
        }
    }
    public void Deselect() // Перекрас назад
    {
        if (!OneUse)
        {
            material.color = defult;
            OneUse = true;
        }
    }
    private void OnCollisionEnter(Collision c) // Если столкнулись с кем - то
    {
        Gazonokosilka G = null;
        Piece P = null;
        if (c.gameObject.GetComponent<Gazonokosilka>()) G = c.gameObject.GetComponent<Gazonokosilka>();
        if (c.gameObject.GetComponent<Piece>()) P = c.gameObject.GetComponent<Piece>(); 
        if (G && type > 0)
        {
            if (!G.Gas)
            {
                G.Gas = true;
                if (AIKill) Manager.AIKillBridge(null);
            }
            if (Stat) Manager.All_Checker[(sbyte)transform.position.x, (sbyte)transform.position.z] = null;
            Manager.Blacks.Remove(this.gameObject);
            Destroy(gameObject);
        }
        if (P) // Если это шашка
        {
            if (P.type != type && (type == 0 || P.type == 0) && Stat) // Если это враг и мы статичны
            {
                if (P.FirstKill || P.type != 2)
                { 
                    if (Armor) ArmorDefuse = true; else HP--; // Снять броню или получить урон
                    P.Kill = true;                
                }
                UnTouched = false;
                if (HP == 0) // Если хп в нулину
                {
                    Manager.All_Checker[(sbyte)transform.position.x, (sbyte)transform.position.z] = null;
                    if (type > 0) Manager.Blacks.Remove(this.gameObject);
                    Destroy(gameObject); // Совершаем роскомнадзор
                }
            }
        }
    }
    private void OnCollisionExit(Collision c) // Еслии столкновение прекратилось
    {
        if (!Stat) // Мы в движении
        {
            FirstKill = true; // Флаг первого кила на вкл
            if (type == 2) speed = 1;
        }
        if (ArmorDefuse) // Если нам ударили по броне
        {
            c.gameObject.GetComponent<Piece>().Kill = true;
            Armor = false; // Выкл броню
            if (type == 3) // Если мы - газетчик
            {
                Confused = true;
                speed = 2;
                Manager.Confus(transform.position);
            }
            ArmorDefuse = false; // Запрещаем повторное использование
        }
    }
}