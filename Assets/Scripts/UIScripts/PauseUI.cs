using UnityEngine;

public class PauseUI : MonoBehaviour
{
    //This script is used so we don't need to reassign button method references
    //when loading scenes.

    public void ResumeGame()
    {
        UIManager.Instance.ResumeGame();
    }

    public void QuitToTitle()
    {
        UIManager.Instance.QuitToTitle();
    }
}
