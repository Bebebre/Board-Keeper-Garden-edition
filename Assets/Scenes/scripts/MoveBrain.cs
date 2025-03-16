using System;
using System.Collections.Generic;
using UnityEngine; // ��� ��������� ����� ��� ���� ����� ����� ���� �� ��������� ���������

public class MoveBrain : MonoBehaviour
{
    [SerializeField]
    private GameObject Qween; 
    [SerializeField]
    private GameObject Plane; // ������
    [SerializeField]
    private GameObject GrayPlane; // ����� ������
    [SerializeField]
    private GameObject Light; // ������� ���������
    [SerializeField]
    private GameManager Manager; // ������ � ���� ����������

    private System.Random random = new();
    private List<Piece> Whites = new();
    public sbyte QPosoh = 3, QPosohCD = 0;
    private GameObject Clicked;   // ����������� ��� �������
    private List<GameObject> Planes = new(), Lights = new();          // �������� ������ � ���������
    private bool Continue, Lighting, End, Blocked, Select;  // ���� ������������ ����������� ������ ��������, ���� ����������� ������ ��������, ���� ����� ����, ���� ����������� ����� ����� � ���� �������� �� �����
    public bool WoB_Move, OnlyKill;                 // ��������� ���� ����
    private Piece Current, Currented;     // ����������� � �������� �����
    private Vector3 PlaneCoordinate;           // ���������� ����� ��� ���������� ����
    private RaycastHit hit;                 // ������������ ���� � ��������
    private Ray ray;                     // ���

    void Awake()           // ����������� �� ��������� ���� �� �� ������ �� ������
    {
        Continue = End = Blocked =  false;
        Select = Lighting = WoB_Move = true;
    }
    void Update()        // ����������� ������ ����  
    {
        if (WoB_Move) //������ ������� ������ � ���� ���
        {
// ************************************    �������� ��������� ����� ��� ��������    **********************************************************
            ray = Camera.main.ScreenPointToRay(Input.mousePosition); // ������ ����
            if (Physics.Raycast(ray, out hit)) //���� ��� ����� �� ��� - ��
            {            
                Piece shecker = hit.collider.gameObject.GetComponent<Piece>(); //������ ����� � ������� ������
                if (shecker && shecker.Stat && Select) //���� ������ � ��������� ����� � ����� ��� �� �������� �� ���� ����
                { 
                    if (hit.collider.gameObject.GetComponent<Piece>().type == 0 && Current != shecker) // �������� �� ����� (�������� ��������) � ������� ����� ��� �������� �� ������� ����� �����
                    {
                        if (Current) Non(); // ������� ����� ����� � ������� ��������� ���� ��� ����
                        Current = shecker; // ���������� ���� ����� ��� �����
                        shecker.Select(); // �������������
                    }
                }
                else Non();  // ����� ����� �����
            }
            else Non();  // ����� ����� �����
// *************************   �������� �������� ����� ���� � ������� ������ ��������   ********************************************
            if (Currented && Currented.Stat) // ���� ������������� ���� ����� ������������
            {
                if (Currented.transform.position.x == 7 && !Currented.isQween) QweenMaker(false);
                if (Blocked) // ���� ��� �������� ��� � ���������
                {
                    if (Currented.Kill) // ��������� ������� ������ �����
                    {
                        if(Currented.isQween ? CheckMoveQween(Currented.gameObject, true) : CheckMoveChecker(Currented.gameObject, true)) // ���� ���� ��������� ���� (�������� �����������)
                            Selected(Currented);              // ������� ������ ��� ������� �����
                        else 
                            End = true;    // ����� ����� ����
                        Blocked = false;  // ���������� �� ���������� �������������
                        Currented.Kill = false;  // ������������� � ������� ����� "������" � �������� �������
                    }
                    else End = true; // ����� ��������� ��������� ����
                }
                if (End) EndTurn(); // ����������� ��� ���� ���������
            }
//*****************************************************   �������� ������� ���   *************************************************************
            if (Input.GetKeyDown(KeyCode.Mouse0) && hit.collider)  // ���� �� ������ ��� �� ��� - ��
            {
                Clicked = hit.collider.gameObject;     // ������������ �������� �������� �������
                if (Clicked.CompareTag("Plane")) //������� �� �����
                {
                    PlaneCoordinate = Clicked.transform.position; // ����������� �������� ��������� ������
                    Currented.PointToMove = PlaneCoordinate; // ��������� �������� �����, ��������� �� ���������� ������
                    Currented.Stat = false;  // ���������� �� ������ ���������� ������� ��������� ��������� ����
                    Manager.All_Checker[(sbyte)Currented.transform.position.x, (sbyte)Currented.transform.position.z] = null; // ������� �� ���������� ���������� ����� �� �������
                    Manager.All_Checker[(sbyte)PlaneCoordinate.x, (sbyte)PlaneCoordinate.z] = Currented; // ���������� ������� ���������� ����� � �������
                    if (Continue) Blocked = true; else End = true; // ���������� - ����� �� �� ��������� ������ �������� � ���� ����
                    PlaneBust();        // ����������� ��� ������
                    Select = false;     // ��������� ����� ����� ��������
                }
                if (Select) // ���� ����� ����� ��������  
                {
                    if (Clicked.name == "QPosoh" && QPosohCD == 0 && QPosoh > 0) QweenMaker(true);
                    if (Currented) //���� ���� ��� ��������
                    {
                        if (Clicked && Currented.transform.position != Clicked.transform.position) // ���� �������� ����� �� ����� �������
                        {
                            if (Clicked.GetComponent<Piece>() && Clicked.GetComponent<Piece>().type == 0) // ���� �������� �� ����� � ��� �����
                            {
                                PlaneBust();                  // ����������� ��� ������
                                Selected(Clicked.GetComponent<Piece>());          // ������� ������ ��� ����� �����
                            }
                        }
                        else //���� ������ �� ���������� ����� 
                        {
                            PlaneBust();               // ����������� ��� ������
                            if (Lights.Count == 0) LightOn(); // ���� ��� ����������, �� ���������� �
                            Currented = null;     // �������� �������� �����
                        }
                    }
                    else if (Current) Selected(Current);//���� ������ �� �����
                }
                DebugMatrice();   // ��� ������������ ��� �������� �������
                Clicked = null;     // �������� �����������
            }
//*****************************************************************************************************************************************
        }
    }
    private void GenerateWhites()
    {
        Whites.Clear();
        for (int x = 0; x < 8; x++)         // ����� ����� � ������ ����� 
        {
            for (int z = 0; z < 8; z++)        // ������� ���� ��������� ������
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
            sbyte x = (sbyte)random.Next(0, Whites.Count), y = 0; // �������� ��������� �����
            while (Whites[x].GetComponent<Piece>().type != 0 && y < 40) // ���� 20 ��� �� ���������� �� ������� �����...
            {
                x = (sbyte)random.Next(0, Whites.Count); // �������� ��������� �����
                y++;
            }
            if (y > 39) // �� ���� ������ ��������� � �������
            {
                for (sbyte i = 0; i < Whites.Count; i++) // ���������� ��������
                {
                    if (Whites[i].GetComponent<Piece>().type == 0) // ���� ����� ������
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
            Selected(Currented);              // ������� ������ ��� ������� �����
        else
            End = true;    // ����� ����� ����
        Blocked = false;
    }
    public void RefreshItems()
    {
        QPosoh = 3;
        QPosohCD = 0;
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
    public void EndTurn()   // ����������� ��� ��������� ����
    {
        bool i = true;   // ���� ��� �������� �� ������
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // ������� �������
            {
                if (Manager.All_Checker[x, z] && Manager.All_Checker[x, z].type > 0 && !Manager.All_Checker[x, z].Confused) 
                { 
                    i = false;  // ��������� ���������� ������ ���� ���� ���� ���� ������ �����
                    break;
                }
            }
            if (!i) break; // �����������
        }
        if (i) Manager.Won(0); // �������� ������ ���� ����� ��� ������ �����
        PlaneBust();  // ��������� ��� ���������...
        End = false;
        LightOff();
        if (Currented) Currented.Deselect();
        Currented = null;
        Continue = Blocked = WoB_Move = false;
        Lighting = Select = true;  // � ��������� �������
        if (QPosohCD > 0) QPosohCD--;
    }
    private void Non() // ����������� ���� �� ������ ������ �� �����
    {
        if (Current)
            Current.Deselect(); //������������� ����� Current ���� ��� ����
        Current = null; // �������� Current
    }
    private void PlaneBust() // ����������� ��� ���������� ���� 
    {
        for (byte y = 0; y < Planes.Count; y++)
        {
            Destroy(Planes[y]); // ������� � ���������� ����
        }
        Planes.Clear(); // ������� ������� ����
    }
    private void Selected(Piece piece) // ����������� ��� ������ ����
    {
        Continue = false;
        if (piece.isQween && CheckMoveQween(piece.gameObject, OnlyKill)) // ���� ���� ��������� ���� �����(�������� �� �����������)
        {
            LightOff();  // ��������� ���������
            Currented = piece;   // ������ ����� ���������
            if (OnlyKill) Select = false;
            PlaneSpawnQween(Currented.gameObject, new(), 0, 0); // ��������� ����� ����
            if (OnlyKill) Select = true;
        }
        else if ((!piece.isQween) && CheckMoveChecker(piece.gameObject, OnlyKill)) // ���� ���� ��������� ���� �����(�������� �� �����������)
        {
            LightOff();  // ��������� ���������
            Currented = piece;   // ������ ����� ���������
            if (OnlyKill) Select = false;
            PlaneSpawnChecker(Currented.gameObject, new()); // ��������� ����� ����
            if (OnlyKill) Select = true;
        }
        else if (Lights.Count == 0) { LightOn(); Currented = null; } // ���� ��� ����� � ���������, �� ���������� ��������� � �������� �������� �����
    }
    public void LightOff() // ����������� ���������
    {
        for (byte i = 0;i < Lights.Count; i++) // ���������� ���������
        {
            Destroy(Lights[i]); // ���������� ���������
        }
        Lights.Clear(); // ������ ��������
    }
    public void LightOn() // ���������� ��������� � ������� �������� �� �����
    {
        Piece I;
        for (byte x = 0; x < 8; x++)
        {
            for (byte z = 0; z < 8; z++) // ������� �������
            {
                I = Manager.All_Checker[x, z];
                if (I && I.type == 0 && (I.isQween ? CheckMoveQween(I.gameObject, OnlyKill) : CheckMoveChecker(I.gameObject, OnlyKill)))
                { 
                    Lights.Add(Instantiate(Light, new Vector3(x - 0.2f, 0, z), Quaternion.Euler(0, 0, 0))); // ���� ���������� �� ����� �����, ������� ����� ������
                }
            }
        }
    }
    private bool Find(List<Vector3> A, int x, int z) // �������� �� ������� ��������� � �������
    {
        Vector3 V = new(x, 0, z); // �������� ��������� ��� ��������
        for (sbyte i = 0; i < A.Count; i++)
        {
            if (A[i] == V) return true; // ���������� �������� � ���������� "��" ���� ����� ����������
        }
        return false; // ���������� "���"
    }
    private bool FindForDestroyPlane(int x, int z) // �������� �� ������� ��������� � �������
    {
        Vector3 V = new(x, 0, z); // �������� ��������� ��� ��������
        bool flag = true;
        for (sbyte i = 0; i < Planes.Count; i++)
        {
            if (Planes[i].transform.position == V) 
            {
                if (Planes[i].CompareTag("Plane")) flag = false;
                Destroy(Planes[i]);
                Planes.Remove(Planes[i]); // ������� � ���������� �����  
                break;
            } 
        }
        return flag;
    }
    public bool CheckMoveChecker(GameObject Object, bool Kill) // �������� ���� ��� �����
    {
        Piece I;
        sbyte x, z; // ��������������� ��������
        for (sbyte w = -1; w < 2; w += 2) 
        {
            for (sbyte b = -1; b < 2; b += 2) // ������� ����������
            {
                x = (sbyte)((sbyte)Object.transform.position.x + w);
                z = (sbyte)((sbyte)Object.transform.position.z + b); // ������������ �������� ����������
                try
                {
                    I = Manager.All_Checker[x, z];
                    if (I && (!Manager.All_Checker[x + w, z + b]) && (I.type > 0) && I.UnTouched) return true; // ���� ����� ���� - �� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { }
                try
                {
                    I = Manager.All_Checker[x, z];
                    if ((!I) && w == 1 && !Kill) return true; // ���� �������� �� ����������� � ������� �����, �� ��� ��������
                }
                catch (IndexOutOfRangeException) { } // ��� ����� ��������� �� ������ ��������� ���� �� ������ �� ������� �������
            }
        }
        return false; // ��� �� ��������
    }
    public bool CheckMoveQween(GameObject Object, bool Kill)  // �������� ���� ��� �����
    {
        Vector3 This = Object.transform.position; // ���������� �������
        Piece I;
        bool Old_Kill = Kill;
        sbyte breaks, x, z; // ��������������� ��������
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // ������� ����������
            { 
                breaks = 0; // ��� ������� ��� ����������� ������ ������������
                Kill = Old_Kill;
                for (sbyte p = 1; p < 9; p++) // ������� ������ �� ���� ���������
                {
                    x = (sbyte)((sbyte)This.x + (p * w));
                    z = (sbyte)((sbyte)This.z + (p * b)); // ������������ �������� ����������
                    try
                    {
                        I = Manager.All_Checker[x, z];
                        if (!I && breaks == 1) breaks--; // ���� ������ ����� �� ������� �� 0
                        if (I && (I.type == 0)) breaks = 2; // ���� ���� ����� �����, �� ������� �� 2
                        if (I && I.type > 0 && I.UnTouched) { breaks++; Kill = false; } // ���� ���� ������ �����, �� ������� +1 � ��������� ���� ������������� ��������
                        if (breaks == 0 && !Kill) return true; // ���� ������� 0 � ���� ������������� �������� ��������, �� ��� ��������
                        if (breaks == 2) break; // ���������� �������� ������� ��������� ���� ������� 2 
                    }
                    catch (IndexOutOfRangeException) { break; } // ���������� �������� ������� ��������� ���� ����� �� ������� �����
                }
            }
        }
        return false; // ��� �� ��������
    }
    // ������ ���������:
    // ������� ����� -> ��������� ����� -> �������� ������ ���� ����������� ����� -> ��������� ����� ����� ������������ ��� -> �������� ������ ���� ����������� ����� �����... � ��� �� �����
    private void PlaneSpawnQween(GameObject Object, List<Vector3> AllThings, sbyte BlockVectorX, sbyte BlockVectorZ) // ������� ���� ��� �����
    {
        Vector3 This = Object.transform.position; // ���������� �������
        GameObject plane;  // ��������������� ���������
        Piece piece = Object.GetComponent<Piece>(), I;
        bool spawn = true; // ���� ����������� ����� ���� �� ���������
        bool killing; // ���� ���������� ����� �������� ���� � ���� � ���������
        sbyte x, z, breaks, statical = (sbyte)AllThings.Count; // ��������������� ��������
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // �������  
            {
                if (w == BlockVectorX && b == BlockVectorZ) continue; // ���� ��� ����������� ������, �� ���������� ���
                breaks = 0; // ������� �� ����
                killing = false; // ���� �������� �� ����
                for (sbyte p = 1; p < 9; p++) // ������� ������ ������ ���������
                {
                    x = (sbyte)((sbyte)This.x + (p * w));
                    z = (sbyte)((sbyte)This.z + (p * b)); // ����������� �������� ����������
                    try
                    {
                        I = Manager.All_Checker[x, z];
                        if (!I) // ���� ������ �����
                        {
                            if (breaks == 1) { killing = true; breaks = 0; } // ���� �� ����� ���� ������ �����, �� ������� ����
                            spawn = true; // ��������� �����
                        }
                        if (I && (I.type == 0)) // ���� ����� �����
                        { 
                            breaks = 2;  // ������� �� ���
                            spawn = false; // ��������� �����
                        }
                        if (I && I.type > 0 && I.UnTouched) // ���� ������ �����
                        { 
                            spawn = false; // ��������� �����
                            if (!Find(AllThings, x, z)) // ���� �� �� ������������ � ���� ������
                            {
                                breaks++; // ������� +1
                                killing = true; // �������� ���� �������
                                AllThings.Add(I.gameObject.transform.position); // ��������� � ��������
                            }
                            else if (breaks == 1) { breaks = 0; } // ����� ������� ������ ������
                        }
                        if (spawn) // ���� �������� ����� � � ���� ������� ������ �� ������������
                        {
                            if ((piece && Select) || killing) // ���� �������� ��� ������� ���
                            {
                                if (FindForDestroyPlane(x, z)) plane = Instantiate(piece ? Plane : GrayPlane, new Vector3(x, 0, z), Quaternion.Euler(0f, 0f, 0f)); // ������� �����
                                else plane = Instantiate(Plane, new Vector3(x, 0, z), Quaternion.Euler(0f, 0f, 0f));
                                Planes.Add(plane); // ��������� ����� � ��������
                                if (!piece) Continue = true; // ���� ������ - �����, �� �������� ���� ������ ��������
                                if (killing) PlaneSpawnQween(plane, AllThings, (sbyte)(w * -1), (sbyte)(b * -1)); // �������� ������� � ��������(��, ��� �����)
                            }
                        }
                        if (breaks == 2) { break;}// ���� ������� 2 - ���������� �������� ���������
                    }
                    catch (IndexOutOfRangeException) { break; } // ���������� �������� ��������� ���� ����� �� �������
                }
                while (statical < AllThings.Count) AllThings.RemoveAt(AllThings.Count - 1);
            }
        }
    }
    private void PlaneSpawnChecker(GameObject Object, List<Vector3> AllThings) // ������� ���� ��� �����
    {
        Vector3 This = Object.transform.position; // ���������� �������
        Piece piece = Object.GetComponent<Piece>(), I;
        GameObject plane; // ��������������� ���������
        sbyte x = (sbyte)This.x, z = (sbyte)This.z; // ��������������� ��������
        if (piece && Select) // ���� ������� ���
        {
            try
            {
                if (!Manager.All_Checker[x + 1, z + 1]) // ���� ������ �����
                {
                    plane = Instantiate(Plane, new Vector3(x + 1, 0, z + 1), Quaternion.Euler(0, 0, 0)); // ���������� �����
                    Planes.Add(plane); // ��������� ����� � ��������
                }
            }catch(IndexOutOfRangeException) { } // ��� ����� ��������� �� ������ ��������� ���� �� ������ �� ������� �������
            try
            {
                if (!Manager.All_Checker[x + 1, z - 1]) // ���� ������ �����
                {
                    plane = Instantiate(Plane, new Vector3(x + 1, 0, z - 1), Quaternion.Euler(0, 0, 0)); // ���������� �����
                    Planes.Add(plane); // ��������� ����� � ��������
                }
            }catch(IndexOutOfRangeException) { } // ��� ����� ��������� �� ������ ��������� ���� �� ������ �� ������� �������
        }
        for (sbyte w = -1; w < 2; w += 2)
        {
            for (sbyte b = -1; b < 2; b += 2) // ������� ���������� 
            {
                x = (sbyte)((sbyte)This.x + w);
                z = (sbyte)((sbyte)This.z + b); // ������������ �������� ����������
                try 
                {
                    I = Manager.All_Checker[x, z];
                    if (I && I.type > 0 && (!Manager.All_Checker[x + w, z + b]) && (!Find(AllThings, x, z)) && I.UnTouched) // ���� �� ������ ������ �����, �� ��� ����� � ��� �� � �������
                    {
                        AllThings.Add(new(x, 0, z)); // ������� ������ � ��������
                        if (!piece) Continue = true; // ���� ������ - �����, �� �������� ���� ������ 
                        if (FindForDestroyPlane(x + w, z + b)) plane = Instantiate(piece ? Plane : GrayPlane, new Vector3(x + w, 0, z + b ), Quaternion.Euler(0, 0, 0));
                        else plane = Instantiate(Plane, new Vector3(x + w, 0, z + b), Quaternion.Euler(0, 0, 0)); // ������� �����
                        Planes.Add(plane); // ������� ����� � ��������
                        if (x + w < 7) PlaneSpawnChecker(plane, AllThings); // �������� ������� � ��������
                        AllThings.RemoveAt(AllThings.Count - 1); // ������� ��� ��������� �������� ����� ����� ����������
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
        }
    }
}