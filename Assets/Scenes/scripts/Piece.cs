using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
     // ������ � ���� ���������� ������ ��������� � ������, ��� ��� ����� ���������, � �� ����� �� �����
    [SerializeField]
    private sbyte HP; // ���, �������� � ��������

    private bool Moving = false, OneUse = true, ArmorDefuse = false; // ������� ����� � �������� ����� �� ���������� �������������
    private Color defult;  // ���� �����
    private Material material;
    
    public GameManager Manager;
    public sbyte speed, type; // ���, �������� � ��������
    public Dictionary<sbyte, sbyte> pryorited = new();
    // 1 = -z; -x;
    // 2 = z; -x;
    // 3 = -z; x;
    // 4 = x; z;
    // 5 >= special;
    
    public bool FirstKill = false, Armor = false, Confused = false, AIKill = false, Kill = false, UnTouched = true, isQween , Stat; // ���� ����� � ���� ������� ��������
    public Vector3 PointToMove;     // ����� � ������� ����������� �������� ����� ���� ��� �� ����� � ��� (���� ������� ���������� ������)
    public List<Vector3> BannedCoords = new();
    void Awake() // ����������� � ������ (���� �������� Awake, �� ����� �����, ��� ��� ������� ����� ����� �� ����������� � ����� �������)
    {
        PointToMove = transform.position; // ���������� ����� ������ �� �����
        BannedCoords.Add(new(0, 2, 0));
        material = transform.GetChild(0).gameObject.GetComponent<Renderer>().material;
        defult = material.color;
    }
    void Update() 
    {
        if (PointToMove != transform.position && !Moving) // ���� ����� ��������, �� ���� ����������� �� ����
        { Moving = true; Stat = false; Kill = false; }
        if (PointToMove == transform.position && Moving)  // ���� ����� �� ��������, �� �������� ���� ����������� 
        { 
            Moving = false; Stat = true;
            if (AIKill) {  AIKill = false; Manager.AIKillBridge(gameObject);}
        }
        transform.position = Vector3.MoveTowards(transform.position, PointToMove, 0.1f); // ������ ��������� ���������
    }
    public void Select() // �������� � �����
    {
        if (OneUse) 
        {
            material.color = Color.gray;
            OneUse = false;
        }
    }
    public void Deselect() // �������� �����
    {
        if (!OneUse)
        {
            material.color = defult;
            OneUse = true;
        }
    }
    private void OnCollisionEnter(Collision c) // ���� ����������� � ��� - ��
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
        if (P) // ���� ��� �����
        {
            if (P.type != type && (type == 0 || P.type == 0) && Stat) // ���� ��� ���� � �� ��������
            {
                if (P.FirstKill || P.type != 2)
                { 
                    if (Armor) ArmorDefuse = true; else HP--; // ����� ����� ��� �������� ����
                    P.Kill = true;                
                }
                UnTouched = false;
                if (HP == 0) // ���� �� � ������
                {
                    Manager.All_Checker[(sbyte)transform.position.x, (sbyte)transform.position.z] = null;
                    if (type > 0) Manager.Blacks.Remove(this.gameObject);
                    Destroy(gameObject); // ��������� ������������
                }
            }
        }
    }
    private void OnCollisionExit(Collision c) // ����� ������������ ������������
    {
        if (!Stat) // �� � ��������
        {
            FirstKill = true; // ���� ������� ���� �� ���
            if (type == 2) speed = 1;
        }
        if (ArmorDefuse) // ���� ��� ������� �� �����
        {
            c.gameObject.GetComponent<Piece>().Kill = true;
            Armor = false; // ���� �����
            if (type == 3) // ���� �� - ��������
            {
                Confused = true;
                speed = 2;
                Manager.Confus(transform.position);
            }
            ArmorDefuse = false; // ��������� ��������� �������������
        }
    }
}