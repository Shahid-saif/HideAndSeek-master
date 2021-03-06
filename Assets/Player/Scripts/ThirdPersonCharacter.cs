using UnityEngine;
using Photon.Pun;



public class ThirdPersonCharacter : MonoBehaviour

{


    public float m_MovingTurnSpeed = 360;
    public float m_StationaryTurnSpeed = 180;
    public float m_JumpPower = 12f;
    public float m_WallJumpPower = 12f;
    [Range(1f, 16f)] public float m_GravityMultiplier = 2f;
    public float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    public float m_MoveSpeedMultiplier = 1f;
    public float m_AnimSpeedMultiplier = 1f;
    public float m_GroundCheckDistance = 0.1f;

    Rigidbody m_Rigidbody;
    Animator m_Animator;
    [SerializeField] bool m_IsGrounded;
    float[] f;

    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    CapsuleCollider m_Capsule;
    bool m_Crouching;


    // debug
    [SerializeField] float mapBottom = -4;
    [SerializeField] float fallingPenalty = 5;
    public static GameObject player;
    private Transform cam;
    public float h, v;
    Vector3 newVelocity;
    PhotonView view;
    [SerializeField] bool wallJumping;
    private bool canWalljump;
    Vector3 wallJumpDirection;
    float wallJumpTimer;
    [SerializeField] float vy;

    [Header("Audio")]
    [SerializeField] AudioSource AD_S;
    [SerializeField] AudioClip JumpClip;
    [SerializeField] AudioClip LandClip;
    [SerializeField] AudioClip stepSound;

    [SerializeField] bool LandCheck = false;


    void Start()
    {
        view = GetComponent<PhotonView>();
        cam = Camera.main.transform;
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
    }


    public void Move(Vector3 move, bool crouch, bool jump)
    {
        if (view.IsMine)
        {

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            m_TurnAmount = Mathf.Atan2(move.x, move.z);
            m_ForwardAmount = move.z;

            ApplyExtraTurnRotation();

            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)
            {
                HandleGroundedMovement(crouch, jump);
            }
            else
            {
                HandleAirborneMovement();
            }

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            // send input and other state parameters to the animator

            UpdateAnimator(move);
        }
    }


    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_IsGrounded && crouch)
        {
            if (m_Crouching) return;
            m_Capsule.height = m_Capsule.height / 2f;
            m_Capsule.center = m_Capsule.center / 2f;
            m_Crouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_Crouching = false;
        }
    }

    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!m_Crouching)
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
            }
        }
    }


    void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        // m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        // m_Animator.SetBool("Crouch", m_Crouching);
        m_Animator.SetBool("OnGround", m_IsGrounded);
        if (!m_IsGrounded)
        {
            m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
           
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        // float runCycle =
        //     Mathf.Repeat(
        //         m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
        // float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
        // if (m_IsGrounded)
        // {
        //     m_Animator.SetFloat("JumpLeg", jumpLeg);
        // }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_IsGrounded && move.magnitude > 0)
        {
            m_Animator.speed = m_AnimSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            m_Animator.speed = 1;

        }
    }


    void HandleAirborneMovement()
    {
        if (view.IsMine)
        {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.5f;
        }
    }


    void HandleGroundedMovement(bool crouch, bool jump)
    {
        //"&& m_Rigidbody.drag > 0" check whether conditions are right to allow a jump:
        if (jump && !crouch && m_IsGrounded)
        {
            // jump!
            LandCheck = true;
            AD_S.PlayOneShot(JumpClip);
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
            m_Animator.applyRootMotion = false;
            m_GroundCheckDistance = 0.5f;
        }
    }

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }
    private void Update()
    {
        vy = m_Rigidbody.velocity.y;
        if (view.IsMine)
        {
            m_GroundCheckDistance = m_Rigidbody.velocity.y <= 0 ? m_OrigGroundCheckDistance : 0.5f;

            if (wallJumping && !m_IsGrounded)
            {
                m_Rigidbody.drag = 0;
            }
            else
            {

                m_Rigidbody.drag = 5;
            }
            if (Input.GetKeyDown(KeyCode.LeftShift)) m_Animator.SetTrigger("Roll");

            if (!wallJumping)
            {
                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");
                var camUp = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                var camRight = Quaternion.Euler(0, 90, 0) * camUp;


                newVelocity = ((camUp * v) + (camRight * h)).normalized * ((GetComponent<PlayerStatus>().isSeeker) ? m_MoveSpeedMultiplier * 1.35f : m_MoveSpeedMultiplier);
            }
            if (Time.deltaTime > 0 && view.IsMine && !wallJumping)
            {
                Vector3 v = newVelocity;

                // preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
            if (transform.position.y <= mapBottom)
            {
                transform.position = GameRulesManager.gameRulesManager.spawnPoints[0].position;
                GameRulesManager.gameRulesManager.SetFallingPlayerToSeeker(view.ControllerActorNr);

            }

            if (canWalljump && Input.GetButtonDown("Jump"))
            {
                AD_S.PlayOneShot(JumpClip);
                wallJumpTimer = Time.time + .2f;
                wallJumping = true;
                newVelocity = Vector3.zero;
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.AddForce(wallJumpDirection * m_WallJumpPower);
            }



        }

    }

    public void OnAnimatorMove()
    {

        if (Time.deltaTime > 0 && view.IsMine && !wallJumping)
        {
            Vector3 v = newVelocity;

            // preserve the existing y part of the current velocity.
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }


    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            if (LandCheck)
            {
                AD_S.PlayOneShot(LandClip);
                LandCheck = false;
            }
         
            m_Animator.applyRootMotion = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }

    }
    private void OnCollisionStay(Collision collision)
    {
        var hit = collision.GetContact(collision.contactCount - 1);
        canWalljump = (collision.collider.tag == "Wall" && !m_IsGrounded && hit.normal.y <= .1f && wallJumpTimer < Time.time);
        if (canWalljump)
        {
            wallJumpDirection = ((hit.normal + Vector3.up).normalized);
        }
        else if (wallJumpTimer < Time.time)
        {
            wallJumping = false;
        }
    }
}