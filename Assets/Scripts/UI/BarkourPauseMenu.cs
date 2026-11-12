using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public sealed class BarkourPauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject overlay, actions, settings;
    [SerializeField] private UnityEngine.UI.Button resume, restart, openSettings, mainMenu, back;
    [SerializeField] private UnityEngine.UI.Slider master, music, sfx;
    [SerializeField] private TMP_Text masterValue, musicValue, sfxValue, seed, status;
    [SerializeField] private LevelChunkSpawner spawner;

    private bool paused, leaving, previousAudioPause;
    private float previousTimeScale;
    private GameObject previousSelection;

    private void Awake()
    {
        overlay.SetActive(false);
        resume.onClick.AddListener(Resume);
        restart.onClick.AddListener(() => Load(SceneManager.GetActiveScene().name));
        mainMenu.onClick.AddListener(() => Load("MainMenu"));
        openSettings.onClick.AddListener(ShowSettings);
        back.onClick.AddListener(CloseSettings);
        BarkourMainMenu.Bind(master, masterValue, "Master", MenuAudioSettings.Master);
        BarkourMainMenu.Bind(music, musicValue, "Music", MenuAudioSettings.Music);
        BarkourMainMenu.Bind(sfx, sfxValue, "Sfx", MenuAudioSettings.Sfx);
    }

    private void Update()
    {
        if (leaving) return;
        bool toggle = Keyboard.current?.escapeKey.wasPressedThisFrame == true ||
            Gamepad.current?.startButton.wasPressedThisFrame == true;
        if (toggle)
        {
            if (!paused) Pause();
            else if (settings.activeSelf) CloseSettings();
            else Resume();
        }
        else if (paused && settings.activeSelf && Gamepad.current?.buttonEast.wasPressedThisFrame == true)
        {
            CloseSettings();
        }
    }

    private void Pause()
    {
        var run = RunManager.Instance;
        if (run == null || run.IsEnteringFromMenu ||
            run.CurrentState == RunManager.RunState.Failed || Time.timeScale <= 0f) return;

        paused = true;
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousSelection = EventSystem.current?.currentSelectedGameObject;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        seed.text = $"SEED  {spawner.CurrentSeed}";
        status.text = "";
        settings.SetActive(false);
        actions.SetActive(true);
        overlay.SetActive(true);
        Select(resume.gameObject);
    }

    private void Resume()
    {
        if (!paused || leaving) return;
        MenuAudioSettings.Save();
        overlay.SetActive(false);
        // The same key or controller button can also be seen by RunManager this frame.
        RunManager.Instance?.IgnoreStartInputThisFrame();
        RestoreGameplay();
        Select(previousSelection);
    }

    private void ShowSettings()
    {
        actions.SetActive(false);
        settings.SetActive(true);
        Select(master.gameObject);
    }

    private void CloseSettings()
    {
        MenuAudioSettings.Save();
        settings.SetActive(false);
        actions.SetActive(true);
        Select(openSettings.gameObject);
    }

    private void Load(string scene)
    {
        if (leaving) return;
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            status.text = "Scene is missing from Build Settings.";
            return;
        }

        leaving = true;
        MenuAudioSettings.Save();
        resume.interactable = restart.interactable = openSettings.interactable = mainMenu.interactable = false;
        Select(null);
        // The spawner retains a custom seed and rolls a new seed in random mode.
        SceneManager.LoadSceneAsync(scene);
    }

    private static void Select(GameObject item)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(item);
    }

    private void RestoreGameplay()
    {
        if (!paused) return;
        paused = false;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
    }

    private void OnDisable()
    {
        overlay.SetActive(false);
        RestoreGameplay();
    }
}
