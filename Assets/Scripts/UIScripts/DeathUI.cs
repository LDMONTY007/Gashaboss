using UnityEngine;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    public static DeathUI instance; // Singleton for easy access

    [Header("UI Elements")]
    public Button respawnButton; // Button to respawn

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //make the respawn button respawn the player.
        respawnButton.onClick.AddListener(Player.instance.Respawn);

        //turn off the death UI.
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    public void OpenDeathUI()
    {
        //set the DeathUI to be active.
        gameObject.SetActive(true);

        //set UI block to be true.
        //Remember we set this to false in the Respawn() method in the player.
        UIManager.Instance.uiBlock = true;
        SetCursorState(true);
        UIManager.Instance.currentUIState = UIManager.UIState.Death; // <<< USE STATE MACHINE
    }

    private void SetCursorState(bool uiMode)
    {
        if (uiMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //Time.timeScale = 0f; // Optional: pause game
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //Time.timeScale = 1f; // Resume game
        }
    }
}
