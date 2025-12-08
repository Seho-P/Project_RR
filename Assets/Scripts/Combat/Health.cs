using UnityEngine;
using System;
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;

    public float CurrentHealth {get; private set;}
    public float MaxHealth => maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
            else CurrentHealth = 0f;
            
        }
    }
}


