using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Data;
using Items.Enums;

/// <summary>
/// 범용 아이템 슬롯 컴포넌트
/// 인벤토리, 레벨업 선택, 장비 슬롯 등 다양한 용도로 사용 가능
/// </summary>
[RequireComponent(typeof(Button))]
public class ItemSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;           // 아이템 아이콘
    [SerializeField] private Image backgroundImage;     // 배경 이미지 (레어리티 색상용)
    [SerializeField] private Image borderImage;         // 테두리 이미지
    [SerializeField] private TextMeshProUGUI countText; // 개수 텍스트 (스택 가능한 아이템용)
    [SerializeField] private GameObject enhancementBadge; // 강화 레벨 배지
    [SerializeField] private TextMeshProUGUI enhancementText; // 강화 레벨 텍스트
    [SerializeField] private GameObject emptySlotVisual; // 빈 슬롯 시각 효과

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.gray;
    [SerializeField] private Color uncommonColor = Color.green;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.magenta;
    [SerializeField] private Color legendaryColor = Color.yellow;

    [Header("Settings")]
    [SerializeField] private bool showEnhancementLevel = true;
    [SerializeField] private bool showEmptyVisual = true;

    private Button slotButton;
    private ItemInstance currentItem;
    private int slotIndex = -1;

    // 이벤트
    public System.Action<ItemSlot, ItemInstance> OnSlotClicked;
    public System.Action<ItemSlot, ItemInstance> OnSlotRightClicked;

    private void Awake()
    {
        slotButton = GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotButtonClicked);
        }

        // UI 참조가 없으면 자동으로 찾기
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (countText == null)
            countText = transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        if (enhancementBadge == null)
            enhancementBadge = transform.Find("EnhancementBadge")?.gameObject;
        if (enhancementText == null)
            enhancementText = enhancementBadge?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        if (emptySlotVisual == null)
            emptySlotVisual = transform.Find("EmptyVisual")?.gameObject;
    }

    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotButtonClicked);
        }
    }

    /// <summary>
    /// 슬롯에 아이템 설정
    /// </summary>
    public void SetItem(ItemInstance item, int index = -1)
    {
        currentItem = item;
        slotIndex = index;

        if (item == null || item.ItemData == null)
        {
            ClearSlot();
            return;
        }

        UpdateSlotVisuals();
    }

    /// <summary>
    /// 슬롯 비우기
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (countText != null)
            countText.gameObject.SetActive(false);

        if (enhancementBadge != null && showEnhancementLevel)
            enhancementBadge.SetActive(false);

        if (emptySlotVisual != null && showEmptyVisual)
            emptySlotVisual.SetActive(true);

        // 배경색 초기화
        if (backgroundImage != null)
            backgroundImage.color = Color.gray;
    }

    /// <summary>
    /// 슬롯 시각적 요소 업데이트
    /// </summary>
    private void UpdateSlotVisuals()
    {
        if (currentItem == null || currentItem.ItemData == null)
        {
            ClearSlot();
            return;
        }

        ItemData data = currentItem.ItemData;

        // 아이콘 설정
        if (iconImage != null)
        {
            iconImage.sprite = data.itemIcon;
            iconImage.enabled = data.itemIcon != null;
        }

        // 레어리티 색상 설정
        if (backgroundImage != null)
        {
            backgroundImage.color = GetRarityColor(data.rarity);
        }

        if (borderImage != null)
        {
            borderImage.color = GetRarityColor(data.rarity);
        }

        // 강화 레벨 표시
        if (showEnhancementLevel && enhancementBadge != null)
        {
            bool hasEnhancement = currentItem.EnhancementLevel > 0;
            enhancementBadge.SetActive(hasEnhancement);
            
            if (hasEnhancement && enhancementText != null)
            {
                enhancementText.text = $"+{currentItem.EnhancementLevel}";
            }
        }

        // 빈 슬롯 시각 효과 숨기기
        if (emptySlotVisual != null)
            emptySlotVisual.SetActive(false);

        // 개수 텍스트 (필요시 확장 가능)
        if (countText != null)
            countText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 레어리티에 따른 색상 반환
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
    /// 슬롯 버튼 클릭 처리
    /// </summary>
    private void OnSlotButtonClicked()
    {
        // 우클릭 감지 (필요시 확장)
        if (Input.GetMouseButtonDown(1))
        {
            OnSlotRightClicked?.Invoke(this, currentItem);
        }
        else
        {
            OnSlotClicked?.Invoke(this, currentItem);
        }
    }

    /// <summary>
    /// 슬롯 활성화/비활성화
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (slotButton != null)
            slotButton.interactable = interactable;
    }

    // 프로퍼티
    public ItemInstance CurrentItem => currentItem;
    public int SlotIndex => slotIndex;
    public bool IsEmpty => currentItem == null || currentItem.ItemData == null;
}

