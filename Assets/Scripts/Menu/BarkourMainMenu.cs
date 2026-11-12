using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class BarkourMainMenu : MonoBehaviour, ICancelHandler
{
    public CanvasGroup mainPanel;
    public GameObject settingsPanel;
    public CanvasGroup fade;
    public UnityEngine.UI.Button playButton, settingsButton, quitButton, backButton;
    public UnityEngine.UI.Slider masterSlider, musicSlider, sfxSlider;
    public TMP_Text masterValue, musicValue, sfxValue, status;
    public Transform kali;
    public Animator kaliAnimator;
    public Camera menuCamera;
    public MenuParallax[] layers;
    public string gameScene = "Game";
    private bool starting;
    private float kaliY;
    private UnityEngine.UI.Button customizeButton;
    private GameObject customizePanel;
    private TMP_InputField seedInput;
    private TMP_Text seedError;

    private void Start()
    {
        Time.timeScale = 1f;
        kaliY = kali.position.y;
        fade.alpha = 0;
        fade.blocksRaycasts = false;
        settingsPanel.SetActive(false);
        playButton.onClick.AddListener(Play);
        settingsButton.onClick.AddListener(OpenSettings);
        backButton.onClick.AddListener(CloseSettings);
        quitButton.onClick.AddListener(Quit);
        Bind(masterSlider, masterValue, "Master", MenuAudioSettings.Master);
        Bind(musicSlider, musicValue, "Music", MenuAudioSettings.Music);
        Bind(sfxSlider, sfxValue, "Sfx", MenuAudioSettings.Sfx);
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButton.gameObject.SetActive(false);
            Link(playButton, settingsButton, settingsButton);
            Link(settingsButton, playButton, playButton);
        }
        BuildCustomizeRun();
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }
    private static void Link(UnityEngine.UI.Selectable item, UnityEngine.UI.Selectable up, UnityEngine.UI.Selectable down)
    {
        var n = item.navigation; n.mode = UnityEngine.UI.Navigation.Mode.Explicit;
        n.selectOnUp = up; n.selectOnDown = down; item.navigation = n;
    }
    internal static void Bind(UnityEngine.UI.Slider slider, TMP_Text label, string channel, float value)
    {
        slider.SetValueWithoutNotify(value);
        label.text = Mathf.RoundToInt(value * 100) + "%";
        slider.onValueChanged.AddListener(v => {
            MenuAudioSettings.Set(channel, v); label.text = Mathf.RoundToInt(v * 100) + "%";
        });
    }
    private void Update()
    {
        if (starting) return;
        // Viewport anchoring preserves bottom-left placement when the aspect ratio changes.
        Vector3 p = menuCamera.ViewportToWorldPoint(new Vector3(.18f, 0, 10));
        kali.position = new Vector3(p.x, kaliY, 0);
        if (EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(customizePanel.activeSelf ? seedInput.gameObject :
                settingsPanel.activeSelf ? masterSlider.gameObject : playButton.gameObject);
        if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
        {
            if (customizePanel.activeSelf) CloseCustomizeRun();
            else if (settingsPanel.activeSelf) CloseSettings();
        }
    }
    public void OnCancel(BaseEventData e)
    {
        if (starting) return;
        if (customizePanel.activeSelf) CloseCustomizeRun();
        else if (settingsPanel.activeSelf) CloseSettings();
    }

    private void BuildCustomizeRun()
    {
        customizeButton = CopyButton(playButton, playButton.transform, "CustomizeRunButton", "");
        var arrow = (RectTransform)customizeButton.transform;
        arrow.anchorMin = arrow.anchorMax = new Vector2(1f, .5f);
        arrow.anchoredPosition = new Vector2(44f, 0f);
        arrow.sizeDelta = new Vector2(68f, 68f);

        // Three corners on a small mesh keep the icon independent of font coverage.
        var icon = new GameObject("Triangle", typeof(RectTransform), typeof(CanvasRenderer));
        var triangle = icon.AddComponent<RunCustomizeTriangle>();
        triangle.color = playButton.GetComponentInChildren<TMP_Text>().color;
        triangle.raycastTarget = false;
        triangle.rectTransform.SetParent(arrow, false);
        triangle.rectTransform.sizeDelta = new Vector2(24f, 28f);
        customizeButton.onClick.AddListener(OpenCustomizeRun);
        var playNavigation = playButton.navigation;
        playNavigation.selectOnRight = customizeButton;
        playButton.navigation = playNavigation;
        var arrowNavigation = playNavigation;
        arrowNavigation.selectOnRight = null;
        arrowNavigation.selectOnLeft = playButton;
        customizeButton.navigation = arrowNavigation;

        var root = new GameObject("CustomizeRunPanel", typeof(RectTransform));
        root.SetActive(false);
        customizePanel = root;
        var panel = (RectTransform)root.transform;
        panel.SetParent(mainPanel.transform.parent, false);
        panel.SetSiblingIndex(fade.transform.GetSiblingIndex());
        panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f);
        panel.sizeDelta = new Vector2(500f, 300f);
        var background = root.AddComponent<UnityEngine.UI.Image>();
        background.color = new Color(.19f, .25f, .22f);

        var template = playButton.GetComponentInChildren<TMP_Text>();
        MakeLabel(template, panel, "Customize Run", new Vector2(0, 106), new Vector2(470, 60), 38);
        seedError = MakeLabel(template, panel, "", new Vector2(0, -38), new Vector2(470, 48), 23);

        var fieldObject = new GameObject("SeedInput", typeof(RectTransform));
        var field = (RectTransform)fieldObject.transform;
        field.SetParent(panel, false);
        field.anchoredPosition = new Vector2(0, 28);
        field.sizeDelta = new Vector2(410, 62);
        var fieldBackground = fieldObject.AddComponent<UnityEngine.UI.Image>();
        fieldBackground.color = new Color(.12f, .17f, .15f);
        var viewport = new GameObject("Text Area", typeof(RectTransform), typeof(UnityEngine.UI.RectMask2D));
        var viewportRect = (RectTransform)viewport.transform;
        viewportRect.SetParent(field, false);
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(16, 4);
        viewportRect.offsetMax = new Vector2(-16, -4);
        var value = MakeLabel(template, viewportRect, "", Vector2.zero, Vector2.zero, 30);
        var placeholder = MakeLabel(template, viewportRect, "Seed (blank for random)", Vector2.zero, Vector2.zero, 26);
        foreach (var label in new[] { value, placeholder })
        {
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.alignment = TextAlignmentOptions.MidlineLeft;
        }
        placeholder.alpha = .7f;
        seedInput = fieldObject.AddComponent<TMP_InputField>();
        seedInput.targetGraphic = fieldBackground;
        seedInput.textViewport = viewportRect;
        seedInput.textComponent = value;
        seedInput.placeholder = placeholder;
        seedInput.lineType = TMP_InputField.LineType.SingleLine;
        seedInput.onValueChanged.AddListener(_ => seedError.text = "");

        var done = CopyButton(backButton, panel, "DoneButton", "Done");
        var doneRect = (RectTransform)done.transform;
        doneRect.anchorMin = doneRect.anchorMax = new Vector2(.5f, .5f);
        doneRect.anchoredPosition = new Vector2(0, -108);
        doneRect.sizeDelta = new Vector2(210, 60);
        done.onClick.AddListener(CloseCustomizeRun);
        Link(seedInput, done, done);
        Link(done, seedInput, seedInput);
    }

    private static UnityEngine.UI.Button CopyButton(UnityEngine.UI.Button template,
        Transform parent, string name, string label)
    {
        var button = Instantiate(template, parent, false);
        button.name = name;
        button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.GetComponentInChildren<TMP_Text>().text = label;
        return button;
    }

    private static TMP_Text MakeLabel(TMP_Text template, Transform parent,
        string label, Vector2 position, Vector2 size, float fontSize)
    {
        var text = Instantiate(template, parent, false);
        text.text = label;
        text.color = new Color(.99f, .96f, .88f);
        text.raycastTarget = false;
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(.5f, .5f);
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = size;
        return text;
    }

    private void OpenCustomizeRun()
    {
        if (starting) return;
        mainPanel.gameObject.SetActive(false);
        customizePanel.SetActive(true);
        seedInput.SetTextWithoutNotify(LevelChunkSpawner.MenuSeed?.ToString(CultureInfo.InvariantCulture) ?? "");
        seedError.text = "";
        EventSystem.current.SetSelectedGameObject(seedInput.gameObject);
    }

    private void CloseCustomizeRun()
    {
        string text = seedInput.text.Trim();
        int value = 0;
        if (text.Length > 0 && !int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            seedError.text = "Enter a whole number from ΓêÆ2147483648 to 2147483647.";
            return;
        }

        LevelChunkSpawner.SetMenuSeed(text.Length == 0 ? (int?)null : value);
        customizePanel.SetActive(false);
        mainPanel.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(customizeButton.gameObject);
    }
    public void OpenSettings()
    {
        if (starting) return;
        mainPanel.gameObject.SetActive(false); settingsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(masterSlider.gameObject);
    }
    public void CloseSettings()
    {
        MenuAudioSettings.Save(); settingsPanel.SetActive(false); mainPanel.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
    }
    public void Play()
    {
        if (starting) return;
        if (!Application.CanStreamedLevelBeLoaded(gameScene))
        { status.text = "Game scene is missing from Build Settings."; return; }
        LevelChunkSpawner.SetMenuSeed(LevelChunkSpawner.MenuSeed);
        starting = true;
        mainPanel.interactable = false; mainPanel.blocksRaycasts = false;
        EventSystem.current.SetSelectedGameObject(null);
        MenuAudioSettings.Save(); StartCoroutine(StartRun());
    }
    private IEnumerator StartRun()
    {
        Vector3 origin = kali.position;
        float endX = menuCamera.ViewportToWorldPoint(new Vector3(1.2f, 0, 10)).x;
        for (float t = 0; t < 1.2f; t += Time.unscaledDeltaTime)
        {
            float u = Mathf.Clamp01(t / 1.2f);
            foreach (var layer in layers) layer.SpeedMultiplier = Mathf.Lerp(1f, 3f, u);
            kaliAnimator.speed = Mathf.Lerp(1f, 2.2f, u);
            kali.position = Vector3.Lerp(origin, new Vector3(endX, origin.y, 0), u * u);
            mainPanel.alpha = 1f - Mathf.SmoothStep(0, 1, u * 1.5f);
            fade.alpha = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.65f, 1f, u));
            yield return null;
        }
        fade.alpha = 1; fade.blocksRaycasts = true;
        RunManager.QueueMenuEntrance(gameScene);
        yield return SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
    }
    public void Quit()
    {
        MenuAudioSettings.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
        Application.Quit();
#endif
    }
    private void OnApplicationPause(bool paused) { if (paused) MenuAudioSettings.Save(); }
    private void OnApplicationQuit() { MenuAudioSettings.Save(); }
}
