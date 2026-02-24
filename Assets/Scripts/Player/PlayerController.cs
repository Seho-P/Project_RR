using UnityEngine;
using Items.Enums;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerDataBridge))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 5f;

    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float invincibilityDuration = 0.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer = 1 << 7; // Enemy 레이어 (인덱스 7)
    [SerializeField] private LayerMask projectileLayer = 0; // Projectile 레이어 (필요시 설정)

    [Header("Refs")]
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private PlayerStats playerStats;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private Vector2 lastFacingDirection = Vector2.down; // 마지막으로 바라본 방향 (기본: 아래)
    private bool isInvincible;
    private int defaultLayer;

    // State 패턴
    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    private PlayerDodgeState dodgeState;
    private PlayerState currentState;

    // Properties
    public Rigidbody2D Rigidbody => rb;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public WeaponHolder WeaponHolder => weaponHolder;
    public float MoveSpeed => GetMoveSpeed();
    public float DodgeDistance => dodgeDistance;
    public float DodgeDuration => dodgeDuration;
    public float InvincibilityDuration => invincibilityDuration;
    public bool IsInvincible => isInvincible;
    public Vector2 LastFacingDirection => lastFacingDirection;

    // State 접근자
    public PlayerIdleState IdleState => idleState;
    public PlayerMoveState MoveState => moveState;
    public PlayerDodgeState DodgeState => dodgeState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // 벽 충돌 시 물리 회전 방지
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultLayer = gameObject.layer;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (weaponHolder == null)
        {
            weaponHolder = GetComponentInChildren<WeaponHolder>();
        }

        // PlayerStats 찾기 (같은 GameObject 또는 자식에서)
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }
            if (playerStats == null)
            {
                playerStats = FindFirstObjectByType<PlayerStats>();
            }
        }
    }

    private void Start()
    {
        // State 초기화
        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
        dodgeState = new PlayerDodgeState(this);

        // 초기 상태 설정
        ChangeState(idleState);
    }

    private void Update()
    {
        currentState?.Tick();
        RotateToMouse();
        
        // 인벤토리 입력 처리 (상태와 무관하게 항상 처리)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.Instance?.ToggleUI<InventoryUI>("InventoryPanel");
        }
    }

    private void FixedUpdate()
    {
        currentState?.FixedTick();
        
        // 이동 처리 (회피 상태가 아닐 때만)
        if (currentState != dodgeState)
        {
            rb.linearVelocity = moveDirection * GetMoveSpeed();
        }
    }

    public void ChangeState(PlayerState nextState)
    {
        if (nextState == null || nextState == currentState) return;
        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction;
    }

    /// <summary>
    /// 애니메이션 파라미터를 업데이트하고, 왼쪽 이동 시 스프라이트를 뒤집습니다.
    /// </summary>
    public void UpdateAnimation(Vector2 moveInput)
    {
        if (animator == null) return;

        // 이동 중이면 마지막 방향 갱신
        if (moveInput.magnitude > 0.01f)
        {
            lastFacingDirection = moveInput;
        }

        // Animator 파라미터 설정
        // MoveX: 오른쪽 모션만 있으므로 항상 절대값 사용 (Blend Tree에서 양수만 사용)
        animator.SetFloat("MoveX", Mathf.Abs(lastFacingDirection.x));
        animator.SetFloat("MoveY", lastFacingDirection.y);
        animator.SetFloat("Speed", moveInput.magnitude);

        // 왼쪽으로 이동할 때 스프라이트 뒤집기
        if (spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
        {
            spriteRenderer.flipX = moveInput.x < 0f;
        }
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        
        if (invincible)
        {
            // 무적 상태: 적과 프로젝타일 레이어와의 충돌 무시
            // 벽과는 여전히 충돌 (기본 레이어 유지)
            SetLayerCollisionIgnore(true);
        }
        else
        {
            // 일반 상태: 모든 충돌 복원
            SetLayerCollisionIgnore(false);
        }
    }

    private void SetLayerCollisionIgnore(bool ignore)
    {
        // Enemy 레이어와의 충돌 설정
        if (enemyLayer != 0)
        {
            int enemyLayerIndex = GetLayerIndex(enemyLayer);
            if (enemyLayerIndex >= 0)
            {
                Physics2D.IgnoreLayerCollision(defaultLayer, enemyLayerIndex, ignore);
            }
        }

        // Projectile 레이어와의 충돌 설정
        if (projectileLayer != 0)
        {
            int projectileLayerIndex = GetLayerIndex(projectileLayer);
            if (projectileLayerIndex >= 0)
            {
                Physics2D.IgnoreLayerCollision(defaultLayer, projectileLayerIndex, ignore);
            }
        }
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerValue = layerMask.value;
        if (layerValue == 0) return -1;
        
        // 가장 낮은 비트의 인덱스를 찾음
        for (int i = 0; i < 32; i++)
        {
            if ((layerValue & (1 << i)) != 0)
            {
                return i;
            }
        }
        return -1;
    }

    private void RotateToMouse()
    {
        if (weaponHolder == null || Time.timeScale <= 0f) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - weaponHolder.FirePoint.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // 플레이어 전체가 아닌 무기(WeaponHolder)만 회전
        weaponHolder.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 현재 이동 속도를 가져옵니다 (기본값 + PlayerStats의 MoveSpeed 스탯)
    /// </summary>
    private float GetMoveSpeed()
    {
        float statBonus = 0f;
        if (playerStats != null)
        {
            statBonus = playerStats.GetStat(StatType.MoveSpeed);
        }
        return baseMoveSpeed + statBonus;
    }
}
