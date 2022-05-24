using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{

    [Header("Controller")]
    public GameObject m_FollowTarget;
    public float m_Sensitivity = 1f;
    public float m_TurnSpeed = 3.0f;
    [SerializeField] private float m_MaxPitchUp = 70f;
    [SerializeField] private float m_MaxPitchDown = -35f;
    [SerializeField] private float m_LookThreshold = .01f;
    [HideInInspector] public bool m_LockCameraRotation = false;

    private float m_TargetYaw;
    private float m_TargetPitch;
    private Vector2 m_LookInput = Vector2.zero;

    private ControlMapping m_Controls;

    private bool IsCurrentDeviceMouse {
        get => GetComponent<PlayerInput>()?.currentControlScheme == "KeyboardMouse";
    }

    private void Awake() {
        m_Controls = new ControlMapping();
        m_Controls.Gameplay.Look.performed += ctx => m_LookInput = ctx.ReadValue<Vector2>();
        m_Controls.Gameplay.Look.canceled += ctx => m_LookInput = ctx.ReadValue<Vector2>();
    }


    private void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable() {
        m_Controls.Enable();
    }

    private void OnDisable() {
        m_Controls.Disable();
    }

    public void Update() {
        CalculateRotation();
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, m_TargetYaw * m_Sensitivity * m_TurnSpeed, 0.0f); //Only Do if rotation is locked to camera
    }

    private void LateUpdate() {
        CameraRotation();
    }

    void CalculateRotation() {
        // if there is an input and camera position is not fixed
        if (m_LookInput.sqrMagnitude >= m_LookThreshold && !m_LockCameraRotation) {
            //Don't multiply mouse input by Time.deltaTime;
            float MouseCompensation = IsCurrentDeviceMouse ? Time.deltaTime * 10.0f : 1;

            m_TargetYaw += m_LookInput.x * MouseCompensation;
            m_TargetPitch += m_LookInput.y * MouseCompensation;
        }

        // clamp our rotations so our values are limited 360 degrees
        m_TargetYaw = ClampAngle(m_TargetYaw, float.MinValue, float.MaxValue);
        m_TargetPitch = ClampAngle(m_TargetPitch, m_MaxPitchDown, m_MaxPitchUp);
    }

    void CameraRotation() {
        // Cinemachine will follow this target
        m_FollowTarget.transform.rotation = Quaternion.Euler(m_TargetPitch, m_TargetYaw * m_Sensitivity * m_TurnSpeed, 0.0f);
    }

    float ClampAngle( float Value, float MinVal, float MaxVal ) {
        if (Value < -360) Value += 360;
        if (Value > 360) Value -= 360;

        return Mathf.Clamp(Value, MinVal, MaxVal);
    }
}
