using System.Collections;
using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 로드 후 플레이어가 생성되면 시네머신 카메라의 타겟을 자동으로 연결한다.
/// </summary>
public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private bool alsoSetLookAt = false;

    [Header("Player Search")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float searchTimeout = 5f;

    private Coroutine bindRoutine;

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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartBindRoutine();
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartBindRoutine();
    }

    private void StartBindRoutine()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
        }

        bindRoutine = StartCoroutine(BindCameraToPlayerWhenReady());
    }

    private IEnumerator BindCameraToPlayerWhenReady()
    {
        float elapsed = 0f;

        while (elapsed < searchTimeout)
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            Transform playerTransform = FindPlayerTransform();

            if (cinemachineCamera != null && playerTransform != null)
            {
                ApplyTarget(cinemachineCamera, playerTransform);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.LogWarning("[CameraManager] 플레이어 또는 CinemachineCamera를 찾지 못했습니다.");
    }

    private Transform FindPlayerTransform()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            return player.transform;
        }

        if (!string.IsNullOrWhiteSpace(playerTag))
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
            if (taggedPlayer != null)
            {
                return taggedPlayer.transform;
            }
        }

        return null;
    }

    private void ApplyTarget(Component cameraComponent, Transform target)
    {
        // Cinemachine 버전에 따라 멤버명이 다를 수 있어 반사로 안전하게 설정한다.
        bool appliedTracking = SetTransformMember(cameraComponent, "TrackingTarget", target);
        bool appliedFollow = SetTransformMember(cameraComponent, "Follow", target);
        bool appliedLookAt = alsoSetLookAt && SetTransformMember(cameraComponent, "LookAt", target);

        if (!appliedTracking && !appliedFollow && !appliedLookAt)
        {
            Debug.LogWarning("[CameraManager] 카메라 타겟 멤버를 찾지 못했습니다. (TrackingTarget/Follow/LookAt)");
            return;
        }

        Debug.Log($"[CameraManager] 카메라 타겟 연결 완료: {target.name}");
    }

    private static bool SetTransformMember(Component targetComponent, string memberName, Transform value)
    {
        if (targetComponent == null)
        {
            return false;
        }

        var type = targetComponent.GetType();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        PropertyInfo property = type.GetProperty(memberName, flags);
        if (property != null && property.CanWrite && typeof(Transform).IsAssignableFrom(property.PropertyType))
        {
            property.SetValue(targetComponent, value);
            return true;
        }

        FieldInfo field = type.GetField(memberName, flags);
        if (field != null && typeof(Transform).IsAssignableFrom(field.FieldType))
        {
            field.SetValue(targetComponent, value);
            return true;
        }

        return false;
    }
}
