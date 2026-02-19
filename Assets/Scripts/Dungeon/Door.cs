using UnityEngine;

/// <summary>
/// 방의 출입구 하나를 관리하는 컴포넌트.
/// 블로커(Blocker) 자식 오브젝트를 켜고 꺼서 문을 열고 닫는다.
/// 
/// [구조]
/// DoorTop (이 컴포넌트)
/// └── Blocker (BoxCollider2D + 선택적 SpriteRenderer)
///     - 활성화 = 문 닫힘 (벽처럼 막힘)
///     - 비활성화 = 문 열림 (통행 가능)
/// 
/// [블로커 설정 가이드]
/// 1. 방의 Wall 타일맵에서 문 위치에 2~3타일 갭을 만든다.
/// 2. 해당 갭 위치에 Blocker 오브젝트를 배치한다.
/// 3. Blocker에 BoxCollider2D를 추가하여 갭을 막는다.
/// 4. 선택적으로 SpriteRenderer로 벽 스프라이트를 표시한다.
/// </summary>
public class Door : MonoBehaviour
{
    [Header("=== 블로커 ===")]
    [Tooltip("문을 막는 오브젝트 (BoxCollider2D 필수, SpriteRenderer 선택)")]
    [SerializeField] private GameObject blocker;

    /// <summary>
    /// 문이 열려 있는지 여부
    /// </summary>
    public bool IsOpen => blocker != null && !blocker.activeSelf;

    /// <summary>
    /// 문 열기/닫기
    /// </summary>
    /// <param name="open">true: 열림(블로커 비활성화), false: 닫힘(블로커 활성화)</param>
    public void SetOpen(bool open)
    {
        if (blocker != null)
        {
            blocker.SetActive(!open);
        }
    }
}
