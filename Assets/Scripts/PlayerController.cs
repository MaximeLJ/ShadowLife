using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;

    public Animator animator;

    public float moveSpeed;
    private Vector2 inputDirection;
    private Vector2 lastDirection = Vector2.down; // Default idle direction
    private Rigidbody2D rb;
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
        player = transform;
    }

    private void Update()
    {
        inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        animator.SetFloat("Horizontal", inputDirection.x);
        animator.SetFloat("Vertical", inputDirection.y);
        animator.SetFloat("Speed", inputDirection.sqrMagnitude);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveSpeed * inputDirection.normalized;
    }

    private void Tick()
    {
        
    }
}
