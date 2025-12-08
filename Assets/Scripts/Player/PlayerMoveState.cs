using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController controller) : base(controller)
    {
    }

    public override void Tick()
    {
        // 이동 입력 처리
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(x, y).normalized;
        controller.SetMoveDirection(moveInput);

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
            controller.WeaponHolder.Attack(mouseWorld);
        }
    }
}

