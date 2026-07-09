using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPhysics : MonoBehaviour
{
    public Transform target;
    public Rigidbody rb;
    private float maxVelocity = 1.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.isKinematic = false;  
    }

        void FixedUpdate()
    {
        //Позиция
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;  

        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        } 

        //Вращение
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }

    private void Update() {
        float distance = Vector3.Distance(transform.position, target.position);
    }
    


}
