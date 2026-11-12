using TMPro;
using UnityEngine;

public class RecoveryCountdownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FailureSystem failureSystem;
    [SerializeField] private TMP_Text countdownText;

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
            failureSystem.RecoveryTimer.ToString("F2");
    }
}