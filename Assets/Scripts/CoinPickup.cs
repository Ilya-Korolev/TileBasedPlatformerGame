using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] AudioClip _sfxClip = default;
    [SerializeField] [Range(0f, 1f)] float _sfxVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Vector3 cameraPosition = Camera.main.transform.position;

        AudioSource.PlayClipAtPoint(_sfxClip, cameraPosition, _sfxVolume);

        Destroy(gameObject);
    }
}