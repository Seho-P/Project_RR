using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSound : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip dodgeClip;
    [SerializeField] private AudioClip hitClip;

    [Header("Volumes")]
    [SerializeField] private float moveVolume = 0.35f;
    [SerializeField] private float attackVolume = 0.8f;
    [SerializeField] private float dodgeVolume = 0.8f;
    [SerializeField] private float hitVolume = 0.9f;

    [Header("Move")]
    [SerializeField] private float moveInterval = 0.3f;
    [SerializeField] private float moveInputThreshold = 0.05f;

    private AudioSource audioSource;
    private Health health;
    private float moveTimer;
    private float lastHealth;
    private bool hasLastHealthSample;

    /// <summary>
    /// 필요한 컴포넌트를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 체력 변경 이벤트를 구독합니다.
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
            hasLastHealthSample = false;
        }
    }

    /// <summary>
    /// 체력 변경 이벤트 구독을 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    /// <summary>
    /// 이동 입력을 받아 발소리를 간격 기반으로 재생합니다.
    /// </summary>
    public void HandleMove(Vector2 moveInput)
    {
        if (moveClip == null)
        {
            return;
        }

        if (moveInput.sqrMagnitude < moveInputThreshold * moveInputThreshold)
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
    /// 공격 효과음을 재생합니다.
    /// </summary>
    public void PlayAttack()
    {
        PlayOneShot(attackClip, attackVolume);
    }

    /// <summary>
    /// 회피 효과음을 재생합니다.
    /// </summary>
    public void PlayDodge()
    {
        PlayOneShot(dodgeClip, dodgeVolume);
    }

    /// <summary>
    /// 피격 효과음을 재생합니다.
    /// </summary>
    public void PlayHit()
    {
        PlayOneShot(hitClip, hitVolume);
    }

    /// <summary>
    /// 체력이 감소했을 때 피격 효과음을 재생합니다.
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
    /// 지정한 클립을 원샷으로 재생합니다.
    /// </summary>
    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }
}
