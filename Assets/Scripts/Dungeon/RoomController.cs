using UnityEngine;
using System;
using System.Collections.Generic;

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
    public enum RoomCombatState
    {
        Unvisited,
        CombatActive,
        Cleared
    }

    [Header("=== 문 (Door) 참조 ===")]
    [Tooltip("윗쪽 문 (위 방으로 연결)")]
    [SerializeField] private Door doorTop;
    [Tooltip("아랫쪽 문 (아래 방으로 연결)")]
    [SerializeField] private Door doorBottom;
    [Tooltip("왼쪽 문 (왼쪽 방으로 연결)")]
    [SerializeField] private Door doorLeft;
    [Tooltip("오른쪽 문 (오른쪽 방으로 연결)")]
    [SerializeField] private Door doorRight;

    [Header("=== 방 진입/전투 ===")]
    [Tooltip("플레이어 방 진입을 감지하는 트리거")]
    [SerializeField] private RoomEnterTrigger roomEnterTrigger;
    [Tooltip("적 스폰 포인트들의 부모 오브젝트(자식들을 포인트로 사용)")]
    [SerializeField] private Transform enemySpawnPointsRoot;
    [Tooltip("방 진입 시 스폰할 적 프리팹 목록")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [Tooltip("보스방 전투 클리어 시 활성화할 포탈 오브젝트")]
    [SerializeField] private GameObject bossClearPortal;

    /// <summary>
    /// 이 방에 대응하는 그래프 데이터
    /// </summary>
    public RoomData Data { get; private set; }
    public RoomCombatState CombatState { get; private set; } = RoomCombatState.Unvisited;

    private readonly List<Health> aliveEnemies = new List<Health>();
    private readonly Dictionary<Health, Action> deathHandlers = new Dictionary<Health, Action>();

    private void Awake()
    {
        if (roomEnterTrigger == null)
        {
            roomEnterTrigger = GetComponentInChildren<RoomEnterTrigger>(true);
        }
    }

    /// <summary>
    /// 방 진입 이벤트 구독을 시작한다.
    /// </summary>
    private void OnEnable()
    {
        if (roomEnterTrigger != null)
        {
            roomEnterTrigger.PlayerEntered += HandlePlayerEntered;
        }
    }

    /// <summary>
    /// 이벤트 구독을 해제하고 전투 이벤트 핸들러를 정리한다.
    /// </summary>
    private void OnDisable()
    {
        if (roomEnterTrigger != null)
        {
            roomEnterTrigger.PlayerEntered -= HandlePlayerEntered;
        }

        CleanupEnemySubscriptions();
    }

    /// <summary>
    /// DungeonGenerator가 방 생성 시 호출. 그래프 데이터를 연결한다.
    /// </summary>
    public void Initialize(RoomData data)
    {
        Data = data;
        CombatState = CanStartCombat() ? RoomCombatState.Unvisited : RoomCombatState.Cleared;

        if (bossClearPortal != null)
        {
            bossClearPortal.SetActive(false);
        }
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

    /// <summary>
    /// 특정 방향의 문을 연결 여부에 맞춰 연다/닫는다.
    /// </summary>
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

    /// <summary>
    /// 플레이어가 방 진입 트리거를 통과했을 때 전투 시작을 시도한다.
    /// </summary>
    private void HandlePlayerEntered(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        TryStartCombat();
    }

    /// <summary>
    /// 현재 상태가 미방문일 때만 방 전투를 시작한다.
    /// </summary>
    private void TryStartCombat()
    {
        if (CombatState != RoomCombatState.Unvisited)
        {
            return;
        }

        if (!CanStartCombat())
        {
            CombatState = RoomCombatState.Cleared;
            return;
        }

        SpawnEnemies();

        if (aliveEnemies.Count == 0)
        {
            CombatState = RoomCombatState.Cleared;
            return;
        }

        CombatState = RoomCombatState.CombatActive;
        LockAllDoors();
    }

    /// <summary>
    /// 이 방에서 전투를 시작할 수 있는지 조건을 검사한다.
    /// </summary>
    private bool CanStartCombat()
    {
        if (Data == null)
        {
            return false;
        }

        // 시작 방은 입장 직후 전투를 열지 않는다.
        if (Data.RoomType == RoomType.Start)
        {
            return false;
        }

        if (enemySpawnPointsRoot == null || enemySpawnPointsRoot.childCount == 0)
        {
            return false;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 스폰 포인트마다 적을 생성하고 사망 이벤트를 등록한다.
    /// </summary>
    private void SpawnEnemies()
    {
        CleanupEnemySubscriptions();
        aliveEnemies.Clear();

        int spawnPointCount = enemySpawnPointsRoot.childCount;
        for (int i = 0; i < spawnPointCount; i++)
        {
            Transform spawnPoint = enemySpawnPointsRoot.GetChild(i);
            GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            if (enemyPrefab == null)
            {
                continue;
            }

            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, transform);
            Health enemyHealth = enemyObject.GetComponent<Health>();
            if (enemyHealth == null)
            {
                enemyHealth = enemyObject.GetComponentInChildren<Health>();
            }

            if (enemyHealth == null)
            {
                Debug.LogWarning($"[RoomController] {enemyObject.name}에서 Health를 찾지 못해 클리어 카운트에서 제외합니다.");
                continue;
            }

            aliveEnemies.Add(enemyHealth);

            Action onDeath = null;
            onDeath = () => HandleEnemyDeath(enemyHealth);
            deathHandlers[enemyHealth] = onDeath;
            enemyHealth.OnDeath += onDeath;
        }
    }

    /// <summary>
    /// 적 사망을 반영하고 전투 종료 조건을 확인한다.
    /// </summary>
    private void HandleEnemyDeath(Health enemyHealth)
    {
        if (enemyHealth == null)
        {
            return;
        }

        if (deathHandlers.TryGetValue(enemyHealth, out Action onDeath))
        {
            enemyHealth.OnDeath -= onDeath;
            deathHandlers.Remove(enemyHealth);
        }

        aliveEnemies.Remove(enemyHealth);

        if (CombatState == RoomCombatState.CombatActive && aliveEnemies.Count == 0)
        {
            CombatState = RoomCombatState.Cleared;
            UnlockDoors();
            ActivateBossClearPortalIfNeeded();
        }
    }

    /// <summary>
    /// 보스방 전투가 종료된 경우에만 클리어 포탈을 활성화한다.
    /// </summary>
    private void ActivateBossClearPortalIfNeeded()
    {
        if (Data == null || Data.RoomType != RoomType.Boss)
        {
            return;
        }

        if (bossClearPortal != null)
        {
            bossClearPortal.SetActive(true);
        }
    }

    /// <summary>
    /// 등록된 적 사망 이벤트 구독을 모두 해제한다.
    /// </summary>
    private void CleanupEnemySubscriptions()
    {
        foreach (var pair in deathHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.OnDeath -= pair.Value;
            }
        }

        deathHandlers.Clear();
        aliveEnemies.Clear();
    }
}
