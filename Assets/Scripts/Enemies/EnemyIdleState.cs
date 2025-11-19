using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float scanTimer;
    private readonly float scanInterval;

    public EnemyIdleState(EnemyController controller, float scanIntervalSeconds = 0.2f) : base(controller)
    {
        scanInterval = Mathf.Max(0.05f, scanIntervalSeconds);
    }

    public override void Enter()
    {
        controller.StopMovement();
        scanTimer = 0f;
    }

    public override void Tick()
    {
        if (controller == null) return;

        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            if (controller.TryEnsureTarget())
            {
                controller.ChangeState(controller.ChaseState);
            }
        }
    }
}


