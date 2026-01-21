using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float invincibilityDuration = 0.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer = 1 << 7; // Enemy 레이어 (인덱스 7)
    [SerializeField] private LayerMask projectileLayer = 0; // Projectile 레이어 (필요시 설정)

    [Header("Refs")]
    [SerializeField] private WeaponHolder weaponHolder;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Vector2 moveDirection;
    private bool isInvincible;
    private int defaultLayer;

    // State 패턴
    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    private PlayerDodgeState dodgeState;
    private PlayerState currentState;

    // Properties
    public Rigidbody2D Rigidbody => rb;
    public WeaponHolder WeaponHolder => weaponHolder;
    public float MoveSpeed => moveSpeed;
    public float DodgeDistance => dodgeDistance;
    public float DodgeDuration => dodgeDuration;
    public float InvincibilityDuration => invincibilityDuration;
    public bool IsInvincible => isInvincible;

    // State 접근자
    public PlayerIdleState IdleState => idleState;
    public PlayerMoveState MoveState => moveState;
    public PlayerDodgeState DodgeState => dodgeState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        defaultLayer = gameObject.layer;
        
        if (weaponHolder == null)
        {
            weaponHolder = GetComponentInChildren<WeaponHolder>();
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
            rb.linearVelocity = moveDirection * moveSpeed;
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
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
