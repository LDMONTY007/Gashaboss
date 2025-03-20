using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour{
    [SerializeField] private string profileId;
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;
    [SerializeField] private TextMeshProUGUI profileIdText;
    [SerializeField] private TextMeshProUGUI coinsLeftText;
    private Button saveSlotButton;

    private void Awake(){
        saveSlotButton = this.GetComponent<Button>();
    }
    public void SetData(GameData data){
        if (data == null){
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
        }else{
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);

            profileIdText.text = profileId;
            coinsLeftText.text = data.coins + " Coins Left";
        }
    }
    public string GetProfileId(){
        return profileId;
    }
    public void SetInteractable(bool interactable){
        saveSlotButton.interactable = interactable;
    }
}