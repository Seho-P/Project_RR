using UnityEngine;

/// <summary>
/// 던전에서 플레이어를 부드럽게 추적하는 카메라 컨트롤러.
/// Main Camera에 부착한다.
/// 
/// 플레이어가 방 사이를 걸어서 이동할 때 자연스럽게 따라간다.
/// </summary>
public class DungeonCameraController : MonoBehaviour
{
    [Header("=== 추적 설정 ===")]
    [Tooltip("추적할 대상 (비어있으면 PlayerController 자동 탐색)")]
    [SerializeField] private Transform target;

    [Tooltip("카메라 이동 부드러움 (높을수록 빠르게 추적)")]
    [SerializeField] private float smoothSpeed = 8f;

    [Tooltip("카메라 Z축 오프셋 (2D에서는 보통 -10)")]
    [SerializeField] private float zOffset = -10f;

    private void Start()
    {
        if (target == null)
        {
            // 씬에서 PlayerController 자동 탐색
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("[DungeonCamera] PlayerController를 찾을 수 없습니다. Target을 수동으로 할당해주세요.");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, zOffset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 추적 대상을 변경한다.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 특정 위치로 카메라를 즉시 이동한다 (씬 시작 시 등).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y, zOffset);
    }
}
