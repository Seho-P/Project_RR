using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthBar;
    private bool isInitialized;

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

    /// <summary>
    /// 체력 이벤트를 바인딩한다.
    /// </summary>
    public void Initialize(Health health)
    {
        if (health == null)
        {
            Debug.LogWarning($"[HealthBar] Initialize에 null Health가 전달되었습니다. object={name}, scene={gameObject.scene.name}");
            return;
        }

        this.health = health;
        health.OnHealthChanged += UpdateBar;
        isInitialized = true;

        Debug.Log($"[HealthBar] Initialize 성공. object={name}, healthObject={health.gameObject.name}, scene={gameObject.scene.name}");
    }

    /// <summary>
    /// 파괴 시 이벤트 구독을 안전하게 해제한다.
    /// </summary>
    private void OnDestroy()
    {
        if (!isInitialized || health == null)
        {
            Debug.LogWarning($"[HealthBar] OnDestroy 시점에 바인딩되지 않은 상태입니다. object={name}, scene={gameObject.scene.name}, isInitialized={isInitialized}, healthNull={health == null}");
            return;
        }

        if (health != null)
        {
            health.OnHealthChanged -= UpdateBar;
            Debug.Log($"[HealthBar] OnDestroy에서 이벤트 해제 완료. object={name}, scene={gameObject.scene.name}");
        }
    }

    private void UpdateBar(float currentHealth, float maxHealth)
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

}