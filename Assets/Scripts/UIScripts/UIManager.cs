// UIManager is a Singleton class that manages the UI Panels in the game
// It controls the Title Screen, In Game UI, Pause Menu, and transitions between them
// It also handles the game state (paused or not)
// The InGameUI currently also displays the BOSS health bar at all times, this needs to be set to only display during the boss fight
// However, the boss fight is not implemented yet, so this is a placeholder

using UnityEngine;
using UnityEngine.SceneManagement;

// UIManager is a Singleton that manages the In-Game UI & Pause Menu
public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject inGameUI;
    public GameObject pauseMenuPanel;

    private bool isPaused = false;

    private void Awake()
    {
        // Singleton: Only one instance can exist
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Only keep UIManager in GameUIScene, not in TitleScreen
        if (SceneManager.GetActiveScene().name != "TitleScreen")
        {
            DontDestroyOnLoad(gameObject);
        }

        // Ensure Pause Menu is disabled on game start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);  // hide the pause menu
        }
    }

    // Start the Game (Load Game Scene)
    public void StartGame()
    {
        Time.timeScale = 1; // Resume normal game speed
        SceneManager.LoadScene("GameUIScene"); // Ensure this scene exists
    }

    // Press ESC to toggle pause menu
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    //LD Montello
    //unlocks the cursor and makes it visible
    //when the game is paused,
    //and then locks the cursor to the center
    //and makes it invisible while in game play.
    public void HandleCursorStates()
    {
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = !isPaused;
    }

    // Toggle Pause Menu (Pause Game)
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);
        inGameUI.SetActive(!isPaused);
        Time.timeScale = isPaused ? 0 : 1;
        HandleCursorStates();
    }

    // Resume Game
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        inGameUI.SetActive(true);
        Time.timeScale = 1;
        HandleCursorStates();
    }

    // Quit to Title Screen (Load Title Scene)
    public void QuitToTitle()
    {
        Time.timeScale = 1;  // Ensure normal speed
        SceneManager.LoadScene("TitleScreen"); // Load the Title Screen scene
        Destroy(gameObject); // Remove UIManager from memory since it's not needed in TitleScreen

        //LD Montello.
        //unlock the cursor and make it visible.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Quit Game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit! (Only works in a built game)");
    }
}

