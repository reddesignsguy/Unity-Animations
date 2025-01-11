using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum CharState
{
    Idle,
    Walking,
    Jumping,
    Falling
}

public class CharacterAiming : MonoBehaviour
{
    [SerializeField] private float m_turnSpeed = 2f;
    [SerializeField] private float inputAcceleration = 6f;
    [SerializeField] private float inputDeceleration = 5f;
    [SerializeField] private float m_maxSpeed = 5f;
    [SerializeField] private float m_stopUTurningThreshold = 2f;
    [SerializeField] private float m_maximumStepThreshold = 0.5f;
    [SerializeField] private Character character;
    [SerializeField] private InputHandler input;
    [SerializeField] private CinemachineCameraOffset camAngleOffset;
    [SerializeField] private CharacterController controller;
    private Animator _animator;

    private Vector3 m_defaultControllerToRootOffset;
    private float m_gravity;
    private float m_targetLookAngle = 0;

    private Coroutine m_walkDownStep;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        m_defaultControllerToRootOffset = _animator.rootPosition - (controller.transform.position);
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

    int i = 0;
    public float raycastLen = 5f;
    private CharState m_state = CharState.Walking;
    private bool m_characterOutOfSyncWithController = false;
    public GameObject root;

    [SerializeField] private float m_characterToControllerSyncThreshold = 0.01f;
    [SerializeField] private float m_characterSyncPercentRate = 0.1f;
    [SerializeField] private float m_stepUpMinimum = 0.2f; // Minimum increase in height to enable height LERPing for going up stairs
    [SerializeField] private float m_slowCharacterSyncPercentRate = 0.005f;

    //private Vector3 m_controllerToCharacterSyncTarget;


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
        Vector3 previousPos = transform.position;
        controller.SimpleMove(transform.forward * Mathf.Abs(m_gravity) * m_maxSpeed);
        if (m_characterOutOfSyncWithController)
        {
            transform.position = previousPos;
        }

        bool steppingUp = transform.position.y - previousPos.y > m_stepUpMinimum;
        if (steppingUp)
        {
            m_characterOutOfSyncWithController = true;
            transform.position = previousPos;
        }

        RaycastHit hit;
        Vector3 characterBottom = controller.transform.position + controller.center - new Vector3(0,controller.height/2f,0);
        Physics.Raycast(characterBottom, Vector3.down, out hit, raycastLen);

        bool foundStepBelow = hit.distance < m_maximumStepThreshold;
        Vector3 offset = hit.point - characterBottom;

        
        // Walking down stairs
        if (foundStepBelow && offset.magnitude > 0.1f && m_state != CharState.Falling && controller.velocity.y < 0)
        {
            Physics.SphereCast(controller.transform.position + controller.center, controller.radius, Vector3.down, out RaycastHit sphereHit, raycastLen);

            if (sphereHit.collider.gameObject == hit.collider.gameObject)
            {

                Vector3 previousPos2 = transform.position;
                controller.transform.position = hit.point;

                transform.position = previousPos2;

                m_characterOutOfSyncWithController = true;
            }
        }

        if (controller.velocity.magnitude < 0.01f)
        {
            m_state = CharState.Idle;
        }
        else if (controller.velocity.y < -1f)
        {
            m_state = CharState.Falling;
        }
        else
        {
            m_state = CharState.Walking;
        }

        if (m_characterOutOfSyncWithController)
        {
            Vector3 targetPos = controller.transform.position;
            float nextStep_y;

            // If idle, adjust for IK
            if (m_state == CharState.Idle)
            {
                targetPos.y -= m_ikHeightBetweenFeet;
                nextStep_y = Mathf.Lerp(transform.position.y, targetPos.y, m_slowCharacterSyncPercentRate * Time.deltaTime);
            }
            // No IK
            else
            {
                nextStep_y = Mathf.Lerp(transform.position.y, targetPos.y, m_characterSyncPercentRate * Time.deltaTime);
            }

            Vector3 nextStep = new Vector3(targetPos.x, nextStep_y, targetPos.z);
            transform.position = nextStep;

            bool doneSyncing = Mathf.Abs(targetPos.y - transform.position.y) < m_characterToControllerSyncThreshold;
            if (doneSyncing)
            {
                m_characterOutOfSyncWithController = false;
            }
        }
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


    public float m_iKRayOriginOffset = 1f;
    public float m_iKBodyOffset = 1;
    public LayerMask m_ikTargetLayer;
    public float m_iKFootDistanceToGround;
    private float m_ikHeightBetweenFeet = 0f;
    private void OnAnimatorIK(int layerIndex)
    {
        //Debug.Log("IK Animating");
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _animator.GetFloat("IKLeftFootWeight"));
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _animator.GetFloat("IKLeftFootWeight"));
        _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, _animator.GetFloat("IKRightFootWeight"));
        _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _animator.GetFloat("IKRightFootWeight"));

        RaycastHit leftHit;
        Ray ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * m_iKRayOriginOffset, Vector3.down * 2f);
        Debug.DrawRay(_animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * m_iKRayOriginOffset, Vector3.down * 2f, Color.red);
        Debug.DrawRay(_animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * m_iKRayOriginOffset, Vector3.down * 2f, Color.red);

        if (Physics.Raycast(ray, out leftHit, 2f, m_ikTargetLayer))
        {
            //Debug.Log("Found hit");
            Vector3 footPosition = leftHit.point;
            footPosition.y += m_iKFootDistanceToGround;
            
            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, leftHit.normal);
            Quaternion footRotation = Quaternion.LookRotation(forward, leftHit.normal);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, footRotation);
        }

        RaycastHit rightHit;
        ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * m_iKRayOriginOffset, Vector3.down * 2f);
        if (Physics.Raycast(ray, out rightHit, 2f, m_ikTargetLayer))
        {
            //Debug.Log("Found hit");
            Vector3 footPosition = rightHit.point;
            footPosition.y += m_iKFootDistanceToGround;

            _animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, rightHit.normal);
            Quaternion footRotation = Quaternion.LookRotation(forward, rightHit.normal);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, footRotation);
        }

        float heightBetweenFeet = Mathf.Abs(rightHit.point.y - leftHit.point.y);
        if (Mathf.Abs(heightBetweenFeet - m_ikHeightBetweenFeet) > 0.1f || heightBetweenFeet == 0f)
        {
            m_characterOutOfSyncWithController = true;
        }

        m_ikHeightBetweenFeet = heightBetweenFeet;
    }
}
