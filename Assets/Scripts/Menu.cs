using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void StartFirstLevel()
    {
        var firstLevelIndex = 1;

        SceneManager.LoadScene(firstLevelIndex);
    }

    public void LoadMainMenu()
    {
        var menuIndex = 0;

        SceneManager.LoadScene(menuIndex);
    }
}