using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private float _levelLoadDelay = 2f;
    [SerializeField] private float _exitLevelTimeScale = 0.2f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        StartCoroutine(LoadNextLevel(_levelLoadDelay));
    }

    private IEnumerator LoadNextLevel(float delay)
    {
        Time.timeScale = _exitLevelTimeScale;

        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 1f;

        var nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        SceneManager.LoadScene(nextSceneIndex);
    }
}