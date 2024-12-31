using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class CharacterLocomotion : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private Character character;
    private CharacterController controller;
    private Animator _animator;
    private Vector2 gravity;

    public float inputAcceleration = 6f;
    public float inputDeceleration = 5f;
    
    //private float speed = 0f;
    public float m_maxSpeed = 5f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    //private void Update()
    //{
    //    Vector2 moveVector = input.awsd;

    //    if (moveVector.x == 0 || character.IsUTurning())
    //    {
    //        gravity.x = Mathf.MoveTowards(gravity.x, 0f, Time.deltaTime * inputDeceleration);
    //    }
    //    else
    //        gravity.x = Mathf.MoveTowards(gravity.x, moveVector.x, Time.deltaTime * inputAcceleration);
    //    gravity.x = Mathf.Clamp(gravity.x, -1, 1);

    //    if (moveVector.y == 0 || character.IsUTurning())
    //        gravity.y = Mathf.MoveTowards(gravity.y, 0f, Time.deltaTime * inputDeceleration);
    //    else
    //        gravity.y = Mathf.MoveTowards(gravity.y, moveVector.y, Time.deltaTime * inputAcceleration);

    //    gravity.y = Mathf.Clamp(gravity.y, -1, 1);
    //}

    //private Vector3 m_lastDirection;
    //private void FixedUpdate()
    //{
    //    if (character.IsUTurning())
    //    {
    //        controller.SimpleMove(m_lastDirection * Mathf.Abs(gravity.magnitude) * m_maxSpeed);
    //    }
    //    else
    //    {
    //        controller.SimpleMove(transform.forward * Mathf.Abs(gravity.magnitude) * m_maxSpeed);
    //        m_lastDirection = controller.velocity;
    //    }

    //    float speed = controller.velocity.magnitude;
    //    float animationSpeed = speed / m_maxSpeed;
    //    _animator.SetFloat("Y", animationSpeed);
    //}

}
