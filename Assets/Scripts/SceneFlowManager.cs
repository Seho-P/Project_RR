using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 + 런타임 플레이어 생성/스폰을 담당한다.
/// 플레이어 데이터는 PlayerRuntimeDataManager가 유지하고,
/// 플레이어 오브젝트는 씬 진입 시 새로 생성한다.
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    private static SceneFlowManager instance;
    public static SceneFlowManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneFlowManager");
                instance = go.AddComponent<SceneFlowManager>();
                DontDestroyOnLoad(go);
            }

            return instance;
        }
    }

    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "Start";
    [SerializeField] private string townSceneName = "Town";
    [SerializeField] private string dungeonSceneName = "Dungeon";

    [Header("Spawn IDs")]
    [SerializeField] private string defaultTownSpawnId = "Town_Default";
    [SerializeField] private string returnToTownSpawnId = "Town_FromDungeon";
    [SerializeField] private string dungeonEntrySpawnId = "Dungeon_Entry";

    private string pendingSpawnId;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private IEnumerator Start()
    {
        // 초기 진입 씬에서도 플레이어가 필요하면 런타임 생성
        yield return null;
        Scene activeScene = SceneManager.GetActiveScene();
        if (ShouldSpawnPlayerInScene(activeScene.name) && FindFirstObjectByType<PlayerController>() == null)
        {
            StartCoroutine(SpawnPlayerWhenReady(activeScene));
        }
    }

    public void StartGame()
    {
        CaptureAndLoadScene(townSceneName, defaultTownSpawnId);
    }

    public void GoToStartScreen()
    {
        CaptureAndLoadScene(startSceneName, string.Empty);
    }

    public void GoToDungeon()
    {
        CaptureAndLoadScene(dungeonSceneName, dungeonEntrySpawnId);
    }

    public void ReturnToTown()
    {
        CaptureAndLoadScene(townSceneName, returnToTownSpawnId);
    }

    public void LoadScene(string sceneName, string spawnId = "")
    {
        string targetSpawnId = string.IsNullOrWhiteSpace(spawnId) ? GetDefaultSpawnId(sceneName) : spawnId;
        CaptureAndLoadScene(sceneName, targetSpawnId);
    }

    private void CaptureAndLoadScene(string sceneName, string spawnId)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[SceneFlowManager] sceneName이 비어 있습니다.");
            return;
        }

        pendingSpawnId = spawnId;
        PlayerRuntimeDataManager.Instance.CaptureCurrentPlayerData();
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ShouldSpawnPlayerInScene(scene.name))
        {
            pendingSpawnId = string.Empty;
            return;
        }

        StartCoroutine(SpawnPlayerWhenReady(scene));
    }

    private IEnumerator SpawnPlayerWhenReady(Scene scene)
    {
        // 씬의 Start 단계가 한 번 돌도록 한 프레임 대기
        yield return null;

        if (FindFirstObjectByType<PlayerController>() != null)
        {
            yield break;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[SceneFlowManager] playerPrefab이 할당되지 않았습니다.");
            yield break;
        }

        Vector3 spawnPosition = Vector3.zero;
        bool hasSpawn = false;

        if (scene.name == dungeonSceneName)
        {
            // 던전 생성 직후 좌표를 쓰기 위해 잠시 대기
            DungeonGenerator dungeonGenerator = FindFirstObjectByType<DungeonGenerator>();
            if (dungeonGenerator != null)
            {
                int guard = 0;
                while (dungeonGenerator.RoomMap.Count == 0 && guard < 30)
                {
                    guard++;
                    yield return null;
                }

                spawnPosition = dungeonGenerator.GetStartRoomWorldCenter();
                hasSpawn = true;
            }
        }

        if (!hasSpawn)
        {
            if (TryGetSpawnPoint(scene.name, pendingSpawnId, out PlayerSpawnPoint spawnPoint))
            {
                spawnPosition = spawnPoint.transform.position;
                hasSpawn = true;
            }
        }

        if (!hasSpawn)
        {
            Debug.LogWarning($"[SceneFlowManager] 스폰 포인트를 찾지 못해 원점에 스폰합니다. scene={scene.name}, spawnId={pendingSpawnId}");
        }

        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        pendingSpawnId = string.Empty;
    }

    private bool TryGetSpawnPoint(string sceneName, string preferredSpawnId, out PlayerSpawnPoint point)
    {
        PlayerSpawnPoint[] points = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);

        if (!string.IsNullOrWhiteSpace(preferredSpawnId))
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].SpawnId == preferredSpawnId)
                {
                    point = points[i];
                    return true;
                }
            }
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].IsDefault)
            {
                point = points[i];
                return true;
            }
        }

        if (points.Length > 0)
        {
            point = points[0];
            return true;
        }

        point = null;
        return false;
    }

    private string GetDefaultSpawnId(string sceneName)
    {
        if (sceneName == townSceneName)
        {
            return defaultTownSpawnId;
        }

        if (sceneName == dungeonSceneName)
        {
            return dungeonEntrySpawnId;
        }

        return string.Empty;
    }

    private bool ShouldSpawnPlayerInScene(string sceneName)
    {
        return sceneName != startSceneName;
    }
}
