using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InstallOffscreenKaliIndicator
{
    [MenuItem("Barkour/Install Editable Offscreen Indicator")]
    private static void Install()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.name != "Game")
        {
            Debug.LogError("Open the Game scene in Edit Mode first.");
            return;
        }

        RecoveryCountdownUI countdown = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            countdown = root.GetComponentInChildren<RecoveryCountdownUI>(true);
            if (countdown != null) break;
        }

        if (countdown == null)
        {
            Debug.LogError("The Game scene is missing RecoveryCountdownUI.");
            return;
        }

        var settings = new SerializedObject(countdown);
        var reference = settings.FindProperty("indicator");
        if (reference.objectReferenceValue is OffscreenKaliIndicator existing)
        {
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var label = settings.FindProperty("countdownText").objectReferenceValue as TMP_Text;
        var failure = settings.FindProperty("failureSystem").objectReferenceValue as FailureSystem;
        var canvas = label == null ? null : label.GetComponentInParent<Canvas>();
        if (canvas == null || failure == null || failure.RecoveringPlayer == null)
        {
            Debug.LogError("Assign the recovery countdown text and player before installing the indicator.");
            return;
        }

        canvas = canvas.rootCanvas;
        var badge = new GameObject("Offscreen Kali Indicator", typeof(RectTransform), typeof(CanvasRenderer));
        badge.transform.SetParent(canvas.transform, false);
        var indicator = badge.AddComponent<OffscreenKaliIndicator>();
        indicator.Initialize(canvas, failure.RecoveringPlayer, label);

        var rect = (RectTransform)badge.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, .5f);
        rect.anchoredPosition = new Vector2(12f, 0f);
        Undo.RegisterCreatedObjectUndo(badge, "Install Offscreen Kali Indicator");
        reference.objectReferenceValue = indicator;
        settings.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = badge;
    }
}
