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
        // Rotating
        Vector2 direction = input.awsd;
        if (direction.magnitude != 0)
        {
            float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
            float rightAngleOfYaw = yawCamera + 90;
            targetAngle = -Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x) + rightAngleOfYaw;
        }

        float turnSpeed;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        if (!character.IsUTurning() && CheckUTurning(targetRotation))
        {
            character.state = CharacterState.UTurning;
            _animator.SetTrigger("180Turn");
            _animator.applyRootMotion = true;
            Debug.Log("Changed to turn");
        }

        if (character.IsUTurning())
        {
            turnSpeed = m_uTurnSpeed;
        }
        else
        {
            turnSpeed = m_turnSpeed;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);

        Debug.Log(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
        if (character.IsUTurning() && _animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion") &&
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {

            Debug.Log("Changed to Walk");
            _animator.applyRootMotion = false;
            gravity = _animator.velocity;
            character.state = CharacterState.Walking;
        }

        // Moving
        Vector2 moveVector = input.awsd;

        if (moveVector.x == 0 || character.IsUTurning())
        {
            gravity.x = Mathf.MoveTowards(gravity.x, 0f, Time.deltaTime * inputDeceleration);
        }
        else
            gravity.x = Mathf.MoveTowards(gravity.x, moveVector.x, Time.deltaTime * inputAcceleration);
        gravity.x = Mathf.Clamp(gravity.x, -1, 1);

        if (moveVector.y == 0 || character.IsUTurning())
            gravity.y = Mathf.MoveTowards(gravity.y, 0f, Time.deltaTime * inputDeceleration);
        else
            gravity.y = Mathf.MoveTowards(gravity.y, moveVector.y, Time.deltaTime * inputAcceleration);

        gravity.y = Mathf.Clamp(gravity.y, -1, 1);
    }

    private void FixedUpdate()
    {
        if (character.IsUTurning())
        {
            Debug.Log("U turning!");
        }
        else
        {
            controller.SimpleMove(transform.forward * Mathf.Abs(gravity.magnitude) * m_maxSpeed);

            float speed = controller.velocity.magnitude;
            float animationSpeed = speed / m_maxSpeed;
            _animator.SetFloat("Y", animationSpeed);
        }
    }

    public float m_speedThresholdForTurning = 0.4f;
    private bool CheckUTurning(Quaternion targetRotation)
    {
        Debug.Log("Difference in angle: " + Quaternion.Angle(targetRotation, transform.rotation));
        Debug.Log("Speed: " + controller.velocity.magnitude + "... when requirement is : " + m_speedThresholdForTurning * m_maxSpeed);
        Debug.Log(controller.velocity.magnitude > m_speedThresholdForTurning * m_maxSpeed);
        Debug.Log(Quaternion.Angle(targetRotation, transform.rotation) > 170);
        if (Quaternion.Angle(targetRotation, transform.rotation) > 170 && controller.velocity.magnitude > m_speedThresholdForTurning * m_maxSpeed)
        {
            Debug.Log("IS U TURNING");
        }
        Debug.Log("-------------");
        return Quaternion.Angle(targetRotation, transform.rotation) > 170 && controller.velocity.magnitude > m_speedThresholdForTurning * m_maxSpeed;
    }
}
