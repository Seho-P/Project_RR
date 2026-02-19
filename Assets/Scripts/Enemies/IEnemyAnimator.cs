using UnityEngine;

/// <summary>
/// 적 애니메이션 전략 인터페이스.
/// 적 종류마다 사용하는 Animator 파라미터가 다르므로 Strategy 패턴으로 분리합니다.
/// </summary>
public interface IEnemyAnimator
{
    /// <summary>
    /// 매 프레임 애니메이션 파라미터를 업데이트합니다.
    /// </summary>
    /// <param name="moveDirection">현재 이동 방향 (크기 0이면 정지 상태)</param>
    void UpdateAnimation(Vector2 moveDirection);

    /// <summary>
    /// 타겟 방향을 바라보도록 스프라이트 방향을 처리합니다.
    /// (전체 오브젝트 회전이 아닌, 스프라이트 flip 등 적 종류에 맞는 방식으로 처리)
    /// </summary>
    /// <param name="direction">타겟을 향한 방향 벡터</param>
    void FaceDirection(Vector2 direction);
}
