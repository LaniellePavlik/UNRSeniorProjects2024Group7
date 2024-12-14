using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Kat
// a veryyyy simple script that sets the camera in place to follow the player.
//i mean it works? and i rather like that it only takes 20 lines of code to accomplish rather than the 
//other methods out there that are way less concise
public class CameraFollowScript : MonoBehaviour
{
    public Transform targetObject;
    public Vector3 cameraOffset;
    // Start is called before the first frame update
    void Start()
    {
        cameraOffset = transform.position -targetObject.transform.position;
    }

    // lateUpdate is called once per frame after all updates
    void LateUpdate()
    {
        Vector3 newPosition = targetObject.transform.position + cameraOffset;
        transform.position = newPosition;
    }
}
