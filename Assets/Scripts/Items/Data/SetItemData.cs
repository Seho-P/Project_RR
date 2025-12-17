using UnityEngine;
using System.Collections.Generic;

namespace Items.Data
{
    [CreateAssetMenu(fileName = "New Set", menuName = "Items/Set Item Data")]
    public class SetItemData : ScriptableObject
{
    [Header("Set Info")]
    public string setName;
    public string setDescription;
    public int totalPieces; // 세트 전체 개수

    [Header("Set Bonuses")]
    public List<SetBonus> setBonuses = new List<SetBonus>(); // 개수별 보너스
    }
}

