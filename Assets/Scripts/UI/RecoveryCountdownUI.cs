using TMPro;
using UnityEngine;

public class RecoveryCountdownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FailureSystem failureSystem;
    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private OffscreenKaliIndicator indicator;

    private void Start()
    {
        if (indicator == null) return;
        indicator.Bind(failureSystem.RecoveringPlayer);
        indicator.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (indicator == null) return;
        var player = failureSystem.RecoveringPlayer;
        bool show = failureSystem.IsRecovering && player.GameplayCamera != null &&
            player.DistanceBehindCamera > 0f;
        indicator.gameObject.SetActive(show);
        if (show) indicator.Refresh();
    }

    private void Awake()
    {
        countdownText.enabled = false;
    }

    private void Update()
    {
        if (!failureSystem.IsRecovering)
        {
            countdownText.enabled = false;
            return;
        }

        countdownText.enabled = true;
        countdownText.text =
            "RECOVER  " + failureSystem.RecoveryTimer.ToString("F2") + "s";
    }
}
