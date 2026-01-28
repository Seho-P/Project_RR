using UnityEngine;
using UnityEngine.UI;
using Items.Data;

/// <summary>
/// 툴팁 생명주기 및 위치를 관리하는 매니저
/// 별도 Canvas에서 툴팁을 관리하여 다른 UI에 가려지지 않도록 합니다.
/// </summary>
public class TooltipManager : MonoBehaviour
{
    private static TooltipManager instance;
    public static TooltipManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TooltipManager>();
                if (instance == null)
                {
                    Debug.LogError("TooltipManager를 찾을 수 없습니다. 씬에 TooltipManager가 있는지 확인하세요.");
                }
            }
            return instance;
        }
    }

    [Header("Tooltip Reference")]
    [SerializeField] private ItemTooltip itemTooltip;

    [Header("Settings")]
    [SerializeField] private float showDelay = 0f; // 툴팁 표시 딜레이 (초) - 0이면 즉시 표시
    [SerializeField] private float offsetX = 10f; // 슬롯으로부터의 X 오프셋
    [SerializeField] private float offsetY = 0f; // 슬롯으로부터의 Y 오프셋
    [SerializeField] private bool autoPosition = true; // 화면 경계를 고려한 자동 위치 조정
    [SerializeField] private bool useUnscaledTime = true; // Time.timeScale = 0일 때도 작동하도록 unscaledTime 사용

    private Canvas tooltipCanvas;
    private RectTransform canvasRect;
    private RectTransform tooltipRect;
    private bool isShowing = false;
    private float hoverTime = 0f;
    private ItemInstance currentItem;
    private RectTransform currentSlotRect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("TooltipManager가 이미 존재합니다. 중복 인스턴스를 제거합니다.");
            Destroy(gameObject);
            return;
        }

        // ItemTooltip 자동 찾기
        if (itemTooltip == null)
        {
            itemTooltip = GetComponentInChildren<ItemTooltip>();
            if (itemTooltip == null)
            {
                itemTooltip = FindFirstObjectByType<ItemTooltip>();
            }
        }

        if (itemTooltip == null)
        {
            Debug.LogError("TooltipManager: ItemTooltip을 찾을 수 없습니다. TooltipManager의 Item Tooltip 필드에 할당하거나 자식으로 배치하세요.");
            return;
        }

        // Canvas 찾기 (자식이거나 부모에서 찾기)
        tooltipCanvas = GetComponentInParent<Canvas>();
        if (tooltipCanvas == null)
        {
            tooltipCanvas = GetComponent<Canvas>();
        }

        if (tooltipCanvas == null)
        {
            Debug.LogError("TooltipManager: Canvas를 찾을 수 없습니다. TooltipManager는 Canvas 또는 Canvas의 자식이어야 합니다.");
            return;
        }
        
        Debug.Log($"TooltipManager 초기화 완료 - Canvas: {tooltipCanvas.name}, ItemTooltip: {itemTooltip.name}");

        canvasRect = tooltipCanvas.GetComponent<RectTransform>();
        tooltipRect = itemTooltip.GetComponent<RectTransform>();

        // GraphicRaycaster 비활성화 (툴팁은 클릭 대상이 아니므로)
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = false;
        }

        // 초기에는 숨김
        HideTooltip();
    }

    private void Update()
    {
        // 딜레이 처리 (Time.timeScale = 0일 때도 작동하도록 unscaledDeltaTime 사용)
        if (showDelay > 0f && !isShowing && currentItem != null && currentSlotRect != null)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            hoverTime += deltaTime;
            if (hoverTime >= showDelay)
            {
                Debug.Log($"TooltipManager: 딜레이 완료, 툴팁 표시");
                ShowTooltipInternal();
            }
        }
    }

    /// <summary>
    /// 툴팁을 표시합니다.
    /// </summary>
    public void ShowTooltip(ItemInstance item, RectTransform slotRect)
    {
        if (item == null || item.ItemData == null || slotRect == null)
        {
            Debug.LogWarning("TooltipManager: ShowTooltip 호출 시 null 값이 있습니다.");
            HideTooltip();
            return;
        }

        if (itemTooltip == null)
        {
            Debug.LogError("TooltipManager: ItemTooltip이 설정되지 않았습니다.");
            return;
        }

        Debug.Log($"TooltipManager: ShowTooltip 호출됨 - Item: {item.ItemData.itemName}, 딜레이: {showDelay}초");

        currentItem = item;
        currentSlotRect = slotRect;
        hoverTime = 0f;

        // 즉시 표시 또는 딜레이 후 표시
        if (!isShowing)
        {
            // 딜레이가 0이면 즉시 표시
            if (showDelay <= 0f)
            {
                Debug.Log("TooltipManager: 딜레이 없음, 즉시 표시");
                ShowTooltipInternal();
            }
            else
            {
                Debug.Log($"TooltipManager: 딜레이 대기 중... ({showDelay}초, unscaledTime: {useUnscaledTime})");
                // Update에서 딜레이 처리 (unscaledDeltaTime 사용)
            }
        }
        else
        {
            // 이미 표시 중이면 즉시 업데이트
            Debug.Log("TooltipManager: 이미 표시 중, 즉시 업데이트");
            ShowTooltipInternal();
        }
    }

    /// <summary>
    /// 툴팁을 숨깁니다.
    /// </summary>
    public void HideTooltip()
    {
        Debug.Log("TooltipManager: HideTooltip 호출됨");
        isShowing = false;
        hoverTime = 0f;
        currentItem = null;
        currentSlotRect = null;

        if (itemTooltip != null)
        {
            itemTooltip.Hide();
        }
    }

    /// <summary>
    /// 내부 툴팁 표시 로직
    /// </summary>
    private void ShowTooltipInternal()
    {
        if (currentItem == null || currentSlotRect == null || itemTooltip == null)
        {
            Debug.LogWarning($"TooltipManager: 표시할 데이터가 없습니다. Item: {currentItem != null}, SlotRect: {currentSlotRect != null}, Tooltip: {itemTooltip != null}");
            return;
        }

        Debug.Log($"TooltipManager: ShowTooltipInternal 실행 - {currentItem.ItemData.itemName}");

        isShowing = true;

        // Canvas가 활성화되어 있는지 확인
        if (tooltipCanvas != null && !tooltipCanvas.gameObject.activeSelf)
        {
            Debug.LogWarning("TooltipManager: TooltipCanvas가 비활성화되어 있습니다. 활성화합니다.");
            tooltipCanvas.gameObject.SetActive(true);
        }
        
        // 먼저 툴팁을 표시 (레이아웃 계산을 위해)
        itemTooltip.Show();
        Debug.Log("TooltipManager: ItemTooltip.Show() 호출됨");

        // 아이템 데이터 설정
        itemTooltip.SetData(currentItem);
        Debug.Log("TooltipManager: ItemTooltip.SetData() 호출됨");

        // 레이아웃 업데이트 후 위치 계산
        Canvas.ForceUpdateCanvases();

        // 위치 계산 및 설정
        SetTooltipPosition(currentSlotRect);
        Debug.Log($"TooltipManager: 위치 설정 완료 - {tooltipRect.anchoredPosition}");
    }

    /// <summary>
    /// 툴팁 위치를 계산하고 설정합니다.
    /// </summary>
    private void SetTooltipPosition(RectTransform slotRect)
    {
        if (tooltipRect == null || slotRect == null || canvasRect == null)
        {
            Debug.LogWarning("TooltipManager: 위치 설정에 필요한 참조가 없습니다.");
            return;
        }

        // 먼저 레이아웃 업데이트 (크기 계산을 위해)
        Canvas.ForceUpdateCanvases();

        // 슬롯의 월드 위치를 툴팁 Canvas의 로컬 좌표로 변환
        Vector2 slotPos;
        Camera cam = tooltipCanvas.worldCamera ?? Camera.main;
        
        // Screen Space - Overlay 모드 처리
        if (tooltipCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Overlay 모드는 카메라 없이 작동
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(null, slotRect.position),
                null,
                out slotPos);
        }
        else
        {
            // Camera 모드
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(cam, slotRect.position),
                cam,
                out slotPos);
        }

        // 툴팁 크기 가져오기
        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;
        
        // 크기가 0이면 기본값 사용
        if (tooltipWidth <= 0) tooltipWidth = 200f;
        if (tooltipHeight <= 0) tooltipHeight = 100f;

        // 기본 위치 계산 (슬롯 오른쪽)
        Vector2 tooltipPosition = new Vector2(
            slotPos.x + slotRect.rect.width * 0.5f + tooltipWidth * 0.5f + offsetX,
            slotPos.y + offsetY
        );

        // 자동 위치 조정 (화면 경계 체크)
        if (autoPosition)
        {
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            // 오른쪽 경계를 넘어가면 왼쪽에 표시
            if (tooltipPosition.x + tooltipWidth * 0.5f > canvasWidth * 0.5f)
            {
                tooltipPosition.x = slotPos.x - slotRect.rect.width * 0.5f - tooltipWidth * 0.5f - offsetX;
            }

            // 위쪽 경계를 넘어가면 아래로 조정
            if (tooltipPosition.y + tooltipHeight * 0.5f > canvasHeight * 0.5f)
            {
                tooltipPosition.y = canvasHeight * 0.5f - tooltipHeight * 0.5f;
            }

            // 아래쪽 경계를 넘어가면 위로 조정
            if (tooltipPosition.y - tooltipHeight * 0.5f < -canvasHeight * 0.5f)
            {
                tooltipPosition.y = -canvasHeight * 0.5f + tooltipHeight * 0.5f;
            }
        }

        // 위치 설정
        tooltipRect.anchoredPosition = tooltipPosition;
    }

    /// <summary>
    /// 툴팁 Canvas의 Sort Order를 설정합니다.
    /// </summary>
    public void SetCanvasSortOrder(int sortOrder)
    {
        if (tooltipCanvas != null)
        {
            tooltipCanvas.sortingOrder = sortOrder;
        }
    }

    // 프로퍼티
    public bool IsShowing => isShowing;
    public ItemTooltip ItemTooltip => itemTooltip;
}
