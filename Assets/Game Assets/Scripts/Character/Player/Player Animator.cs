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

        FlipSprite();
    }

    private void WalkAnimation()
    {
        if (!isLocalPlayer) return;
        if (rb2d.linearVelocity.magnitude >= 0.1f)
        {
            animator.SetBool("Is Walking", true);
        }
        else
        {
            animator.SetBool("Is Walking", false);
        }
    }

    private void FlipSprite()
    {
        if (!isLocalPlayer) return;
        if (rb2d.linearVelocity.x > 0.1f)
        {
            CmdSetFlipX(false);
        }
        else if (rb2d.linearVelocity.x < -0.1f)
        {
            CmdSetFlipX(true);
        }
    }

    [Command]// Update on the server
    private void CmdSetFlipX(bool flipX)
    {
        OnFlipXChanged(flipX);
    }

    [ClientRpc]//Client call this
    private void OnFlipXChanged(bool flipX)
    {
        spriteRenderer.flipX = flipX;
    }
}
