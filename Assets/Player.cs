using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    private float speed = 0f;
    private float movementAngle = 90.0f;

    private float targetSpeed = 0f;
    private float targetMovementAngle = 90.0f;
    public float walkSpeed = 5f;
    public float runSpeed = 15f;
    public float acceleration = 0.5f;
    public float decceleration = 1f;
    public float angularAcceleration = 600f;// degrees
    public float angularDeccelleration = 300f; // used when "relaxing" (i.e: not pressing anything)
    public float diagonal_strafeAngle = 45.0f;
    public float horizontal_strafeAngle = 90.0f;

    //private RunState runState = RunState.Idle;
    //private HorizontalState strafeState = HorizontalState.None;
    //private VerticalState verticalState = VerticalState.None;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        // Check for directional inputs
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool backwardPressed = Input.GetKey(KeyCode.S);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool sprintPressed = Input.GetKey(KeyCode.LeftShift);

        // Get target speed
        if (!forwardPressed && (rightPressed || leftPressed))
        {
            targetSpeed = walkSpeed;
        }
        else if (forwardPressed)
        {
            if (sprintPressed)
            {
                targetSpeed = runSpeed;
            }
            else
            {
                targetSpeed = walkSpeed;
            }
        }
        else
        {
            targetSpeed = 0f;
        }

        if (forwardPressed)
        {
            if (leftPressed)
            {
                targetMovementAngle = 135f;
            }
            else if (rightPressed)
            {
                targetMovementAngle = 45f;
            }
            else
            {
                targetMovementAngle = 90f;
            }
        }
        else if (backwardPressed)
        {
            //if (leftPressed)
            //{
            //    targetMovementAngle = 225f;
            //}
            //else if (rightPressed)
            //{
            //    targetMovementAngle = 315;
            //}
            //else
            //{
            //    targetMovementAngle = 270f;
            //}
        }
        else
        {
            if (leftPressed)
            {
                targetMovementAngle = 180f;
            }
            else if (rightPressed)
            {
                targetMovementAngle = 0f;
            }
            else
            {
                // Nothing pressed, do nothing
            }
        }
    }

   void FixedUpdate()
    {
        UpdateSpeed();
        UpdateMovementAngle();
        Move();
        UpdateAnimation();
    }

    private Vector3 GetMovementUnitVector(float angle)
    {
        Quaternion yawRotation = Quaternion.Euler(0, -angle, 0);
        Vector3 rotatedVector = yawRotation * Vector3.right;
        //print(rotatedVector);
        return rotatedVector;
    }

    private void Move()
    {
        bool tryingToStop = targetSpeed == 0f;
        float finalSpeed = tryingToStop ? (targetSpeed + speed) / 1.5f : speed;
        Vector3 movementVector = GetMovementUnitVector(targetMovementAngle) * finalSpeed;
        controller.SimpleMove(movementVector);
    }

    private void UpdateAnimation()
    {
        float velocityX = Mathf.Cos(movementAngle * Mathf.Deg2Rad);
        float velocityZ = Mathf.Sin(movementAngle * Mathf.Deg2Rad);

        animator.SetFloat("VelocityZ", GetPercentageOfMaxSpeed() * velocityZ);
        animator.SetFloat("VelocityX", GetPercentageOfMaxSpeed() * velocityX);
    }

    private void UpdateSpeed()
    {
        if (speed < targetSpeed)
        {
            speed += acceleration * Time.fixedDeltaTime;
            speed = Mathf.Min(speed, targetSpeed);
        }
        else if (speed > targetSpeed)
        {
            speed -= decceleration * Time.fixedDeltaTime;
            speed = Mathf.Max(speed, targetSpeed);
        }

    }

    private void UpdateMovementAngle()
    {
        bool tryingToStop = targetSpeed == 0f;
        float acceleration = tryingToStop ? angularDeccelleration: angularAcceleration;

        print(acceleration);
        if (movementAngle < targetMovementAngle)
        {
            movementAngle += acceleration * Time.fixedDeltaTime;
            movementAngle = Mathf.Min(movementAngle, targetMovementAngle);
        }
        else if (movementAngle > targetMovementAngle)
        {
            movementAngle -= acceleration * Time.fixedDeltaTime;
            movementAngle = Mathf.Max(movementAngle, targetMovementAngle);
        }

    }

    private float GetPercentageOfMaxSpeed()
    {
        return speed / runSpeed;
    }
}
