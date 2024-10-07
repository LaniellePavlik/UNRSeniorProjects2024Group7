using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    public float dashSpeed;
    public float dashDistance;

    private Vector3 dashStartPosiiton;
    private Vector3 dashEndPosiiton;
    private bool dashing;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dashing)
            Dash();
    }

    public void MovePlayer(Vector2 input)
    {
        if (dashing)
            return;

        Vector3 moveVector = new Vector3(input.x * Time.deltaTime, 0, input.y * Time.deltaTime) * moveSpeed;

        transform.position += moveVector;
    }

    public void ChangeDirection(Vector2 mousePos)
    {
        if (dashing)
            return;

        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out hit, float.MaxValue);
        Vector3 lookAtVector = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        transform.LookAt(lookAtVector, Vector3.up);
    }


    public void StartDash(Vector2 mousePos, Vector2 moveInput)
    {
        Vector3 dashDirection;
        if (moveInput != Vector2.zero)
        {
            Vector3 moveVector = Vector3.Normalize(new Vector3(moveInput.x, 0, moveInput.y));
            dashDirection = moveVector;
        }
        else
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out hit, float.MaxValue);
            Vector3 mouseWorld = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            dashDirection = Vector3.Normalize(mouseWorld - transform.position);
        }

        dashStartPosiiton = transform.position;
        dashEndPosiiton = transform.position + dashDirection * dashDistance;
        dashing = true;
    }

    float dashTimer = 0;
    public void Dash()
    {
        dashTimer += Time.deltaTime;
        if (dashTimer < dashSpeed) 
        { 
            Vector3 newPos = Vector3.Lerp(dashStartPosiiton, dashEndPosiiton, dashTimer/dashSpeed);
            transform.position = newPos;
        }
        else
        {
            dashTimer = 0;
            dashing = false;
        }

    }
}
