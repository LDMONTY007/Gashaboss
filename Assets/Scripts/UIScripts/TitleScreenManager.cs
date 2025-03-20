using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject titleScreenUI;
    [SerializeField] private GameObject saveMenuUI;
    [SerializeField] private Button continueGameButton;
    private SaveSlot[] saveSlots;
    private bool isLoading = false;

    private void Awake(){
        saveSlots = this.GetComponentsInChildren<SaveSlot>(true);
    }
    private void Start(){
        if (!SaveDataManager.instance.HasData()){
            continueGameButton.interactable = false;
        }
    }
    private void ActivateSaveMenu(bool isLoading){
        this.isLoading = isLoading;
        titleScreenUI.SetActive(false);
        saveMenuUI.SetActive(true);
        //Load all existing profiles for display
        Dictionary<string, GameData> profiles = SaveDataManager.instance.GetDataAllProfiles();

        foreach(SaveSlot saveslot in saveSlots){
            GameData profileData = null;
            //get data from dict, in not will be null and button will be set to empty
            profiles.TryGetValue(saveslot.GetProfileId(), out profileData);
            saveslot.SetData(profileData);
            //change slots so you can't interact with them while loading if empty
            if(profileData == null && isLoading)saveslot.SetInteractable(false);
            else saveslot.SetInteractable(true);
        }
    }
    private void ActivateMainMenu(){
        saveMenuUI.SetActive(false);
        titleScreenUI.SetActive(true);
    }
    public void OnNewGameClicked(){
        ActivateSaveMenu(false);
    }
    public void OnContinueClicked(){
        ActivateSaveMenu(true);
    }
    public void OnBackClicked(){
        ActivateMainMenu();
    }
    public void OnSaveSlotClicked(SaveSlot saveslot){
        //Change to profile and preload data
        SaveDataManager.instance.ChangeLoadedProfile(saveslot.GetProfileId());
        //overwrite data with new game data if not loading
        if(!isLoading)SaveDataManager.instance.NewGame();
        SaveDataManager.instance.SaveGame();
        SceneManager.LoadSceneAsync("GameUIScene"); //TODO: Change to load Gameplay Scene
    }
    public void StartGame()
    {
        // Handle New Game Loading
        SceneManager.LoadScene("GameUIScene"); // Load the main game scene
    }
    public void QuitGame()
    {
        SaveDataManager.instance.SaveGame();
        Application.Quit();
        Debug.Log("Game Quit! (Only works in a built game)");
    }
}
