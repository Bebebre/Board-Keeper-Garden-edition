using System;
using System.Collections.Generic;
using UnityEngine;    // ��� ��������� ����� ��� ���� ����� ����� ���� �� ��������� ���������

public class BlackBrain : MonoBehaviour
{
    [SerializeField]
    private GameObject Plane; // ������
    [SerializeField]
    private GameObject GrayPlane; // ����� ������

    [SerializeField]
    private GameObject ActivGazonokosilka; // ������������� �������������
    [SerializeField]
    private GameObject Gazonokocilka; // �������������
    [SerializeField]
    private GameManager Manager; // ������ � ���� ����������


    private GameObject Gas1, Gas2, Clicked;             // ������ � ������ �������������, ������� ����� ������������ � ����������� ��� �������
    private List<GameObject> Planes = new();            // �������� ������
    private Vector3 PlaneCoordinate;             // ���������� ����� ��� ���������� ����
    private Piece Current, Currented;             // ����������� � �������� �����
    private sbyte CurrentSpeed = 1;             // ������� ��� ������� �������� 
    private bool Continue, Blocked, End, Select, Selecting;  // ���� ������������ ����������� ������ ��������, ���� ����������� ������ ��������, ���� ����� ����, ���� ����������� ����� ����� � ���� �������� �� �����
    public bool WoB_Move, OnlyKill;                 // ��������� ���� ����
    private RaycastHit hit;                 // ������������ ���� � ��������
    private Ray ray;                     // ���

    private void Awake()
    {
        Continue = Blocked = End = false;
        WoB_Move = Select = Selecting  = true;
    }
    private void Update()         // ����������� ������ ����  
    {
        if (!WoB_Move) // ������ ������� ������ � ���� ���
        {
// ************************************    �������� ��������� ����� ��� ��������    **********************************************************
            ray = Camera.main.ScreenPointToRay(Input.mousePosition); // ������ ����
            if (Physics.Raycast(ray, out hit)) // ���� ��� ����� �� ��� - ��
            {
                Piece shecker = hit.collider.gameObject.GetComponent<Piece>(); // ���������� ����� � ������� ������
                if (shecker && shecker.Stat && Select) //���� ������ � ��������� ����� � ����� ��� �� �������� �� ���� ����
                {
                    if (Current != shecker && (!shecker.Confused) && shecker.type > 0 ) // �������� �� ����� (�������� ��������) � ������� ����� ��� �������� �� ������� ����� �����
                    {
                        if (Current) Non(); // ������� ����� ����� � ������� ��������� ���� ��� ����
                        Current = shecker; // ���������� ���� ����� ��� �����
                        shecker.Select(); // �������������
                    }
                }
                else Non(); // ����� ����� �����
            }
            else  Non(); // ����� ����� �����
// *************************   �������� ��������, �������� ����� ���� � ������� ������ ��������   ********************************************
            if (Currented && Currented.Stat && (End || Blocked))   // ���� ������������� ���� ����� ������������
            {
                if (CurrentSpeed < Currented.speed)
                {
                    Blocked = End = false;
                    if (CheckMove(Currented.transform.position, Currented.type, false)) 
                    { 
                        Selected(Currented);
                    }
                    else 
                        End = true;    // ����� ����� ����
                    Currented.Kill = false;  // ������������� � ������� ����� "������" � �������� �������
                    CurrentSpeed++;
                } 
                if (Blocked)  // ���������� ��� ���� ���������
                {
                    if (Currented.Kill) // ���� �� ����-�� ����� (��� ������ �� ���������� �������������)
                    {
                        if (CheckMove(Currented.transform.position, Currented.type, true)) // ���� ���� ��������� ���� (�������� �����������)
                            Selected(Currented);              // ������� ������ ��� ������� �����
                        else
                            End = true;    // ����� ����� ����
                        Blocked = false;   // ������ �� ���������� ������������� 
                        Currented.Kill = false;  // ������������� � ������� ����� "������" � �������� �������
                    }
                    else End = true; // ����� ���� ���� ��� ������
                }
                if (End) { EndTurn(); }  // ����� ���� ���� ��� ���������
            }
//*****************************************************   �������� ������� ���   *************************************************************
            if (Input.GetKeyDown(KeyCode.Mouse0) && hit.collider) // ���� �� ������ ��� �� ��� - ��
            {
                Clicked = hit.collider.gameObject;     // ������������ �������� �������� �������
                if (Clicked.GetComponent<Gazonokosilka>())    //������� �� �������������
                {
                    PlaneCoordinate = Clicked.transform.position;    // ���������� ���������� �������������
                    Currented.PointToMove = PlaneCoordinate;      // ��������� �������� ����� � �������������
                    Currented.Stat = false;                             // ���������� �� ������ ���������� ������� ��������� ��������� ����
                    Manager.All_Checker[(sbyte)Currented.transform.position.x, (sbyte)Currented.transform.position.z] = null;      // ������� ���������� ����� �� �������
                    if (Clicked == Gas1) Destroy(Gas2);     // ������� ���� ������ �� ����...
                    if (Clicked == Gas2) Destroy(Gas1);     // �������������� �������������
                    Destroy(Manager.Gases[(sbyte)PlaneCoordinate.z]);   // ������� ������������� �� ��� �������������� �������������
                    EndTurn();  // ����������� ���
                }
                if (Clicked.CompareTag("Plane")) //������� �� �����
                {
                    PlaneCoordinate = Clicked.transform.position;    // ���������� ���������� �������������
                    Currented.PointToMove = PlaneCoordinate;      // ��������� �������� ����� � �������������
                    Currented.Stat = false;                             // ���������� �� ������ ���������� ������� ��������� ��������� ����
                    Manager.All_Checker[(sbyte)Currented.transform.position.x, (sbyte)Currented.transform.position.z] = null;      // ������� ���������� ����� �� �������
                    if (PlaneCoordinate.x != -1)  // ���� ��� �� ��� � �������� ����
                        Manager.All_Checker[(sbyte)PlaneCoordinate.x, (sbyte)PlaneCoordinate.z] = Currented; // ���������� � ������� ������� ���������� ����� �����
                    if (Continue) Blocked = true; else End = true; // ���������� - ����� �� �� ��������� ������ ��������
                    PlaneBust();        // ����������� ��� ������
                    Selecting = Select = false; // ��������� ����� ����� ��������
                }
                if (Select) // ���� ����� ����� ��������  
                {  
                    if (Currented) //���� ���� ��� ��������
                    { 
                        if (Currented.transform.position != Clicked.transform.position) // ���� �������� ����� �� ����� �������
                        {
                            Piece A = Clicked.GetComponent<Piece>();
                            if (A && A.type > 0 && !A.Confused)  // ���� �������� �� ����� � ��� ������
                            {
                                PlaneBust();           // ����������� ��� ������
                                Selected(Clicked.GetComponent<Piece>());          // ������� ������ ��� ����� �����
                            }
                        }
                        else    // ���� ������ �� ���������� �����
                        {
                            PlaneBust();                // ����������� ��� ������
                            Currented = null;     // �������� �������� �����
                        }
                    }
                    else if (Current) Selected(Current); // ���� �� ������ �� �����
                }
                //DebugMatrice();   // ��� ������������ ��� �������� �������
                Clicked = null;     // �������� �����������
            }
//*****************************************************************************************************************************************
        }
    }

    private void DebugMatrice()   // �������� ���������� �������
    {
        string matrice = " "; // ����� � ��������
        for (sbyte x = 7; x > -1; x--)
        {
            for (sbyte z = 7; z > -1; z--) // ���������� �������
            {
                if (!Manager.All_Checker[x, z]) matrice += "0 "; // 0 - �����
                else if (Manager.All_Checker[x, z].type == 0) matrice += "3 "; // 3 - �����
                else if (Manager.All_Checker[x, z].type > 0) matrice += "2 ";  // 2 - ������
            }
            matrice += "\n "; // ������� ������ ������ 8 ��������
        }
        Debug.Log(matrice); // ����� ����������
        // �����:
        // 0 2 0 2 0 2 0 2
        // 2 0 2 0 2 0 2 0
        // 0 2 0 0 0 2 0 2
        // 0 0 0 0 2 0 0 0
        // 0 0 0 3 0 0 0 0
        // 3 0 0 0 3 0 3 0
        // 0 3 0 3 0 3 0 3
        // 3 0 3 0 3 0 3 0
    }
    public void EndTurn()    // ����������� ��� ��������� ����
    {
        bool i = true;  // ���� ��� �������� �� ������ ����� ��������
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // ������� �������
            {
                if (Manager.All_Checker[x, z] && Manager.All_Checker[x, z].type == 0) 
                { 
                    i = false; // ��������� ���������� ������ ���� ���� ���� ���� ����� �����
                    break; 
                }
            }
            if (!i) break; // �����������
        }
        if (i || (Currented && Currented.transform.position.x == -1))
            Manager.Won(1); // �������� ������ ���� �������� ��� ��� ��� ����� �����
        if (Currented)   // ��������� ��� ���������...
            Currented.Deselect();
        PlaneBust();
        Currented = null;
        End = Continue = Blocked = false;
        Manager.WoB_Move = WoB_Move = Select = Selecting = true;
        CurrentSpeed = 1; // � ��������� �������
    }
    private void Non() // ����������� ���� �� ������ ������ �� �����
    {
        if (Current) 
            Current.Deselect(); //������������� ����� Current ���� ��� ����
        Current = null; // �������� Current
    }
    private void PlaneBust() // ����������� ��� ���������� ���� � �� ������
    {
        for (byte y = 0; y < Planes.Count; y++) 
        {
            Destroy(Planes[y]); // ������� � ���������� ����
        }
        if (Gas1 && PlaneCoordinate != Gas1.transform.position && Gas1.transform.position.x == -1.25)
        {
            Manager.Gases[(int)Gas1.transform.position.z] = Instantiate(Gazonokocilka, new Vector3((float)-1.25, (float)-0.08, (int)Gas1.transform.position.z), Quaternion.Euler(0, 180, 0));
            Destroy(Gas1);  // ������� �� ������������� ������������� �������������
        }
        if (Gas2 && PlaneCoordinate != Gas2.transform.position && Gas2.transform.position.x == -1.25)
        {
            Manager.Gases[(int)Gas2.transform.position.z] = Instantiate(Gazonokocilka, new Vector3((float)-1.25, (float)-0.08, (int)Gas2.transform.position.z), Quaternion.Euler(0, 180, 0));
            Destroy(Gas2);  // ������� �� ������������� ������������� �������������
        }
        Planes.Clear(); // ������� ������� ����
    }
    private void Selected(Piece piece) // ����������� ��� ������ ����
    {
        Continue = false;  // ��������� ����� ���������� �������� �� ���������
        if (CheckMove(piece.transform.position, piece.type, OnlyKill)) // ���� ���� ��������� ���� (�������� �� �����������)
        {
            Currented = piece;   // ������ ����� ���������
            if (OnlyKill) Select = false;
            PlaneSpawn(Currented.gameObject); // ��������� ����� ����
            if (OnlyKill) Select = true;
        }
        else Currented = null; // ����� - �������� ���������� �����
    }
    private bool Find(List<Vector3> A, sbyte x, sbyte z) // �������� �� ������� ��������� � �������
    {
        Vector3 V = new(x, 0, z); // �������� ��������� ��� ��������
        for (sbyte i = 0; i < A.Count; i++)
        {
            if (A[i] == V) return true; // ���������� �������� � ���������� "��" ���� ����� ����������
        }
        return false; // ���������� "���"
    }
    private void FindForDestroyPlane(sbyte x, sbyte z) // �������� �� ������� ��������� � �������
    {
        Vector3 V = new(x, 0, z); // �������� ��������� ��� ��������
        for (sbyte i = 0; i < Planes.Count; i++)

        {
            if (Planes[i].transform.position == V && Planes[i].CompareTag("PlaneGray")) { Destroy(Planes[i]); Planes.Remove(Planes[i]); break; } // ������� � ���������� �����
        }
    }
    public bool CheckMove(Vector3 Object, sbyte Type, bool Kill) // ���������� ��� ������ ����� �����
    {
        switch (Type)  // ��������� ������ ��������
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
    private void PlaneSpawn(GameObject Object) // ���������� ��� ������ ����� �����
    {
        sbyte Type = Object.GetComponent<Piece>().type; // ���������� ��� �����
        switch (Type)  // ��������� ������ �������
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
        sbyte x, z; // ��������������� ��������
        for (sbyte v = -1; v < 2; v += 2)
        {
            for (sbyte d = -1; d < 2; d += 2) // ������� 4 ����������
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
    private bool CheckMoveChecker(Vector3 Object, bool Kill) // �������� ���� ��� �����
    {
        Piece I;
        sbyte x, z; // ��������������� ��������
        if (Object.x == 0 && ((!Kill) || OnlyKill)) return true;  // ��� ��������, ��� ��� �� ������� � ����
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // ������� 4 ����������
            {
                x = (sbyte)((sbyte)Object.x + w);
                z = (sbyte)((sbyte)Object.z + b); // ����������� �������� ����������
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
                        else return true; // ���� ����� ����-�� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Manager.All_Checker[(int)Object.x, (int)Object.z];
                    if ((!Manager.All_Checker[x, z]) && w == -1 && (!Kill) && !I.BannedCoords.Contains(new(x, 0, z))) return true; // ���� ����� ����-�� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        return false; //��� �� ��������
    }
    private bool CheckMoveJumper(Vector3 Object, bool Kill)
    {
        Piece I;
        sbyte x, z; // ��������������� ��������
        x = (sbyte)Object.x;
        z = (sbyte)Object.z; // ������������ �������� ��������� �����
        if (x == 0 && ((!Kill) || OnlyKill)) return true;  // ��� ��������, ��� ��� �� ������� � ����
        if (x == 2 && Manager.All_Checker[x, z].FirstKill) 
        {          // ��� �������� ���� � ��� ���� ������, �� � ���� ������� �� ���� � ����� ���� �����
            try {if (Manager.All_Checker[x - 1, z + 1] && (!Manager.All_Checker[x - 2, z + 2]) && z < 5) return true; }
            catch (IndexOutOfRangeException) { }
            try {if (Manager.All_Checker[x - 1, z - 1] && (!Manager.All_Checker[x - 2, z - 2]) && z > 2) return true; }
            catch (IndexOutOfRangeException) { }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // ������� 4 ����������
            {
                x = (sbyte)((sbyte)Object.x + w);
                z = (sbyte)((sbyte)Object.z + b); // ����������� �������� ����������
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
                        else return true; // ���� ����� ����-�� �����, �� ��� ��������// ���� ����� ����-�� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { }// ��� ����� �������� �� ������ ������ ���� �� ������ �� ������� �������
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
                        else return true; // ���� ����� ����-�� �����, �� ��� ��������// ���� ����� ����-�� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Manager.All_Checker[x - w, z - b];
                    if ((!Manager.All_Checker[x, z]) && w == -1 && (!Kill) && !I.BannedCoords.Contains(new(x, 0, z))) return true; // ���� ����� ����-�� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { }// ��� ����� �������� �� ������ ������ ���� �� ������ �� ������� �������
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
    private bool CheckMoveGiant(Vector3 Object, bool Kill) { return false; } // ������� ���������
    // ������ ���������:
    // ������� ����� -> ��������� ����� -> �������� ������ ���� ����������� ����� -> ��������� ����� ����� ������������ ��� -> �������� ������ ���� ����������� ����� �����... � ��� �� �����
    private void PlaneSpawnChecker(GameObject Object, List<Vector3> AllThings) // ������� ���� ��� ������� �����
    {
        Vector3 This = Object.transform.position; // ���������� �������
        Piece piece = Object.GetComponent<Piece>(), I;
        GameObject plane;  // ��������������� ���������
        sbyte x = (sbyte)This.x, z = (sbyte)This.z; // ������������ �������� ���������
        if (piece)
        {
            if (x == 0) //���� �� � ������ � ���� � ������� ������ ��� �����
            {
                if (z < 7) // ���� ���������� ��������� �� ����� �� ������� �����
                {
                    if (Manager.Gases[z + 1])   // ���� ������������� ����� ����� �� ���
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z + 1), Quaternion.Euler(0, 180, 0)); // ������� �������������� �������������
                        Destroy(Manager.Gases[z + 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // ����� ������� ������� ������ ���� ��� �����
                        Planes.Add(plane); // ��������� ����� � ��������
                    }
                }
                if (z > 0) // ���� ���������� ��������� �� ����� �� ������� �����
                {
                    if (Manager.Gases[z - 1])    // ���� ������������� ����� ������ �� ���
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z - 1), Quaternion.Euler(0, 180, 0)); // ������� �������������� �������������
                        Destroy(Manager.Gases[z - 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // ����� ������� ������� ������ ���� ��� �����
                        Planes.Add(plane); // ��������� ����� � ��������
                    }
                }
            }
            if(!Blocked)
            {
                try
                {
                    if ((!Manager.All_Checker[x - 1, z + 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z + 1)))// ���� ������ ����� �����
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // ������� �����
                        Planes.Add(plane);  // ��������� � � �������
                    }
                }
                catch (IndexOutOfRangeException)  { }
                try
                {
                    if ((!Manager.All_Checker[x - 1, z - 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z - 1))) // ���� ������ ������ �����
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // ������� �����
                        Planes.Add(plane);     // ��������� ����� � �������
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // ���������� ���������
            {
                x = (sbyte)((sbyte)This.x + w);
                z = (sbyte)((sbyte)This.z + b); // ������������ �������� ����������
                try
                {
                    I = Manager.All_Checker[x, z]; 
                    if (I && I.type == 0 && (!Manager.All_Checker[x + w, z + b]) && (!Find(AllThings, x, z))) // ���� ����� ����� ����� �� ��������� � ��������
                    {
                        if (!piece) // ���� ������ �������� ������, �� �������� ���� ������ ��������
                            Continue = true;     
                        else
                        {
                            FilterAnalis(new(x + w, 0, z + b), piece, new(0, 2, 0));
                            if (Manager.All_Checker[x - w, z - b].Stat) continue;
                            Manager.All_Checker[x - w, z - b].Stat = true;
                        }
                        FindForDestroyPlane((sbyte)(x + w), (sbyte)(z + b));
                        AllThings.Add(new(x, 0, z));
                        plane = Instantiate(piece ? Plane : GrayPlane , new Vector3(x + w , 0, z + b), Quaternion.Euler(0, 0, 0)); // ������� �����
                        Planes.Add(plane);  // ��������� ����� � ��������
                        PlaneSpawnChecker(plane, AllThings);  // �������� ������� ������ �������� (��, ��� �����)
                        AllThings.RemoveAt(AllThings.Count - 1);
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
    private void PlaneSpawnJumper(GameObject Object, List<Vector3> AllThings)  // ������� ���� ��� ��������
    {
        GameObject plane;  // ��������������� ���������
        Piece piece = Object.GetComponent<Piece>(), I;
        sbyte x = (sbyte)Object.transform.position.x, z = (sbyte)Object.transform.position.z; // ������������ �������� ���������
        if (Object.GetComponent<Piece>())
        {
            piece = Object.GetComponent<Piece>();
            if (x == 0) //���� �� � ������ � ���� � ������� ������ ��� �����
            {
                if (z < 7) // ���� ���������� ��������� �� ����� �� ������� �����
                {
                    if (Manager.Gases[z + 1])   // ���� ������������� ����� ����� �� ���
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z + 1), Quaternion.Euler(0, 180, 0)); // ������� �������������� �������������
                        Destroy(Manager.Gases[z + 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // ����� ������� ������� ������ ���� ��� �����
                        Planes.Add(plane); // ��������� ����� � ��������
                    }
                }
                if (z > 0) // ���� ���������� ��������� �� ����� �� ������� �����
                {
                    if (Manager.Gases[z - 1])    // ���� ������������� ����� ������ �� ���
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z - 1), Quaternion.Euler(0, 180, 0)); // ������� �������������� �������������
                        Destroy(Manager.Gases[z - 1]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // ����� ������� ������� ������ ���� ��� �����
                        Planes.Add(plane); // ��������� ����� � ��������
                    }
                }
            }
            if (x == 2 && !piece.FirstKill) // ���� �� � ���� ������� �� ����, ������ - ����� � � �� �� ����������� ������
            {
                if (z < 5 && Manager.All_Checker[1, z + 1] && (!Manager.All_Checker[0, z + 2])) // ���� ���������� � ����� ��������� ������� ������ �� ����� �������������
                {
                    AllThings.Add(new(1, 0, z + 1));
                    if (Manager.Gases[z + 3])  // ���� � ����� �������� ������ ������ ���� �������������
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z + 3), Quaternion.Euler(0, 180, 0)); // ������� �������������� �������������
                        Destroy(Manager.Gases[z + 3]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z + 3), Quaternion.Euler(0, 0, 0)); // ����� ������� �����
                        Planes.Add(plane); // ���������� ����� � ��������
                    }
                }
                if (z > 2 && Manager.All_Checker[1, z - 1] && (!Manager.All_Checker[0, z - 2])) // ���� ���������� � ����� ��������� ������� ������ �� ������ �������������
                {
                    AllThings.Add(new(1, 0, z - 1));
                    if (Manager.Gases[z - 3]) // ���� � ����� �������� ������� ������ ���� �������������
                    {
                        Gas2 = Instantiate(ActivGazonokosilka, new Vector3((float)-1.25, (float)-0.08, z - 3), Quaternion.Euler(0, 180, 0)); // ������� �������������� �������������
                        Destroy(Manager.Gases[z - 3]);
                    }
                    else
                    {
                        plane = Instantiate(Plane, new Vector3(-1, 0, z - 3), Quaternion.Euler(0, 0, 0)); // ����� ������� �����
                        Planes.Add(plane); // ���������� ����� � ��������
                    }
                }
            }
            if (!Blocked) // ���� ������� ��� �������� � ������ - �����
            {
                try
                {
                    if ((!Manager.All_Checker[x - 1, z + 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z + 1))) // ���� ����� ������ ������
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z + 1), Quaternion.Euler(0, 0, 0));  // ������� �����
                        Planes.Add(plane);  // ���������� ����� � �������
                    }
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    if ((!Manager.All_Checker[x - 1, z - 1]) && !piece.BannedCoords.Contains(new(x - 1, 0, z - 1))) // ���� ������ ������ ������
                    {
                        plane = Instantiate(Plane, new Vector3(x - 1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // ������� �����                        Planes.Add(plane);  // ���������� ����� � �������
                        Planes.Add(plane);  // ���������� ����� � �������
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // ���������� ���������
            {
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // ������������ �������� ����������
                try
                {
                    I = Manager.All_Checker[x, z]; 
                    if (I && !Manager.All_Checker[x + w, z + b] && (!Find(AllThings, x, z))) // ���� ����� ���� ����� � ��� �� � �������
                    {
                        if (piece && !piece.FirstKill)
                        {
                            try
                            {
                                if (!Manager.All_Checker[x + (w * 2), z + (b * 2)])
                                {
                                    FindForDestroyPlane((sbyte)(x + w * 2), (sbyte)(z + b * 2));
                                    plane = Instantiate(Plane, new Vector3(x + (w * 2), 0, z + (b * 2)), Quaternion.Euler(0f, 0f, 0f)); // ������� �����
                                    Planes.Add(plane); // ���������� ����� � �������
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                FindForDestroyPlane((sbyte)(x + w), (sbyte)(z + b));
                                plane = Instantiate(Plane, new Vector3(x + w, 0, z + b), Quaternion.Euler(0f, 0f, 0f)); // ������� �����
                                Planes.Add(plane); // ���������� ����� � �������
                            }
                        }
                        else if (I.type == 0)
                        {
                            FindForDestroyPlane((sbyte)(x + w), (sbyte)(z + b));
                            AllThings.Add(new(x, 0, z));
                            if (!piece)   // ���� ������ �������� ������, �� �������� ���� ������������� ��������
                                Continue = true;
                            plane = Instantiate(!Object.GetComponent<Piece>() ? GrayPlane : Plane, new Vector3(x + w, 0, z + b), Quaternion.Euler(0, 0, 0)); // ������� �����
                            Planes.Add(plane); // ���������� ����� � �������
                            PlaneSpawnJumper(plane, AllThings);   // �������� ������� ������ �������� (��, ��� �����)
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
    private void PlaneSpawnGiant(GameObject Object) { } // ���������
}