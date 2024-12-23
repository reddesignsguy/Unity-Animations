using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAiming : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private InputHandler input;
    [SerializeField] private CinemachineCameraOffset camAngleOffset;
    public float m_turnSpeed = 2;
    public float m_uTurnSpeed = 4;
    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private float targetAngle = 0;
    public float m_stopUTurningThreshold = 2f;
    void Update()
    {

        Vector2 direction = input.awsd;
        if (direction.magnitude != 0)
        {
            float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
            float rightAngleOfYaw = yawCamera + 90;
            targetAngle = -Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x) + rightAngleOfYaw;
        }

        float turnSpeed;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        if (character.IsUTurning() || Quaternion.Angle(targetRotation, transform.rotation) > 170)
        {
            character.state = CharacterState.UTurning;
            turnSpeed = m_uTurnSpeed;
        }
        else
        {
            turnSpeed = m_turnSpeed;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);

        if (character.IsUTurning() && Quaternion.Angle(targetRotation, transform.rotation) < m_stopUTurningThreshold)
        {
            character.state = CharacterState.Walking;
        }
    }
}
