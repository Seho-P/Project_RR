using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    
    private Health playerHealth;

    private void Start()
    {
        // 플레이어의 Health 컴포넌트 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                healthBar.maxValue = playerHealth.MaxHealth;
                healthBar.value = playerHealth.CurrentHealth;
                playerHealth.OnHealthChanged += UpdateHPUI;
            }
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHPUI;
        }
    }

    private void UpdateHPUI(float currentHealth, float maxHealth)
    {
        healthBar.value = currentHealth;
    }
}