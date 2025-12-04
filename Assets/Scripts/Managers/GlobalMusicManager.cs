using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GlobalMusicManager : MonoBehaviour
{
    public static GlobalMusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;

    [System.Serializable]
    public class musicClip
    {
        public AudioClip clip;
        public string name;
    }
    [SerializeField] private List<musicClip> musicClips;
    [SerializeField] private BoolEventSO bossThemeEventSO;

    [Header("Music Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private bool playOnAwake = true;

    [Header("Fade Settings (Optional)")]
    [SerializeField] private bool useFadeTransitions = true;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private string currentMusicName;
    private Dictionary<string, AudioClip> musicClipDict; // Fast lookup dictionary
    private string cachedSceneName;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize AudioSource if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f; // 2D music
        musicSource.ignoreListenerPause = true; // Continue playing during pause (dialog, menus, etc.)

        // Build dictionary for fast lookups
        BuildMusicDictionary();

        // Preload all music clips to prevent lag on first play
        PreloadMusicClips();
    }

    /// <summary>
    /// Builds a dictionary from the musicClips list for O(1) lookups
    /// </summary>
    private void BuildMusicDictionary()
    {
        musicClipDict = new Dictionary<string, AudioClip>();
        if (musicClips != null)
        {
            foreach (var musicClip in musicClips)
            {
                if (musicClip != null && !string.IsNullOrEmpty(musicClip.name) && musicClip.clip != null)
                {
                    musicClipDict[musicClip.name] = musicClip.clip;
                }
            }
        }
    }

    /// <summary>
    /// Preloads all music clips to prevent lag when playing them for the first time
    /// </summary>
    private void PreloadMusicClips()
    {
        if (musicClipDict == null) return;

        foreach (var kvp in musicClipDict)
        {
            if (kvp.Value != null && !kvp.Value.preloadAudioData)
            {
                // Force preload the audio data
                kvp.Value.LoadAudioData();
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (bossThemeEventSO != null)
        {
            bossThemeEventSO.onEventRaised.AddListener(OnBossThemeEvent);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (bossThemeEventSO != null)
        {
            bossThemeEventSO.onEventRaised.RemoveListener(OnBossThemeEvent);
        }
    }

    private void OnDestroy()
    {
        // Safety cleanup in case OnDisable wasn't called
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (bossThemeEventSO != null)
        {
            bossThemeEventSO.onEventRaised.RemoveListener(OnBossThemeEvent);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cache scene name to avoid repeated calls
        cachedSceneName = scene.name;
        ReturnToStageTheme();
    }

    private void OnBossThemeEvent(bool isBossTheme)
    {
        if (isBossTheme)
        {
            // Use cached scene name if available, otherwise get it
            string sceneName = cachedSceneName ?? SceneManager.GetActiveScene().name;
            if (sceneName.Contains("Prison"))
            {
                PlayMusic("Prison Boss");
            }
            else if (sceneName.Contains("Cave"))
            {
                PlayMusic("Cave Boss");
            }
        }
        else
        {
            ReturnToStageTheme();
        }
    }

    private void ReturnToStageTheme()
    {
        // Use cached scene name if available, otherwise get it
        string currentSceneName = cachedSceneName ?? SceneManager.GetActiveScene().name;
        if (currentSceneName.Contains("Prison"))
        {
            PlayMusic("Prison");
        }
        else if (currentSceneName.Contains("Cave"))
        {
            PlayMusic("Cave");
        }
        else if (currentSceneName.Contains("Main Menu"))
        {
            PlayMusic("MainMenu");
        }
        else
        { //play nothing
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Plays music by name. Handles fading and null checks.
    /// </summary>
    public void PlayMusic(string musicName)
    {
        if (string.IsNullOrEmpty(musicName) || musicSource == null)
        {
            Debug.LogWarning($"GlobalMusicManager: Cannot play music '{musicName}' - invalid parameters");
            return;
        }

        // Don't restart if already playing the same music
        if (currentMusicName == musicName && musicSource.isPlaying)
        {
            return;
        }

        // Use dictionary for fast O(1) lookup instead of Find()
        if (musicClipDict == null || !musicClipDict.TryGetValue(musicName, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"GlobalMusicManager: Music clip '{musicName}' not found in musicClips list!");
            return;
        }

        // Ensure audio data is loaded (safety check)
        if (!clip.preloadAudioData)
        {
            clip.LoadAudioData();
        }

        currentMusicName = musicName;

        if (useFadeTransitions && musicSource.isPlaying)
        {
            // Fade out current, then fade in new
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeTransition(clip));
        }
        else
        {
            // Instant switch - this should be lag-free now since clip is preloaded
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Fades out current music, then fades in new music
    /// </summary>
    private IEnumerator FadeTransition(AudioClip newClip)
    {
        // Fade out
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }
        musicSource.volume = 0f;

        // Switch clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeDuration);
            yield return null;
        }
        musicSource.volume = musicVolume;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Stops the music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            if (useFadeTransitions)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                StartCoroutine(FadeOut());
            }
            else
            {
                musicSource.Stop();
            }
        }
    }

    private IEnumerator FadeOut()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = musicVolume; // Reset for next play
    }

    /// <summary>
    /// Sets the music volume (0-1)
    /// </summary>
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// Gets the current music volume
    /// </summary>
    public float GetVolume()
    {
        return musicVolume;
    }
}
