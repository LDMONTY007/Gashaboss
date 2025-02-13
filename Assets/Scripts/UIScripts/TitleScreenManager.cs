using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    // start "new" game, currently also being used for "continue" game. Needs implementation of save files to proceed furtheer
    public void StartGame()
    {
        SceneManager.LoadScene("GameUIScene"); // Load the main game scene
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit! (Only works in a built game)");
    }
}
