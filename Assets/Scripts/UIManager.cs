using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Items.Events;
using UnityEngine.UI;

/// <summary>
/// UI 관리 싱글톤 클래스
/// UI의 표시/숨기기만 담당하며, 각 UI는 자신의 생명주기를 관리
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UIManager");
                instance = go.AddComponent<UIManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("UI Canvas")]
    [SerializeField] private Canvas uiCanvas; // UI가 표시될 캔버스

    // 인벤토리 UI 캐싱
    private InventoryUI cachedInventoryUI;
    private GameObject cachedInventoryGameObject;
    // 레벨업 오버레이 캐싱
    private GameObject levelUpOverlay;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 캔버스 찾기 (없으면 생성)
        if (uiCanvas == null)
        {
            uiCanvas = FindFirstObjectByType<Canvas>();
            if (uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("UI Canvas");
                uiCanvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        // 레벨업 이벤트 구독
        PlayerLevelEvents.OnLevelUp += HandleLevelUp;
    }

    /// <summary>
    /// UI 표시 (제네릭)
    /// </summary>
    public async UniTask<T> ShowUI<T>(string addressableKey = null) where T : MonoBehaviour
    {
        T ui = FindFirstObjectByType<T>();
        if (ui != null)
        {
            // Show 메서드가 있으면 호출, 없으면 SetActive 사용
            var showMethod = typeof(T).GetMethod("Show", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showMethod != null)
            {
                showMethod.Invoke(ui, null);
            }
            else
            {
                ui.gameObject.SetActive(true);
            }
            return ui;
        }
        else
        {
            // UI가 없으면 어드레서블로 로드 시도
            if (!string.IsNullOrEmpty(addressableKey))
            {
                return await LoadAndShowUI<T>(addressableKey);
            }
            else
            {
                Debug.LogWarning($"UIManager: {typeof(T).Name}을 찾을 수 없고 addressableKey도 제공되지 않았습니다.");
                return null;
            }
        }
    }

    /// <summary>
    /// UI 숨기기 (제네릭)
    /// </summary>
    public void HideUI<T>() where T : MonoBehaviour
    {
        T ui = FindFirstObjectByType<T>();
        if (ui != null)
        {
            // Hide 메서드가 있으면 호출, 없으면 SetActive 사용
            var hideMethod = typeof(T).GetMethod("Hide", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (hideMethod != null)
            {
                hideMethod.Invoke(ui, null);
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// UI 토글 (제네릭) - 하나로 통합 관리
    /// </summary>
    public async UniTask ToggleUI<T>(string addressableKey) where T : MonoBehaviour
    {
        // InventoryUI는 특별 처리 (캐싱)
        if (typeof(T) == typeof(InventoryUI))
        {
            await ToggleInventoryUI(addressableKey);
            return;
        }

        T ui = FindFirstObjectByType<T>();
        if (ui != null)
        {
            // Toggle 메서드가 있으면 호출
            var toggleMethod = typeof(T).GetMethod("Toggle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (toggleMethod != null)
            {
                toggleMethod.Invoke(ui, null);
            }
            else
            {
                // Toggle 메서드가 없으면 Show/Hide로 처리
                bool isActive = ui.gameObject.activeSelf;
                if (isActive)
                {
                    HideUI<T>();
                }
                else
                {
                    await ShowUI<T>(addressableKey);
                }
            }
        }
        else
        {
            // UI가 없으면 어드레서블로 로드하고 Show
            await LoadAndShowUI<T>(addressableKey);
        }
    }

    /// <summary>
    /// 인벤토리 UI 토글 (캐싱 처리)
    /// </summary>
    private async UniTask ToggleInventoryUI(string addressableKey)
    {
        // 캐싱된 인벤토리가 있으면 토글만 수행
        if (cachedInventoryUI != null && cachedInventoryGameObject != null)
        {
            cachedInventoryUI.Toggle();
            return;
        }

        // 캐싱된 인벤토리가 없으면 첫 로드 (어드레서블)
        var go = await Addressables.InstantiateAsync(addressableKey, uiCanvas.transform).ToUniTask();
        cachedInventoryGameObject = go;
        
        var ui = go.GetComponent<InventoryUI>();
        if (ui == null)
        {
            Debug.LogError($"UIManager: {addressableKey}에서 InventoryUI 컴포넌트를 찾을 수 없습니다.");
            Addressables.ReleaseInstance(go);
            cachedInventoryGameObject = null;
            return;
        }

        // 캐싱
        cachedInventoryUI = ui;

        // Initialize 메서드 호출
        ui.Initialize();

        // Show (토글이므로 활성화)
        ui.gameObject.SetActive(true);
    }

    /// <summary>
    /// 어드레서블로 UI를 로드하고 표시
    /// </summary>
    private async UniTask<T> LoadAndShowUI<T>(string addressableKey) where T : MonoBehaviour
    {
        // 어드레서블로 로드 (UniTask 사용)
        var go = await Addressables.InstantiateAsync(addressableKey, uiCanvas.transform).ToUniTask();
        
        T ui = go.GetComponent<T>();
        if (ui == null)
        {
            Debug.LogError($"UIManager: {addressableKey}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            Addressables.ReleaseInstance(go);
            return null;
        }

        // Initialize 메서드가 있으면 호출
        var initMethod = typeof(T).GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (initMethod != null)
        {
            initMethod.Invoke(ui, null);
        }

        // Show 메서드가 있으면 호출, 없으면 SetActive 사용
        var showMethod = typeof(T).GetMethod("Show", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (showMethod != null)
        {
            showMethod.Invoke(ui, null);
        }
        else
        {
            ui.gameObject.SetActive(true);
        }

        return ui;
    }

    /// <summary>
    /// Canvas 가져오기 (UI들이 사용)
    /// </summary>
    public Canvas GetCanvas()
    {
        return uiCanvas;
    }

    /// <summary>
    /// 인벤토리 UI 캐시 해제
    /// </summary>
    public void ReleaseInventoryUI()
    {
        if (cachedInventoryGameObject != null)
        {
            // InventoryUI의 Unload 호출 (이벤트 정리 등)
            if (cachedInventoryUI != null)
            {
                cachedInventoryUI.Unload();
            }
            
            // 어드레서블 리소스 해제
            Addressables.ReleaseInstance(cachedInventoryGameObject);
            
            cachedInventoryUI = null;
            cachedInventoryGameObject = null;
        }
    }

    /// <summary>
    /// 레벨업 오버레이 생성 (반투명 검은 배경)
    /// </summary>
    private GameObject CreateLevelUpOverlay()
    {
        // 오버레이가 이미 있으면 재사용
        if (levelUpOverlay != null)
        {
            levelUpOverlay.SetActive(true);
            return levelUpOverlay;
        }

        // 오버레이 패널 생성
        GameObject overlayObj = new GameObject("LevelUpOverlay");
        overlayObj.transform.SetParent(uiCanvas.transform, false);
        
        // RectTransform 설정 (전체 화면)
        RectTransform rectTransform = overlayObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Image 컴포넌트 추가 (반투명 검은색)
        Image overlayImage = overlayObj.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.8f); // 알파값 조절 가능 (0.7 = 70% 불투명도)
        
        // 레벨업 UI보다 뒤에 배치되도록 순서 설정
        overlayObj.transform.SetAsFirstSibling();
        
        levelUpOverlay = overlayObj;
        return overlayObj;
    }

    /// <summary>
    /// 레벨업 오버레이 제거
    /// </summary>
    private void RemoveLevelUpOverlay()
    {
        if (levelUpOverlay != null)
        {
            Destroy(levelUpOverlay);
            levelUpOverlay = null;
        }
    }

    public async UniTaskVoid OnLevelUp(int newLevel)
    {
        Time.timeScale = 0f;

        CreateLevelUpOverlay();

        var ui = await ShowUI<LevelUpUI>("LevelUpPanel");
        ui.OnLevelUpCompleted += OnLevelUpCompleted;
    }

    private void HandleLevelUp(int newLevel)
    {
        OnLevelUp(newLevel).Forget();
    }

    private void OnLevelUpCompleted(LevelUpUI ui)
    {
        ui.OnLevelUpCompleted -= OnLevelUpCompleted;
        PlayerLevelEvents.OnLevelUp -= HandleLevelUp;
        Addressables.ReleaseInstance(ui.gameObject);
        Time.timeScale = 1f;
        levelUpOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        // UIManager가 파괴될 때 인벤토리 캐시 해제
        ReleaseInventoryUI();
        RemoveLevelUpOverlay();
    }

    // 프로퍼티
    public bool IsInventoryOpen => cachedInventoryUI != null && cachedInventoryUI.gameObject.activeSelf;
}
