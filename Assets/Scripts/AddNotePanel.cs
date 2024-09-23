using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddNotePanel : MonoBehaviour
{

    public PopupManager popupManager;

    [SerializeField] UIDocument panelUIDocument;
    private VisualElement root;

    private bool isClicked = false;
    private static AddNotePanel instance;
    private string newDescription;
    private ScreenshotData selectedScreenshot;

    public static AddNotePanel Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AddNotePanel>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AddNotePanel");
                    instance = obj.AddComponent<AddNotePanel>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        root = panelUIDocument.rootVisualElement;
        root.RegisterCallback<MouseDownEvent>(OnMouseDownOutside);
        root.Q<VisualElement>("RightAreaClick").RegisterCallback<MouseDownEvent>(evt => { CloseAddNotePanel(); });
        root.Q<VisualElement>("LeftAreaClick").RegisterCallback<MouseDownEvent>(evt => { CloseAddNotePanel(); });
        root.Q<VisualElement>("MidAreaClick").RegisterCallback<MouseDownEvent>(evt => { CloseAddNotePanel(); });
    }


    void ClickedOut()
    {

       if (isClicked)
        {
            return;
        }
        isClicked = true;

        CloseAddNotePanel();
    }



    void OnMouseDownOutside(MouseDownEvent evt)
    {
        if (isClicked)
        {
            return;
        }
        isClicked = true;

        if (evt.target is VisualElement ve && ve.name == "BodyElement")
        {
            CloseAddNotePanel();
        }
    }

    public void OpenAddNotePanel()
    {
        popupManager.ShowPopup();
        StartCoroutine(ResetClickState());
    }


    private IEnumerator ResetClickState()
    {
        yield return new WaitForSeconds(0.6f);
        isClicked = false;
    }

    public void CloseAddNotePanel()
    {
        SaveScreenshotDescriptionToJson();
        popupManager.ClosePopup();
    }



    public void EditLastScreenshot()
    {
        List<ScreenshotData> screenshots = ReportManager.Instance.GetActiveReportScreenshots();
        if (screenshots.Count > 0)
        {
            ScreenshotData lastScreenshot = screenshots[screenshots.Count - 1];
            SetScreenshotDetails(lastScreenshot, screenshots.Count - 1);
            OpenAddNotePanel();
        }
        else
        {
            Debug.LogError("No screenshots found.");
        }
    }


    public void EditSelectedScreenshot(int index)
    {
        List<ScreenshotData> screenshots = ReportManager.Instance.GetActiveReportScreenshots();
        if (screenshots.Count > 0)
        {
            ScreenshotData selectedScreenshot = screenshots[index];
            SetScreenshotDetails(selectedScreenshot, index);
            //Invoke(nameof(OpenAddNotePanel), 0.1f);
            OpenAddNotePanel();
        }
        else
        {
            Debug.LogError("No screenshots found.");
        }
    }




    public void SetScreenshotDetails(ScreenshotData screenshot, int scrnShotIndex)
    {
        selectedScreenshot = screenshot;

        var screenshotImage = root.Q<VisualElement>("AddNoteScreenshotImage");

        if (screenshotImage != null)
        {
            var texture = LoadTextureFromFile(screenshot.Path);
            if (texture != null)
            {
                screenshotImage.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                Debug.LogError("Failed to load texture from path: " + screenshot.Path);
            }
        }
        else
        {
            Debug.LogError("AddNoteScreenshotImage element not found.");
        }

        var dateLabel = root.Q<Label>("AddNoteDateText");

        if (dateLabel != null)
        {
            if (screenshot.Timestamp != null)
            {
                string stringDate = StringToDate(screenshot.Timestamp);
                string stringTime = StringToTime(screenshot.Timestamp);

                string fixedTimestamp = stringTime + "  " + stringDate;
                dateLabel.text = fixedTimestamp;
            }
        }
        else
        {
            Debug.LogError("DateLabel element not found.");
        }

        var scrnshIndexLabel = root.Q<Label>("ScrnshIndexLabel");
        if (scrnshIndexLabel != null)
        {
            string screenshotNo;
            scrnShotIndex++;
            if (scrnShotIndex < 10)
            {
                screenshotNo = "#0" + scrnShotIndex;
            }
            else
            {
                screenshotNo = "#" + scrnShotIndex;
            }
            scrnshIndexLabel.text = screenshotNo;
        }
        else
        {
            Debug.LogError("ScrnshIndexLabel element not found.");
        }

        var descField = root.Q<TextField>("DescField");

        if (descField != null)
        {
            descField.value = screenshot.Description;
            descField.RegisterValueChangedCallback(evt =>
            {
                UpdateScreenshotDescription(screenshot, evt.newValue);
            });
        }
        else
        {
            Debug.LogError("DescField element not found.");
        }

    }

    void UpdateScreenshotDescription(ScreenshotData screenshot, string newValue)
    {

        selectedScreenshot = screenshot;
        newDescription = newValue;
    }


    public void SaveScreenshotDescriptionToJson()
    {

        if (selectedScreenshot != null)
        {
            ReportManager.Instance.UpdateScreenshotDescriptionInJson(
            ReportManager.Instance.ActiveReport.ReportName,
            selectedScreenshot.Path,
            newDescription
        );

            Debug.Log("Screenshot description saved to JSON.");
        }
        else
        {
            Debug.LogWarning("No screenshot selected.");
        }
    }



    Texture2D LoadTextureFromFile(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"File not found at path: {path}");
            return null;
        }

        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
    public string StringToDate(string timestamp)
    {
        string[] parts = timestamp.Split('_');
        if (parts.Length >= 3)
        {
            return $"{parts[0]}.{parts[1]}.{parts[2]}";
        }
        return string.Empty;
    }


    public string StringToTime(string timestamp)
    {
        string[] parts = timestamp.Split('_');
        if (parts.Length >= 6)
        {
            return $"{parts[3]}:{parts[4]}:{parts[5]}";
        }
        return string.Empty;
    }

}




