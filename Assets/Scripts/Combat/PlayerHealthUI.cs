using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private Health playerHealth;

    private void Start()
    {
        // 기존 씬 구성과의 호환을 위해 인스펙터 참조가 있으면 그대로 초기화.
        // 런타임 플레이어 생성 기반에서는 SceneFlowManager가 Bind()를 호출한다.
        if (playerHealth != null)
        {
            Bind(playerHealth);
        }
    }

    public void Bind(Health health)
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHPUI;
        }

        playerHealth = health;

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthUI: PlayerHealth가 할당되지 않았습니다!");
            return;
        }

        // 초기 UI 즉시 갱신
        UpdateHPUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        playerHealth.OnHealthChanged += UpdateHPUI;
    }

    public void Unbind()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHPUI;
            playerHealth = null;
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void UpdateHPUI(float currentHealth, float maxHealth)
    {
        // Health 컴포넌트가 이미 PlayerStats의 MaxHealth를 사용하므로
        // 이벤트로 전달된 값만 사용하면 됨
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthText.text = currentHealth.ToString();
        maxHealthText.text = maxHealth.ToString();
    }
}