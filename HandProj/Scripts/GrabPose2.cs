using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabPose2 : MonoBehaviour
{
    private float pose_thumb, pose_point, pose_middle, pose_ring, pose_pinky;
    public GameObject GrabZone2;
    public XRSocketInteractor socket;
    [SerializeField] private Rigidbody rb;
    [Space]
    public float pose_min = 30f;
    public float pose_max = 50f;
    [Space]
    public float pose_pinky_min = 8f;
    public float pose_pinky_max = 8f;


    void Start()
    {
        socket = GrabZone2.GetComponent<XRSocketInteractor>();
        rb = GetComponent<Rigidbody>();
        socket.socketActive = false;
        GrabZone2.GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        float pose_ring = FingerCalib.Cal_ring;
        float pose_middle = FingerCalib.Cal_middle;
        float pose_point = FingerCalib.Cal_point;
        float pose_thumb = FingerCalib.Cal_thumb;
        float pose_pinky = FingerCalib.Cal_pinky;

        if(pose_ring >= pose_min && pose_ring <= pose_max && pose_pinky >= pose_pinky_min && pose_pinky <= pose_pinky_max && pose_middle <= pose_max 
        && pose_middle >= pose_min && pose_point <= pose_max && pose_point >= pose_min && pose_thumb <= pose_max && pose_thumb >= pose_min){
            
            socket.socketActive = true;
            GrabZone2.GetComponent<Renderer>().enabled = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        else{
            socket.socketActive = false;
            GrabZone2.GetComponent<Renderer>().enabled = false;
            rb.constraints = RigidbodyConstraints.None;
        }

    }
}
