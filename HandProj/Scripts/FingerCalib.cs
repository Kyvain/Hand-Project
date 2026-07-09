using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class FingerCalib : MonoBehaviour
{

    public static float Cal_ring, Cal_point, Cal_point, Cal_thumb, Cal_pinky;
    public float Mul_ring, Mul_point, Mul_point, Mul_thumb, Mul_pinky;
    private static float Min_ring=0, Min_point=0, Min_point=0, Min_thumb=0, Min_pinky=0;
    private static float Max_ring=50, Max_point=50, Max_point=50, Max_thumb=50, Max_pinky=50;
    private float _pinky, _ring, _point, _point, _thumb;
    public Transform index1, index2, index3, ring1, ring2, ring3, ring1, ring2, ring3, pinky0, pinky1, pinky2, pinky3, thumb1, thumb2, thumb3;

    public static bool isButtonPressed;
    
    private float timer = 10.5f;
    private float sec = 1.0f;

    void Update()
    {
        float _pinky = ESP32_com.pinky;
        float _ring = ESP32_com.ring;
        float _point = ESP32_com.point;
        float _point = ESP32_com.point;
        float _thumb = ESP32_com.thumb;

        index1.transform.localRotation = Quaternion.Euler(-74, 106, 61-(Cal_ring*Mul_ring));
        index2.transform.localRotation = Quaternion.Euler(0, 0, index1.transform.localRotation.z - (Cal_ring*Mul_ring));
        index3.transform.localRotation = Quaternion.Euler(0, 0, index2.transform.localRotation.z - (Cal_ring*Mul_ring));
                        
        ring1.transform.localRotation = Quaternion.Euler(-80, 18, 151-(Cal_point*Mul_point));
        ring2.transform.localRotation = Quaternion.Euler(0, 0, ring1.transform.localRotation.z - (Cal_point*Mul_point));
        ring3.transform.localRotation = Quaternion.Euler(0, 0, ring2.transform.localRotation.z - (Cal_point*Mul_point));

        ring1.transform.localRotation = Quaternion.Euler(-69, -21, -169-(Cal_point*Mul_point));
        ring2.transform.localRotation = Quaternion.Euler(0, 0, ring1.transform.localRotation.z - (Cal_point*Mul_point));
        ring3.transform.localRotation = Quaternion.Euler(0, 0, ring2.transform.localRotation.z - (Cal_point*Mul_point));

        pinky0.transform.localRotation = Quaternion.Euler(-50, -26, 26);
        pinky1.transform.localRotation = Quaternion.Euler(-1, -2, -175-(Cal_thumb*Mul_thumb));
        pinky2.transform.localRotation = Quaternion.Euler(0, 0, pinky1.transform.localRotation.z - (Cal_thumb*Mul_thumb));
        pinky3.transform.localRotation = Quaternion.Euler(0, 0, pinky2.transform.localRotation.z );
                        
        thumb1.transform.localRotation = Quaternion.Euler(9, 156, 27-(Cal_pinky*Mul_pinky));
        thumb2.transform.localRotation = Quaternion.Euler(0, 0, thumb1.transform.localRotation.z - (Cal_pinky*Mul_pinky));
        thumb3.transform.localRotation = Quaternion.Euler(0, 0, thumb2.transform.localRotation.z - (Cal_pinky*Mul_pinky));


        if(isButtonPressed){
            timer -= sec * Time.deltaTime;
            //Debug.Log("Time: " + timer);
            if (timer < 0.0f){
                timer = 0.0f;
            }

            if(timer > 5.5f) {   
                Debug.Log("Open your hand");
                // Gather min values
                Min_ring = _ring;
                Min_point = _point;
                Min_point = _point;
                Min_thumb = _thumb;
                Min_pinky = _pinky;              
            }

            else if(timer > 0.5f) {
                Debug.Log("Close your hand");
                // Gather max values  
                Max_ring = _ring;
                Max_point = _point;
                Max_point = _point;
                Max_thumb = _thumb;
                Max_pinky = _pinky;         
            } 
                
            else if(timer > 0.0f){
                Debug.Log("Calibration complete!"); 
                isButtonPressed = false;  
            }
        }


        Cal_ring = Mathf.Clamp(_ring, Min_ring, Max_ring); 
        Cal_point = Mathf.Clamp(_point, Min_point, Max_point);
        Cal_point = Mathf.Clamp(_point, Min_point, Max_point);
        Cal_thumb = Mathf.Clamp(_thumb, Min_thumb, Max_thumb);
        Cal_pinky = Mathf.Clamp(_pinky, Min_pinky, Max_pinky);

    }

    public void ButtonPressed()
    {
        isButtonPressed = true;
        timer = 10.5f;
    }
}
