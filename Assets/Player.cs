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
    public float strafeRunAngle = 45.0f;
    public float strafeAngle = 90.0f;

    private RunState runState = RunState.Idle;
    private StrafeState strafeState = StrafeState.None;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private enum RunState
    {
        Idle,
        Walking,
        Running
    }

    private enum StrafeState
    {
        None,
        Right,
        Left
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                runState = RunState.Running;
            }
            else
            {
                runState = RunState.Walking;
            }
        }
        else
        {
            runState = RunState.Idle;
        }
    }

    void FixedUpdate()
    {
        switch(runState)
        {
            case RunState.Walking:
                Walking();
                break;
            case RunState.Running:
                Running();
                break;
            case RunState.Idle:
                Idle();
                break;
        }
    }

    private void Idle()
    {
        // Decellerate
        speed -= decceleration * Time.deltaTime;
        speed = Mathf.Max(speed, 0f);

        Move(Vector3.forward);
    }

    private void Running()
    {
        // Aceelerate
        speed += acceleration * Time.deltaTime;
        speed = Mathf.Min(speed, runSpeed);

        Move(Vector3.forward);
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

        if (strafeState == StrafeState.Left)
        {

        }

        Move(Vector3.forward);
    }

    private void Move(Vector3 direction)
    {
        Vector3 movementVector = direction * speed;
        controller.SimpleMove(movementVector);
        animator.SetFloat("VelocityZ", GetPercentageOfMaxSpeed());
    }

    private float GetPercentageOfMaxSpeed()
    {
        return speed / runSpeed;
    }
}
