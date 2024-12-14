//Script: PlayerController.cs
//Contributors: Liam Francisco and Fenn Edmonds
//Summary: Called by InputMgr to convert inputs to player actions
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Windows;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public Entity playerEnt;
    public Animator playerAni;
    public AudioSource dashSound;


    public float dashSpeed;
    public float dashDistance;
    public float dashCooldown;
    public float dashCooldownTimer;
    public float attackCooldown;
    public float attackCooldownTimer;

    private Vector3 dashStartPosiiton;
    private Vector3 dashEndPosiiton;
    private bool dashing;

    // Start is called before the first frame update
    void Start()
    {
        playerEnt = GetComponent<Entity>();
        playerAni = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dashing)
            Dash();
        dashCooldownTimer += Time.deltaTime;
        attackCooldownTimer += Time.deltaTime;
    }

    public void MovePlayer(Vector2 input)
    {
        if (input.x >=.5)
            playerAni.SetBool("isrunning", true);

        if (input.x <= -.5)
            playerAni.SetBool("isbackwards", true);

        if (input.y >= .5)
            playerAni.SetBool("isrunning", true);

        if (input.y <= -.5)
            playerAni.SetBool("isrunning", true);

        if (input.y == 0 && input.x == 0)
        {
            playerAni.SetBool("isrunning", false);
            playerAni.SetBool("isbackwards", false);
        }

        // playerAni.SetBool("isrunning", true);
        if (dashing)
            return;

        Vector3 moveVector = new Vector3(input.x * Time.deltaTime, 0, input.y * Time.deltaTime) * playerEnt.speed;

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
        AudioMgr.Instance.PlaySFX("Dash", dashSound);
        if (dashCooldown < dashCooldownTimer)
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
            dashCooldownTimer = 0;
        }
    }

    public void StartAttack()
    {
        if(attackCooldown < attackCooldownTimer)
        {
            playerEnt.weapons[0].StartAttack();
            // playerAni.SetTrigger("attack");
            // print("here");
            attackCooldownTimer = 0;
        }
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

    public void Interact()
    {
        foreach(Interactable inter in PlayerMgr.inst.interactables)
        {
            if(inter.interactable)
                inter.Interact();
        }
    }
}
