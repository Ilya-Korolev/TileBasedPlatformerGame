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
    }

    private void Run()
    {
        var horizontalThrow = Input.GetAxis("Horizontal");
        var playerVelocity = _rigidbody2D.velocity;
        var deltaAcceleration = Time.deltaTime * horizontalThrow != 0f ? _runAcceleration : _groundDeceleration;

        playerVelocity.x = Mathf.MoveTowards(playerVelocity.x, horizontalThrow * _runSpeed, deltaAcceleration);

        _rigidbody2D.velocity = playerVelocity;
    }
}