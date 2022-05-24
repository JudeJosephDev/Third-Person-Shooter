using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {
    [SerializeField] private int current;
    [SerializeField] private int max;

    public int Current {
        get => current;
        set => current = Mathf.Clamp(value, 0, max);
    }
    public int Max {
        get => max;
        set {
            AdjustCurrentHealthByRatio(max, current, value);
        }
    }

    public UnityEvent OnHealthUpdated;
    public UnityEvent OnObjectHealed;
    public UnityEvent OnObjectDamaged;
    public UnityEvent OnObjectKilled;

    void AdjustCurrentHealthByRatio( int previousMaxHealth, int previousHealth, int newMaxHealth ) {
        newMaxHealth = Mathf.Clamp(newMaxHealth, 1, int.MaxValue);
        var adjustedCurHealth = newMaxHealth * GetHealthRatio();

        max = newMaxHealth;
        Current = Mathf.RoundToInt(Mathf.Clamp(previousHealth > adjustedCurHealth ? previousHealth : adjustedCurHealth, 0, newMaxHealth));
    }

    public float GetHealthRatio() {
        return Current / (float)Max;
    }

    public void Heal( int healAmount ) {

        healAmount = Mathf.Abs(healAmount); 

        Current += healAmount;
        OnObjectHealed?.Invoke();

    }

    public void Damage( int damageAmount ) {

        damageAmount = Mathf.Abs(damageAmount);

        Current -= damageAmount;

        if (GetHealthRatio() <= 0) {
            OnObjectKilled?.Invoke();
            enabled = false;
        } else {
            OnObjectDamaged?.Invoke();
        }

    }

    public static void Heal( Health objectToHeal, int healAmount ) {
        objectToHeal.Heal(healAmount);
    }

    public static void Damage( Health objectToDamage, int damageAmount ) {
        objectToDamage.Damage(damageAmount);
    }

    public static void Kill( Health objectToDestroy ) {
        Damage(objectToDestroy, objectToDestroy.Max);
    }
}
