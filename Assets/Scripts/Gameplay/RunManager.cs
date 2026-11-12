
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

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

    private static string entranceScene;
    public bool IsEnteringFromMenu { get; private set; }

    public static void QueueMenuEntrance(string sceneName)
    {
        entranceScene = sceneName;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetMenuEntrance()
    {
        entranceScene = null;
    }

    public void CompleteMenuEntrance()
    {
        IsEnteringFromMenu = false;
        IgnoreStartInputThisFrame();
    }

    private int ignoreStartInputFrame = -1;

    public void IgnoreStartInputThisFrame()
    {
        ignoreStartInputFrame = Time.frameCount;
    }

    private void Awake()
    {
        Instance = this;
        IsEnteringFromMenu = gameObject.scene.name == entranceScene;
        entranceScene = null;
    }

    private void Update()
    {
        if (IsEnteringFromMenu || Time.timeScale <= 0f || Time.frameCount == ignoreStartInputFrame ||
            CurrentState != RunState.WaitingToStart)
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

    public void RestartRun()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
