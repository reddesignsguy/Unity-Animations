using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum CharState
{
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

        if (controller.velocity.y < -1f)
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
            //Debug.Log("Interpolating");
            float nextStep_y = Mathf.Lerp(transform.position.y, targetPos.y, m_characterSyncPercentRate * Time.deltaTime);
            Vector3 nextStep = new Vector3(targetPos.x, nextStep_y, targetPos.z);
            transform.position = nextStep;

            
            if (Mathf.Abs(targetPos.y - transform.position.y) < m_characterToControllerSyncThreshold)
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

    private void HandleControllerCollision(ControllerColliderHit hit)
    {
        Debug.Log(hit.gameObject);
        //throw new System.NotImplementedException();
    }

    private IEnumerator InterpolatePosition(Transform transformToMove, Vector3 start, Vector3 target, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor (0 to 1)
            float t = elapsedTime / duration;

            // Interpolate the position using Lerp
            transformToMove.position = Vector3.Lerp(start, target, t);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            // Wait until the next frame
            Debug.Log("Position: " + transformToMove.position);
            yield return null;
        }

        // Ensure the final position is set
        transformToMove.position = target;
        Debug.Log(transformToMove.position + " vs: " + controller.transform.position);
        Debug.Log("-----------------");
    }

    // Function to get the capsule properties
    private void GetControllerCapsuleProperties(out Vector3 top, out Vector3 bottom, out float radius, out Vector3 center, out float height)
    {

        // Get the center and height of the CharacterController
        center = controller.bounds.center;
        height = controller.height;
        radius = controller.radius;

        // The capsule is aligned along the Y-axis, so calculate the top and bottom points of the capsule
        top = center + Vector3.up * (height / 2f);
        bottom = center - Vector3.up * (height / 2f);
    }

    public float m_iKRayOriginOffset = 1f;
    public float m_iKBodyOffset = 1;
    public LayerMask m_ikTargetLayer;
    public float m_iKFootDistanceToGround;
    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("IK Animating");
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);

        RaycastHit hit;
        Ray ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * m_iKRayOriginOffset, Vector3.down);
        
        if (Physics.Raycast(ray, out hit, m_iKBodyOffset, m_ikTargetLayer))
        {
            Vector3 footPosition = hit.point;
            footPosition.y += m_iKFootDistanceToGround;
            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
        }

    }
}
