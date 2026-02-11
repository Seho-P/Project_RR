using UnityEngine;

/// <summary>
/// MoveX, MoveY, Speed 파라미터를 모두 사용하는 방향 애니메이터 (예: Skeleton).
/// Animator에 "MoveX", "MoveY", "Speed" float 파라미터가 필요합니다.
/// PlayerController의 UpdateAnimation과 동일한 방식으로 동작합니다.
/// </summary>
public class DirectionalEnemyAnimator : MonoBehaviour, IEnemyAnimator
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastFacingDirection = Vector2.down;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public void UpdateAnimation(Vector2 moveDirection)
    {
        if (animator == null) return;

        // 이동 중이면 마지막 방향 갱신
        if (moveDirection.magnitude > 0.01f)
        {
            lastFacingDirection = moveDirection;
        }

        // Animator 파라미터 설정
        // MoveX: 오른쪽 모션만 있으므로 항상 절대값 사용 (Blend Tree에서 양수만 사용)
        animator.SetFloat("MoveX", Mathf.Abs(lastFacingDirection.x));
        animator.SetFloat("MoveY", lastFacingDirection.y);
        animator.SetFloat("Speed", moveDirection.magnitude);

        // 왼쪽으로 이동할 때 스프라이트 뒤집기
        if (spriteRenderer != null && Mathf.Abs(moveDirection.x) > 0.01f)
        {
            spriteRenderer.flipX = moveDirection.x < 0f;
        }
    }

    public void FaceDirection(Vector2 direction)
    {
        // 방향에 따라 마지막 바라본 방향 갱신
        if (direction.magnitude > 0.01f)
        {
            lastFacingDirection = direction;
        }

        // 스프라이트 좌우 반전
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }
}
