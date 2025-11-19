using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float attackTimer;

    public EnemyAttackState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        attackTimer = 0f;
        controller.StopMovement();
    }

    public override void Tick()
    {
        if (controller == null) return;

        if (!controller.HasTarget)
        {
            controller.ChangeState(controller.IdleState);
            return;
        }

        if (!controller.IsTargetWithinRange(controller.AttackRange))
        {
            controller.ChangeState(controller.ChaseState);
            return;
        }

        controller.FaceTarget();

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && controller.TryAttack())
        {
            attackTimer = controller.AttackCooldown;
        }
    }
}


