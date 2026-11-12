using UnityEngine;

// Preserves the gameplay width and bottom edge authored for Full HD.
[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(Camera))]
public sealed class GameplayFraming : MonoBehaviour
{
    [SerializeField] private float referenceHalfHeight = 10f;
    [SerializeField] private float referenceAspect = 16f / 9f;
    [SerializeField] private float tallestAspect = 16f / 10f;
    [SerializeField] private RectTransform hudViewport;
    [SerializeField] private UnityEngine.UI.CanvasScaler canvasScaler;
    [SerializeField] private RectTransform leftBar, rightBar, bottomBar, topBar;
    private Camera target;
    private float bottom;
    private int width, height;
    public float GameplayWidth => 2f * referenceHalfHeight * referenceAspect;

    private void Awake()
    {
        target = GetComponent<Camera>();
        bottom = transform.position.y - referenceHalfHeight;
        Apply();
    }
    private void Update()
    {
        if (Screen.width != width || Screen.height != height) Apply();
    }
    private void Apply()
    {
        width = Screen.width; height = Screen.height;
        float screenAspect = width / (float)Mathf.Max(1, height);
        float viewAspect = Mathf.Clamp(screenAspect, tallestAspect, referenceAspect);
        Rect view = new Rect(0, 0, 1, 1);
        if (screenAspect > viewAspect) { view.width = viewAspect / screenAspect; view.x = (1f - view.width) * .5f; }
        else { view.height = screenAspect / viewAspect; view.y = (1f - view.height) * .5f; }
        target.rect = view;
        target.aspect = viewAspect;
        target.orthographicSize = GameplayWidth / (2f * viewAspect);
        var position = transform.position;
        position.y = bottom + target.orthographicSize;
        transform.position = position;
        Fit(hudViewport, view);
        // Scale against the playable viewport, including when black borders are present.
        canvasScaler.referenceResolution = new Vector2(1920f / view.width, 1080f / view.height);
        Fit(leftBar, new Rect(0, 0, view.xMin, 1));
        Fit(rightBar, new Rect(view.xMax, 0, 1f - view.xMax, 1));
        Fit(bottomBar, new Rect(view.xMin, 0, view.width, view.yMin));
        Fit(topBar, new Rect(view.xMin, view.yMax, view.width, 1f - view.yMax));
    }
    private static void Fit(RectTransform rect, Rect bounds)
    {
        rect.anchorMin = bounds.min; rect.anchorMax = bounds.max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
