using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    private Vector3 speed = Vector3.zero;
    private float movementAngle = 90.0f;
    private float targetSpeed_z = 0f;
    private float targetSpeed_x = 0f;
    private float targetSpeed = 0f;
    private float targetMovementAngle = 90.0f;
    public float walkSpeed = 5f;
    public float runSpeed = 15f;
    public float walkAcceleration = 0.5f;
    public float runAcceleration = 20f;
    public float strafeAcceleration = 10f;
    public float verticalDecelleration = 1f;
    public float angularAcceleration = 600f;// degrees
    public float angularDeccelleration = 300f; // used when "relaxing" (i.e: not pressing anything)
    public float diagonal_strafeAngle = 45.0f;
    public float horizontal_strafeAngle = 90.0f;

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
        if ((!forwardPressed && (rightPressed || leftPressed)) || backwardPressed)
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

        // Find target vector components
        if (forwardPressed)
        {
            if (rightPressed)
            {
                targetSpeed_z = Mathf.Sin(diagonal_strafeAngle * Mathf.Deg2Rad) * targetSpeed;
                targetSpeed_x = Mathf.Cos(diagonal_strafeAngle * Mathf.Deg2Rad) * targetSpeed;
            }
            else if (leftPressed)
            {
                targetSpeed_z = Mathf.Sin((180 - diagonal_strafeAngle) * Mathf.Deg2Rad) * targetSpeed;
                targetSpeed_x = Mathf.Cos((180 - diagonal_strafeAngle) * Mathf.Deg2Rad) * targetSpeed;
            }
            else
            {
                targetSpeed_z = targetSpeed;
                targetSpeed_x = 0;
            }
        }
        else if (backwardPressed)
        {
            if (rightPressed)
            {
                // Backward + Right (diagonal)
                targetSpeed_z = -Mathf.Sin(diagonal_strafeAngle * Mathf.Deg2Rad) * targetSpeed;
                targetSpeed_x = Mathf.Cos(diagonal_strafeAngle * Mathf.Deg2Rad) * targetSpeed;
            }
            else if (leftPressed)
            {
                // Backward + Left (diagonal)
                targetSpeed_z = -Mathf.Sin((180 - diagonal_strafeAngle) * Mathf.Deg2Rad) * targetSpeed;
                targetSpeed_x = Mathf.Cos((180 - diagonal_strafeAngle) * Mathf.Deg2Rad) * targetSpeed;
            }
            else
            {
                // Backward only
                targetSpeed_z = -targetSpeed;
                targetSpeed_x = 0;
            }
        }
        else
        {
            if (rightPressed)
            {
                // Backward + Right (diagonal)
                targetSpeed_z = 0;
                targetSpeed_x = targetSpeed;
            }
            else if (leftPressed)
            {
                targetSpeed_z = 0;
                targetSpeed_x = -targetSpeed;
            }
            else
            {
                // No movement
                targetSpeed_z = 0;
                targetSpeed_x = 0;
            }
        }

        // Move left and right horizontally
        if (speed.x > targetSpeed_x)
        {
            float aceelerationPercentage = Math.Min(GetPercentageOfTargetZSpeed(speed.z) + 0.15f, 1f);
            float acceleration = sprintPressed ? strafeAcceleration * aceelerationPercentage : walkAcceleration;
            //print(acceleration);
            speed.x -= acceleration * Time.fixedDeltaTime;
            speed.x = Mathf.Max(speed.x, targetSpeed_x);
        }
        else if (speed.x < targetSpeed_x)
        {
            float aceelerationPercentage = Math.Min(GetPercentageOfMaxSpeed(new Vector2(speed.x, speed.z).magnitude) + 0.15f, 1f);
            float acceleration = sprintPressed ? strafeAcceleration * aceelerationPercentage : walkAcceleration;
            //print(acceleration);
            speed.x += acceleration * Time.fixedDeltaTime;
            speed.x = Mathf.Min(speed.x, targetSpeed_x);
        }


        // Slow down
        if (speed.z > targetSpeed_z)
        {
            speed.z -= verticalDecelleration * Time.fixedDeltaTime;
            speed.z = Mathf.Max(speed.z, targetSpeed_z);
        }
        // Speed up
        else if (speed.z < targetSpeed_z)
        {
            float acceleration = sprintPressed ? runAcceleration : walkAcceleration;
            speed.z += acceleration * Time.fixedDeltaTime;
            speed.z = Mathf.Min(speed.z, targetSpeed_z);
        }
    }

   void FixedUpdate()
    {
        //UpdateMovementAngle();
        Move();
        UpdateAnimation();
    }

    private Vector3 GetMovementUnitVector(float angle)
    {
        Quaternion yawRotation = Quaternion.Euler(0, -angle, 0);
        Vector3 rotatedVector = yawRotation * Vector3.right;
        return rotatedVector;
    }

    private void Move()
    {
        Vector3 finalTransform = transform.TransformDirection(speed);
        controller.SimpleMove(finalTransform);
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("VelocityZ", GetPercentageOfMaxSpeed(speed.z));
        animator.SetFloat("VelocityX", GetPercentageOfMaxSpeed(speed.x));
    }

    //private void UpdateSpeed()
    //{
    //    if (speed < targetSpeed)
    //    {
    //        speed += acceleration * Time.fixedDeltaTime;
    //        speed = Mathf.Min(speed, targetSpeed);
    //    }
    //    else if (speed > targetSpeed)
    //    {
    //        speed -= decceleration * Time.fixedDeltaTime;
    //        speed = Mathf.Max(speed, targetSpeed);
    //    }

    //}

    private void UpdateMovementAngle()
    {
        bool tryingToStop = targetSpeed == 0f;
        float acceleration = tryingToStop ? angularDeccelleration: angularAcceleration;
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

    private float GetPercentageOfMaxSpeed(float speed)
    {
        return speed / runSpeed;
    }

    private float GetPercentageOfTargetZSpeed(float speed)
    {
        return speed / targetSpeed_z;
    }
}
