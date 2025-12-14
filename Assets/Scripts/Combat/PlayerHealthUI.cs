using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Health playerhealth;
    [SerializeField] private Slider healthBar;

    private void Awake()
    {
        if (playerhealth == null)
        {
            playerhealth = GetComponent<Health>();
        }
        if (healthBar == null)
        {
            healthBar = GetComponent<Slider>();
        }
    }

    private void Start(){
        healthBar.maxValue = playerhealth.MaxHealth;
        healthBar.value = playerhealth.CurrentHealth;

        playerhealth.OnHealthChanged += UpdateHPUI;
    }

    public void OnDestroy()
    {
        playerhealth.OnHealthChanged -= UpdateHPUI;
    }

    private void UpdateHPUI(float currentHealth, float maxHealth){
        healthBar.value = currentHealth;
    }
}