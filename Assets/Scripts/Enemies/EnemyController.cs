using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Ranges")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float maxChaseRange = 10f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Targeting")]
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private Transform initialTarget;
    [SerializeField] private bool autoFindTargetByTag = true;
    [SerializeField] private string targetTag = "Player";

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private HealthBar healthBar;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private Health health;

    private EnemyIdleState idleState;
    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    private EnemyState currentState;

    private Vector2 moveDirection;

    public bool HasTarget => currentTarget != null;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float MaxChaseRange => maxChaseRange;
    public EnemyIdleState IdleState => idleState;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (weaponHolder == null)
        {
            weaponHolder = GetComponentInChildren<WeaponHolder>();
        }
        currentTarget = initialTarget;
    }

    private void Start()
    {
        health = GetComponent<Health>();
        healthBar.Initialize(health);

        if (autoFindTargetByTag && currentTarget == null && !string.IsNullOrEmpty(targetTag))
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
            {
                currentTarget = targetObj.transform;
            }
        }

        idleState = new EnemyIdleState(this);
        chaseState = new EnemyChaseState(this);
        attackState = new EnemyAttackState(this);

        ChangeState(idleState);
    }

    private void Update()
    {
        currentState?.Tick();
    }

    private void FixedUpdate()
    {
        if (moveDirection != Vector2.zero)
        {
            Vector2 newPos = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    public void ChangeState(EnemyState nextState)
    {
        if (nextState == null || nextState == currentState) return;
        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    public bool TryEnsureTarget()
    {
        if (currentTarget != null) return true;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, targetMask);
        if (hit != null)
        {
            currentTarget = hit.transform;
            return true;
        }
        return false;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public Vector2 GetTargetPosition()
    {
        return currentTarget != null ? (Vector2)currentTarget.position : rb.position;
    }

    public bool IsTargetWithinRange(float range)
    {
        if (currentTarget == null) return false;
        float sqrDist = ((Vector2)currentTarget.position - rb.position).sqrMagnitude;
        return sqrDist <= range * range;
    }

    public void MoveTowardsTarget()
    {
        if (!HasTarget)
        {
            moveDirection = Vector2.zero;
            return;
        }

        Vector2 dir = ((Vector2)currentTarget.position - rb.position).normalized;
        moveDirection = dir;
        FaceTarget();
    }

    public void ResumeMovement()
    {
        if (moveDirection == Vector2.zero && HasTarget)
        {
            MoveTowardsTarget();
        }
    }

    public void StopMovement()
    {
        moveDirection = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void FaceTarget()
    {
        if (!HasTarget) return;
        Vector2 dir = ((Vector2)currentTarget.position - rb.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

     public bool TryAttack()
    {
        if (weaponHolder == null || currentTarget == null) return false;
        weaponHolder.Attack(currentTarget.position);
        return true;
    }

    public bool TryAttackAtPosition(Vector2 targetPosition)
    {
        if (weaponHolder == null) return false;
        weaponHolder.Attack(targetPosition);
        return true;
    }

    // 코루틴 실행을 위한 헬퍼 메서드
    public Coroutine StartCoroutineFromState(System.Collections.IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public bool IsWeaponOnCooldown()
    {
        if (weaponHolder == null) return false;
        // WeaponHolder의 무기 쿨타임 확인을 위해 리플렉션 사용
        var weaponField = typeof(WeaponHolder).GetField("currentWeapon", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (weaponField == null) return false;
        
        var weapon = weaponField.GetValue(weaponHolder) as IWeapon;
        if (weapon == null) return false;
        
        // CanAttack()이 false면 쿨타임 중
        return !weapon.CanAttack();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxChaseRange);
    }
    
}


