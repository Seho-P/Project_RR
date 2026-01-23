using System.Collections.Generic;
using UnityEngine;
using Items.Data;
using Items.Enums;

/// <summary>
/// ItemManager 사용 예시 클래스
/// 실제 게임에서는 이 코드를 참고하여 사용하세요.
/// </summary>
public class ItemManagerUsageExample : MonoBehaviour
{
    // 예시: 레벨업 시 랜덤 아이템 5개 뽑기
    public void OnLevelUp()
    {
        // 기본 사용법 - 레벨업 아이템 5개 뽑기
        List<ItemData> randomItems = ItemManager.Instance.GetRandomItems(5);

        // ItemData를 ItemInstance로 변환
        List<ItemInstance> itemInstances = ItemManager.Instance.ConvertToItemInstances(randomItems);

        // UI에 표시하거나 인벤토리에 추가
        foreach (var item in itemInstances)
        {
            Debug.Log($"랜덤 아이템: {item.ItemData.itemName}");
        }
    }

    // 예시: 특정 레어도만 뽑기
    public void GetRareItems()
    {
        List<ItemData> rareItems = ItemManager.Instance.GetRandomItemsByRarity(3, ItemRarity.Rare);
        // ...
    }

    // 예시: 필터링 옵션 사용
    public void GetFilteredItems()
    {
        // 레어도 필터
        List<ItemRarity> allowedRarities = new List<ItemRarity> { ItemRarity.Rare, ItemRarity.Epic };
        
        // 타입 필터
        List<ItemType> allowedTypes = new List<ItemType> { ItemType.Offensive, ItemType.Defensive };

        // 필터링된 아이템 뽑기
        List<ItemData> filteredItems = ItemManager.Instance.GetRandomItems(
            count: 5,
            useLevelUpItems: true,
            useNormalItems: false,
            useSetItems: true,
            allowDuplicates: false,
            rarityFilter: allowedRarities,
            typeFilter: allowedTypes
        );
    }
}
