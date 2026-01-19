using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Items.Data;

/// <summary>
/// 인벤토리 UI 관리 클래스
/// 슬롯들을 생성하고 관리하며, 자신의 생명주기를 관리
/// </summary>
public class InventoryUI : MonoBehaviour
{
    private static InventoryUI instance;
    public static InventoryUI Instance
    {
        get
        {
            if (instance == null)
            {
                // UIManager를 통해 로드하거나 직접 찾기
                instance = FindFirstObjectByType<InventoryUI>();
            }
            return instance;
        }
    }


    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemSlot slotPrefab; // 슬롯 프리팹
    [SerializeField] private Transform slotContainer; // 슬롯들이 배치될 부모 오브젝트
    [SerializeField] private GridLayoutGroup gridLayout; // 그리드 레이아웃 (선택사항)

    [Header("Settings")]
    [SerializeField] private bool createSlotsOnStart = true;
    [SerializeField] private int slotsPerRow = 6; // 한 줄에 표시할 슬롯 수

    private List<ItemSlot> slots = new List<ItemSlot>();
    // private GameObject loadedGameObject;
    private bool isInitialized = false;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("InventoryUI: 중복된 인스턴스가 감지되었습니다. 기존 인스턴스를 사용합니다.");
            Destroy(gameObject);
            return;
        }

        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (slotContainer == null)
            slotContainer = transform;

        // 그리드 레이아웃 설정
        if (gridLayout == null)
            gridLayout = slotContainer.GetComponent<GridLayoutGroup>();

        if (gridLayout != null)
        {
            // 자동으로 그리드 설정 (필요시 수정)
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = slotsPerRow;
        }
    }

    private void Start()
    {
        // 이미 초기화되었으면 스킵 (Load()에서 이미 초기화됨)
        if (isInitialized) return;

        // 씬에 직접 배치된 경우 자동 초기화
        Initialize();
    }

    private void OnDestroy()
    {
        Unload();
    }

    /// <summary>
    /// 슬롯들 생성
    /// </summary>
    public void CreateSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("InventoryUI: 슬롯 프리팹이 설정되지 않았습니다!");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("InventoryUI: 인벤토리가 설정되지 않았습니다!");
            return;
        }

        // 기존 슬롯 제거
        ClearSlots();

        // 새 슬롯 생성
        int slotCount = inventory.MaxSlots;
        for (int i = 0; i < slotCount; i++)
        {
            ItemSlot slot = Instantiate(slotPrefab, slotContainer);
            slot.SetActive(true);
            slot.name = $"Slot_{i}";
            slot.SetItem(null, i);

            // 슬롯 클릭 이벤트 구독
            slot.OnSlotClicked += OnSlotClicked;
            slot.OnSlotRightClicked += OnSlotRightClicked;

            slots.Add(slot);
        }

        // 인벤토리 아이템들 업데이트
        RefreshAllSlots();
    }

    /// <summary>
    /// 모든 슬롯 새로고침
    /// </summary>
    public void RefreshAllSlots()
    {
        if (inventory == null) return;

        for (int i = 0; i < slots.Count && i < inventory.MaxSlots; i++)
        {
            ItemInstance item = inventory.GetItem(i);
            slots[i].SetItem(item, i);
        }
    }

    /// <summary>
    /// 특정 슬롯 새로고침
    /// </summary>
    public void RefreshSlot(int index)
    {
        if (index < 0 || index >= slots.Count || inventory == null)
            return;

        ItemInstance item = inventory.GetItem(index);
        slots[index].SetItem(item, index);
    }

    /// <summary>
    /// 슬롯 클릭 처리
    /// </summary>
    private void OnSlotClicked(ItemSlot slot, ItemInstance item)
    {
        if (item == null)
        {
            // 빈 슬롯 클릭 처리 (필요시 확장)
            Debug.Log($"빈 슬롯 클릭: {slot.SlotIndex}");
            return;
        }

        // 아이템 클릭 처리
        Debug.Log($"아이템 클릭: {item.ItemData.itemName} (슬롯 {slot.SlotIndex})");
        
        // 여기에 아이템 상세 정보 표시, 장착 등의 로직 추가 가능
        // 예: ShowItemDetail(item);
        // 예: EquipItem(item);
    }

    /// <summary>
    /// 슬롯 우클릭 처리
    /// </summary>
    private void OnSlotRightClicked(ItemSlot slot, ItemInstance item)
    {
        if (item == null)
            return;

        Debug.Log($"아이템 우클릭: {item.ItemData.itemName} (슬롯 {slot.SlotIndex})");
        
        // 여기에 빠른 사용, 버리기 등의 로직 추가 가능
        // 예: UseItem(item);
        // 예: DropItem(item);
    }

    /// <summary>
    /// 인벤토리 아이템 추가 이벤트 처리
    /// </summary>
    private void OnInventoryItemAdded(ItemInstance item, int index)
    {
        RefreshSlot(index);
    }

    /// <summary>
    /// 인벤토리 아이템 제거 이벤트 처리
    /// </summary>
    private void OnInventoryItemRemoved(int index)
    {
        RefreshSlot(index);
    }

    /// <summary>
    /// 인벤토리 아이템 변경 이벤트 처리
    /// </summary>
    private void OnInventoryItemChanged(int index, ItemInstance newItem)
    {
        RefreshSlot(index);
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
    /// 인벤토리 UI 로드 (어드레서블) uimanger에서 로드하게 변경되면서 필요없어짐
    /// </summary>
    // public async UniTask<InventoryUI> Load(string addressableKey)
    // {
    //     var canvas = UIManager.Instance?.GetCanvas();
    //     if (canvas == null)
    //     {
    //         Debug.LogError("InventoryUI: UIManager의 Canvas를 찾을 수 없습니다.");
    //         return null;
    //     }

    //     loadedGameObject = await Addressables.InstantiateAsync(addressableKey, canvas.transform).ToUniTask();
    //     var ui = loadedGameObject.GetComponent<InventoryUI>();
    //     if (ui != null)
    //     {
    //         ui.Initialize();
    //     }
    //     return ui;
    // }

    /// <summary>
    /// 초기화 (로드 후 호출)
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // Inventory 찾기
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        // 슬롯 생성
        if (createSlotsOnStart)
        {
            CreateSlots();
        }

        // 이벤트 구독
        if (inventory != null)
        {
            inventory.OnItemAdded += OnInventoryItemAdded;
            inventory.OnItemRemoved += OnInventoryItemRemoved;
            inventory.OnItemChanged += OnInventoryItemChanged;
        }

        isInitialized = true;
    }

    /// <summary>
    /// 인벤토리 UI 언로드 (이벤트 정리 및 슬롯 정리만 수행)
    /// Addressables 해제는 UIManager에서 관리
    /// </summary>
    public void Unload()
    {
        // 이벤트 구독 해제
        if (inventory != null)
        {
            inventory.OnItemAdded -= OnInventoryItemAdded;
            inventory.OnItemRemoved -= OnInventoryItemRemoved;
            inventory.OnItemChanged -= OnInventoryItemChanged;
        }

        // 슬롯 정리
        ClearSlots();

        // 어드레서블 리소스 해제는 UIManager에서 관리하므로 여기서는 하지 않음
        // UIManager에서 ReleaseInventoryUI()를 호출하면 Addressables.ReleaseInstance가 호출됨
        // loadedGameObject = null;

        instance = null;
        isInitialized = false;
    }

    /// <summary>
    /// 인벤토리 UI 토글
    /// </summary>
    public void Toggle()
    {
        if (instance == null || !instance.gameObject.activeSelf)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }



    /// <summary>
    /// Inventory 참조 설정 (어드레서블 로드 시 사용)
    /// </summary>
    public void SetInventory(Inventory inv)
    {
        // 기존 이벤트 구독 해제
        if (inventory != null)
        {
            inventory.OnItemAdded -= OnInventoryItemAdded;
            inventory.OnItemRemoved -= OnInventoryItemRemoved;
            inventory.OnItemChanged -= OnInventoryItemChanged;
        }

        inventory = inv;

        // 새 인벤토리 이벤트 구독
        if (inventory != null)
        {
            inventory.OnItemAdded += OnInventoryItemAdded;
            inventory.OnItemRemoved += OnInventoryItemRemoved;
            inventory.OnItemChanged += OnInventoryItemChanged;

            // 슬롯이 이미 생성되어 있다면 새로고침
            if (slots.Count > 0)
            {
                RefreshAllSlots();
            }
        }
    }

    // 프로퍼티
    public List<ItemSlot> Slots => slots;
    public Inventory Inventory => inventory;
}

