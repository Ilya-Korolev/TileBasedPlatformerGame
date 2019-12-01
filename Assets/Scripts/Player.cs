using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _runAcceleration = 10f;
    [SerializeField] private float _groundDeceleration = 10f;

    private Rigidbody2D _rigidbody2D = default;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Run();
        Flip();
    }

    private void Run()
    {
        float horizontalThrow = Input.GetAxis("Horizontal");
        Vector2 playerVelocity = _rigidbody2D.velocity;
        float deltaAcceleration = Time.deltaTime * horizontalThrow != 0f ? _runAcceleration : _groundDeceleration;

        playerVelocity.x = Mathf.MoveTowards(playerVelocity.x, horizontalThrow * _runSpeed, deltaAcceleration);

        _rigidbody2D.velocity = playerVelocity;
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