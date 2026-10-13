using UnityEngine;

public class FailureSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Recovery")]
    [SerializeField] private float recoveryTime = 3f;

    private float recoveryTimer;
    private bool isRecovering;

    public bool IsRecovering => isRecovering;
    public float RecoveryTimer => recoveryTimer;

    private void Awake()
    {
        recoveryTimer = recoveryTime;
    }

    private void Update()
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunning)
        {
            return;
        }

        if (playerMovement.IsOffScreenLeft())
        {
            HandleRecovery();
        }
        else
        {
            ResetRecovery();
        }
    }

    private void HandleRecovery()
    {
        if (!isRecovering)
        {
            isRecovering = true;
            recoveryTimer = recoveryTime;
        }

        recoveryTimer -= Time.deltaTime;

        if (recoveryTimer <= 0f)
        {
            recoveryTimer = 0f;
            isRecovering = false;
            RunManager.Instance.FailRun();
        }
    }

    private void ResetRecovery()
    {
        if (!isRecovering)
        {
            return;
        }

        isRecovering = false;
        recoveryTimer = recoveryTime;
    }
}