using UnityEngine;

[CreateAssetMenu(fileName = "WeaponName_Data", menuName = "Weapons/Data")]
public class WeaponData : ScriptableObject {
    public int m_Damage = 20;
    public int m_MaxAmmo = 150;
    public int m_AmmoPerClip = 30;
    public int m_InitialClips = 3;
    public float m_FireRate = 0.1f;
    public float m_ReloadTime = 1.0f;
    public float m_BaseWeaponSpread = 1.0f;
    public float m_SpreadIncrement = 0.25f;
    public float m_MaxWeaponSpread = 5.0f;
    public float m_AimingSpreadModifier = 0.25f;
    public float m_Range = 100.0f;
}
