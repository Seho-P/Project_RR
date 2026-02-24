using System.Collections.Generic;
using UnityEngine;
using Items.Data;
using Items.Enums;
using Items.Events;

public class SetEffectManager : MonoBehaviour
{
    private Dictionary<string, int> equippedSetCounts = new Dictionary<string, int>();
    private Dictionary<string, Dictionary<int, SetBonus>> activeSetBonuses = new Dictionary<string, Dictionary<int, SetBonus>>();
    private Dictionary<string, SetItemData> setDataCache = new Dictionary<string, SetItemData>();

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("SetEffectManager: PlayerStats 컴포넌트를 찾을 수 없습니다. 런타임에 바인딩될 수 있습니다.");
        }
    }

    /// <summary>
    /// 씬 전환 후 새 플레이어의 PlayerStats를 바인딩한다.
    /// </summary>
    public void BindPlayerStats(PlayerStats newPlayerStats)
    {
        playerStats = newPlayerStats;
    }

    // 세트 개수 업데이트
    public void UpdateSetCounts(List<ItemInstance> equippedItems)
    {
        equippedSetCounts.Clear();
        activeSetBonuses.Clear();

        // 세트별 개수 계산
        foreach (var item in equippedItems)
        {
            if (item.IsSetItem && item.ItemData != null)
            {
                string setName = item.ItemData.setName;
                if (!string.IsNullOrEmpty(setName))
                {
                    if (!equippedSetCounts.ContainsKey(setName))
                        equippedSetCounts[setName] = 0;
                    equippedSetCounts[setName]++;
                }
            }
        }

        // 세트 보너스 적용
        ApplySetBonuses();
    }

    // 세트 보너스 적용
    private void ApplySetBonuses()
    {
        if (playerStats == null) return;

        // 기존 세트 보너스 제거
        playerStats.ClearSetBonusStats();

        foreach (var kvp in equippedSetCounts)
        {
            string setName = kvp.Key;
            int count = kvp.Value;

            // SetItemData 로드 (캐시에서)
            SetItemData setData = GetSetData(setName);
            if (setData == null) continue;

            // 해당 개수에 맞는 보너스 찾기
            foreach (var bonus in setData.setBonuses)
            {
                if (count >= bonus.requiredPieces)
                {
                    // 보너스 적용
                    ApplyBonus(bonus, setName);
                    
                    // 이벤트 발생
                    ItemEvents.InvokeSetCountChanged(setName, count);
                    
                    // 완성 이벤트 (필요시)
                    if (count == bonus.requiredPieces)
                    {
                        ItemEvents.InvokeSetComplete(setName, bonus.requiredPieces);
                    }
                }
            }
        }
    }

    // 보너스 적용
    private void ApplyBonus(SetBonus bonus, string setName)
    {
        if (playerStats == null) return;

        foreach (var modifier in bonus.statModifiers)
        {
            playerStats.AddSetBonusStat(modifier.statType, modifier.flatBonus, modifier.percentageBonus);
        }
    }

    // SetItemData 가져오기 (캐시 사용)
    private SetItemData GetSetData(string setName)
    {
        if (setDataCache.ContainsKey(setName))
        {
            return setDataCache[setName];
        }

        // Resources 폴더에서 로드 (또는 다른 방식)
        SetItemData data = Resources.Load<SetItemData>($"Sets/{setName}");
        if (data != null)
        {
            setDataCache[setName] = data;
        }

        return data;
    }

    // 특정 세트의 현재 개수 가져오기
    public int GetSetCount(string setName)
    {
        return equippedSetCounts.ContainsKey(setName) ? equippedSetCounts[setName] : 0;
    }

    // 모든 세트 개수 초기화
    public void ClearSetCounts()
    {
        equippedSetCounts.Clear();
        activeSetBonuses.Clear();
        if (playerStats != null)
        {
            playerStats.ClearSetBonusStats();
        }
    }
}

