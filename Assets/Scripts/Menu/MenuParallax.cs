using UnityEngine;

// Menu-only movement; no gameplay camera, physics, or run-state dependencies.
public sealed class MenuParallax : MonoBehaviour
{
    public float speed = 1f;
    public float period = 20f;
    public float SpeedMultiplier { get; set; } = 1f;
    private Vector3 origin;
    private float offset;
    private void Awake() { origin = transform.localPosition; }
    private void Update()
    {
        offset = Mathf.Repeat(offset + speed * SpeedMultiplier * Time.unscaledDeltaTime, period);
        transform.localPosition = origin + Vector3.left * offset;
    }
}
