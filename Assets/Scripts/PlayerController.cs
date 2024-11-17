using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;

    public AnimatorController walkFront, walkBack, walkRight, walkDownRight, walkUpRight;
    public AnimatorController idleFront, idleBack, idleRight, idleDownRight, idleUpRight;

    public float moveSpeed;
    private Vector2 inputDirection;
    private Vector2 lastDirection = Vector2.down; // Default idle direction
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private Dictionary<Vector2, AnimatorController> walkAnimations;
    private Dictionary<Vector2, AnimatorController> idleAnimations;

    private void OnEnable()
    {
        Ticker.OnTickAction += Tick;
    }

    private void OnDisable()
    {
        Ticker.OnTickAction -= Tick;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = transform;

        // Initialize animation dictionaries
        walkAnimations = new Dictionary<Vector2, AnimatorController>
        {
            { Vector2.right, walkRight },
            { Vector2.left, walkRight },
            { new Vector2(1, 1).normalized, walkUpRight },
            { new Vector2(-1, 1).normalized, walkUpRight },
            { new Vector2(1, -1).normalized, walkDownRight },
            { new Vector2(-1, -1).normalized, walkDownRight },
            { Vector2.down, walkFront },
            { Vector2.up, walkBack }
        };

        idleAnimations = new Dictionary<Vector2, AnimatorController>
        {
            { Vector2.right, idleRight },
            { Vector2.left, idleRight },
            { new Vector2(1, 1).normalized, idleUpRight },
            { new Vector2(-1, 1).normalized, idleUpRight },
            { new Vector2(1, -1).normalized, idleDownRight },
            { new Vector2(-1, -1).normalized, idleDownRight },
            { Vector2.down, idleFront },
            { Vector2.up, idleBack }
        };
    }

    private void Update()
    {
        inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = moveSpeed * inputDirection.normalized;
    }

    private void Tick()
    {
        AnimatorController currentAnimation;

        if (inputDirection != Vector2.zero) {
            // Set walking animation based on current input direction
            currentAnimation = walkAnimations.TryGetValue(inputDirection.normalized, out var walkAnim) ? walkAnim : null;

            // Flip player for left movement
            player.localScale = new Vector3(inputDirection.x < 0 ? -1 : 1, 1, 1);
        } else {
            // Set idle animation based on lastDirection
            currentAnimation = idleAnimations.TryGetValue(lastDirection, out var idleAnim) ? idleAnim : null;
        }

               // Update lastDirection if there is movement input
        if (inputDirection != Vector2.zero) {
            lastDirection = inputDirection.normalized;
        }

        // Update animation only if changed
        if (currentAnimation != animator.runtimeAnimatorController)
        {
            animator.runtimeAnimatorController = currentAnimation;
        }
    }
}
