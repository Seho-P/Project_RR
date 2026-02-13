using UnityEngine;

/// <summary>
/// 개별 방의 문(Door)을 관리하는 컨트롤러.
/// 방 프리팹의 루트 오브젝트에 부착한다.
/// 
/// [프리팹 구조 예시]
/// Room_Normal (RoomController)
/// ├── Grid
/// │   ├── Ground (Tilemap)
/// │   └── Wall (Tilemap) - 4방향 문 위치에 타일 갭 필요
/// ├── Doors
/// │   ├── DoorTop (Door 컴포넌트)
/// │   │   └── Blocker (BoxCollider2D + SpriteRenderer)
/// │   ├── DoorBottom (Door 컴포넌트)
/// │   │   └── Blocker
/// │   ├── DoorLeft (Door 컴포넌트)
/// │   │   └── Blocker
/// │   └── DoorRight (Door 컴포넌트)
/// │       └── Blocker
/// └── EnemySpawnPoints
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("=== 문 (Door) 참조 ===")]
    [Tooltip("윗쪽 문 (위 방으로 연결)")]
    [SerializeField] private Door doorTop;
    [Tooltip("아랫쪽 문 (아래 방으로 연결)")]
    [SerializeField] private Door doorBottom;
    [Tooltip("왼쪽 문 (왼쪽 방으로 연결)")]
    [SerializeField] private Door doorLeft;
    [Tooltip("오른쪽 문 (오른쪽 방으로 연결)")]
    [SerializeField] private Door doorRight;

    /// <summary>
    /// 이 방에 대응하는 그래프 데이터
    /// </summary>
    public RoomData Data { get; private set; }

    /// <summary>
    /// DungeonGenerator가 방 생성 시 호출. 그래프 데이터를 연결한다.
    /// </summary>
    public void Initialize(RoomData data)
    {
        Data = data;
    }

    /// <summary>
    /// 연결 정보에 따라 문을 열거나 닫는다.
    /// 연결된 방향 → 문 열림 (블로커 비활성화, 통행 가능)
    /// 연결 안 된 방향 → 문 닫힘 (블로커 활성화, 벽처럼 막힘)
    /// </summary>
    public void SetupDoors(RoomData data)
    {
        SetDoor(doorTop, Vector2Int.up, data);
        SetDoor(doorBottom, Vector2Int.down, data);
        SetDoor(doorLeft, Vector2Int.left, data);
        SetDoor(doorRight, Vector2Int.right, data);
    }

    private void SetDoor(Door door, Vector2Int direction, RoomData data)
    {
        if (door == null) return;

        bool isConnected = data.HasConnectionInDirection(direction);
        door.SetOpen(isConnected);
    }

    /// <summary>
    /// 모든 문을 닫는다 (전투 중 잠금 등에 사용).
    /// </summary>
    public void LockAllDoors()
    {
        if (doorTop != null) doorTop.SetOpen(false);
        if (doorBottom != null) doorBottom.SetOpen(false);
        if (doorLeft != null) doorLeft.SetOpen(false);
        if (doorRight != null) doorRight.SetOpen(false);
    }

    /// <summary>
    /// 연결 정보에 맞게 문을 다시 연다 (전투 클리어 후 등에 사용).
    /// </summary>
    public void UnlockDoors()
    {
        if (Data != null)
            SetupDoors(Data);
    }
}
