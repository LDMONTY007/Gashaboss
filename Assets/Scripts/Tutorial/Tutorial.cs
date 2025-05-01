using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public Toggle attackToggle;
    public Toggle altAttackToggle;
    public Toggle specialAttackToggle;
    public Toggle dashToggle;
    public Toggle jumpToggle;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
/*        //Unsub from these events OnDisable so we don't have errors on scene exits.
        Player.instance.OnAttack -= OnAttack;
        Player.instance.OnAltAttack -= OnAltAttack;
        Player.instance.OnSpecialAttack -= OnSpecialAttack;
        Player.instance.OnDash -= OnDash;
        Player.instance.OnJump -= OnJump;*/
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //subscribe to each of the events in the player for the different attacks and abilities
        //so we can register if they've fulfilled the different parts of the tutorial.
        Player.instance.OnAttack += OnAttack;
        Player.instance.OnAltAttack += OnAltAttack;
        Player.instance.OnSpecialAttack += OnSpecialAttack;
        Player.instance.OnDash += OnDash;
        Player.instance.OnJump += OnJump;

        //set the toggles all to be false.
        attackToggle.isOn = false;
        altAttackToggle.isOn = false;
        specialAttackToggle.isOn = false;
        dashToggle.isOn = false;
        jumpToggle.isOn = false;

        //start the tutorial
        StartCoroutine(TutorialCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region bools and methods so we know when the player has completed each tutorial task.

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

    #endregion

    public IEnumerator TutorialCoroutine()
    {
        //for each part of the tutorial make a coroutine and yield return those coroutines in here 
        //for readability.
        //So for example if we want to have text popup and wait until the player does a specific attack
        //then we'll have a whole specific coroutine that will wait inside itself until the player does that input.

        //while any objective of the tutorial is incomplete
        //we sit here and wait until it is complete.
        while (!didAttack || !didAltAttack || !didSpecialAttack || !didDash || !didJump)
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
