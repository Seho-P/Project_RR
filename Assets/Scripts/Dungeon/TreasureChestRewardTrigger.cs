using System.Collections.Generic;
using UnityEngine;
using Items.Data;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// 보물상자 상호작용 트리거.
/// 플레이어가 범위 안에서 상호작용 키를 누르면 상자를 열고 보상 선택 UI를 띄운다.
/// </summary>
public class TreasureChestRewardTrigger : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    [Header("Chest Open")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private Collider2D interactionCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite chestOpenSprite;

    [Header("Reward UI")]
    [SerializeField] private string rewardPanelAddressableKey = "LevelUpPanel";
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool pauseGameWhileSelecting = true;
    [SerializeField] private int rewardItemCount = 5;

    private bool isPlayerInRange;
    private bool isOpened;
    private LevelUpUI activeRewardUI;
    private GameObject activeRewardUIObject;

    /// <summary>
    /// 레퍼런스가 비어있을 때 자동으로 보완한다.
    /// </summary>
    private void Awake()
    {
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<Collider2D>();
        }

        if (chestAnimator == null)
        {
            chestAnimator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// 플레이어가 범위 내에서 상호작용 키를 누르면 보물상자를 연다.
    /// </summary>
    private void Update()
    {
        if (!isPlayerInRange || isOpened)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            OpenChestAndShowRewardAsync().Forget();
        }
    }

    /// <summary>
    /// 플레이어가 트리거 범위에 들어오면 상호작용 가능 상태로 전환한다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        isPlayerInRange = true;
    }

    /// <summary>
    /// 플레이어가 트리거 범위를 벗어나면 상호작용 불가 상태로 전환한다.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        isPlayerInRange = false;
    }

    /// <summary>
    /// 상자 오픈 상태를 확정하고 보상 선택 UI를 생성한다.
    /// </summary>
    private async UniTaskVoid OpenChestAndShowRewardAsync()
    {
        isOpened = true;

        if (interactionCollider != null)
        {
            interactionCollider.enabled = false;
        }

        if(chestOpenSprite != null)
        {
            spriteRenderer.sprite = chestOpenSprite;
        }

        //애니메이션 나중에 필요하면 추가
        // if (chestAnimator != null && !string.IsNullOrEmpty(openTriggerName))
        // {
        //     chestAnimator.SetTrigger(openTriggerName);
        // }

        await ShowRewardUIAsync();
    }

    /// <summary>
    /// 레벨업 패널을 재사용해 랜덤 5개 중 1개를 선택하는 보상 UI를 표시한다.
    /// </summary>
    private async UniTask ShowRewardUIAsync()
    {
        if (string.IsNullOrEmpty(rewardPanelAddressableKey))
        {
            Debug.LogError("TreasureChestRewardTrigger: rewardPanelAddressableKey가 설정되지 않았습니다.");
            isOpened = false;
            if (interactionCollider != null)
            {
                interactionCollider.enabled = true;
            }
            return;
        }

        Canvas canvas = ResolveCanvas();
        if (canvas == null)
        {
            Debug.LogError("TreasureChestRewardTrigger: 보상 UI를 붙일 Canvas를 찾지 못했습니다.");
            isOpened = false;
            if (interactionCollider != null)
            {
                interactionCollider.enabled = true;
            }
            return;
        }

        activeRewardUIObject = await Addressables.InstantiateAsync(rewardPanelAddressableKey, canvas.transform).ToUniTask();
        activeRewardUI = activeRewardUIObject.GetComponent<LevelUpUI>();
        if (activeRewardUI == null)
        {
            Debug.LogError($"TreasureChestRewardTrigger: {rewardPanelAddressableKey} 프리팹에 LevelUpUI가 없습니다.");
            Addressables.ReleaseInstance(activeRewardUIObject);
            activeRewardUIObject = null;
            isOpened = false;
            if (interactionCollider != null)
            {
                interactionCollider.enabled = true;
            }
            return;
        }

        activeRewardUI.CreateSlots();

        int itemCount = Mathf.Max(1, rewardItemCount);
        List<ItemData> randomItems = ItemManager.Instance.GetRandomItems(itemCount, true, true);
        List<ItemInstance> itemInstances = ItemManager.Instance.ConvertToItemInstances(randomItems);
        activeRewardUI.SetSlotItems(itemInstances);
        activeRewardUI.OnLevelUpCompleted += HandleRewardSelectionCompleted;

        if (pauseGameWhileSelecting)
        {
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// 보상 선택 UI 종료 시 구독을 해제하고 UI를 정리한다.
    /// </summary>
    private void HandleRewardSelectionCompleted(LevelUpUI ui)
    {
        ui.OnLevelUpCompleted -= HandleRewardSelectionCompleted;

        if (pauseGameWhileSelecting)
        {
            Time.timeScale = 1f;
        }

        if (activeRewardUIObject != null)
        {
            Addressables.ReleaseInstance(activeRewardUIObject);
        }
        else
        {
            Destroy(ui.gameObject);
        }

        activeRewardUI = null;
        activeRewardUIObject = null;
    }

    /// <summary>
    /// 오브젝트 파괴 시 보상 UI 구독/시간정지를 안전하게 정리한다.
    /// </summary>
    private void OnDestroy()
    {
        if (activeRewardUI != null)
        {
            activeRewardUI.OnLevelUpCompleted -= HandleRewardSelectionCompleted;
        }

        if (activeRewardUIObject != null)
        {
            Addressables.ReleaseInstance(activeRewardUIObject);
            activeRewardUIObject = null;
            activeRewardUI = null;
        }

        if (pauseGameWhileSelecting && Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// UI를 표시할 Canvas를 우선순위에 따라 찾아 반환한다.
    /// </summary>
    private Canvas ResolveCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }

        if (UIManager.Instance != null)
        {
            Canvas managerCanvas = UIManager.Instance.GetCanvas();
            if (managerCanvas != null)
            {
                return managerCanvas;
            }
        }

        return FindFirstObjectByType<Canvas>();
    }
}
