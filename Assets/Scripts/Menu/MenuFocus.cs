using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MenuFocus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool hovered;
    private UnityEngine.UI.Selectable control;
    private void Awake() { control = GetComponent<UnityEngine.UI.Selectable>(); }
    public void OnPointerEnter(PointerEventData e) { hovered = true; }
    public void OnPointerExit(PointerEventData e) { hovered = false; }
    private void OnDisable() { hovered = false; transform.localScale = Vector3.one; }
    private void Update()
    {
        bool focused = control.IsInteractable() && (hovered ||
            (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject));
        float target = focused ? 1.045f + Mathf.Sin(Time.unscaledTime * 7f) * .008f : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * target,
            1f - Mathf.Exp(-16f * Time.unscaledDeltaTime));
    }
}
