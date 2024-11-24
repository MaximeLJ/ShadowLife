using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;

    public Animator animator;

    public float moveSpeed;
    public float transitionDampTime;
    private Vector2 inputDirection;
    private Vector2 lastDirection = Vector2.down; // Default idle direction
    private Rigidbody2D rb;
    private Transform player;


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
        player = transform;
    }

    private void Update()
    {
        inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        animator.SetFloat("Speed", inputDirection.sqrMagnitude);

        // Update the animator parameters only when there is significant movement
        if (inputDirection.sqrMagnitude > 0.1f) {
            animator.SetFloat("Horizontal", inputDirection.x, transitionDampTime, Time.deltaTime);
            animator.SetFloat("Vertical", inputDirection.y, transitionDampTime, Time.deltaTime);
        } else {
            // When stopping, use the last direction for the idle animation
            animator.SetFloat("Horizontal", lastDirection.x);
            animator.SetFloat("Vertical", lastDirection.y);
        }
        
        rb.linearVelocity = moveSpeed * inputDirection.normalized;
    }
    
    private void Tick()
    {
        if (inputDirection.sqrMagnitude > 0.1f) {
            lastDirection = inputDirection.normalized; // Save the last direction
        }
    }
}
