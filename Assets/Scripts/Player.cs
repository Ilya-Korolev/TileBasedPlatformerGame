using System.Collections.Generic;
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

    [Header("Climb settings")]
    [SerializeField] private float _climbSpeed = 20f;
    [SerializeField] private float _climbOffset = 0.114f;

    // State
    private bool _isRunning;
    private bool _isTurning;
    private bool _isClimbing;

    // Cached componentReferences
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private Collider2D _collider2D;

    public void SetClimbingState()
    {
        _isClimbing = true;
    }
    public void ClearAllStates()
    {
        _isRunning = false;
        _isTurning = false;
        _isClimbing = false;

        _animator.SetBool("IsRunning", false);
        _animator.SetBool("IsTurning", false);
        _animator.SetBool("IsClimbing", false);

        _animator.SetFloat("AnimationSpeed", 1f);
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Run();
        Climb();
        Jump();
        Flip();
    }

    private void Run()
    {
        if (_isClimbing || _isTurning)
        {
            _isRunning = false;
            _animator.SetBool("IsRunning", false);
            return;
        }

        float controlHorizontalThrow = Input.GetAxis("Horizontal");
        bool isButtonPressed = controlHorizontalThrow != 0f;
        float deltaAcceleration = Time.deltaTime * (isButtonPressed ? _runAcceleration : _groundDeceleration);
        Vector2 playerVelocity = _rigidbody2D.velocity;

        playerVelocity.x = Mathf.MoveTowards(playerVelocity.x, controlHorizontalThrow * _runSpeed, deltaAcceleration);

        _rigidbody2D.velocity = playerVelocity;
        _isRunning = Mathf.Abs(_rigidbody2D.velocity.x) > Mathf.Epsilon;
        _animator.SetBool("IsRunning", _isRunning);
    }

    private void Climb()
    {
        bool isOnGroundLayer = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (isOnGroundLayer && !_isRunning && !_isTurning && !_isClimbing && Input.GetButtonDown("Action"))
        {
            List<Collider2D> climbingColliders = OverlapClimbingColliders();

            if (climbingColliders.Count == 0)
                return;

            _isTurning = true;
            _animator.SetBool("IsTurning", true);

            Collider2D ladderToClimbCollider = climbingColliders[0];

            float playerDirection = Mathf.Sign(transform.localScale.x);
            float objectToClimbPositionX = ladderToClimbCollider.bounds.center.x + playerDirection * _climbOffset;

            transform.position = new Vector2(objectToClimbPositionX, transform.position.y);
            _rigidbody2D.velocity = Vector2.zero;
        }

        if (isOnGroundLayer && !_isTurning && _isClimbing && Input.GetButtonDown("Action"))
        {
            _isClimbing = false;
            _animator.SetBool("IsClimbing", false);

            _isTurning = true;
            _animator.SetBool("IsTurning", true);
            _rigidbody2D.velocity = Vector2.zero;
        }

        if (_isTurning)
        {
            _rigidbody2D.velocity = Vector2.zero;
        }

        if (_isClimbing)
        {
            _isTurning = false;
            _animator.SetBool("IsTurning", false);

            float controlVerticalThrow = Input.GetAxis("Vertical");
            float climbDirection = Mathf.Sign(controlVerticalThrow);
            float animationSpeed = controlVerticalThrow != 0f ? climbDirection : 0f;

            _animator.SetFloat("AnimationSpeed", animationSpeed);
            _animator.SetBool("IsClimbing", true);

            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.velocity = new Vector2(0f, controlVerticalThrow * _climbSpeed);
        }

        if (!_isClimbing)
        {
            _rigidbody2D.gravityScale = 1f;
        }
    }

    private List<Collider2D> OverlapClimbingColliders()
    {
        ContactFilter2D climbingContactFilter = new ContactFilter2D();
        climbingContactFilter.SetLayerMask(LayerMask.GetMask("Climbing"));
        climbingContactFilter.useTriggers = true;

        List<Collider2D> ladderColliders = new List<Collider2D>();

        _rigidbody2D.OverlapCollider(climbingContactFilter, ladderColliders);

        return ladderColliders;
    }

    private void Jump()
    {
        bool isOnGroundLayer = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (!Input.GetButtonDown("Jump") || !isOnGroundLayer)
            return;

        Vector2 jumpVelocityToAdd = new Vector2(0f, _jumpSpeed);
        _rigidbody2D.velocity += jumpVelocityToAdd;
    }

    private void Flip()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(_rigidbody2D.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            float playerHorizontalDirection = Mathf.Sign(_rigidbody2D.velocity.x);
            Vector2 playerScale = new Vector2(playerHorizontalDirection, transform.localScale.y);

            transform.localScale = playerScale;
        }
    }
}