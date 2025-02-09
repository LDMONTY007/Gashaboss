using UnityEngine;
using UnityEngine.SceneManagement;

// UIManager is a Singleton class that manages the UI Panels in the game
// It controls the Title Screen, In Game UI, Pause Menu, and transitions between them
// It also handles the game state (paused or not)
// The InGameUI currently also displays the BOSS health bar at all times, this needs to be set to only display during the boss fight
// However, the boss fight is not implemented yet, so this is a placeholder

public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance; 

    [Header("UI Panels")]
    public GameObject titleScreenPanel;
    public GameObject inGameUI;
    public GameObject pauseMenuPanel;

    private bool isPaused = false;

    private void Awake()
    {
        // Singleton: Only one instance can exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // If UIManager is missing in the scene, create it
        if (titleScreenPanel == null || pauseMenuPanel == null || inGameUI == null)
        {
            Debug.LogError("UIManager is missing. Make sure it's assigned in the Inspector.");
        }
    }

    // game always start with the title screen active
    private void Start()
    {
        ShowTitleScreen(); 
    }

    // Press ESC to toggle pause menu
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }


    // Show the Title Screen, all other menus are inactive
    public void ShowTitleScreen()
    {
        titleScreenPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        inGameUI.SetActive(false);
        Time.timeScale = 0; // Pause game until Start is pressed
    }

    // Start the Game (Title Screen deactivates, ingame UI starts)
    public void StartGame()
    {
        titleScreenPanel.SetActive(false);
        inGameUI.SetActive(true);
        Time.timeScale = 1; // Resume game
    }

    // Toggle Pause Menu (Pause Game)
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);
        inGameUI.SetActive(!isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    // Resume Game, dactivates pause menu and re-activates in game UI
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        inGameUI.SetActive(true);
        Time.timeScale = 1;
    }

    // Restart Level (keep or discard?) WE CAN PROBABLY DISCARD THIS
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Return to Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 0;
        SceneManager.LoadScene("TitleScreen"); // Make sure the scene exists
    }

    // Quit Game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit! (Only works in a built game)");
    }
}
