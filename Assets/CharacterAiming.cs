using UnityEngine;

public class CharacterAiming : MonoBehaviour
{
    [SerializeField] private float m_turnSpeed = 2f;
    [SerializeField] private float inputAcceleration = 6f;
    [SerializeField] private float inputDeceleration = 5f;
    [SerializeField] private float m_maxSpeed = 5f;
    [SerializeField] private float m_stopUTurningThreshold = 2f;
    [SerializeField] private Character character;
    [SerializeField] private InputHandler input;
    [SerializeField] private CinemachineCameraOffset camAngleOffset;
    private CharacterController controller;
    private Animator _animator;

    private float m_gravity;
    private float m_targetLookAngle = 0;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        RotateCharacter();
        MoveCharacter();
        AnimateCharacter();
    }

    private void MoveCharacter()
    {
        Vector2 moveVector = input.awsd;

        if (moveVector.magnitude == 0)
        {
            m_gravity = Mathf.MoveTowards(m_gravity, 0f, Time.deltaTime * inputDeceleration);
        }
        else
        {
            m_gravity = Mathf.MoveTowards(m_gravity, 1, Time.deltaTime * inputAcceleration);
        }

        // Apply movement
        controller.SimpleMove(transform.forward * Mathf.Abs(m_gravity) * m_maxSpeed);
    }

    private void AnimateCharacter()
    {
        float speed = controller.velocity.magnitude;
        float animationSpeed = speed / m_maxSpeed;
        _animator.SetFloat("Y", animationSpeed);
    }

    private void RotateCharacter()
    {
        Vector2 movementVector = input.awsd;
        bool tryingToMove = movementVector.magnitude != 0;
        if (tryingToMove)
        {
            float cameraYawAngle = Camera.main.transform.rotation.eulerAngles.y;
            float rightAngleOfYaw = cameraYawAngle + 90;
            m_targetLookAngle = -Mathf.Rad2Deg * Mathf.Atan2(movementVector.y, movementVector.x) + rightAngleOfYaw;
        }
        Quaternion targetRotation = Quaternion.Euler(0, m_targetLookAngle, 0);

        // Apply rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_turnSpeed * Time.fixedDeltaTime);
    }
}
