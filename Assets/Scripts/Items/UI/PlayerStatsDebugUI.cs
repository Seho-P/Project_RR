using UnityEngine;
using Items.Enums;
using Items.Events;
using Items.Data;
using System.Collections.Generic;

/// <summary>
/// 플레이어 스탯을 실시간으로 화면에 표시하는 디버그 UI
/// 테스트 시 아이템 장착/해제 시 스탯 변화를 확인할 수 있습니다.
/// </summary>
public class PlayerStatsDebugUI : MonoBehaviour
{
    [Header("디버그 설정")]
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    
    [Header("참조")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private ItemManager itemManager;

    private bool isVisible = true;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private Vector2 scrollPosition;
    private Dictionary<StatType, float> previousStats = new Dictionary<StatType, float>();

    private void Awake()
    {
        if (playerStats == null)
            Debug.LogError("PlayerStatsDebugUI: PlayerStats 컴포넌트를 찾을 수 없습니다.");
        
        if (itemManager == null)
            Debug.LogError("PlayerStatsDebugUI: ItemManager 컴포넌트를 찾을 수 없습니다.");

        // 이전 스탯 초기화
        InitializePreviousStats();
    }

    private void Start()
    {
        ItemEvents.OnStatsChanged += OnStatsChanged;
        ItemEvents.OnItemEquipped += OnItemEquipped;
        ItemEvents.OnItemUnequipped += OnItemUnequipped;
    }

    private void OnDestroy()
    {
        ItemEvents.OnStatsChanged -= OnStatsChanged;
        ItemEvents.OnItemEquipped -= OnItemEquipped;
        ItemEvents.OnItemUnequipped -= OnItemUnequipped;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
        }
    }

    private void OnGUI()
    {
        if (!showDebugUI || !isVisible || playerStats == null)
            return;

        // GUI 스타일 설정
        if (labelStyle == null)
            InitializeStyles();

        // 스크롤 가능한 영역
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(400), GUILayout.Height(Screen.height - 20));
        
        GUILayout.BeginVertical(boxStyle);
        
        // 헤더
        GUILayout.Label("=== 플레이어 스탯 디버그 ===", headerStyle);
        GUILayout.Label($"토글 키: {toggleKey}", labelStyle);
        GUILayout.Space(10);

        // 모든 스탯 표시
        DisplayAllStats();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    private void InitializeStyles()
    {
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.normal.textColor = Color.white;

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
    }

    private void InitializePreviousStats()
    {
        previousStats.Clear();
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            if (playerStats != null)
            {
                previousStats[statType] = playerStats.GetStat(statType);
            }
        }
    }

    private void DisplayAllStats()
    {
        if (playerStats == null) return;

        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            float currentValue = playerStats.GetStat(statType);
            float previousValue = previousStats.ContainsKey(statType) ? previousStats[statType] : 0f;
            float difference = currentValue - previousValue;

            // 스탯 이름
            string statName = GetStatName(statType);
            
            // 값 표시
            string valueText = $"{statName}: {currentValue:F2}";
            
            // 변화량 표시
            if (Mathf.Abs(difference) > 0.01f)
            {
                Color changeColor = difference > 0 ? Color.green : Color.red;
                string changeText = difference > 0 ? $"+{difference:F2}" : $"{difference:F2}";
                
                GUIStyle changeStyle = new GUIStyle(labelStyle);
                changeStyle.normal.textColor = changeColor;
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(valueText, labelStyle);
                GUILayout.Label($"({changeText})", changeStyle);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(valueText, labelStyle);
            }
        }

        // 장착된 아이템 목록
        GUILayout.Space(10);
        GUILayout.Label("=== 장착된 아이템 ===", headerStyle);
        
        if (itemManager != null)
        {
            var equippedItems = itemManager.GetEquippedItems();
            if (equippedItems.Count == 0)
            {
                GUILayout.Label("장착된 아이템 없음", labelStyle);
            }
            else
            {
                foreach (var item in equippedItems)
                {
                    if (item.ItemData != null)
                    {
                        GUILayout.Label($"- {item.ItemData.itemName}", labelStyle);
                    }
                }
            }
        }
    }

    private string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHealth: return "최대 체력";
            case StatType.HealthRegen: return "체력 재생";
            case StatType.MoveSpeed: return "이동 속도";
            case StatType.AttackSpeed: return "공격 속도";
            case StatType.AttackDamage: return "공격력";
            case StatType.CriticalChance: return "치명타 확률";
            case StatType.CriticalDamage: return "치명타 피해";
            case StatType.Defense: return "방어력";
            case StatType.DamageReduction: return "피해 감소";
            case StatType.LifeSteal: return "생명력 흡수";
            case StatType.CooldownReduction: return "재사용 대기시간 감소";
            case StatType.ExperienceGain: return "경험치 획득";
            case StatType.GoldGain: return "골드 획득";
            case StatType.ProjectileSpeed: return "투사체 속도";
            case StatType.AttackRange: return "공격 범위";
            default: return statType.ToString();
        }
    }

    private void OnStatsChanged()
    {
        // 스탯 변경 시 이전 스탯 업데이트는 아이템 장착/해제 이벤트에서 처리
    }

    private void OnItemEquipped(ItemInstance item)
    {
        if (item?.ItemData != null)
        {
            // 아이템 장착 후 스탯이 변경되었으므로,
            // 현재 스탯에서 아이템의 플랫 스탯을 빼서 장착 전 스탯을 계산
            UpdatePreviousStatsFromItem(item, true);
        }
    }

    private void OnItemUnequipped(ItemInstance item)
    {
        if (item?.ItemData != null)
        {
            // 아이템 해제 후 스탯이 변경되었으므로,
            // 현재 스탯에 아이템의 플랫 스탯을 더해서 해제 전 스탯을 계산
            UpdatePreviousStatsFromItem(item, false);
        }
    }

    private void UpdatePreviousStatsFromItem(ItemInstance item, bool equipped)
    {
        // 아이템 장착/해제 전 스탯을 계산하여 저장
        // 플랫 스탯만 고려하여 간단하게 계산
        if (playerStats == null || item?.ItemData == null) return;

        var stats = item.GetTotalStats();

        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            float currentValue = playerStats.GetStat(statType);
            float itemFlatValue = stats.ContainsKey(statType) ? stats[statType] : 0f;

            if (equipped)
            {
                // 장착된 경우: 현재 값에서 아이템 플랫 값을 빼서 장착 전 값 계산
                previousStats[statType] = currentValue - itemFlatValue;
            }
            else
            {
                // 해제된 경우: 현재 값에 아이템 플랫 값을 더해서 해제 전 값 계산
                previousStats[statType] = currentValue + itemFlatValue;
            }
        }
    }

}
