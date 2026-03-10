using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController controller) : base(controller)
    {
    }

    /// <summary>
    /// 이동 상태 입력을 처리하고 공격/회피를 실행합니다.
    /// </summary>
    public override void Tick()
    {
        // 이동 입력 처리
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(x, y).normalized;
        controller.SetMoveDirection(moveInput);
        controller.UpdateMoveSound(moveInput);

        // 애니메이션 업데이트 (방향 + Speed + 스프라이트 Flip)
        controller.UpdateAnimation(moveInput);

        // 이동 입력이 없으면 Idle 상태로 전환
        if (moveInput.magnitude < 0.01f)
        {
            controller.ChangeState(controller.IdleState);
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

