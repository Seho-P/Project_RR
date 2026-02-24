using UnityEngine;
using System;
using Items.Enums;
using Items.Events;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private bool usePlayerStats = false; // 플레이어인 경우 true로 설정

    private PlayerStats playerStats;
    private bool hasExternalHealthInitialization;

    public float CurrentHealth {get; private set;}
    public float MaxHealth 
    { 
        get 
        { 
            // PlayerStats를 사용하는 경우 스탯에서 가져옴
            if (usePlayerStats && playerStats != null)
            {
                return playerStats.GetStat(StatType.MaxHealth);
            }
            return maxHealth;
        } 
    }

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public void Awake()
    {
        if (usePlayerStats)
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }
            
            if (playerStats == null)
            {
                Debug.LogWarning($"Health: usePlayerStats가 true인데 PlayerStats를 찾을 수 없습니다. GameObject: {gameObject.name}");
            }
        }
    }

    private void Start()
    {
        // Start()에서 CurrentHealth 설정 (PlayerStats의 Awake()가 완료된 후)
        // 외부에서 복원된 값이 있으면 유지하고, 없으면 최대 체력으로 초기화한다.
        float maxHp = MaxHealth;
        if (!hasExternalHealthInitialization)
        {
            CurrentHealth = maxHp;
        }
        else
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHp);
        }
        
        Debug.Log($"Health Start: usePlayerStats={usePlayerStats}, playerStats={playerStats != null}, MaxHealth={maxHp}, CurrentHealth={CurrentHealth}");
        
        // PlayerStats를 사용하는 경우 스탯 변경 이벤트 구독
        if (usePlayerStats)
        {
            ItemEvents.OnStatsChanged += OnStatsChanged;
        }
        
        // 초기화 완료 후 UI에 현재 상태 알림
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void OnDestroy()
    {
        if (usePlayerStats)
        {
            ItemEvents.OnStatsChanged -= OnStatsChanged;
        }
    }

    private void OnStatsChanged()
    {
        // 스탯이 변경되면 최대 체력이 변경될 수 있으므로 현재 체력 조정
        float newMaxHealth = MaxHealth;
        if (CurrentHealth > newMaxHealth)
        {
            CurrentHealth = newMaxHealth;
        }
        OnHealthChanged?.Invoke(CurrentHealth, newMaxHealth);
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
            else CurrentHealth = 0f;
            
        }
    }

    /// <summary>
    /// 씬 전환 후 런타임 데이터로 현재 체력을 복원한다.
    /// </summary>
    public void SetCurrentHealthFromData(float healthValue)
    {
        hasExternalHealthInitialization = true;
        CurrentHealth = Mathf.Clamp(healthValue, 0f, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}


