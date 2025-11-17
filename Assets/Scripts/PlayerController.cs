using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour {
  [SerializeField] private float jumpForce = 5f;
  [SerializeField] private LayerMask groundMask;
  [SerializeField] private Transform groundCheck;
  [SerializeField] private float groundCheckRadius = 0.1f;

  private Rigidbody2D _rb;
  private Animator _animator;

  public bool IsGrounded { get; private set; }

  private void Start() {
    _rb = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();
  }

  private void Update() {
    IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

    if (Input.GetKeyDown(KeyCode.Space) && IsGrounded) {
      _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0f);
      _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    _animator.SetBool("isJumping", !IsGrounded);
  }

}
