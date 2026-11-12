using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject scoreHud;
    [SerializeField] private DistanceScore distanceScore;
    [SerializeField] private LevelChunkSpawner levelSpawner;
    [SerializeField] private TMP_Text distanceLabel, bestLabel, seedLabel, statusLabel;
    [SerializeField] private UnityEngine.UI.Button retryButton, menuButton, quitButton;
    [SerializeField] private CanvasGroup fadeOverlay;
    private bool shown, leaving;
    private float previousTimeScale = 1f;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        fadeOverlay.alpha = 0f; fadeOverlay.blocksRaycasts = false;
        if (RunManager.Instance?.IsEnteringFromMenu == true)
            StartCoroutine(RevealEntrance());
        retryButton.onClick.AddListener(RestartGame);
        menuButton.onClick.AddListener(ReturnToMenu);
        quitButton.onClick.AddListener(Quit);
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButton.gameObject.SetActive(false);
            Link(retryButton, menuButton); Link(menuButton, retryButton);
        }
    }
    // Score updates run first, so the panel captures the final distance on failure.
    private void LateUpdate()
    {
        if (leaving || RunManager.Instance == null) return;
        if (!shown && RunManager.Instance.CurrentState == RunManager.RunState.Failed) Show();
        if (shown && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }
    private IEnumerator RevealEntrance()
    {
        const float duration = .22f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            fadeOverlay.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fadeOverlay.alpha = 0f;
    }

    private void Show()
    {
        shown = true;
        distanceLabel.text = "DISTANCE  " + distanceScore.CurrentDistance + " m";
        bestLabel.text = "BEST  " + distanceScore.BestDistance + " m";
        seedLabel.text = "SEED  " + levelSpawner.CurrentSeed;
        statusLabel.text = "";
        gameOverPanel.SetActive(true); scoreHud.SetActive(false);
        previousTimeScale = Time.timeScale; Time.timeScale = 0f;
        PlayerPrefs.Save();
        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }
    public void RestartGame() { BeginLoad(SceneManager.GetActiveScene().name); }
    public void ReturnToMenu() { BeginLoad("MainMenu"); }
    private void BeginLoad(string scene)
    {
        if (!shown || leaving) return;
        if (!Application.CanStreamedLevelBeLoaded(scene)) { statusLabel.text = "Scene is missing from Build Settings."; return; }
        leaving = true;
        retryButton.interactable = menuButton.interactable = quitButton.interactable = false;
        fadeOverlay.blocksRaycasts = true;
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(FadeAndLoad(scene));
    }
    private IEnumerator FadeAndLoad(string scene)
    {
        for (float t = 0; t < .35f; t += Time.unscaledDeltaTime)
        { fadeOverlay.alpha = Mathf.SmoothStep(0f, 1f, t / .35f); yield return null; }
        fadeOverlay.alpha = 1f;
        // Keep the old scene frozen until activation. Awake in the new scene runs at normal time.
        var load = SceneManager.LoadSceneAsync(scene);
        load.allowSceneActivation = false;
        while (load.progress < .9f) yield return null;
        Time.timeScale = previousTimeScale;
        load.allowSceneActivation = true;
        yield return load;
    }
    public void Quit()
    {
        if (leaving) return;
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
        Application.Quit();
#endif
    }
    private void OnDestroy() { if (shown) Time.timeScale = previousTimeScale; }
    private static void Link(UnityEngine.UI.Selectable item, UnityEngine.UI.Selectable other)
    {
        item.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp = other, selectOnDown = other };
    }
}
