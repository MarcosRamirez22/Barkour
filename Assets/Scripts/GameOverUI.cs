
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        bool hasFailed =
            RunManager.Instance.CurrentState ==
            RunManager.RunState.Failed;

        if (gameOverPanel.activeSelf != hasFailed)
        {
            gameOverPanel.SetActive(hasFailed);
        }
    }

    public void RestartGame()
    {
        RunManager.Instance.RestartRun();
    }
}
