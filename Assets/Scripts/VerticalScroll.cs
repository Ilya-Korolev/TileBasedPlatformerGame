using UnityEngine;

public class VerticalScroll : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 2f;

    private Rigidbody2D _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _rigidbody.velocity = new Vector2(0f, _scrollSpeed);
    }
}