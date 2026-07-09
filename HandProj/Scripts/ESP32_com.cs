using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO.Ports;


public class ESP32_com : MonoBehaviour
{
    SerialPort stream = new SerialPort("COM3", 9600);
    
    public string strReceived; 
    private string[] strData = new string[9];
    public string[] strData_received = new string[9];
    public static float qw, qx, qy, qz, serce, yuzuk, orta, isaret, bas;
    //public Transform hand, index1, index2, index3, middle1, middle2, middle3, ring1, ring2, ring3, pinky0, pinky1, pinky2, pinky3, thumb1, thumb2, thumb3;
    
    void Start()
    {
        stream.Open();  
        stream.ReadTimeout = 5000;  
    }

   void Update()
{
    if (stream.IsOpen)
    {
        try
        {
            if (stream.BytesToRead > 0)
            {
                strReceived = stream.ReadLine();
                strData = strReceived.Split(',');

                if (strData.Length == 9)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        strData_received[i] = strData[i].Trim();

                        if (!float.TryParse(strData_received[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                        {
                            throw new FormatException($"Некорректное значение в позиции {i}: {strData_received[i]}");
                        }
                    }

                    qw = float.Parse(strData_received[0], CultureInfo.InvariantCulture);
                    qx = float.Parse(strData_received[1], CultureInfo.InvariantCulture);
                    qy = float.Parse(strData_received[2], CultureInfo.InvariantCulture);
                    qz = float.Parse(strData_received[3], CultureInfo.InvariantCulture);

                    serce = float.Parse(strData_received[4], CultureInfo.InvariantCulture);
                    yuzuk = float.Parse(strData_received[5], CultureInfo.InvariantCulture);
                    orta = float.Parse(strData_received[6], CultureInfo.InvariantCulture);
                    isaret = float.Parse(strData_received[7], CultureInfo.InvariantCulture);
                    bas = float.Parse(strData_received[8], CultureInfo.InvariantCulture);
                }
                else
                {
                    Debug.LogWarning("Получено некорректное количество данных. Ожидалось 9 значений.");
                }
            }
        }
        catch (FormatException e)
        {
            Debug.LogError("Ошибка формата данных: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Неизвестная ошибка: " + e.Message);
        }
    }
}
}
