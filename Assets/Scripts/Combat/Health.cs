using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            if (destroyOnDeath) Destroy(gameObject);
            else currentHealth = 0f;
        }
    }
}


