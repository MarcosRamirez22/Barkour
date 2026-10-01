using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class RunManager : MonoBehaviour
{
    public enum RunState
    {
        WaitingToStart,
        Running,
        Failed
    }

    public static RunManager Instance { get; private set; }

    public RunState CurrentState { get; private set; } =
        RunState.WaitingToStart;

    public bool IsRunning =>
        CurrentState == RunState.Running;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (CurrentState != RunState.WaitingToStart)
        {
            return;
        }

        if (AnyInputPressed())
        {
            StartRun();
        }
    }

    private bool AnyInputPressed()
    {
        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button &&
                    button.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void StartRun()
    {
        CurrentState = RunState.Running;
    }

    public void FailRun()
    {
        if (CurrentState != RunState.Running)
        {
            return;
        }

        CurrentState = RunState.Failed;
    }
}