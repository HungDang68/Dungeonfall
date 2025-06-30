using UnityEngine;
using Mirror;

public class PlayerAnimator : NetworkBehaviour
{
    private Animator animator;
    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        if (!isLocalPlayer) return;
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        Animate();
    }
    private void Animate()
    {
        WalkAnimation();
    }

    private void WalkAnimation()
    {
        if (!isLocalPlayer) return;
        if (rb2d.linearVelocity.magnitude >= 0.1f && rb2d.linearVelocity.x > 0.1f)
        {
            animator.SetBool("Is Walking Right", true);
            animator.SetBool("Is Walking Left", false);
        }
        else if (rb2d.linearVelocity.magnitude >= 0.1f && rb2d.linearVelocity.x < -0.1f)
        {
            animator.SetBool("Is Walking Right", false);
            animator.SetBool("Is Walking Left", true);
        }
        else
        {
            animator.SetBool("Is Walking Right", false);
            animator.SetBool("Is Walking Left", false);
        }

        if (rb2d.linearVelocity.y > 0.1f)
        {
            animator.SetBool("Is Walking Up", true);
        }
        else
        {
            animator.SetBool("Is Walking Up", false);
        }

        if (rb2d.linearVelocity.y < -0.1f)
        {
            animator.SetBool("Is Walking Down", true);
        }
        else
        {
            animator.SetBool("Is Walking Down", false);
        }
    }
}
