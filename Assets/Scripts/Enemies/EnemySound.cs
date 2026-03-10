using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(EnemyController))]
public class EnemySound : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Volumes")]
    [SerializeField] private float moveVolume = 0.25f;
    [SerializeField] private float attackVolume = 0.8f;
    [SerializeField] private float hitVolume = 0.85f;
    [SerializeField] private float deathVolume = 1f;

    [Header("Move")]
    [SerializeField] private float moveInterval = 0.35f;
    [SerializeField] private float moveInputThreshold = 0.05f;

    private AudioSource audioSource;
    private EnemyController enemyController;
    private Health health;
    private float moveTimer;
    private float lastHealth;
    private bool hasLastHealthSample;

    /// <summary>
    /// 필요한 컴포넌트 참조를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        enemyController = GetComponent<EnemyController>();
    }

    /// <summary>
    /// 체력 이벤트를 구독합니다.
    /// </summary>
    private void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }
        }

        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDeath += HandleDeath;
            hasLastHealthSample = false;
        }
    }

    /// <summary>
    /// 체력 이벤트 구독을 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// 이동 입력에 따라 이동 사운드를 간격 기반으로 재생합니다.
    /// </summary>
    public void HandleMove(Vector2 moveDirection)
    {
        if (moveClip == null || enemyController == null)
        {
            return;
        }

        if (moveDirection.sqrMagnitude < moveInputThreshold * moveInputThreshold)
        {
            moveTimer = 0f;
            return;
        }

        moveTimer -= Time.deltaTime;
        if (moveTimer > 0f)
        {
            return;
        }

        PlayOneShot(moveClip, moveVolume);
        moveTimer = moveInterval;
    }

    /// <summary>
    /// 공격 사운드를 재생합니다.
    /// </summary>
    public void PlayAttack()
    {
        PlayOneShot(attackClip, attackVolume);
    }

    /// <summary>
    /// 피격 사운드를 재생합니다.
    /// </summary>
    public void PlayHit()
    {
        PlayOneShot(hitClip, hitVolume);
    }

    /// <summary>
    /// 사망 사운드를 재생합니다.
    /// </summary>
    public void PlayDeath()
    {
        if (deathClip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(deathClip, transform.position, deathVolume);
    }

    /// <summary>
    /// 체력 감소를 감지해 피격 사운드를 재생합니다.
    /// </summary>
    private void HandleHealthChanged(float current, float max)
    {
        if (hasLastHealthSample && current < lastHealth)
        {
            PlayHit();
        }

        lastHealth = current;
        hasLastHealthSample = true;
    }

    /// <summary>
    /// 사망 이벤트 발생 시 사망 사운드를 재생합니다.
    /// </summary>
    private void HandleDeath()
    {
        PlayDeath();
    }

    /// <summary>
    /// AudioSource를 통해 원샷 효과음을 재생합니다.
    /// </summary>
    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }
}
