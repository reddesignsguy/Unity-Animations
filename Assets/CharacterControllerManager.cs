using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CharacterControllerManager : MonoBehaviour
{
    private CharacterController _controller;
    public event Action<ControllerColliderHit> OnCharacterCollision;
    private Vector3 previousPosition;
    public Vector3 previousDisplacement;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        previousPosition = transform.position;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Call the method in the external script
        if (OnCharacterCollision != null)
        {
            OnCharacterCollision.Invoke(hit);
        }
    }

    private void Update()
    {
        previousDisplacement = transform.position - previousPosition;
        previousPosition = transform.position;
    }
}
