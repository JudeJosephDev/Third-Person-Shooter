using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(WeaponBase))]
public class ShooterController : MonoBehaviour {

    [Header("Controller")]

    [SerializeField] private CinemachineVirtualCamera m_MainCamera;
    [SerializeField] private CinemachineVirtualCamera m_AimCamera;
    [SerializeField] private Image m_Crosshair;
    private Animator m_Animator;
    private WeaponBase m_Weapon;
    private ControlMapping m_Controls;

    private void Awake() {
        m_Controls = new ControlMapping();
        m_Controls.Gameplay.Fire.performed += StartWeaponFire;
        m_Controls.Gameplay.Fire.canceled += EndWeaponFire;
    }

    private void Start() {
        m_Animator = GetComponent<Animator>();
        m_Weapon = GetComponent<WeaponBase>();
    }

    private void OnEnable() {
        m_Controls.Enable();
    }

    private void OnDisable() {
        m_Controls.Disable();
    }

    private void StartWeaponFire( InputAction.CallbackContext obj ) {
        m_Weapon.FireWeapon();
    }

    void EndWeaponFire( InputAction.CallbackContext obj ) {

    }
}
