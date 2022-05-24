using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData m_WeaponData;
    [HideInInspector] public int m_ReserveAmmo;
    public int m_AmmoInClip;

    public AudioClip m_FiringSound;
    public AudioClip m_ReloadSound;
    public AudioClip m_OutOfAmmoSound;
    public GameObject m_Mesh;
    public Transform m_FiringPoint;

    protected bool m_CanFire;
    protected float m_CurrentMaxSpread;

    protected Coroutine m_SpreadResetTimer;
    private static float SpreadResetDelay = 2.0f;
    private static float SpreadResetRate = 0.1f;

    [SerializeField] private LayerMask m_HitLayerMask;
    protected LayerMask GetHitLayer() { return m_HitLayerMask; }

    protected void Awake() {
        if (!m_WeaponData) {
            Application.Quit();
            throw new System.NullReferenceException($"{gameObject.name}'s Weapon component does not contain Weapon Data");
        }
    }

    protected void Start() {
        m_AmmoInClip = m_WeaponData.m_AmmoPerClip;
        m_ReserveAmmo = m_WeaponData.m_AmmoPerClip * m_WeaponData.m_InitialClips;
        m_CurrentMaxSpread = m_WeaponData.m_BaseWeaponSpread;
    }

    public virtual void FireWeapon() {
        if (m_AmmoInClip > 0) {

            if (m_SpreadResetTimer != null) { StopCoroutine(m_SpreadResetTimer); }
            m_AmmoInClip--;

            Vector3 Direction = CalculateSpread();
            Ray FiringRay = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
            FiringRay.direction = Direction;
            RaycastHit HitInfo;

            if (Physics.Raycast(FiringRay, out HitInfo, m_WeaponData.m_Range, GetHitLayer(), QueryTriggerInteraction.Ignore)) {

                Ray RayFromMuzzle = new Ray(m_FiringPoint.position, HitInfo.point - m_FiringPoint.position);
                RaycastHit RFMHitInfo;

                if (Physics.Raycast(RayFromMuzzle, out RFMHitInfo, m_WeaponData.m_Range)) {
                    GameObject HitObject = RFMHitInfo.collider.gameObject;
                    Health HitObjHealth = HitObject.GetComponentInChildren<Health>();

                    if (HitObjHealth) {
                        HitObjHealth.Damage(m_WeaponData.m_Damage);
                        HitObject.GetComponent<Rigidbody>()?.AddForce(Camera.main.transform.forward * 2);
                    }
                }
               
            }
            Debug.DrawLine(FiringRay.origin, FiringRay.origin + FiringRay.direction * HitInfo.distance, Color.magenta, 10.0f, true);
            m_SpreadResetTimer = StartCoroutine(ResetSpread());
        }
    }

    protected Vector3 CalculateSpread() {

        Vector3 CurrentSpread = Random.insideUnitCircle * m_CurrentMaxSpread;
        Quaternion OffsetAngle = Quaternion.LookRotation(m_WeaponData.m_Range * Vector3.forward + CurrentSpread);
        Vector3 Value = Camera.main.transform.rotation * OffsetAngle * Vector3.forward;

        m_CurrentMaxSpread = Mathf.Clamp(m_CurrentMaxSpread + m_WeaponData.m_SpreadIncrement, m_WeaponData.m_BaseWeaponSpread, m_WeaponData.m_MaxWeaponSpread);
        return Value;
    }
    protected void Reload() {
        int AmmoDelta = Mathf.Min(m_WeaponData.m_AmmoPerClip - m_AmmoInClip, m_ReserveAmmo - m_AmmoInClip);

        if (AmmoDelta > 0) {
            m_AmmoInClip += AmmoDelta;
        }
    }

    protected IEnumerator ResetSpread() {
        yield return new WaitForSeconds(SpreadResetDelay);

        while(m_CurrentMaxSpread > m_WeaponData.m_BaseWeaponSpread) {
            m_CurrentMaxSpread = Mathf.Clamp(m_CurrentMaxSpread - m_WeaponData.m_SpreadIncrement, m_WeaponData.m_BaseWeaponSpread, m_WeaponData.m_MaxWeaponSpread);
            yield return new WaitForSeconds(SpreadResetRate);
        }
    }

    public void GiveAmmo( int amount ) {
        m_ReserveAmmo = Mathf.Clamp(m_ReserveAmmo + amount, 0, m_WeaponData.m_MaxAmmo);
    }
}
    