using UnityEngine;

/// <summary>
/// Speed 파라미터만 사용하는 심플 애니메이터 (예: Slime).
/// Animator에 "Speed" float 파라미터가 필요합니다.
/// </summary>
public class SimpleEnemyAnimator : MonoBehaviour, IEnemyAnimator
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

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

        animator.SetFloat("Speed", moveDirection.magnitude);

        // 이동 방향에 따라 스프라이트 좌우 반전
        if (spriteRenderer != null && Mathf.Abs(moveDirection.x) > 0.01f)
        {
            spriteRenderer.flipX = moveDirection.x < 0f;
        }
    }

    public void FaceDirection(Vector2 direction)
    {
        // 스프라이트 좌우 반전만 처리 (회전 없음)
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }
}
