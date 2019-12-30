using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [SerializeField] private int _playerLives = 3;
    [SerializeField] private int _playerScore = 0;

    [Header("UI")]
    [SerializeField] public LivesBar _livesBar = default;
    [SerializeField] public Text _scoreText = default;

    private void Awake()
    {
        int gameSessionNumber = FindObjectsOfType<GameSession>().Length;

        if (gameSessionNumber > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _livesBar.SetLives(_playerLives);
        _scoreText.text = _playerScore.ToString();
    }

    public void ProcessPlayerDamage()
    {
        if (_playerLives > 1)
            TakeLife();
        else
            ResetGameSession();
    }

    public void AddToScore(int pointToAdd)
    {
        _playerScore += pointToAdd;
        _scoreText.text = _playerScore.ToString();
    }

    private void TakeLife()
    {
        --_playerLives;
        _livesBar.RemoveLifeContainer();

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void ResetGameSession()
    {
        int menuIndex = 0;
        SceneManager.LoadScene(menuIndex);

        var scenePersist = FindObjectOfType<ScenePersist>();

        Destroy(scenePersist);
        Destroy(gameObject);
    }
}