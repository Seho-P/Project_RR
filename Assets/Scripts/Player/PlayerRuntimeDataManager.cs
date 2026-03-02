using System.Collections.Generic;
using UnityEngine;
using Items.Enums;

/// <summary>
/// 씬이 바뀌어도 플레이어 데이터(스탯/체력/레벨/인벤토리)를 유지한다.
/// 플레이어 오브젝트는 씬마다 새로 생성하고, 데이터만 이 매니저가 보관한다.
/// </summary>
public class PlayerRuntimeDataManager : MonoBehaviour
{
    private static PlayerRuntimeDataManager instance;
    public static bool IsQuitting { get; private set; }
    public static PlayerRuntimeDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PlayerRuntimeDataManager");
                instance = go.AddComponent<PlayerRuntimeDataManager>();
                DontDestroyOnLoad(go);
            }

            return instance;
        }
    }

    private PlayerStats currentPlayerStats;
    private Health currentHealth;
    private Inventory currentInventory;
    private PlayerLevelSystem currentLevelSystem;

    private bool hasSavedData;
    private bool ignoreCaptureUntilNextRegister;
    private readonly Dictionary<StatType, float> savedBaseStats = new Dictionary<StatType, float>();
    private readonly List<ItemInstance> savedInventorySlots = new List<ItemInstance>();
    private int savedLevel = 1;
    private int savedXP;
    private float savedCurrentHealth = -1f;

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
        }
    }

    private void OnApplicationQuit()
    {
        IsQuitting = true;
    }

    /// <summary>
    /// 씬에서 새로 생성된 플레이어 컴포넌트를 현재 런타임 참조로 등록한다.
    /// </summary>
    public void RegisterCurrentPlayer(
        PlayerStats playerStats,
        Health health,
        Inventory inventory,
        PlayerLevelSystem levelSystem)
    {
        currentPlayerStats = playerStats;
        currentHealth = health;
        currentInventory = inventory;
        currentLevelSystem = levelSystem;
        ignoreCaptureUntilNextRegister = false;
    }

    /// <summary>
    /// 현재 플레이어 상태를 다음 씬 적용용 저장 데이터로 캡처한다.
    /// </summary>
    public void CaptureCurrentPlayerData()
    {
        if (ignoreCaptureUntilNextRegister)
        {
            return;
        }

        if (currentPlayerStats == null || currentHealth == null || currentInventory == null || currentLevelSystem == null)
        {
            return;
        }

        // 1) 기본 스탯 저장
        savedBaseStats.Clear();
        foreach (var kvp in currentPlayerStats.GetBaseStatsSnapshot())
        {
            savedBaseStats[kvp.Key] = kvp.Value;
        }

        // 2) 레벨/경험치 저장
        savedLevel = currentLevelSystem.CurrentLevel;
        savedXP = currentLevelSystem.CurrentXP;

        // 3) 체력 저장
        savedCurrentHealth = currentHealth.CurrentHealth;

        // 4) 인벤토리 슬롯 상태 저장
        savedInventorySlots.Clear();
        var snapshot = currentInventory.GetSlotItemsSnapshot();
        for (int i = 0; i < snapshot.Count; i++)
        {
            savedInventorySlots.Add(snapshot[i]);
        }

        hasSavedData = true;
    }

    /// <summary>
    /// 저장 데이터가 필요 없는 씬 전환(예: 마을 복귀)을 위해 저장본을 비우고,
    /// 다음 플레이어 등록 전까지 캡처를 무시하도록 설정한다.
    /// </summary>
    public void PrepareForFreshSpawnTransition()
    {
        hasSavedData = false;
        savedBaseStats.Clear();
        savedInventorySlots.Clear();
        savedLevel = 1;
        savedXP = 0;
        savedCurrentHealth = -1f;
        ignoreCaptureUntilNextRegister = true;
    }

    /// <summary>
    /// 저장된 플레이어 데이터를 현재 씬의 플레이어에 적용한다.
    /// </summary>
    public void ApplySavedDataToCurrentPlayer()
    {
        if (!hasSavedData)
        {
            return;
        }

        if (currentPlayerStats == null || currentHealth == null || currentInventory == null || currentLevelSystem == null)
        {
            return;
        }

        currentPlayerStats.LoadBaseStatsSnapshot(savedBaseStats);
        currentLevelSystem.SetLevel(savedLevel, savedXP);
        currentHealth.SetCurrentHealthFromData(savedCurrentHealth);
        currentInventory.LoadSlotItemsSnapshot(savedInventorySlots);
    }
}
