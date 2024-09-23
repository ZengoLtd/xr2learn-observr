using UnityEngine;
using UnityEngine.UIElements;

public class HandleFPS : MonoBehaviour
{
    private UIDocument uiDocument;
    public PanelSettings panelSettings;
    public bool displayFPS = true;
    public float currentFps;
    public int targetFPS = 30;
    public bool vSyncOff = true;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();

        if (vSyncOff)
        {
            QualitySettings.vSyncCount = 0;
        }

        Application.targetFrameRate = targetFPS;
    }

    void Update()
    {
        if (displayFPS)
        {
            panelSettings.sortingOrder = 5;
            currentFps = 1 / Time.deltaTime;
            int intFps = (int)currentFps;
            uiDocument.rootVisualElement.Q<Label>("FPSLabel").text = intFps.ToString();
        }
        else
        {
            panelSettings.sortingOrder = -5;
        }
    }
}
