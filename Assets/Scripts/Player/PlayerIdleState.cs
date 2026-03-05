using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController controller) : base(controller)
    {
    }

    /// <summary>
    /// Idle 상태 진입 시 이동을 정지하고 애니메이션을 갱신합니다.
    /// </summary>
    public override void Enter()
    {
        controller.SetMoveDirection(Vector2.zero);
        // Idle 상태 진입 시 Speed를 0으로 설정 (마지막 방향은 유지)
        controller.UpdateAnimation(Vector2.zero);
    }

    /// <summary>
    /// Idle 상태 입력을 처리하고 상태 전환을 수행합니다.
    /// </summary>
    public override void Tick()
    {
        // 이동 입력 확인
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(x, y).normalized;
        controller.UpdateMoveSound(moveInput);

        if (moveInput.magnitude > 0.01f)
        {
            controller.ChangeState(controller.MoveState);
            return;
        }

        // 회피 입력 확인
        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.ChangeState(controller.DodgeState);
            return;
        }

        // 공격 입력 처리
        if (Input.GetMouseButtonDown(0) && controller.WeaponHolder != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (controller.WeaponHolder.TryAttack(mouseWorld))
            {
                controller.PlayAttackSound();
            }
        }
    }
}

