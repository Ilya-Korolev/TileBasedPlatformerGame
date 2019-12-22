using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [SerializeField] int playerLives = 3;


    private void Awake()
    {
        int gameSessionNumber = FindObjectsOfType<GameSession>().Length;

        if (gameSessionNumber > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ProcessPlayerDamage()
    {
        if (playerLives > 1)
            TakeLife();
        else
            ResetGameSession();
    }

    private void TakeLife()
    {
        --playerLives;

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void ResetGameSession()
    {
        int menuIndex = 0;
        SceneManager.LoadScene(menuIndex);

        Destroy(gameObject);
    }
}