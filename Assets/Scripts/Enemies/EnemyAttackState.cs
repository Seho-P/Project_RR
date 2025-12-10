using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private bool hasAttacked; // 공격 실행 여부
    private bool isInCooldown; // 무기 쿨타임 중인지
    private Coroutine attackCoroutine; // 공격 코루틴 참조

    [Header("Attack Settings")]
    [SerializeField] private float preAttackDelayTime = 0.2f; // 선딜레이 시간

    public EnemyAttackState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        hasAttacked = false;
        isInCooldown = false;
        controller.StopMovement();
        
        // Enter에서 바로 코루틴 시작
        if (controller.HasTarget)
        {
            controller.FaceTarget();
            attackCoroutine = controller.StartCoroutineFromState(AttackAfterDelay());
        }
    }

    public override void Exit()
    {
        // 상태 전환 시 코루틴 정리
        if (attackCoroutine != null)
        {
            controller.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackAfterDelay()
    {
        // 0.2초 대기
        yield return new WaitForSeconds(preAttackDelayTime);
        
        // 공격 실행 (타겟이 도망가도 마지막 위치로 공격)
        if (controller.HasTarget)
        {
            controller.TryAttack();
            hasAttacked = true;
            isInCooldown = true;
        }
    }

    public override void Tick()
    {
        if (controller == null) return;

        // 공격 전까지는 타겟을 바라봄
        if (!hasAttacked && controller.HasTarget)
        {
            controller.FaceTarget();
            return; // 공격 전까지는 상태 전환하지 않음
        }

        // 공격 후 무기 쿨타임 체크
        if (isInCooldown)
        {
            // 무기 쿨타임이 끝났는지 확인
            if (!controller.IsWeaponOnCooldown())
            {
                isInCooldown = false;
                // 쿨타임이 끝났으므로 상태 전환 가능
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
                // 공격 범위 내에 있으면 다시 공격
                hasAttacked = false;
                attackCoroutine = controller.StartCoroutineFromState(AttackAfterDelay());
            }
        }
    }
}