using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] int _points = 100;

    [Header("Audio")]
    [SerializeField] AudioClip _sfxClip = default;
    [SerializeField] [Range(0f, 1f)] float _sfxVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        GameSession gameSession = FindObjectOfType<GameSession>();
        gameSession.AddToScore(_points);

        Vector3 cameraPosition = Camera.main.transform.position;

        AudioSource.PlayClipAtPoint(_sfxClip, cameraPosition, _sfxVolume);

        Destroy(gameObject);
    }
}