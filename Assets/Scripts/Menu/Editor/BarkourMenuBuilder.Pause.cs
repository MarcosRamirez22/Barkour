using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class BarkourMenuBuilder
{
    [MenuItem("Barkour/Install Pause Menu")]
    private static void InstallPauseMenu()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.name != "Game")
        {
            Debug.LogError("Open the Game scene in Edit Mode first.");
            return;
        }

        RectTransform viewport = null;
        LevelChunkSpawner spawner = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var installed = root.GetComponentInChildren<BarkourPauseMenu>(true);
            if (installed != null)
            {
                Selection.activeGameObject = installed.gameObject;
                return;
            }

            if (viewport == null && root.TryGetComponent<Canvas>(out _))
                viewport = root.transform.Find("GameplayViewport") as RectTransform;
            if (spawner == null) spawner = root.GetComponentInChildren<LevelChunkSpawner>(true);
        }

        if (viewport == null || spawner == null)
        {
            Debug.LogError("The Game scene needs GameplayViewport and LevelChunkSpawner.");
            return;
        }

        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        var rootRect = Rect("PauseMenu", viewport, Vector2.zero, Vector2.zero);
        Stretch(rootRect);
        var overlay = Box("Overlay", rootRect, Vector2.zero, Vector2.zero, new Color(.16f, .2f, .19f, .65f), true);
        Stretch(overlay);
        var card = Box("PauseCard", overlay, Vector2.zero, new Vector2(650f, 620f), Cream, true);
        var outline = card.gameObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Brown;
        outline.effectDistance = new Vector2(4f, -4f);
        Label("Title", card, "PAUSED", new Vector2(0f, 251f), new Vector2(560f, 64f), 54f, Brown, FontStyles.Bold);
        var seed = Label("Seed", card, "SEED", new Vector2(0f, 190f), new Vector2(560f, 48f), 27f, Brown);
        var status = Label("Status", card, "", new Vector2(0f, -270f), new Vector2(570f, 38f), 23f, Brown);

        var actions = Rect("Actions", card, Vector2.zero, Vector2.zero);
        Stretch(actions);
        var resume = Button(actions, "Resume", new Vector2(0f, 104f), new Vector2(390f, 70f), Sage);
        var restart = Button(actions, "Restart", new Vector2(0f, 15f), new Vector2(390f, 70f), Cream);
        var openSettings = Button(actions, "Settings", new Vector2(0f, -74f), new Vector2(390f, 70f), Cream);
        var mainMenu = Button(actions, "Main Menu", new Vector2(0f, -163f), new Vector2(390f, 70f), Cream);
        Navigate(new UnityEngine.UI.Selectable[] { resume, restart, openSettings, mainMenu });

        var settings = Rect("Settings", card, Vector2.zero, Vector2.zero);
        Stretch(settings);
        var master = Slider(settings, "Master", 100f, out var masterValue);
        var music = Slider(settings, "Music", 6f, out var musicValue);
        var sfx = Slider(settings, "Sfx", -88f, out var sfxValue);
        var back = Button(settings, "Back", new Vector2(0f, -190f), new Vector2(260f, 65f), Sage);
        Navigate(new UnityEngine.UI.Selectable[] { master, music, sfx, back });
        settings.gameObject.SetActive(false);

        var controller = rootRect.gameObject.AddComponent<BarkourPauseMenu>();
        var wiring = new SerializedObject(controller);
        var references = new (string name, Object value)[]
        {
            ("overlay", overlay.gameObject), ("actions", actions.gameObject), ("settings", settings.gameObject),
            ("resume", resume), ("restart", restart), ("openSettings", openSettings), ("mainMenu", mainMenu), ("back", back),
            ("master", master), ("music", music), ("sfx", sfx),
            ("masterValue", masterValue), ("musicValue", musicValue), ("sfxValue", sfxValue),
            ("seed", seed), ("status", status), ("spawner", spawner)
        };
        foreach (var (name, value) in references)
            wiring.FindProperty(name).objectReferenceValue = value;

        wiring.ApplyModifiedPropertiesWithoutUndo();
        Undo.RegisterCreatedObjectUndo(rootRect.gameObject, "Install Barkour Pause Menu");
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = rootRect.gameObject;
    }
}
