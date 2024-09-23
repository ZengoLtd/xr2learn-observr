using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class FadeAlphaByName : MonoBehaviour
{
    public string elementName = "ElementName";
    private VisualElement visualElement;

    public float fadeInAlpha = 0.95f;
    public float fadeOutAlpha = 0.0f;
    public float fadeTime = 1.0f;


    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        visualElement = uiDocument.rootVisualElement.Q(elementName);

        if (visualElement == null)
        {
            Debug.LogError("Nem található VisualElement ezzel a névvel: " + elementName);
            this.enabled = false;
        }
        
    }


    public void FadeIn()
    {
        float startAlpha = fadeOutAlpha;
        float endAlpha = fadeInAlpha;
        float duration = fadeTime;

        ChangeAlpha(startAlpha, endAlpha, duration);
    }

    public void FadeOut()
    {
        float startAlpha = fadeInAlpha;
        float endAlpha = fadeOutAlpha;
        float duration = fadeTime;

        ChangeAlpha(startAlpha, endAlpha, duration);
    }


    public void ChangeAlpha(float startAlpha, float endAlpha, float duration)
    {
        if (visualElement != null)
        {
            StartCoroutine(FadeAlpha(startAlpha, endAlpha, duration));
        }
    }

    private IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);

            Color currentColor = visualElement.resolvedStyle.backgroundColor;
            currentColor.a = newAlpha;
            visualElement.style.backgroundColor = currentColor;

            yield return null;
        }
    }
}