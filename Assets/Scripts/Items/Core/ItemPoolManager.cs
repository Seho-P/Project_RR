using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Items.Data;
using Items.Enums;

/// <summary>
/// 아이템 풀 매니저 - 랜덤 아이템 뽑기, 필터링, 확률 계산 담당
/// 싱글톤 패턴으로 전역 접근 가능
/// </summary>
public class ItemPoolManager : MonoBehaviour
{
    private static ItemPoolManager instance;
    public static ItemPoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ItemPoolManager");
                instance = go.AddComponent<ItemPoolManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Database Settings")]
    [SerializeField] private string databasePath = "Item Database"; // Resources 폴더 내 경로
    [SerializeField] private bool loadOnAwake = true;

    private ItemDatabase database;
    private List<ItemData> cachedItems = new List<ItemData>();

    // 레어도별 가중치 (확률 조정용)
    [Header("Rarity Weights (Optional)")]
    [SerializeField] private RarityWeight[] rarityWeights;

    [System.Serializable]
    public class RarityWeight
    {
        public ItemRarity rarity;
        public float weight = 1f; // 가중치 (높을수록 더 자주 나옴)
    }

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

        if (loadOnAwake)
        {
            LoadDatabase();
        }
    }

    /// <summary>
    /// 데이터베이스 로드
    /// </summary>
    public void LoadDatabase()
    {
        database = Resources.Load<ItemDatabase>(databasePath);
        
        if (database == null)
        {
            Debug.LogError($"ItemPoolManager: 데이터베이스를 찾을 수 없습니다! 경로: {databasePath}");
            return;
        }

        // 캐시 업데이트
        RefreshCache();
        Debug.Log($"ItemPoolManager: 데이터베이스 로드 완료. 총 {cachedItems.Count}개 아이템");
    }

    /// <summary>
    /// 캐시 새로고침
    /// </summary>
    public void RefreshCache()
    {
        if (database == null)
        {
            Debug.LogWarning("ItemPoolManager: 데이터베이스가 로드되지 않았습니다.");
            return;
        }

        cachedItems = database.GetAllItems();
    }

    /// <summary>
    /// 랜덤 아이템 뽑기 (기본 - 레벨업 아이템만)
    /// </summary>
    /// <param name="count">뽑을 개수</param>
    /// <returns>랜덤으로 선택된 ItemData 리스트</returns>
    public List<ItemData> GetRandomItems(int count)
    {
        return GetRandomItems(count, useLevelUpItems: true, useNormalItems: false, useSetItems: false);
    }

    /// <summary>
    /// 랜덤 아이템 뽑기 (고급 - 필터링 옵션)
    /// </summary>
    /// <param name="count">뽑을 개수</param>
    /// <param name="useLevelUpItems">레벨업 아이템 포함 여부</param>
    /// <param name="useNormalItems">일반 아이템 포함 여부</param>
    /// <param name="useSetItems">세트 아이템 포함 여부</param>
    /// <param name="allowDuplicates">중복 허용 여부</param>
    /// <param name="rarityFilter">레어도 필터 (null이면 모든 레어도)</param>
    /// <param name="typeFilter">타입 필터 (null이면 모든 타입)</param>
    /// <returns>랜덤으로 선택된 ItemData 리스트</returns>
    public List<ItemData> GetRandomItems(
        int count,
        bool useLevelUpItems = true,
        bool useNormalItems = false,
        bool useSetItems = false,
        bool allowDuplicates = false,
        List<ItemRarity> rarityFilter = null,
        List<ItemType> typeFilter = null)
    {
        if (database == null)
        {
            Debug.LogError("ItemPoolManager: 데이터베이스가 로드되지 않았습니다. LoadDatabase()를 먼저 호출하세요.");
            return new List<ItemData>();
        }

        // 필터링된 아이템 목록 가져오기
        List<ItemData> availableItems = GetFilteredItems(useLevelUpItems, useNormalItems, useSetItems, rarityFilter, typeFilter);

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("ItemPoolManager: 사용 가능한 아이템이 없습니다.");
            return new List<ItemData>();
        }

        // 가중치 적용
        if (rarityWeights != null && rarityWeights.Length > 0)
        {
            availableItems = ApplyRarityWeights(availableItems);
        }

        // 랜덤 선택
        List<ItemData> selectedItems = new List<ItemData>();
        List<ItemData> pool = new List<ItemData>(availableItems);

        int attempts = 0;
        int maxAttempts = count * 10; // 무한 루프 방지

        while (selectedItems.Count < count && pool.Count > 0 && attempts < maxAttempts)
        {
            ItemData selected = SelectRandomItem(pool);
            
            if (selected != null)
            {
                selectedItems.Add(selected);
                
                // 중복 방지
                if (!allowDuplicates)
                {
                    pool.Remove(selected);
                }
            }
            
            attempts++;
        }

        return selectedItems;
    }

    /// <summary>
    /// 필터링된 아이템 목록 가져오기
    /// </summary>
    private List<ItemData> GetFilteredItems(
        bool useLevelUpItems,
        bool useNormalItems,
        bool useSetItems,
        List<ItemRarity> rarityFilter,
        List<ItemType> typeFilter)
    {
        List<ItemData> items = new List<ItemData>();

        if (useLevelUpItems && database.levelUpItems != null)
            items.AddRange(database.levelUpItems.Where(item => item != null));

        if (useNormalItems && database.normalItems != null)
            items.AddRange(database.normalItems.Where(item => item != null));

        if (useSetItems && database.setItems != null)
            items.AddRange(database.setItems.Where(item => item != null));

        // 레어도 필터
        if (rarityFilter != null && rarityFilter.Count > 0)
        {
            items = items.Where(item => rarityFilter.Contains(item.rarity)).ToList();
        }

        // 타입 필터
        if (typeFilter != null && typeFilter.Count > 0)
        {
            items = items.Where(item => typeFilter.Contains(item.itemType)).ToList();
        }

        return items;
    }

    /// <summary>
    /// 레어도 가중치 적용
    /// </summary>
    private List<ItemData> ApplyRarityWeights(List<ItemData> items)
    {
        if (rarityWeights == null || rarityWeights.Length == 0)
            return items;

        Dictionary<ItemRarity, float> weightDict = new Dictionary<ItemRarity, float>();
        foreach (var rw in rarityWeights)
        {
            weightDict[rw.rarity] = rw.weight;
        }

        // 가중치에 따라 아이템을 여러 번 추가
        List<ItemData> weightedItems = new List<ItemData>();
        foreach (var item in items)
        {
            float weight = weightDict.ContainsKey(item.rarity) ? weightDict[item.rarity] : 1f;
            int count = Mathf.Max(1, Mathf.RoundToInt(weight));
            
            for (int i = 0; i < count; i++)
            {
                weightedItems.Add(item);
            }
        }

        return weightedItems;
    }

    /// <summary>
    /// 풀에서 랜덤 아이템 선택
    /// </summary>
    private ItemData SelectRandomItem(List<ItemData> pool)
    {
        if (pool == null || pool.Count == 0)
            return null;

        int index = Random.Range(0, pool.Count);
        return pool[index];
    }

    /// <summary>
    /// 특정 레어도의 아이템만 뽑기
    /// </summary>
    public List<ItemData> GetRandomItemsByRarity(int count, ItemRarity rarity, bool allowDuplicates = false)
    {
        List<ItemRarity> rarityFilter = new List<ItemRarity> { rarity };
        return GetRandomItems(count, rarityFilter: rarityFilter, allowDuplicates: allowDuplicates);
    }

    /// <summary>
    /// 특정 타입의 아이템만 뽑기
    /// </summary>
    public List<ItemData> GetRandomItemsByType(int count, ItemType type, bool allowDuplicates = false)
    {
        List<ItemType> typeFilter = new List<ItemType> { type };
        return GetRandomItems(count, typeFilter: typeFilter, allowDuplicates: allowDuplicates);
    }

    /// <summary>
    /// 데이터베이스 직접 설정 (에디터에서 사용)
    /// </summary>
    public void SetDatabase(ItemDatabase db)
    {
        database = db;
        RefreshCache();
    }

    /// <summary>
    /// 현재 데이터베이스 가져오기
    /// </summary>
    public ItemDatabase GetDatabase()
    {
        return database;
    }
}
