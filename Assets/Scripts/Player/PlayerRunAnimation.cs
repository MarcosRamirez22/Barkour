
using UnityEngine;

public class PlayerRunAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float minimumRunSpeed = 0.1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (rb == null || animator == null)
            return;

        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);

        bool isRunning = horizontalSpeed > minimumRunSpeed;

        animator.SetBool("IsRunning", isRunning);
    }
}
