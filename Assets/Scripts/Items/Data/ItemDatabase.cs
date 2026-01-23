using UnityEngine;
using System.Collections.Generic;
using Items.Enums;

namespace Items.Data
{
    /// <summary>
    /// 아이템 데이터베이스 - 레벨업 시 나올 수 있는 아이템들을 관리
    /// Resources 폴더에 저장하여 런타임에 로드
    /// </summary>
    [CreateAssetMenu(fileName = "Item Database", menuName = "Items/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("Level Up Items")]
        [Tooltip("레벨업 시 랜덤으로 나올 아이템 목록")]
        public List<ItemData> levelUpItems = new List<ItemData>();

        [Header("Category Items (Optional)")]
        [Tooltip("일반 아이템 목록")]
        public List<ItemData> normalItems = new List<ItemData>();

        [Tooltip("세트 아이템 목록")]
        public List<ItemData> setItems = new List<ItemData>();

        [Header("Rarity Filter (Optional)")]
        [Tooltip("특정 레어도만 필터링할 때 사용")]
        public List<ItemRarity> allowedRarities = new List<ItemRarity>();

        [Header("Type Filter (Optional)")]
        [Tooltip("특정 타입만 필터링할 때 사용")]
        public List<ItemType> allowedTypes = new List<ItemType>();

        /// <summary>
        /// 모든 아이템 목록 가져오기
        /// </summary>
        public List<ItemData> GetAllItems()
        {
            List<ItemData> allItems = new List<ItemData>();
            allItems.AddRange(levelUpItems);
            if (normalItems != null) allItems.AddRange(normalItems);
            if (setItems != null) allItems.AddRange(setItems);
            return allItems;
        }

        /// <summary>
        /// 레벨업 아이템만 가져오기
        /// </summary>
        public List<ItemData> GetLevelUpItems()
        {
            return new List<ItemData>(levelUpItems);
        }

        /// <summary>
        /// 필터링된 아이템 목록 가져오기
        /// </summary>
        public List<ItemData> GetFilteredItems(bool useLevelUpItems = true, bool useNormalItems = false, bool useSetItems = false)
        {
            List<ItemData> filtered = new List<ItemData>();

            if (useLevelUpItems && levelUpItems != null)
                filtered.AddRange(levelUpItems);
            if (useNormalItems && normalItems != null)
                filtered.AddRange(normalItems);
            if (useSetItems && setItems != null)
                filtered.AddRange(setItems);

            // 레어도 필터
            if (allowedRarities != null && allowedRarities.Count > 0)
            {
                filtered.RemoveAll(item => item == null || !allowedRarities.Contains(item.rarity));
            }

            // 타입 필터
            if (allowedTypes != null && allowedTypes.Count > 0)
            {
                filtered.RemoveAll(item => item == null || !allowedTypes.Contains(item.itemType));
            }

            return filtered;
        }
    }
}
