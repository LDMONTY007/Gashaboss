using System.Collections;
using TMPro;
using UnityEngine;

public class PopupUIManager : MonoBehaviour
{
    public GameObject popupPrefab;

    private GameObject currentPopup;

    private Coroutine currentPopupRoutine = null;

    bool isDoingPopup = false;

    bool isPopupInterruptable = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Player.instance != null)
        {
            healthBarSlider.maxValue = Player.instance.health;
            healthBarSlider.value = Player.instance.curHealth;

            //make sure score text is aligned left
            scoreText.text = "SCORE: " + score.ToString();
        }*/
    }

    public void DoPopup(string textToDisplay, Color color, bool interruptable = true)
    {
        Debug.LogWarning(textToDisplay);

        /*if (!isDoingPopup)*/

        //cancel the previous popup
        //and create the new one
        if (currentPopupRoutine != null)
        {
            if (isPopupInterruptable)
            {
                //set the text color.
                popupPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.clear;




                StopCoroutine(currentPopupRoutine);
                Destroy(currentPopup);
                if (interruptable)
                    Debug.LogWarning("HERE");
                currentPopupRoutine = StartCoroutine(PopupCoroutine(textToDisplay, interruptable, color));
            }
        }
        else
        {
            //set the text color.
            popupPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.clear;




            currentPopupRoutine = StartCoroutine(PopupCoroutine(textToDisplay, interruptable, color));
        }


    }

    private IEnumerator PopupCoroutine(string textToDisplay, bool interruptable, Color color)
    {

        if (interruptable)
            Debug.LogWarning("HERE");

        isPopupInterruptable = interruptable;


        isDoingPopup = true;

        //the gameobject is disabled when created
        //so we shouldn't have any issues setting
        //the text before it plays it's animation.
        currentPopup = Instantiate(popupPrefab, transform);
        currentPopup.GetComponentInChildren<TextMeshProUGUI>().text = textToDisplay;
        currentPopup.SetActive(true);


        //this code is where we fade in the text and then fade out the text as part of the popup.

        //Fade the text color from alpha 0 to alpha 1 for our font color over 0.5 seconds
        yield return FadeTextRoutine(currentPopup.GetComponentInChildren<TextMeshProUGUI>(), Color.clear, color, 0.5f);

        //TODO: 
        //Wait here for like 0.5 seconds so that we can fade out after and this gives the player some time to read.
        yield return new WaitForSeconds(0.5f);


        //Fade the text color from alpha 1 to alpha 0 for our font color over 0.5 seconds
        yield return FadeTextRoutine(currentPopup.GetComponentInChildren<TextMeshProUGUI>(), color, Color.clear, 0.5f);


        Destroy(currentPopup);
        isDoingPopup = false;

        //set back to false so the pop up can be interrupted again.
        if (interruptable == false)
        {
            isPopupInterruptable = true;
        }
    }

    public IEnumerator FadeTextRoutine(TextMeshProUGUI textToFade, Color startColor, Color endColor, float totalTime)
    {
        float curTime = 0f;

        //loop until reaching the total time.
        while (curTime < totalTime)
        {
            //set the text color to be the lerped color based on our time.
            textToFade.color = Color.Lerp(startColor, endColor, curTime / totalTime);

            //increment time
            curTime += Time.deltaTime;

            yield return null;
        }

        //set the end color.
        textToFade.color = endColor;

        //exit coroutine.
    }
}
