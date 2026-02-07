using UnityEngine;
using System.Collections;

public class PlayerDodgeState : PlayerState
{
    private Vector2 dodgeDirection;
    private float dodgeDistance;
    private float dodgeDuration;
    private float invincibilityDuration;
    private Coroutine dodgeCoroutine;

    public PlayerDodgeState(PlayerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        // 회피 방향 결정
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(x, y).normalized;

        if (moveInput.magnitude > 0.01f)
        {
            // 방향키를 누르고 있으면 그 방향으로 회피
            dodgeDirection = moveInput;
        }
        else
        {
            // 방향키를 누르지 않았으면 마우스 방향 반대로 회피
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseDir = (mouseWorld - controller.transform.position).normalized;
            dodgeDirection = -mouseDir;
        }

        dodgeDistance = controller.DodgeDistance;
        dodgeDuration = controller.DodgeDuration;
        invincibilityDuration = controller.InvincibilityDuration;

        // 회피 방향에 맞게 애니메이션 업데이트 (스프라이트 Flip 포함)
        controller.UpdateAnimation(dodgeDirection);

        // 회피 코루틴 시작
        dodgeCoroutine = controller.StartCoroutine(PerformDodge());
    }

    public override void Exit()
    {
        if (dodgeCoroutine != null)
        {
            controller.StopCoroutine(dodgeCoroutine);
            dodgeCoroutine = null;
        }
        controller.SetInvincible(false);
        controller.SetMoveDirection(Vector2.zero);
    }

    private IEnumerator PerformDodge()
    {
        // 무적 효과 시작
        controller.SetInvincible(true);

        Vector2 startPos = controller.Rigidbody.position;
        Vector2 targetPos = startPos + dodgeDirection * dodgeDistance;
        float elapsed = 0f;

        // 회피 이동
        while (elapsed < dodgeDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / dodgeDuration;
            // 부드러운 이동을 위한 커브 적용 (선택사항)
            float smoothT = 1f - (1f - t) * (1f - t); // ease-out
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, smoothT);
            controller.Rigidbody.MovePosition(currentPos);

            yield return new WaitForFixedUpdate();
        }

        // 최종 위치 보정
        controller.Rigidbody.MovePosition(targetPos);
        // 이동 방향 및 속도 초기화 (추가 움직임 방지)
        controller.SetMoveDirection(Vector2.zero);
        controller.Rigidbody.linearVelocity = Vector2.zero;

        // 무적 지속 시간이 회피 시간보다 길면 추가로 대기
        if (invincibilityDuration > dodgeDuration)
        {
            yield return new WaitForSeconds(invincibilityDuration - dodgeDuration);
        }

        // 무적 효과 종료
        controller.SetInvincible(false);

        // Idle 상태로 전환
        controller.ChangeState(controller.IdleState);
    }
}

