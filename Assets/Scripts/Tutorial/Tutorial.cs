using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public Toggle pickupSwordToggle;
    public Toggle attackToggle;
    public Toggle altAttackToggle;
    public Toggle specialAttackToggle;
    public Toggle dashToggle;
    public Toggle jumpToggle;
    public Toggle gashaInteractToggle;
    public Toggle bossDefeatedToggle;
    public Toggle collectionOpenToggle;

    public GachaMachine gachaMachine;

    public CollectionManager collectionManager;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //subscribe to each of the events in the player for the different attacks and abilities
        //so we can register if they've fulfilled the different parts of the tutorial.
        Player.instance.OnPickup += OnPickupSword;
        Player.instance.OnAttack += OnAttack;
        Player.instance.OnAltAttack += OnAltAttack;
        Player.instance.OnSpecialAttack += OnSpecialAttack;
        Player.instance.OnDash += OnDash;
        Player.instance.OnJump += OnJump;

        //Add the callback for the gachamachine
        gachaMachine.OnDispense += OnGashaDispense;
        gachaMachine.BossDefeated += OnBossDefeated;

        collectionManager.OnCollectionOpen += OnOpenCollection;
        collectionManager.OnCollectionClose += OnCloseCollection;

        //set the toggles all to be false.
        pickupSwordToggle.isOn = false;
        attackToggle.isOn = false;
        altAttackToggle.isOn = false;
        specialAttackToggle.isOn = false;
        dashToggle.isOn = false;
        jumpToggle.isOn = false;
        gashaInteractToggle.isOn = false;
        bossDefeatedToggle.isOn = false;
        collectionOpenToggle.isOn = false;

        //start the tutorial
        StartCoroutine(TutorialCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region bools and methods so we know when the player has completed each tutorial task.

    public bool didPickupSword = false;

    public void OnPickupSword()
    {
        didPickupSword = true;
        pickupSwordToggle.isOn = true;
    }

    public bool didAttack = false;

    public void OnAttack()
    {
        didAttack = true;
        attackToggle.isOn = true;
    }

    public bool didAltAttack = false;

    public void OnAltAttack()
    {
        didAltAttack = true;
        altAttackToggle.isOn = true;
    }

    public bool didSpecialAttack = false;

    public void OnSpecialAttack()
    {
        didSpecialAttack = true;
        specialAttackToggle.isOn = true;
    }

    public bool didDash = false;

    public void OnDash()
    {
        didDash = true;
        dashToggle.isOn = true;
    }

    public bool didJump = false;

    public void OnJump()
    {
        didJump = true;
        jumpToggle.isOn = true;
    }

    public bool didGashaDispense = false;

    public void OnGashaDispense()
    {
        didGashaDispense = true;
        gashaInteractToggle.isOn = true;
    }

    public bool didDefeatBoss = false;

    public void OnBossDefeated()
    {
        didDefeatBoss = true;
        bossDefeatedToggle.isOn = true;
    }

    public bool didOpenCollection = false;

    public void OnOpenCollection()
    {
        didOpenCollection = true;
        collectionOpenToggle.isOn = true;
    }

    public bool didCloseCollection = false;

    //WE DON'T HAVE A TOGGLE FOR THIS BUT THE PLAYER
    //MUST CLOSE THE COLLECTION BEFORE WE SAY THE TUTORIAL IS OVER
    //AND LOAD INTO THE NEXT SCENE.
    public void OnCloseCollection()
    {
        didCloseCollection = true;
    }

    #endregion

    public IEnumerator TutorialCoroutine()
    {
        //for each part of the tutorial make a coroutine and yield return those coroutines in here 
        //for readability.
        //So for example if we want to have text popup and wait until the player does a specific attack
        //then we'll have a whole specific coroutine that will wait inside itself until the player does that input.

        //while any objective of the tutorial is incomplete
        //we sit here and wait until it is complete.
        while (!didAttack || !didAltAttack || !didSpecialAttack || !didDash || !didJump || !didPickupSword || !didGashaDispense || !didDefeatBoss || !didCloseCollection)
        {
            yield return null;
        }



        Debug.Log("HERE!!!");

        //Make a popup that shows that the tutorial is complete

        UIManager.Instance.popupUIManager.DoPopup("Tutorial Complete!", Color.black, false);

        //wait until the popup is done before we load into the gashapon scene.
        while (UIManager.Instance.popupUIManager.isDoingPopup)
        {
            yield return null;
        }

        //Say we completed the tutorial
        SaveDataManager.instance.gameData.didCompleteTutorial = true;

        //make sure to save the game data before we leave this scene.
        SaveDataManager.instance.SaveGame();

        //load into the gashapon scene.
        SceneManager.LoadScene("GatchaMachine_test");
    }

}
