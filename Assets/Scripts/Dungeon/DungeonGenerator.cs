using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 던전 생성기: Random Walk 알고리즘으로 방 그래프를 생성하고,
/// 방 타입을 배정한 뒤, 프리팹을 월드에 배치한다.
/// 
/// [배치 좌표 체계]
/// - 그리드 좌표 (0,0)이 시작 방
/// - 월드 좌표 = 그리드 좌표 × 방 크기(22, 16)
/// - 방 프리팹의 Grid 원점이 (0,0)이고 타일이 (0,0)~(21,15)에 배치된 경우,
///   방의 시각적 중심은 프리팹 위치 + (11, 8)이 된다.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("=== 방 프리팹 ===")]
    [Tooltip("일반 전투 방 프리팹 (시작 방으로도 사용)")]
    [SerializeField] private GameObject normalRoomPrefab;
    [Tooltip("보물 방 프리팹")]
    [SerializeField] private GameObject treasureRoomPrefab;
    [Tooltip("보스 방 프리팹")]
    [SerializeField] private GameObject bossRoomPrefab;

    [Header("=== 생성 설정 ===")]
    [Tooltip("생성할 총 방 개수 (시작 방 포함)")]
    [SerializeField] private int totalRooms = 10;
    [Tooltip("보물 방 최대 개수")]
    [SerializeField] private int maxTreasureRooms = 2;
    [Tooltip("시드 값 (-1이면 매번 랜덤)")]
    [SerializeField] private int seed = -1;
    [Tooltip("Random Walk 중 기존 방으로 점프할 확률 (분기 생성용)")]
    [Range(0f, 1f)]
    [SerializeField] private float branchProbability = 0.3f;

    [Header("=== 방 레이아웃 ===")]
    [Tooltip("방 프리팹의 타일 크기 (가로 × 세로)")]
    [SerializeField] private Vector2 roomSize = new Vector2(22f, 16f);
    [Tooltip("프리팹 원점(0,0)에서 방 중심까지의 오프셋\n(타일이 (0,0)~(21,15)에 배치된 경우 (11, 8))")]
    [SerializeField] private Vector2 roomCenterOffset = new Vector2(11f, 8f);

    [Header("=== 플레이어 ===")]
    [Tooltip("플레이어 Transform (시작 방 중심에 자동 배치)")]
    [SerializeField] private Transform playerTransform;

    // 4방향 상수
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,    // (0, 1)
        Vector2Int.down,  // (0, -1)
        Vector2Int.left,  // (-1, 0)
        Vector2Int.right  // (1, 0)
    };

    // 생성된 데이터
    private Dictionary<Vector2Int, RoomData> roomMap = new Dictionary<Vector2Int, RoomData>();
    private Dictionary<Vector2Int, RoomController> roomControllers = new Dictionary<Vector2Int, RoomController>();

    // 외부 접근용 프로퍼티
    public Dictionary<Vector2Int, RoomData> RoomMap => roomMap;
    public Vector2 RoomSize => roomSize;
    public Vector2 RoomCenterOffset => roomCenterOffset;

    private void Start()
    {
        GenerateDungeon();
    }

    /// <summary>
    /// 던전 생성 전체 파이프라인 실행
    /// </summary>
    public void GenerateDungeon()
    {
        ClearDungeon();

        // 시드 설정
        if (seed >= 0)
            Random.InitState(seed);
        else
            Random.InitState(System.Environment.TickCount);

        // 1단계: Random Walk로 그래프 생성
        GenerateGraph();

        // 2단계: 방 타입 배정
        AssignRoomTypes();

        // 3단계: 프리팹 인스턴스 생성 및 배치
        InstantiateRooms();

        // 4단계: 연결 정보에 따라 문 설정
        SetupAllDoors();

        // 5단계: 플레이어 시작 위치 설정
        PositionPlayer();

        Debug.Log($"[DungeonGenerator] 던전 생성 완료! 총 {roomMap.Count}개 방");
    }

    // =====================================================================
    //  1단계: Random Walk 그래프 생성
    // =====================================================================
    #region Graph Generation

    private void GenerateGraph()
    {
        List<Vector2Int> roomPositions = new List<Vector2Int>();
        HashSet<Vector2Int> positionSet = new HashSet<Vector2Int>();

        // 시작점
        Vector2Int current = Vector2Int.zero;
        roomPositions.Add(current);
        positionSet.Add(current);

        int maxAttempts = totalRooms * 20;
        int attempts = 0;

        while (roomPositions.Count < totalRooms && attempts < maxAttempts)
        {
            attempts++;

            // 랜덤 방향으로 한 칸 이동
            Vector2Int direction = Directions[Random.Range(0, Directions.Length)];
            Vector2Int next = current + direction;

            // 새로운 위치면 방 추가
            if (!positionSet.Contains(next))
            {
                roomPositions.Add(next);
                positionSet.Add(next);
            }

            // 이동 (이미 방문한 곳이어도 이동)
            current = next;

            // 일정 확률로 기존 방에서 새 분기 시작 → 가지형 구조 생성
            if (Random.value < branchProbability)
            {
                current = roomPositions[Random.Range(0, roomPositions.Count)];
            }
        }

        // RoomData 생성
        foreach (var pos in roomPositions)
        {
            roomMap[pos] = new RoomData(pos);
        }

        // 인접 연결: 그리드에서 이웃한 방끼리 자동 연결
        foreach (var kvp in roomMap)
        {
            foreach (var dir in Directions)
            {
                Vector2Int neighbor = kvp.Key + dir;
                if (roomMap.ContainsKey(neighbor))
                {
                    kvp.Value.AddConnection(neighbor);
                }
            }
        }

        // BFS로 시작점에서의 거리 계산
        CalculateDistances();
    }

    /// <summary>
    /// BFS를 사용하여 시작점(0,0)에서 각 방까지의 최단 거리를 계산한다.
    /// </summary>
    private void CalculateDistances()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(Vector2Int.zero);
        visited.Add(Vector2Int.zero);
        roomMap[Vector2Int.zero].DistanceFromStart = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            RoomData currentRoom = roomMap[current];

            foreach (var neighborPos in currentRoom.ConnectedPositions)
            {
                if (!visited.Contains(neighborPos))
                {
                    visited.Add(neighborPos);
                    roomMap[neighborPos].DistanceFromStart = currentRoom.DistanceFromStart + 1;
                    queue.Enqueue(neighborPos);
                }
            }
        }
    }

    #endregion

    // =====================================================================
    //  2단계: 방 타입 배정
    // =====================================================================
    #region Room Type Assignment

    private void AssignRoomTypes()
    {
        // ① 시작 방
        roomMap[Vector2Int.zero].RoomType = RoomType.Start;

        // ② 보스 방: 시작점에서 가장 먼 방
        var bossEntry = roomMap
            .Where(kvp => kvp.Key != Vector2Int.zero)
            .OrderByDescending(kvp => kvp.Value.DistanceFromStart)
            .First();
        bossEntry.Value.RoomType = RoomType.Boss;

        // ③ 보물 방: 막다른 길 중 시작/보스 제외, 거리 먼 순 우선
        var treasureCandidates = roomMap
            .Where(kvp => kvp.Value.IsDeadEnd
                          && kvp.Key != Vector2Int.zero
                          && kvp.Key != bossEntry.Key)
            .OrderByDescending(kvp => kvp.Value.DistanceFromStart)
            .Take(maxTreasureRooms)
            .ToList();

        // 막다른 길이 부족하면 연결 수가 적은 방으로 대체
        if (treasureCandidates.Count < maxTreasureRooms)
        {
            int remaining = maxTreasureRooms - treasureCandidates.Count;
            var alreadyAssigned = new HashSet<Vector2Int>(treasureCandidates.Select(kvp => kvp.Key));

            var additional = roomMap
                .Where(kvp => kvp.Key != Vector2Int.zero
                              && kvp.Key != bossEntry.Key
                              && !alreadyAssigned.Contains(kvp.Key)
                              && kvp.Value.RoomType == RoomType.Normal)
                .OrderBy(kvp => kvp.Value.ConnectedPositions.Count)
                .ThenByDescending(kvp => kvp.Value.DistanceFromStart)
                .Take(remaining);

            treasureCandidates.AddRange(additional);
        }

        foreach (var candidate in treasureCandidates)
        {
            candidate.Value.RoomType = RoomType.Treasure;
        }

        // ④ 나머지는 Normal (RoomData 생성 시 기본값)

        // 디버그 로그
        foreach (var kvp in roomMap)
        {
            Debug.Log($"[Dungeon] ({kvp.Key.x},{kvp.Key.y}) " +
                      $"Type={kvp.Value.RoomType} " +
                      $"Dist={kvp.Value.DistanceFromStart} " +
                      $"Connections={kvp.Value.ConnectedPositions.Count}");
        }
    }

    #endregion

    // =====================================================================
    //  3단계: 프리팹 인스턴스 생성
    // =====================================================================
    #region Room Instantiation

    private void InstantiateRooms()
    {
        foreach (var kvp in roomMap)
        {
            Vector2Int gridPos = kvp.Key;
            RoomData data = kvp.Value;

            GameObject prefab = GetPrefab(data.RoomType);
            if (prefab == null)
            {
                Debug.LogError($"[DungeonGenerator] {data.RoomType} 프리팹이 할당되지 않았습니다!");
                continue;
            }

            Vector3 worldPos = GridToWorldPosition(gridPos);
            GameObject roomObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            roomObj.name = $"Room_{data.RoomType}_{gridPos.x}_{gridPos.y}";

            // RoomController 확인/추가
            RoomController controller = roomObj.GetComponent<RoomController>();
            if (controller == null)
            {
                Debug.LogWarning($"[DungeonGenerator] {roomObj.name}에 RoomController가 없어 자동 추가합니다.");
                controller = roomObj.AddComponent<RoomController>();
            }

            controller.Initialize(data);
            roomControllers[gridPos] = controller;
        }
    }

    /// <summary>
    /// 모든 방의 문을 연결 정보에 맞게 설정한다.
    /// </summary>
    private void SetupAllDoors()
    {
        foreach (var kvp in roomControllers)
        {
            kvp.Value.SetupDoors(roomMap[kvp.Key]);
        }
    }

    private GameObject GetPrefab(RoomType type)
    {
        return type switch
        {
            RoomType.Start => normalRoomPrefab,  // 시작 방은 일반방 프리팹 사용
            RoomType.Normal => normalRoomPrefab,
            RoomType.Treasure => treasureRoomPrefab,
            RoomType.Boss => bossRoomPrefab,
            _ => normalRoomPrefab
        };
    }

    #endregion

    // =====================================================================
    //  유틸리티
    // =====================================================================
    #region Utility

    /// <summary>
    /// 플레이어를 시작 방 중심에 배치한다.
    /// </summary>
    private void PositionPlayer()
    {
        if (playerTransform == null)
        {
            // 씬에서 PlayerController 자동 탐색
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
                playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            playerTransform.position = GetStartRoomWorldCenter();
            Debug.Log($"[DungeonGenerator] 플레이어 시작 위치: {playerTransform.position}");
        }
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표(프리팹 배치 좌표)로 변환한다.
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * roomSize.x, gridPos.y * roomSize.y, 0f);
    }

    /// <summary>
    /// 시작 방의 월드 중심 좌표를 반환한다.
    /// (프리팹 배치 좌표 + roomCenterOffset)
    /// </summary>
    public Vector3 GetStartRoomWorldCenter()
    {
        Vector3 prefabPos = GridToWorldPosition(Vector2Int.zero);
        return prefabPos + new Vector3(roomCenterOffset.x, roomCenterOffset.y, 0f);
    }

    /// <summary>
    /// 특정 그리드 좌표의 방 중심 월드 좌표를 반환한다.
    /// </summary>
    public Vector3 GetRoomWorldCenter(Vector2Int gridPos)
    {
        Vector3 prefabPos = GridToWorldPosition(gridPos);
        return prefabPos + new Vector3(roomCenterOffset.x, roomCenterOffset.y, 0f);
    }

    public RoomController GetRoomController(Vector2Int gridPos)
    {
        roomControllers.TryGetValue(gridPos, out var controller);
        return controller;
    }

    /// <summary>
    /// 생성된 던전을 모두 제거한다.
    /// </summary>
    public void ClearDungeon()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        roomMap.Clear();
        roomControllers.Clear();
    }

    #endregion
}
