using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬별 배경음악 재생을 담당하는 싱글톤 매니저.
/// 씬 전환 시 페이드 아웃 → 새 BGM 페이드 인 방식으로 전환된다.
/// </summary>
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    public static BGMManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("BGMManager");
                instance = go.AddComponent<BGMManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Serializable]
    public struct SceneBGM
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Header("BGM 매핑")]
    [SerializeField] private SceneBGM[] sceneBGMs;

    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    /// <summary>
    /// 씬 이름에 매핑된 BGM을 재생한다.
    /// 매핑이 없으면 현재 BGM을 페이드 아웃으로 종료한다.
    /// </summary>
    public void PlayBGMForScene(string sceneName)
    {
        for (int i = 0; i < sceneBGMs.Length; i++)
        {
            if (sceneBGMs[i].sceneName == sceneName)
            {
                PlayBGM(sceneBGMs[i].clip, sceneBGMs[i].volume);
                return;
            }
        }

        // 매핑 없는 씬 → BGM 페이드 아웃
        if (audioSource.isPlaying)
            StartCrossFade(null, 0f);
    }

    /// <summary>
    /// 지정한 클립을 재생한다. 이미 같은 클립이 재생 중이면 아무것도 하지 않는다.
    /// </summary>
    public void PlayBGM(AudioClip clip, float targetVolume = 1f)
    {
        if (clip == null)
        {
            if (audioSource.isPlaying)
                StartCrossFade(null, 0f);
            return;
        }

        // 동일 클립이 이미 재생 중이면 유지
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        StartCrossFade(clip, targetVolume);
    }

    /// <summary>
    /// BGM을 즉시 정지하고 볼륨을 0으로 초기화한다.
    /// </summary>
    public void StopBGM()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.volume = 0f;
    }

    private void StartCrossFade(AudioClip newClip, float targetVolume)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossFadeRoutine(newClip, targetVolume));
    }

    private IEnumerator CrossFadeRoutine(AudioClip newClip, float targetVolume)
    {
        // 페이드 아웃
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.clip = newClip;

        if (newClip == null)
        {
            fadeCoroutine = null;
            yield break;
        }

        // 페이드 인
        audioSource.Play();
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
        fadeCoroutine = null;
    }
}
