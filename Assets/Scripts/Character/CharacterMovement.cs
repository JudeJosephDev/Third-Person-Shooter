using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour {

    public float m_WalkSpeed = 3.0f;
    public float m_RunSpeed = 8.0f;
    public float m_TurnSpeed = 1.0f;
    public float m_RotationSmoothTime = 0.12f;

    //Jump
    public float m_JumpHeight = 2.5f;
    public float m_TimeToJumpApex = 0.8f;
    public float m_AirControl = 0.333f;
    public float m_GroundOffset = -0.14f;
    public LayerMask m_GroundLayerMask;

    private bool m_IsSprinting;
    private bool m_Grounded;
    private float m_CurrentSpeed;
    private float m_Gravity;
    private float m_JumpForce;
    private float m_TargetRotation;
    private float m_RotationVelocity;
    private float m_VerticalVelocity;
    private float m_TerminalVelocity = -55.0f;
    private float m_MovementAnimationBlend;

    private Vector2 m_MoveInput;

    //Animaion IDs
    private int m_AnimIDSpeed;
    private int m_AnimIDForwardSpeed;
    private int m_AnimIDRightSpeed;
    private int m_AnimIDSprinting;
    private int m_AnimIDGrounded;
    private int m_AnimIDJump;

    //References
    private Animator m_Animator;
    private CharacterController m_Controller;
    private ControlMapping m_Controls;

    private void Awake() {
        m_Controls = new ControlMapping();
        m_Controls.Gameplay.Move.performed += ctx => m_MoveInput = ctx.ReadValue<Vector2>();
        m_Controls.Gameplay.Move.canceled += ctx => m_MoveInput = Vector2.zero;
        m_Controls.Gameplay.Sprint.performed += ctx => m_IsSprinting = true;
        m_Controls.Gameplay.Sprint.canceled += ctx => m_IsSprinting = false;
        m_Controls.Gameplay.Jump.performed += Jump;

    }

    private void Start() {
        m_Animator = GetComponent<Animator>();
        m_Controller = GetComponent<CharacterController>();

        m_AnimIDSpeed = Animator.StringToHash("Speed");
        m_AnimIDForwardSpeed = Animator.StringToHash("Forward Speed");
        m_AnimIDRightSpeed = Animator.StringToHash("Right Speed");
        m_AnimIDSprinting = Animator.StringToHash("Sprinting");
        m_AnimIDGrounded = Animator.StringToHash("Grounded");
        m_AnimIDJump = Animator.StringToHash("Jumping");

        m_Gravity = -( 2 * m_JumpHeight ) / Mathf.Pow(m_TimeToJumpApex, 2);
        m_JumpForce = Mathf.Abs(m_Gravity) * m_TimeToJumpApex;
    }

    private void OnEnable() {
        m_Controls.Enable();
    }

    private void OnDisable() {
        m_Controls.Disable();
    }

    private void Update() {
        ApplyGravity();
        GroundCheck();
        Move();
    }

    private void Move() {
        Vector3 NormalizedInput = new Vector3(m_MoveInput.x, 0, m_MoveInput.y).normalized;
        float TargetSpeed =  m_MoveInput.magnitude * ( m_IsSprinting ? m_RunSpeed : m_WalkSpeed );
        m_CurrentSpeed = new Vector3(m_Controller.velocity.x, 0, m_Controller.velocity.z).magnitude;
        if (m_MoveInput == Vector2.zero) { m_CurrentSpeed = 0; }

        if (m_MoveInput != Vector2.zero) {
            m_TargetRotation = Mathf.Atan2(NormalizedInput.x, NormalizedInput.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float rot = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_TargetRotation, ref m_RotationVelocity, m_RotationSmoothTime);
        }

        Vector3 TargetDirection = Quaternion.Euler(0.0f, m_TargetRotation, 0.0f) * Vector3.forward;
        m_Controller.Move(TargetDirection.normalized * ( TargetSpeed * Time.deltaTime ) + new Vector3(0.0f, m_VerticalVelocity, 0.0f) * Time.deltaTime);
        UpdateAnimator();
    }

    public void Jump( InputAction.CallbackContext context ) {
        if (m_Grounded) {
            m_VerticalVelocity = m_JumpForce;
            m_Grounded = false;
            m_Animator.SetBool(m_AnimIDJump, true);
        }
    }

    private void ApplyGravity() {
        if (m_VerticalVelocity > m_TerminalVelocity) {
            m_VerticalVelocity += m_Gravity * Time.deltaTime;
        }
    }

    private void GroundCheck() {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m_GroundOffset, transform.position.z);
        m_Grounded = Physics.CheckSphere(spherePosition, m_Controller.radius, m_GroundLayerMask, QueryTriggerInteraction.Ignore);
        m_Animator.SetBool(m_AnimIDGrounded, m_Grounded);

        if (m_Grounded && m_VerticalVelocity < 0) {
            m_VerticalVelocity = 0.0f;
        }
    }

    void UpdateAnimator() {
        if (m_Animator != null) {
            
        }
    }
}
