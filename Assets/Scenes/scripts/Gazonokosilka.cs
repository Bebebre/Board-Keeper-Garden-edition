using System.Collections;
using System.Collections.Generic;
using UnityEngine;   // Это всяческие штуки для того чтобы юнити смог всё правильно прочитать

public class Gazonokosilka : MonoBehaviour
{
    public bool Gas; // Флаг для движения 
    void Update()          // Срабатывает КАЖДЫЙ кадр
    {
        if (Gas) // Если движение запущено
        {
            transform.Translate(Vector3.right * Time.deltaTime * 6.5f, Space.World); // Двигатель
            if(transform.position.x > 9) Destroy(gameObject); // Уничтожитель для оптимизации
        }
    }
}
