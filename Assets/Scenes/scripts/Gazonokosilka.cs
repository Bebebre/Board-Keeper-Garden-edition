using System.Collections;
using System.Collections.Generic;
using UnityEngine;   // ��� ��������� ����� ��� ���� ����� ����� ���� �� ��������� ���������

public class Gazonokosilka : MonoBehaviour
{
    public bool Gas; // ���� ��� �������� 
    void Update()          // ����������� ������ ����
    {
        if (Gas) // ���� �������� ��������
        {
            transform.Translate(Vector3.right * Time.deltaTime * 6.5f, Space.World); // ���������
            if(transform.position.x > 9) Destroy(gameObject); // ������������ ��� �����������
        }
    }
}
