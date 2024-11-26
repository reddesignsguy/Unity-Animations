using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    private float speed = 0f;
    public float walkSpeed = 5f;
    public float runSpeed = 15f;
    public float acceleration = 0.5f;
    public float decceleration = 1f;

    private State state = State.Idle;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private enum State
    {
        Idle,
        Walking,
        Running
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                state = State.Running;
            }
            else
            {
                state = State.Walking;
            }
        }
        else
        {
            state = State.Idle;
        }
    }

    void FixedUpdate()
    {
        switch(state)
        {
            case State.Walking:
                Walking();
                break;
            case State.Running:
                Running();
                break;
            case State.Idle:
                Idle();
                break;
        }
    }

    private void Idle()
    {
        // Decellerate
        speed -= decceleration * Time.deltaTime;
        speed = Mathf.Max(speed, 0f);

        Move();
    }

    private void Running()
    {
        // Aceelerate
        speed += acceleration * Time.deltaTime;
        speed = Mathf.Min(speed, runSpeed);

        Move();
    }

    private void Walking()
    {
        // Accelerate
        if (speed < walkSpeed)
        {
            speed += acceleration * Time.deltaTime;
            speed = Mathf.Min(speed, walkSpeed);
        }

        // Decellerate
        else
        {
            speed -= decceleration * Time.deltaTime;
            speed = Mathf.Max(speed, walkSpeed);
        }

        Move();
    }

    private void Move()
    {
        Vector3 movementVector = Vector3.forward * speed;
        controller.SimpleMove(movementVector);
        animator.SetFloat("Speed", GetPercentageOfMaxSpeed());
    }

    private float GetPercentageOfMaxSpeed()
    {
        return speed / runSpeed;
    }
}
