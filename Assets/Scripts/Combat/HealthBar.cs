using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthBar;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }
        if (healthBar == null)
        {
            healthBar = GetComponent<Image>();
        }
    }

    public void Initialize(Health health)
    {
        this.health = health;
        health.OnHealthChanged += UpdateBar;
    }

    private void OnDestroy()
    {
        health.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(float currentHealth, float maxHealth)
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

}