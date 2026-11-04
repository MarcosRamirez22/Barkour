
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Parallax Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxSpeed = 0.05f;

    private SpriteRenderer spriteRenderer;
    private float tileWidth;
    private float worldTileWidth;
    private float previousCameraX;
    private bool initialized;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null || spriteRenderer.sprite == null ||
            !targetCamera.orthographic)
        {
            Debug.LogError("ParallaxLayer: Missing camera or sprite.");
            enabled = false;
            return;
        }

        tileWidth = spriteRenderer.sprite.bounds.size.x;
        worldTileWidth = tileWidth * Mathf.Abs(transform.lossyScale.x);

        if (worldTileWidth <= 0f)
        {
            enabled = false;
            return;
        }

        // Make enough copies to cover the camera view.
        float cameraWidth = targetCamera.orthographicSize *
                            2f * targetCamera.aspect;

        int copies = Mathf.CeilToInt(cameraWidth / worldTileWidth) + 1;

        for (int i = 1; i <= copies; i++)
        {
            CreateRepeat(-i);
            CreateRepeat(i);
        }

        previousCameraX = targetCamera.transform.position.x;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        float cameraX = targetCamera.transform.position.x;
        float cameraMovement = cameraX - previousCameraX;

        // Move slower than the camera.
        transform.position += Vector3.right *
            (cameraMovement * (1f - parallaxSpeed));

        previousCameraX = cameraX;

        // Wrap the repeating sprites as the camera moves.
        float difference = cameraX - transform.position.x;

        while (difference > worldTileWidth / 2f)
        {
            transform.position += Vector3.right * worldTileWidth;
            difference -= worldTileWidth;
        }

        while (difference < -worldTileWidth / 2f)
        {
            transform.position -= Vector3.right * worldTileWidth;
            difference += worldTileWidth;
        }
    }

    private void CreateRepeat(int offset)
    {
        GameObject repeat = new GameObject("Repeat_" + offset);
        repeat.transform.SetParent(transform, false);
        repeat.transform.localPosition =
            new Vector3(offset * tileWidth, 0f, 0f);

        SpriteRenderer renderer = repeat.AddComponent<SpriteRenderer>();

        renderer.sprite = spriteRenderer.sprite;
        renderer.color = spriteRenderer.color;
        renderer.sharedMaterial = spriteRenderer.sharedMaterial;
        renderer.sortingLayerID = spriteRenderer.sortingLayerID;
        renderer.sortingOrder = spriteRenderer.sortingOrder;
        renderer.flipX = spriteRenderer.flipX;
        renderer.flipY = spriteRenderer.flipY;
    }
}
