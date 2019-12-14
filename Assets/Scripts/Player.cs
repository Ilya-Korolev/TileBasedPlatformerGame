using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Congig
    [Header("Run settings")]
    [SerializeField] private float _runSpeed = 10f;

    [Header("Jump settings")]
    [SerializeField] private float _jumpSpeed = 20f;
    [SerializeField] private float _landingPredictTime = 0.1f;

    [Header("Climb settings")]
    [SerializeField] private float _climbSpeed = 5f;
    [SerializeField] private float _climbOffset = 0.114f;

    // State
    private CharacterState _currentState;
    private float _animationSpeed = 1f;

    // Cached component references
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private Collider2D _collider2D;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();

        _currentState = CharacterState.Idling;
    }

    private void Update()
    {
        ChangeStateOnTurningEnds();
        ChangeStateOnLandingEnds();

        Run();

        Jump();
        Fall();
        Land();

        TurnAwayBeforeClimbing();
        Climb();
        TurnTowardAfterClimbing();

        Flip();

        UpdateAnimation();
    }

    private void ChangeStateOnTurningEnds()
    {
        if (_currentState != CharacterState.Turning)
            return;

        bool currentAnimationIsIdling = _animator.GetCurrentAnimatorStateInfo(0).IsName("Idling");

        if (currentAnimationIsIdling)
        {
            _currentState = CharacterState.Idling;
            return;
        }

        bool currentAnimationIsClimbing = _animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing");

        if (currentAnimationIsClimbing)
        {
            _currentState = CharacterState.Climbing;
            return;
        }
    }

    private void ChangeStateOnLandingEnds()
    {
        if (_currentState != CharacterState.Landing)
            return;

        bool currentAnimationIsIdling = _animator.GetCurrentAnimatorStateInfo(0).IsName("Idling");

        if (currentAnimationIsIdling)
            _currentState = CharacterState.Idling;
    }

    private void Run()
    {
        if (_currentState == CharacterState.Turning || _currentState == CharacterState.Climbing)
            return;

        float controlHorizontalThrow = Input.GetAxis("Horizontal");

        _rigidbody2D.velocity = new Vector2(_runSpeed * controlHorizontalThrow, _rigidbody2D.velocity.y);

        bool hasHorizontalSpeed = Mathf.Abs(_rigidbody2D.velocity.x) > Mathf.Epsilon;

        if (hasHorizontalSpeed && (_currentState == CharacterState.Idling || _currentState == CharacterState.Landing))
            _currentState = CharacterState.Running;

        if (!hasHorizontalSpeed && _currentState == CharacterState.Running)
            _currentState = CharacterState.Idling;
    }

    private void Jump()
    {
        if (_currentState != CharacterState.Running && _currentState != CharacterState.Idling)
            return;

        if (!Input.GetButtonDown("Jump"))
            return;

        _rigidbody2D.velocity += new Vector2(0f, _jumpSpeed);
        _currentState = CharacterState.Jumping;
    }

    private void Fall()
    {
        if (_currentState != CharacterState.Running && _currentState != CharacterState.Jumping)
            return;

        bool isOnGroundLayer = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
        bool playerGoingDown = _rigidbody2D.velocity.y < -Mathf.Epsilon;

        if (!isOnGroundLayer && playerGoingDown)
            _currentState = CharacterState.Falling;
    }

    private void Land()
    {
        if (_currentState != CharacterState.Falling)
            return;

        float gravitationalAcceleration = Physics2D.gravity.y;
        float langingPredictDistanse = Mathf.Abs(_rigidbody2D.velocity.y * _landingPredictTime + gravitationalAcceleration * Mathf.Pow(_landingPredictTime, 2f) / 2f);

        Vector2 bottomPointLeft = _collider2D.bounds.min;
        Vector2 bottomPointRight = new Vector2(_collider2D.bounds.max.x, _collider2D.bounds.min.y);

        RaycastHit2D groundHitLeft = Physics2D.Raycast(bottomPointLeft, Vector2.down, langingPredictDistanse, LayerMask.GetMask("Ground"));
        RaycastHit2D groundHitRight = Physics2D.Raycast(bottomPointRight, Vector2.down, langingPredictDistanse, LayerMask.GetMask("Ground"));

        if (groundHitLeft || groundHitRight)
            _currentState = CharacterState.Landing;
    }

    private void TurnAwayBeforeClimbing()
    {
        if (_currentState != CharacterState.Idling)
            return;
        
        float controlVerticalThrow = Input.GetAxis("Vertical");

        bool isUpPressed = controlVerticalThrow > Mathf.Epsilon;
        bool isOnGroundLayer = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (!isUpPressed || !isOnGroundLayer)
            return;

        List<Collider2D> climbColliders = OverlapClimbColliders();

        if (climbColliders.Count == 0)
            return;

        Collider2D climbCollider = climbColliders[0];

        float playerDirection = Mathf.Sign(transform.localScale.x);
        float climbPositionX = climbCollider.bounds.center.x + playerDirection * _climbOffset;

        transform.position = new Vector2(climbPositionX, transform.position.y);
        _rigidbody2D.velocity = Vector2.zero;

        _currentState = CharacterState.Turning;
    }

    private List<Collider2D> OverlapClimbColliders()
    {
        ContactFilter2D climbingContactFilter = new ContactFilter2D();
        climbingContactFilter.SetLayerMask(LayerMask.GetMask("Climbing"));
        climbingContactFilter.useTriggers = true;

        List<Collider2D> climbColliders = new List<Collider2D>();

        _rigidbody2D.OverlapCollider(climbingContactFilter, climbColliders);

        return climbColliders;
    }

    private void TurnTowardAfterClimbing()
    {
        if (_currentState != CharacterState.Climbing)
            return;

        float controlVerticalThrow = Input.GetAxis("Vertical");

        bool isDownPressed = controlVerticalThrow < -Mathf.Epsilon;
        bool isOnGroundLayer = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (!isOnGroundLayer || !isDownPressed)
            return;

        _rigidbody2D.velocity = Vector2.zero;
        _rigidbody2D.gravityScale = 1f;
        _animationSpeed = 1f;

        _currentState = CharacterState.Turning;
    }

    private void Climb()
    {
        if (_currentState != CharacterState.Climbing)
            return;

        float controlVerticalThrow = Input.GetAxis("Vertical");

        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.velocity = new Vector2(0f, controlVerticalThrow * _climbSpeed);

        _animationSpeed = controlVerticalThrow;
    }

    private void Flip()
    {
        if (_currentState == CharacterState.Turning || _currentState == CharacterState.Climbing)
            return;

        bool playerHasHorizontalSpeed = Mathf.Abs(_rigidbody2D.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            float playerHorizontalDirection = Mathf.Sign(_rigidbody2D.velocity.x);
            Vector2 playerScale = new Vector2(playerHorizontalDirection, transform.localScale.y);

            transform.localScale = playerScale;
        }
    }

    private void UpdateAnimation()
    {
        _animator.SetBool("IsIdling", false);
        _animator.SetBool("IsRunning", false);
        _animator.SetBool("IsJumping", false);
        _animator.SetBool("IsFalling", false);
        _animator.SetBool("IsLanding", false);
        _animator.SetBool("IsClimbing", false);
        _animator.SetBool("IsTurning", false);

        _animator.SetBool($"Is{_currentState}", true);

        _animator.SetFloat("AnimationSpeed", _animationSpeed);
    }
}