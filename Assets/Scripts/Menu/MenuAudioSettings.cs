using UnityEngine;
using UnityEngine.SceneManagement;

// Existing gameplay AudioSources are SFX. Their authored volumes remain the baseline.
public sealed class MenuAudioSettings : MonoBehaviour
{
    public bool music;
    private AudioSource source;
    private float authoredVolume;
    public static float Master => PlayerPrefs.GetFloat("Barkour.Audio.Master", 1f);
    public static float Music => PlayerPrefs.GetFloat("Barkour.Audio.Music", .7f);
    public static float Sfx => PlayerPrefs.GetFloat("Barkour.Audio.Sfx", 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
        AudioListener.volume = Master;
    }
    private static void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var audioSource in root.GetComponentsInChildren<AudioSource>(true))
                if (!audioSource.TryGetComponent<MenuAudioSettings>(out _))
                    audioSource.gameObject.AddComponent<MenuAudioSettings>();
    }
    private void Awake() { source = GetComponent<AudioSource>(); authoredVolume = source.volume; }
    private void OnEnable() { Apply(); }
    private void OnDisable() { if (source) source.volume = authoredVolume; }
    private void Apply() { if (source) source.volume = authoredVolume * (music ? Music : Sfx); }
    public static void Set(string channel, float value)
    {
        PlayerPrefs.SetFloat("Barkour.Audio." + channel, Mathf.Clamp01(value));
        AudioListener.volume = Master;
        foreach (var item in FindObjectsByType<MenuAudioSettings>(FindObjectsSortMode.None)) item.Apply();
    }
    public static void Save() { PlayerPrefs.Save(); }
}
