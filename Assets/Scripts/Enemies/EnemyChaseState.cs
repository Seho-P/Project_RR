using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller?.ResumeMovement();
    }

    public override void Exit()
    {
        controller?.StopMovement();
    }

    public override void Tick()
    {
        if (controller == null) return;
        if (!controller.HasTarget)
        {
            if (!controller.TryEnsureTarget())
            {
                controller.ChangeState(controller.IdleState);
                return;
            }
        }

        if (!controller.IsTargetWithinRange(controller.MaxChaseRange))
        {
            controller.ClearTarget();
            controller.ChangeState(controller.IdleState);
            return;
        }

        if (controller.IsTargetWithinRange(controller.AttackRange))
        {
            controller.ChangeState(controller.AttackState);
            return;
        }

        controller.MoveTowardsTarget();
    }
}


