using UnityEngine;

public class Slime : MonoBehaviour
{
    // Congig
    [Header("Move settings")]
    [SerializeField] private float _moveSpeed = 4f;

    // Cached component references
    private Rigidbody2D _rigidbody2D;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float moveDirection = Mathf.Sign(transform.localScale.x);

        _rigidbody2D.velocity = new Vector2(moveDirection * _moveSpeed, _rigidbody2D.velocity.y);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Flip();
    }

    private void Flip()
    {
        float oppositeDirection = -Mathf.Sign(_rigidbody2D.velocity.x);
        transform.localScale = new Vector3(oppositeDirection, 1f, 1f);
    }
}