using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items.Data;
using Items.Events;

/// <summary>
/// 레벨업 UI 관리 클래스
/// 오각형 방향으로 5개 슬롯을 배치하고 아이템 선택 UI를 표시
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemSlot slotPrefab; // 슬롯 프리팹
    [SerializeField] private Transform slotContainer; // 슬롯들이 배치될 부모 오브젝트 (중심점)

    [Header("Pentagon Layout Settings")]
    [SerializeField] private float radius = 200f; // 중심에서 슬롯까지의 거리
    [SerializeField] private float startAngle = -90f; // 시작 각도 (상단부터 시작하려면 -90도)
    [SerializeField] private int slotCount = 5; // 슬롯 개수 (오각형이므로 5개)

    [Header("Settings")]
    [SerializeField] private bool createSlotsOnStart = true;
    [SerializeField] private bool hideOnStart = true; // 시작 시 숨김

    private List<ItemSlot> slots = new List<ItemSlot>();

    private void Awake()
    {
        if (slotContainer == null)
            slotContainer = transform;

        if (hideOnStart)
            gameObject.SetActive(false);
    }

    private void Start()
    {
        if (createSlotsOnStart)
        {
            CreateSlots();
        }

        // 레벨업 이벤트 구독
        PlayerLevelEvents.OnLevelUp += OnLevelUp;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        PlayerLevelEvents.OnLevelUp -= OnLevelUp;

        // 슬롯 정리
        ClearSlots();
    }

    /// <summary>
    /// 오각형 배치로 슬롯들 생성
    /// </summary>
    public void CreateSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("LevelUpUI: 슬롯 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 기존 슬롯 제거
        ClearSlots();

        // 오각형 배치로 슬롯 생성
        float angleStep = 360f / slotCount; // 각 슬롯 사이의 각도 (72도)

        for (int i = 0; i < slotCount; i++)
        {
            // 각도 계산 (도 단위)
            float angle = startAngle + (angleStep * i);
            float angleRad = angle * Mathf.Deg2Rad; // 라디안으로 변환

            // 위치 계산 (중심점 기준)
            Vector2 position = new Vector2(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius
            );

            // 슬롯 생성
            ItemSlot slot = Instantiate(slotPrefab, slotContainer);
            slot.name = $"LevelUpSlot_{i}";
            
            // RectTransform을 사용하여 위치 설정
            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }
            else
            {
                // RectTransform이 없으면 일반 Transform 사용
                slot.transform.localPosition = position;
            }

            slot.SetItem(null, i);

            // 슬롯 클릭 이벤트 구독
            slot.OnSlotClicked += OnSlotClicked;
            slot.OnSlotRightClicked += OnSlotRightClicked;

            slots.Add(slot);
        }
    }

    /// <summary>
    /// 슬롯에 아이템 설정
    /// </summary>
    public void SetSlotItem(int index, ItemInstance item)
    {
        if (index < 0 || index >= slots.Count)
        {
            Debug.LogWarning($"LevelUpUI: 잘못된 슬롯 인덱스 {index}");
            return;
        }

        slots[index].SetItem(item, index);
    }

    /// <summary>
    /// 모든 슬롯에 아이템 설정
    /// </summary>
    public void SetSlotItems(List<ItemInstance> items)
    {
        int count = Mathf.Min(items.Count, slots.Count);
        for (int i = 0; i < count; i++)
        {
            slots[i].SetItem(items[i], i);
        }
    }

    /// <summary>
    /// 모든 슬롯 비우기
    /// </summary>
    public void ClearSlotItems()
    {
        foreach (var slot in slots)
        {
            slot.SetItem(null, -1);
        }
    }

    /// <summary>
    /// 슬롯 클릭 처리
    /// </summary>
    private void OnSlotClicked(ItemSlot slot, ItemInstance item)
    {
        if (item == null)
        {
            Debug.Log($"빈 슬롯 클릭: {slot.SlotIndex}");
            return;
        }

        // 아이템 선택 처리
        Debug.Log($"레벨업 아이템 선택: {item.ItemData.itemName} (슬롯 {slot.SlotIndex})");
        
        // 여기에 아이템 선택 로직 추가
        // 예: OnItemSelected?.Invoke(item);
        
        // 선택 후 UI 숨기기 (필요시)
        // Hide();
    }

    /// <summary>
    /// 슬롯 우클릭 처리
    /// </summary>
    private void OnSlotRightClicked(ItemSlot slot, ItemInstance item)
    {
        if (item == null)
            return;

        Debug.Log($"레벨업 아이템 우클릭: {item.ItemData.itemName} (슬롯 {slot.SlotIndex})");
    }

    /// <summary>
    /// 레벨업 이벤트 처리
    /// </summary>
    private void OnLevelUp(int newLevel)
    {
        // 레벨업 시 UI 표시 (나중에 아이템 로직 추가 시 사용)
        // Show();
    }

    /// <summary>
    /// 모든 슬롯 제거
    /// </summary>
    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnSlotClicked;
                slot.OnSlotRightClicked -= OnSlotRightClicked;
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();
    }

    /// <summary>
    /// 레벨업 UI 표시/숨기기
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 레벨업 UI 표시
    /// </summary>
    public void Show()
    {
        SetVisible(true);
    }

    /// <summary>
    /// 레벨업 UI 숨기기
    /// </summary>
    public void Hide()
    {
        SetVisible(false);
    }

    /// <summary>
    /// 레벨업 UI 토글
    /// </summary>
    public void Toggle()
    {
        SetVisible(!gameObject.activeSelf);
    }

    // 프로퍼티
    public List<ItemSlot> Slots => slots;
    public int SlotCount => slots.Count;
    public bool IsVisible => gameObject.activeSelf;
}

