
using TMPro;
using UnityEngine;

public class DistanceScore : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text bestDistanceText;

    private const string BestDistanceKey = "BestDistance";

    public int CurrentDistance => Mathf.FloorToInt(farthestX - startingX);
    public int BestDistance => bestDistance;

    private float startingX;
    private float farthestX;
    private int bestDistance;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("DistanceScore: Player is not assigned.");
            enabled = false;
            return;
        }

        startingX = player.position.x;
        farthestX = startingX;

        bestDistance = PlayerPrefs.GetInt(BestDistanceKey, 0);

        UpdateScoreText(0);
    }

    private void Update()
    {
        farthestX = Mathf.Max(farthestX, player.position.x);

        int currentDistance = Mathf.FloorToInt(farthestX - startingX);

        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            PlayerPrefs.SetInt(BestDistanceKey, bestDistance);
        }

        UpdateScoreText(currentDistance);
    }

    private void UpdateScoreText(int currentDistance)
    {
        if (distanceText != null)
            distanceText.text = $"DISTANCE: {currentDistance} m";

        if (bestDistanceText != null)
            bestDistanceText.text = $"BEST: {bestDistance} m";
    }
}

