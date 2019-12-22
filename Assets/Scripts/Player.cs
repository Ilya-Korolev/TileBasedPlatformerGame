using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Congig
    [Header("Run settings")]
    [SerializeField] private float _runSpeed = 10f;

    [Header("Jump settings")]
    [SerializeField] private float _jumpSpeed = 20f;

    [Header("Climb settings")]
    [SerializeField] private float _climbSpeed = 5f;

    // States
    private CharacterState _currentState;
    private float _animationSpeed = 1f;
    private float _distanceToGround = 0f;
    private float _velocityY = 0f;
    private bool _isOnGround;

    // Cached component references
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private BoxCollider2D _feetCollider2D;
    private BoxCollider2D _bodyCollider2D;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        _bodyCollider2D = transform.Find("Colliders/Вody Collider").GetComponent<BoxCollider2D>();
        _feetCollider2D = transform.Find("Colliders/Feet Collider").GetComponent<BoxCollider2D>();

        _currentState = CharacterState.Idling;
    }

    private void Update()
    {
        if (_currentState == CharacterState.Dying)
            return;

        _distanceToGround = GetDistanceToGround();
        _velocityY = _rigidbody2D.velocity.y;
        _isOnGround = _feetCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

        ChangeStateOnTurningEnds();
        ChangeStateOnLandingEnds();

        Run();

        Jump();
        InAir();
        Land();

        TurnAwayBeforeClimbing();
        Climb();
        TurnTowardAfterClimbing();

        Flip();

        Die();

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
        if (_currentState != CharacterState.Running && _currentState != CharacterState.Idling && _currentState != CharacterState.Climbing)
            return;

        if (!Input.GetButtonDown("Jump"))
            return;

        _rigidbody2D.velocity += new Vector2(0f, _jumpSpeed);

        if (_currentState == CharacterState.Climbing)
        {
            _animationSpeed = 1f;
            _rigidbody2D.gravityScale = 1f;

            _currentState = CharacterState.InAir;
        }
    }

    private void InAir()
    {
        if (_currentState != CharacterState.Running && _currentState != CharacterState.Idling)
            return;

        if (_distanceToGround > Mathf.Epsilon && !_isOnGround)
            _currentState = CharacterState.InAir;
    }

    private void Land()
    {
        if (_currentState != CharacterState.InAir)
            return;

        if (_isOnGround)
            _currentState = CharacterState.Landing;
    }

    private void TurnAwayBeforeClimbing()
    {
        if (_currentState != CharacterState.Idling)
            return;
        
        float controlVerticalThrow = Input.GetAxis("Vertical");

        bool isUpPressed = controlVerticalThrow > Mathf.Epsilon;

        if (!isUpPressed || !_isOnGround)
            return;

        List<Collider2D> climbColliders = OverlapClimbColliders();

        if (climbColliders.Count == 0)
            return;

        Collider2D climbCollider = climbColliders[0];

        transform.position = new Vector2(climbCollider.bounds.center.x, transform.position.y);
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

        if (!_isOnGround || !isDownPressed)
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

    private void Die()
    {
        bool isTouchingEnemy = _bodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards"));

        if (!isTouchingEnemy)
            return;

        _currentState = CharacterState.Dying;

        int deathLayer = LayerMask.NameToLayer("Death");

        _bodyCollider2D.gameObject.layer = deathLayer;
        _feetCollider2D.gameObject.layer = deathLayer;

        _rigidbody2D.velocity = Vector2.zero;
    }

    private float GetDistanceToGround()
    {
        int groundLayerMask = LayerMask.GetMask("Ground");

        Vector2 bottomPointLeft = _feetCollider2D.bounds.min;
        Vector2 bottomPointRight = new Vector2(_feetCollider2D.bounds.max.x, _feetCollider2D.bounds.min.y);

        RaycastHit2D groundHitLeft = Physics2D.Raycast(bottomPointLeft, Vector2.down, 10f, groundLayerMask);
        RaycastHit2D groundHitRight = Physics2D.Raycast(bottomPointRight, Vector2.down, 10f, groundLayerMask);

        return Mathf.Min(groundHitLeft.distance, groundHitRight.distance);
    }

    private void UpdateAnimation()
    {
        _animator.SetBool("IsIdling", false);
        _animator.SetBool("IsRunning", false);
        _animator.SetBool("IsInAir", false);
        _animator.SetBool("IsLanding", false);
        _animator.SetBool("IsClimbing", false);
        _animator.SetBool("IsTurning", false);
        _animator.SetBool("IsDying", false);

        _animator.SetBool($"Is{_currentState}", true);

        _animator.SetFloat("AnimationSpeed", _animationSpeed);
        _animator.SetFloat("DistanceToGround", _distanceToGround);
        _animator.SetFloat("VelocityY", _velocityY);
    }
}