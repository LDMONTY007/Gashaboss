// UIManager is a Singleton class that manages the UI Panels in the game
// It controls the Title Screen, In Game UI, Pause Menu, and transitions between them
// It also handles the game state (paused or not)

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// UIManager is a Singleton that manages the In-Game UI & Pause Menu
public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance;

    [Header("UI Managers")]
    public PlayerUIManager playerUIManager;
    public BossUIManager bossUIManager;

    [Header("UI Panels")]
    public GameObject inGameUI;
    public GameObject pauseMenuPanel;

    private bool isPaused = false;

    // UI Block Flag
    public bool uiBlock = false; // True when a non-pause UI (e.g., Collection or ObjectViewer) is open

    public enum UIState { None, Collection, ObjectViewer, Pause }
    public UIState currentUIState = UIState.None;


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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (currentUIState)
            {
                case UIState.ObjectViewer:
                    ObjectViewer.instance.CloseViewer();
                    break;
                case UIState.Collection:
                    CollectionManager.instance.CloseCollection();
                    break;
                case UIState.Pause:
                    ResumeGame();
                    break;
                case UIState.None:
                    TogglePause();
                    break;
            }
        }
    }




    // LD Montello
    // unlocks the cursor and makes it visible
    // when the game is paused,
    // and then locks the cursor to the center
    // and makes it invisible while in game play.
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

        currentUIState = isPaused ? UIState.Pause : UIState.None;
    }


    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        inGameUI.SetActive(true);
        Time.timeScale = 1;
        HandleCursorStates();

        currentUIState = UIState.None;
    }


    // Quit to Title Screen (Load Title Scene)
    public void QuitToTitle()
    {
        Time.timeScale = 1;  // Ensure normal speed
        SaveDataManager.instance.SaveGame();
        SceneManager.LoadScene("TitleScreen"); // Load the Title Screen scene
        Destroy(gameObject); // Remove UIManager from memory since it's not needed in TitleScreen

        // LD Montello.
        // unlock the cursor and make it visible.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Quit Game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit! (Only works in a built game)");
    }

    // LD Montello
    // will disable the boss ui
    // object when false. 
    // used to hide/display the boss UI if a boss is alive or dead.
    // called from the boss itself.
    public void SetBossUI(bool visible)
    {
        bossUIManager.gameObject.SetActive(visible);
    }
}
