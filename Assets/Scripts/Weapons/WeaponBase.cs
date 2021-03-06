//TODO: Create a series of Unity Events that will notify other systems (I.e. UI) of the state of the Weapon Component (I.e. OnWeaaponFired)
//TODO: Create an enum for the firing mode of the weapon (Auto, Burst, Single, etc) 
//TODO: Implement Firing timer to prevent player from firing faster than the fire rate of the weapon

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{

    [SerializeField] private WeaponData m_WeaponData;
    [SerializeField] private LayerMask m_HitLayerMask;

    //Ammo
    [HideInInspector] public int m_ReserveAmmo;
    [HideInInspector] public int m_AmmoInClip;

    [Header("Additional Weapon Data")]
    public AudioClip m_FiringSound;
    public AudioClip m_ReloadSound;
    public AudioClip m_OutOfAmmoSound;
    public GameObject m_Mesh;
    public Transform m_FiringPoint;

    protected bool m_CanFire = true; 

    //Weapon Spread Data
    protected float m_CurrentMaxSpread;
    protected Coroutine m_SpreadResetTimer;
    private static float SpreadResetDelay = 2.0f;
    private static float SpreadResetRate = 0.1f;


    public WeaponData WeaponData { get => m_WeaponData; set => m_WeaponData =  value ; }
    protected LayerMask HitLayer { get => m_HitLayerMask; }

    protected void Awake() {
        if (!m_WeaponData) {
            throw new System.NullReferenceException($"{gameObject.name}'s Weapon component does not contain Weapon Data");
        }
    }

    // Built In function that is called at the start of an object's lifetime.
    /// <summary>
    ///  Used to Initialize realtime Data from the Weapon Data Scriptable Object.
    /// </summary>
    protected void Start() {
        m_AmmoInClip = m_WeaponData.m_AmmoPerClip;
        m_ReserveAmmo = m_WeaponData.m_AmmoPerClip * m_WeaponData.m_InitialClips;
        m_CurrentMaxSpread = m_WeaponData.m_BaseWeaponSpread;
    }

    /// <summary>
    /// Primary Fire for a weapon
    /// </summary>
    public virtual void FireWeapon() {
        //Ensure the player has enough ammo to use the weapon before firing.
        if (m_AmmoInClip > 0) {

            if (m_SpreadResetTimer != null) { StopCoroutine(m_SpreadResetTimer); }
            m_AmmoInClip--;

            Vector3 Direction = CalculateSpread();
            Ray FiringRay = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
            FiringRay.direction = Direction;
            RaycastHit HitInfo;

            if (Physics.Raycast(FiringRay, out HitInfo, m_WeaponData.m_Range, HitLayer, QueryTriggerInteraction.Ignore)) {

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

        //TODO: If there is not enough ammo in the clip, Play the m_OutOfAmmoSound Sound clip 
        //TODO: If there is not enough ammo in the clip, Cancel the AutoFire Coroutine for AutoFire Weapons
        //TODO: If there is not enough ammo in the clip, Call the Out of Ammo Event
    }

    /// <summary>
    /// Calculates a psuedo-random amount of spread at the weapons max range
    /// </summary>
    /// <returns></returns>
    protected Vector3 CalculateSpread() {

        Vector3 CurrentSpread = Random.insideUnitCircle * m_CurrentMaxSpread;
        Quaternion OffsetAngle = Quaternion.LookRotation(m_WeaponData.m_Range * Vector3.forward + CurrentSpread);
        Vector3 Value = Camera.main.transform.rotation * OffsetAngle * Vector3.forward;

        m_CurrentMaxSpread = Mathf.Clamp(m_CurrentMaxSpread + m_WeaponData.m_SpreadIncrement, m_WeaponData.m_BaseWeaponSpread, m_WeaponData.m_MaxWeaponSpread);
        return Value;
    }

    /// <summary>
    /// Takes ammo from the reserve and moves it into the clip.
    /// </summary>
    protected void Reload() {
        int AmmoDelta = Mathf.Min(m_WeaponData.m_AmmoPerClip - m_AmmoInClip, m_ReserveAmmo - m_AmmoInClip);

        if (AmmoDelta > 0) {
            m_AmmoInClip += AmmoDelta;
            m_ReserveAmmo -= AmmoDelta;
            //TODO: Play m_ReloadSound Sound Clip.
        }
    }

    /// <summary>
    /// Decrements the Max Spread after an inital delay;
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ResetSpread() {
        yield return new WaitForSeconds(SpreadResetDelay);

        while(m_CurrentMaxSpread > m_WeaponData.m_BaseWeaponSpread) {
            m_CurrentMaxSpread = Mathf.Clamp(m_CurrentMaxSpread - m_WeaponData.m_SpreadIncrement, m_WeaponData.m_BaseWeaponSpread, m_WeaponData.m_MaxWeaponSpread);
            yield return new WaitForSeconds(SpreadResetRate);
        }
    }

    /// <summary>
    /// Adds Ammo to the weapon's reserves
    /// </summary>
    /// <param name="amount"></param>
    public void GiveAmmo( int amount ) {
        m_ReserveAmmo = Mathf.Clamp(m_ReserveAmmo + amount, 0, m_WeaponData.m_MaxAmmo);
    }
}
    