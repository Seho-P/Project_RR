using UnityEngine;
using System.Collections.Generic;
using Items.Enums;

namespace Items.Data
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
    public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    public string itemName;
    public string description;
    public Sprite itemIcon;
    public ItemType itemType;
    public ItemRarity rarity;

    [Header("Base Stats")]
    public List<StatModifier> baseStats = new List<StatModifier>();

    [Header("Set Info (Optional)")]
    public string setName; // 세트 이름 (비어있으면 일반 아이템)
    public int setPieceNumber; // 세트 내 아이템 번호 (1, 2, 3...)
    }
}

