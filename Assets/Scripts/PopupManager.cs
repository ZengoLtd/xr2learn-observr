using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PopupManager : MonoBehaviour
{

    public GameObject popupObject;
    private UIDocument uiDocument;
    private VisualElement bodyElement;

    public float animationDuration = 4f;

    public int openTop = 200;
    public int hideTop = -600;

    public PanelSettings panelSettingsMain;
    public PanelSettings panelSettingsFadeLayer;
    public PanelSettings panelSettingsPopupPanel;

    FadeAlphaByName fadeAlphaByName;

    void Start()
    {
        uiDocument = popupObject.GetComponent<UIDocument>();
        fadeAlphaByName = FindObjectOfType<FadeAlphaByName>();

        SetPanelsToDefaultOrder();

        if (uiDocument != null)
        {
            bodyElement = uiDocument.rootVisualElement.Q<VisualElement>("BodyElement");

            if (bodyElement != null)
            {
                bodyElement.style.top = hideTop;
            }
            else
            {
                Debug.LogError("BodyElement nem található az UXML-ben!");
            }
        }
        else
        {
            Debug.LogError("UIDocument nem található a popupObject-en!");
        }
    }

    public void ShowPopup()
    {
        if (bodyElement != null)
        {
            Debug.Log("Body Element mozgatás");


            MovePopup(bodyElement);

            StartCoroutine(OpenPopupCoroutine());

            bodyElement.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.5f, TimeUnit.Second) }); // Set transition duration to 0.5 seconds
            bodyElement.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
        }
    }


    public void ClosePopup()
    {
        if (bodyElement != null)
        {
            Debug.Log("Body Element mozgatás");


            MovePopup(bodyElement);

            StartCoroutine(ClosePopupCoroutine());

            bodyElement.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.5f, TimeUnit.Second) }); // Set transition duration to 0.5 seconds
            bodyElement.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
        }
    }



    protected void MovePopup(VisualElement element)
    {
        int topValue = openTop;

        Debug.Log("Body Element mozgatás");

        element.experimental.animation.Start(element.resolvedStyle.top, hideTop, 1, (b, val) =>
        {
            b.style.transitionDuration = new List<TimeValue>() { new TimeValue(animationDuration, TimeUnit.Second) };
            b.style.top = new StyleLength(hideTop);
        }).Ease(Easing.Linear);
    }

    IEnumerator OpenPopupCoroutine() 
    {
        SortingOrderNegate();
        fadeAlphaByName.FadeIn();
        bodyElement.style.top = hideTop;
        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            elapsedTime = elapsedTime + Time.deltaTime;
            float newTop = Mathf.Lerp(hideTop, openTop, elapsedTime / animationDuration);
            bodyElement.style.top = newTop;

            yield return null;  // kivárja egy framet
        }
        bodyElement.style.top = openTop;

    }

    IEnumerator ClosePopupCoroutine()  // popup.style.top értékét megváltoztatja openTop értékre moveTime idõ alatt 
    {
        fadeAlphaByName.FadeOut();
        bodyElement.style.top = openTop;
        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            elapsedTime = elapsedTime + Time.deltaTime;
            float newTop = Mathf.Lerp(openTop, hideTop, elapsedTime / animationDuration);
            bodyElement.style.top = newTop;

            yield return null;  // kivárja egy framet
        }
        bodyElement.style.top = hideTop;

        yield return new WaitForSeconds(animationDuration + 0.1f);
        SortingOrderNegate();
    }

    public void HidePopup()
    {
        if (bodyElement != null)
        {
            bodyElement.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.5f, TimeUnit.Second) }); // Set transition duration to 0.5 seconds
            bodyElement.style.bottom = new StyleLength(new Length(-100, LengthUnit.Percent));

        }
    }
    void SetPanelsToDefaultOrder()
    {

        if (panelSettingsMain != null)
            panelSettingsMain.sortingOrder = 0;

        if (panelSettingsFadeLayer != null)
            panelSettingsFadeLayer.sortingOrder = 1;

        if (panelSettingsFadeLayer != null)
            panelSettingsPopupPanel.sortingOrder = 2;

        SortingOrderNegate();
    }

    void SortingOrderNegate()
    {

        if (panelSettingsFadeLayer != null)
        {
            panelSettingsFadeLayer.sortingOrder = -panelSettingsFadeLayer.sortingOrder;
        }
        else
        {
            Debug.Log("panelSettingsFadeLayer missing");
        }

        if (panelSettingsPopupPanel != null)
        {
            panelSettingsPopupPanel.sortingOrder = -panelSettingsPopupPanel.sortingOrder;
        }
        else
        {
            Debug.Log("panelSettingsSetupPanel missing");
        }
    }
}