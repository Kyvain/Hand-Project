using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour
{
    private float _qx, _qy, _qz, _qw;
    private Rigidbody rb;
    private float speed = 0.2f;
    private float maxVelocity = 0.5f;
    private Quaternion initialRotation;
    
    public float torqueX = 100f;
    public float torqueY = 100f;
    public float torqueZ = 100f;
   

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		initialRotation = transform.rotation; // Сохраняем начальную ориентацию
	}

    void FixedUpdate()
    {
        // ESP дата
        float _qx = ESP32_com.qx;
        float _qy = ESP32_com.qy;
        float _qz = ESP32_com.qz;
        float _qw = ESP32_com.qw;

        transform.rotation = initialRotation * new Quaternion(_qy, -_qz, _qx, _qw);



        // Movement
        if (!Input.GetKey(KeyCode.Mouse1)) // For Camera Movement
        {
            float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            float vertical = Input.GetAxis("Vertical") * Time.deltaTime * speed;
            float updown = Input.GetAxis("updown") * Time.deltaTime * speed;
            transform.position += new Vector3(vertical, updown, horizontal);
        }

        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
    }
}