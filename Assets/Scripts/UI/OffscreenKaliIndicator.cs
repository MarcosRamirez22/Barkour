using TMPro;
using UnityEngine;

public sealed class OffscreenKaliIndicator : UnityEngine.UI.MaskableGraphic
{
    private const float Radius = 30f;
    private const float CircleX = 54f;
    [SerializeField, Min(0f)] private float edgeInset = 12f;
    [SerializeField] private UnityEngine.UI.Image portrait;
    [SerializeField] private TMP_Text distance;
    private Canvas canvas;
    private PlayerMovement player;
    private int displayedMeters = -1;

    public void Bind(PlayerMovement recoveringPlayer)
    {
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        player = recoveringPlayer;
    }

    public void Initialize(Canvas rootCanvas, PlayerMovement recoveringPlayer, TMP_Text textStyle)
    {
        canvas = rootCanvas;
        player = recoveringPlayer;
        raycastTarget = false;
        rectTransform.pivot = new Vector2(0f, .5f);
        rectTransform.sizeDelta = new Vector2(84f, 2f * Radius);

        var portraitObject = new GameObject("Kali", typeof(RectTransform));
        portrait = portraitObject.AddComponent<UnityEngine.UI.Image>();
        portrait.raycastTarget = false;
        portrait.preserveAspect = true;
        PlaceChild(portrait.rectTransform, new Vector2(CircleX, 0f), new Vector2(46f, 46f));
        var renderer = player.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null) portrait.sprite = renderer.sprite;
        portrait.enabled = portrait.sprite != null;

        var label = new GameObject("Meters to camera", typeof(RectTransform));
        distance = label.AddComponent<TextMeshProUGUI>();
        distance.font = textStyle.font;
        distance.fontSize = 25f;
        distance.fontStyle = FontStyles.Bold;
        distance.color = new Color32(255, 243, 213, 255);
        distance.raycastTarget = false;
        distance.alignment = TextAlignmentOptions.Center;
        distance.text = "3 m";
        PlaceChild(distance.rectTransform, new Vector2(CircleX, -48f), new Vector2(100f, 30f));
    }

    public void Refresh()
    {
        Camera camera = player.GameplayCamera;
        Rect view = camera.rect;
        float y = Mathf.Clamp01(camera.WorldToViewportPoint(player.RecoveryIndicatorPosition).y);
        var anchor = new Vector2(view.xMin, Mathf.Lerp(view.yMin, view.yMax, y));
        rectTransform.anchorMin = rectTransform.anchorMax = anchor;

        // Clamp the badge and its label inside the playable viewport.
        float height = ((RectTransform)canvas.transform).rect.height * view.height;
        float localY = Mathf.Lerp(Mathf.Min(76f, height * .5f),
            height - Mathf.Min(42f, height * .5f), y);
        rectTransform.anchoredPosition = new Vector2(edgeInset, localY - height * y);

        int meters = Mathf.CeilToInt(player.DistanceBehindCamera);
        if (meters == displayedMeters) return;
        displayedMeters = meters;
        distance.text = $"{meters} m";
    }

    private void PlaceChild(RectTransform child, Vector2 position, Vector2 size)
    {
        child.SetParent(rectTransform, false);
        child.anchorMin = child.anchorMax = new Vector2(0f, .5f);
        child.anchoredPosition = position;
        child.sizeDelta = size;
    }

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        DrawBadge(mesh, Radius, 0f, new Color32(32, 39, 54, 255));
        DrawBadge(mesh, Radius - 3f, 6f, new Color32(104, 175, 126, 255));
    }

    private static void DrawBadge(UnityEngine.UI.VertexHelper mesh, float radius, float tipX, Color32 tint)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(new Vector2(tipX, 0f), tint, Vector2.zero);
        mesh.AddVert(new Vector2(CircleX, radius), tint, Vector2.zero);
        mesh.AddVert(new Vector2(CircleX, -radius), tint, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2);

        const int segments = 40;
        int center = mesh.currentVertCount;
        mesh.AddVert(new Vector2(CircleX, 0f), tint, Vector2.zero);
        for (int i = 0; i <= segments; i++)
        {
            float angle = -i * 2f * Mathf.PI / segments;
            var point = new Vector2(CircleX + Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            mesh.AddVert(point, tint, Vector2.zero);
            if (i > 0) mesh.AddTriangle(center, center + i, center + i + 1);
        }
    }
}
