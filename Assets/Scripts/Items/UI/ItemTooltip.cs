using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Data;
using Items.Enums;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 아이템 툴팁 UI 컴포넌트
/// 아이템의 상세 정보를 표시합니다.
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI enhancementText;
    [SerializeField] private TextMeshProUGUI setInfoText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private CanvasGroup tooltipGroup;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.gray;
    [SerializeField] private Color uncommonColor = Color.green;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.magenta;
    [SerializeField] private Color legendaryColor = Color.yellow;

    private void Awake()
    {
        // UI 참조 자동 찾기
        if (itemNameText == null)
            itemNameText = transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        if (rarityText == null)
            rarityText = transform.Find("Rarity")?.GetComponent<TextMeshProUGUI>();
        if (descriptionText == null)
            descriptionText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        if (statsText == null)
            statsText = transform.Find("Stats")?.GetComponent<TextMeshProUGUI>();
        if (enhancementText == null)
            enhancementText = transform.Find("Enhancement")?.GetComponent<TextMeshProUGUI>();
        if (setInfoText == null)
            setInfoText = transform.Find("SetInfo")?.GetComponent<TextMeshProUGUI>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (tooltipRect == null)
            tooltipRect = GetComponent<RectTransform>();
        
        // CanvasGroup 자동 찾기 또는 생성
        if (tooltipGroup == null)
            tooltipGroup = GetComponent<CanvasGroup>();
        
        if (tooltipGroup == null)
            tooltipGroup = gameObject.AddComponent<CanvasGroup>();

        // 초기에는 숨김 (CanvasGroup으로 관리)
        tooltipGroup.alpha = 0f;
        tooltipGroup.blocksRaycasts = false;
        tooltipGroup.interactable = false;
        
        // GameObject는 항상 활성화 상태 유지
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 아이템 데이터를 설정합니다. (표시만 담당)
    /// </summary>
    public void SetData(ItemInstance item)
    {
        if (item == null || item.ItemData == null)
        {
            return;
        }

        ItemData data = item.ItemData;

        // 아이템 이름
        if (itemNameText != null)
        {
            itemNameText.text = data.itemName;
            itemNameText.color = GetRarityColor(data.rarity);
        }

        // 레어리티
        if (rarityText != null)
        {
            rarityText.text = GetRarityName(data.rarity);
            rarityText.color = GetRarityColor(data.rarity);
        }

        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrEmpty(data.description) ? "설명 없음" : data.description;
        }

        // 최종 스탯 (기본 스탯 + 강화 + 랜덤 옵션 통합)
        if (statsText != null)
        {
            statsText.text = FormatTotalStats(item);
        }

        // 강화 레벨
        if (enhancementText != null)
        {
            if (item.EnhancementLevel > 0)
            {
                enhancementText.text = $"강화 레벨: +{item.EnhancementLevel}";
                enhancementText.gameObject.SetActive(true);
            }
            else
            {
                enhancementText.gameObject.SetActive(false);
            }
        }

        // 세트 정보
        if (setInfoText != null)
        {
            if (item.IsSetItem && !string.IsNullOrEmpty(data.setName))
            {
                setInfoText.text = $"세트: {data.setName} ({data.setPieceNumber}부위)";
                setInfoText.gameObject.SetActive(true);
            }
            else
            {
                setInfoText.gameObject.SetActive(false);
            }
        }

        // 배경색 설정
        if (backgroundImage != null)
        {
            Color rarityColor = GetRarityColor(data.rarity);
            backgroundImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.9f);
        }
    }

    /// <summary>
    /// 툴팁을 표시합니다.
    /// </summary>
    public void Show()
    {
        if (tooltipGroup == null)
        {
            Debug.LogError("ItemTooltip: CanvasGroup이 없습니다.");
            return;
        }

        // GameObject는 항상 활성화 상태 유지
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // CanvasGroup으로 표시
        tooltipGroup.alpha = 1f;
        tooltipGroup.blocksRaycasts = false; // 툴팁은 클릭 대상이 아니므로
        tooltipGroup.interactable = false;
        
        Debug.Log($"ItemTooltip.Show() 완료 - CanvasGroup alpha: {tooltipGroup.alpha}");
    }

    /// <summary>
    /// 툴팁을 숨깁니다.
    /// </summary>
    public void Hide()
    {
        if (tooltipGroup == null)
        {
            Debug.LogError("ItemTooltip: CanvasGroup이 없습니다.");
            return;
        }

        // CanvasGroup으로 숨김
        tooltipGroup.alpha = 0f;
        tooltipGroup.blocksRaycasts = false;
        tooltipGroup.interactable = false;
        
        // GameObject는 활성화 상태 유지 (레이아웃 계산을 위해)
        // gameObject.SetActive(false); // 제거됨
    }

    /// <summary>
    /// 최종 스탯을 포맷팅합니다 (기본 스탯 + 강화 + 랜덤 옵션 통합).
    /// </summary>
    private string FormatTotalStats(ItemInstance item)
    {
        if (item == null || item.ItemData == null)
            return "";

        // 최종 플랫 스탯 가져오기
        Dictionary<StatType, float> totalStats = item.GetTotalStats();
        // 퍼센트 보너스 가져오기
        Dictionary<StatType, float> percentageBonuses = item.GetPercentageBonuses();

        if (totalStats.Count == 0 && percentageBonuses.Count == 0)
            return "";

        StringBuilder sb = new StringBuilder();

        // 모든 스탯 타입을 수집
        HashSet<StatType> allStatTypes = new HashSet<StatType>();
        foreach (var stat in totalStats.Keys)
            allStatTypes.Add(stat);
        foreach (var stat in percentageBonuses.Keys)
            allStatTypes.Add(stat);

        // 스탯 표시
        foreach (var statType in allStatTypes)
        {
            bool hasValue = false;
            sb.Append(GetStatName(statType));
            sb.Append(": ");

            // 플랫 값
            if (totalStats.ContainsKey(statType) && totalStats[statType] != 0f)
            {
                sb.Append($"+{totalStats[statType]:F1}");
                hasValue = true;
            }

            // 퍼센트 값
            if (percentageBonuses.ContainsKey(statType) && percentageBonuses[statType] != 0f)
            {
                if (hasValue)
                    sb.Append(" ");
                sb.Append($"+{percentageBonuses[statType]:F1}%");
                hasValue = true;
            }

            if (hasValue)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// 스탯 타입 이름을 반환합니다.
    /// </summary>
    private string GetStatName(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHealth => "최대 체력",
            StatType.HealthRegen => "체력 재생",
            StatType.MoveSpeed => "이동 속도",
            StatType.AttackSpeed => "공격 속도",
            StatType.AttackDamage => "공격력",
            StatType.CriticalChance => "치명타 확률",
            StatType.CriticalDamage => "치명타 피해",
            StatType.Defense => "방어력",
            StatType.DamageReduction => "피해 감소",
            StatType.LifeSteal => "생명력 흡수",
            StatType.CooldownReduction => "재사용 대기시간 감소",
            StatType.ExperienceGain => "경험치 획득량",
            StatType.GoldGain => "골드 획득량",
            StatType.ProjectileSpeed => "투사체 속도",
            StatType.AttackRange => "공격 범위",
            _ => statType.ToString()
        };
    }

    /// <summary>
    /// 레어리티 색상을 반환합니다.
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => commonColor,
            ItemRarity.Uncommon => uncommonColor,
            ItemRarity.Rare => rareColor,
            ItemRarity.Epic => epicColor,
            ItemRarity.Legendary => legendaryColor,
            _ => Color.white
        };
    }

    /// <summary>
    /// 레어리티 이름을 반환합니다.
    /// </summary>
    private string GetRarityName(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "일반",
            ItemRarity.Uncommon => "고급",
            ItemRarity.Rare => "희귀",
            ItemRarity.Epic => "영웅",
            ItemRarity.Legendary => "전설",
            _ => "알 수 없음"
        };
    }
}
