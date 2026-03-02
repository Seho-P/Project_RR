using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Town에서 생성된 HUD를 씬 전환 간 유지하고, 로비 씬에서만 숨긴다.
/// </summary>
public class HUDRootPersistence : MonoBehaviour
{
    private static HUDRootPersistence instance;

    [Header("HUD Visible Control")]
    [SerializeField] private GameObject hudVisualRoot;
    [SerializeField] private string lobbySceneName = "Lobby";

    [Header("Player Search")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float searchTimeout = 3f;

    private Canvas[] cachedCanvases;
    private GraphicRaycaster[] cachedRaycasters;

    // 실행 순서 보장을 위해 코루틴 사용
    private Coroutine refreshRoutine;

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

        if (hudVisualRoot == null)
        {
            hudVisualRoot = gameObject;
        }

        CacheVisualComponents();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyVisibilityForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVisibilityForScene(scene.name);

        if (scene.name == lobbySceneName)
        {
            UnbindHud();
            return;
        }

        StartRefreshRoutine();
    }

    private void ApplyVisibilityForScene(string sceneName)
    {
        bool isLobby = sceneName == lobbySceneName;
        SetHudVisible(!isLobby);
    }

    private void SetHudVisible(bool visible)
    {
        if (hudVisualRoot != null && hudVisualRoot != gameObject)
        {
            hudVisualRoot.SetActive(visible);
            return;
        }

        for (int i = 0; i < cachedCanvases.Length; i++)
        {
            cachedCanvases[i].enabled = visible;
        }

        for (int i = 0; i < cachedRaycasters.Length; i++)
        {
            cachedRaycasters[i].enabled = visible;
        }
    }

    private void StartRefreshRoutine()
    {
        if (refreshRoutine != null)
        {
            StopCoroutine(refreshRoutine);
        }
        // 플레이어 체력을 찾아 HUD를 바인딩하는 루틴 시작 
        refreshRoutine = StartCoroutine(RefreshHudBindingWhenReady());
    }

    private IEnumerator RefreshHudBindingWhenReady()
    {
        float elapsed = 0f;

        while (elapsed < searchTimeout)
        {
            Health health = FindPlayerHealth();
            if (health != null)
            {
                BindHud(health);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private Health FindPlayerHealth()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health != null)
            {
                return health;
            }

            return player.GetComponentInChildren<Health>();
        }

        if (!string.IsNullOrWhiteSpace(playerTag))
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
            if (taggedPlayer != null)
            {
                Health health = taggedPlayer.GetComponent<Health>();
                if (health != null)
                {
                    return health;
                }

                return taggedPlayer.GetComponentInChildren<Health>();
            }
        }

        return null;
    }

    private void BindHud(Health playerHealth)
    {
        PlayerHealthUI playerHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        if (playerHealthUI != null)
        {
            playerHealthUI.Bind(playerHealth);
        }

        GameOverUI gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.Bind(playerHealth);
        }
    }

    private void UnbindHud()
    {
        PlayerHealthUI playerHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        if (playerHealthUI != null)
        {
            playerHealthUI.Unbind();
        }

        GameOverUI gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.Unbind();
        }
    }

    private void CacheVisualComponents()
    {
        if (hudVisualRoot == null)
        {
            cachedCanvases = new Canvas[0];
            cachedRaycasters = new GraphicRaycaster[0];
            return;
        }

        cachedCanvases = hudVisualRoot.GetComponentsInChildren<Canvas>(true);
        cachedRaycasters = hudVisualRoot.GetComponentsInChildren<GraphicRaycaster>(true);
    }
}
