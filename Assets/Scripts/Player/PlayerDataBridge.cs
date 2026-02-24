using UnityEngine;

/// <summary>
/// 씬에서 생성된 플레이어 컴포넌트를 런타임 데이터 매니저에 연결한다.
/// </summary>
public class PlayerDataBridge : MonoBehaviour
{
    private PlayerStats playerStats;
    private Health health;
    private Inventory inventory;
    private PlayerLevelSystem levelSystem;
    private SetEffectManager setEffectManager;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>() ?? GetComponentInParent<PlayerStats>();
        health = GetComponent<Health>() ?? GetComponentInParent<Health>();
        inventory = GetComponent<Inventory>() ?? GetComponentInParent<Inventory>();
        levelSystem = GetComponent<PlayerLevelSystem>() ?? GetComponentInParent<PlayerLevelSystem>();
        setEffectManager = GetComponent<SetEffectManager>() ?? GetComponentInParent<SetEffectManager>();
    }

    private void Start()
    {
        RegisterAndApply();
        BindItemSystems();
    }

    private void OnDestroy()
    {
        if (PlayerRuntimeDataManager.IsQuitting)
        {
            return;
        }

        // 씬 언로드 시점에 현재 상태를 저장해 다음 씬 플레이어가 이어받도록 한다.
        PlayerRuntimeDataManager.Instance.CaptureCurrentPlayerData();
    }

    private void RegisterAndApply()
    {
        if (playerStats == null || health == null || inventory == null || levelSystem == null)
        {
            Debug.LogWarning("[PlayerDataBridge] 플레이어 필수 컴포넌트가 없어 데이터 연동을 건너뜁니다.");
            return;
        }

        var runtimeData = PlayerRuntimeDataManager.Instance;
        runtimeData.RegisterCurrentPlayer(playerStats, health, inventory, levelSystem);
        runtimeData.ApplySavedDataToCurrentPlayer();
    }

    private void BindItemSystems()
    {
        if (playerStats == null)
        {
            return;
        }

        ItemManager itemManager = FindFirstObjectByType<ItemManager>();
        if (itemManager != null)
        {
            itemManager.BindPlayerStats(playerStats, setEffectManager);
        }
    }
}
