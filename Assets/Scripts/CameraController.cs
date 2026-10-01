using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Scrolling")]
    [SerializeField] private float startingSpeed = 2.5f;
    [SerializeField] private float maximumSpeed = 5f;
    [SerializeField] private float acceleration = 0.05f;

    public float ScrollSpeed { get; private set; }

    private void Awake()
    {
        ScrollSpeed = startingSpeed;
    }

    private void Update()
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunning)
        {
            return;
        }

        ScrollSpeed = Mathf.MoveTowards(
            ScrollSpeed,
            maximumSpeed,
            acceleration * Time.deltaTime
        );

        transform.position +=
            Vector3.right * ScrollSpeed * Time.deltaTime;
    }
}