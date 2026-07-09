using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public GameObject Hand;

    float Speed = 0.3f;

    void Update()
    {
  
        if (Input.GetKey(KeyCode.A))
        {

            Hand.transform.Translate(Vector3.left * Speed * Time.deltaTime);         
        }
        if (Input.GetKey(KeyCode.D))
        {

            Hand.transform.Translate(Vector3.right * Speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {

            Hand.transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {

            Hand.transform.Translate(Vector3.back * Speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Space))
        {

            Hand.transform.Translate(Vector3.up * Speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {

            Hand.transform.Translate(Vector3.down * Speed * Time.deltaTime);
        }
    }
}
