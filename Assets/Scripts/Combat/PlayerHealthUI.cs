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
        // Health의 Awake()가 먼저 실행된 후에 UI 초기화
        // Start()는 Awake() 이후에 실행되므로 Health의 MaxHealth가 이미 설정된 상태
        InitializeHealthUI();
    }

    private void InitializeHealthUI()
    {
        if (playerHealth != null)
        {
            // 초기 UI 설정
            healthBar.maxValue = playerHealth.MaxHealth;
            healthBar.value = playerHealth.CurrentHealth;

            healthText.text = playerHealth.CurrentHealth.ToString();
            maxHealthText.text = playerHealth.MaxHealth.ToString();

            // 이벤트 구독
            playerHealth.OnHealthChanged += UpdateHPUI;
        }
        else
        {
            Debug.LogError("PlayerHealthUI: PlayerHealth가 할당되지 않았습니다!");
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
        // Health 컴포넌트가 이미 PlayerStats의 MaxHealth를 사용하므로
        // 이벤트로 전달된 값만 사용하면 됨
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthText.text = currentHealth.ToString();
        maxHealthText.text = maxHealth.ToString();
    }
}