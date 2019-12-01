using UnityEngine;

public class Player : MonoBehaviour
{
    // Congig
    [Header("Run settings")]
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _runAcceleration = 100f;
    [SerializeField] private float _groundDeceleration = 100f;

    [Header("Jump settings")]
    [SerializeField] private float _jumpSpeed = 20f;

    // State
    private bool _isRunning;
    private bool _hasHorizontalSpeed;

    // Cached componentReferences
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private Collider2D _collider2D;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Run();
        Jump();
        Flip();
    }

    private void Run()
    {
        float horizontalThrow = Input.GetAxis("Horizontal");
        bool isButtonPressed = horizontalThrow != 0f;
        float deltaAcceleration = Time.deltaTime * (isButtonPressed ? _runAcceleration : _groundDeceleration);
        Vector2 playerVelocity = _rigidbody2D.velocity;

        playerVelocity.x = Mathf.MoveTowards(playerVelocity.x, horizontalThrow * _runSpeed, deltaAcceleration);

        _rigidbody2D.velocity = playerVelocity;
        _hasHorizontalSpeed = Mathf.Abs(_rigidbody2D.velocity.x) > Mathf.Epsilon;
        _isRunning = _hasHorizontalSpeed;
        _animator.SetBool("IsRunning", _isRunning);
    }

    private void Jump()
    {
        bool isOnGround = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (!Input.GetButtonDown("Jump") || !isOnGround)
            return;

        Vector2 jumpVelocityToAdd = new Vector2(0f, _jumpSpeed);
        _rigidbody2D.velocity += jumpVelocityToAdd;
    }

    private void Flip()
    {
        if (_hasHorizontalSpeed)
        {
            float playerHorizontalDirection = Mathf.Sign(_rigidbody2D.velocity.x);
            Vector2 playerScale = new Vector2(playerHorizontalDirection, transform.localScale.y);

            transform.localScale = playerScale;
        }
    }
}