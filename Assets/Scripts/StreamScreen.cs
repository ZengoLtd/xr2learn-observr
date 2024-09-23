using UnityEngine;
using UnityEngine.UIElements;

public class StreamScreen : MonoBehaviour
{
    [SerializeField] UIDocument mainUIDocument;
    VisualElement capturedStreamScreenTexture;
    public ScreenCapture screenCapture;

    bool isScreenCaptureNotNull = false;

    float ratio = 0.5625f;

    float streamScreenWidth;
    float lastStreamScreenWidth;

    public Texture2D noSignalImage;

    Color32[] blackArray;
    Texture2D blackTexture;


    private static StreamScreen instance;

    public static StreamScreen Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StreamScreen>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("StreamScreen");
                    instance = obj.AddComponent<StreamScreen>();
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
        /*
        blackTexture = new Texture2D(16, 9);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
        */

        int width = 16;
        int height = 9;

        blackArray = new Color32[width * height];

        for (int i = 0; i < blackArray.Length; i++)
        {
            blackArray[i] = new Color32(0, 0, 0, 255); 
        }

        blackTexture = new Texture2D(width, height);
        blackTexture.SetPixels32(blackArray);
        blackTexture.Apply();

        capturedStreamScreenTexture = mainUIDocument.rootVisualElement.Q<VisualElement>("StreamScreen");

        capturedStreamScreenTexture.style.backgroundPositionY = new StyleBackgroundPosition(0);
    }
    void Update()
    {
        AddTextureToScreenStream();
    }

    public void AddTextureToScreenStream()
    {

        if (screenCapture.trimmedTexture != null)
        {
            if (screenCapture.trimmedTexture.height >= screenCapture.trimmedTexture.width)
            {
                capturedStreamScreenTexture.style.backgroundImage = new StyleBackground(blackTexture);
                // noSignalImage helyett egy color tömb kellene ami #000000 fekete
            }
            else
            {
               capturedStreamScreenTexture.style.backgroundImage = new StyleBackground(screenCapture.trimmedTexture);
            }

            // capturedStreamScreenTexture.style.backgroundImage = new StyleBackground(screenCapture.trimmedTexture);
            float streamScreenWidth = capturedStreamScreenTexture.resolvedStyle.width;
            ResizeScreenStreamVisualElement();


            // for optimalization
            // if (streamScreenWidth != lastStreamScreenWidth) { ResizeScreenStreamVisualElement(); }
            // lastStreamScreenWidth = capturedStreamScreenTexture.resolvedStyle.width;
        }
    }

    public void SwitchRatio()
    {
        Device activeDevice = ConnectionManager.Instance.ActiveDevice;
        string deviceModel;

        if (activeDevice != null)
        {
            deviceModel = activeDevice.ModelName;
        }
        else
        {
            deviceModel = "Quest";
        }

        // float ratio = (float)screenCapture.trimmedTexture.height / (float)screenCapture.trimmedTexture.width;
        // Debug.LogWarning("ratio: " + ratio);

        // Ratio calculation
        // screenCapture.trimmedTexture.height / screenCapture.trimmedTexture.width;


        // Ratios for simple view angle
        // Quest 3 ratio = 0.5813665f
        // Quest 2 ratio = 0.5621622; 
        // Quest Pro ratio = 0.5625; 
        // Quest 1 ratio = 0.5625;

        switch (deviceModel)
        {
            case "Quest 3":
                ratio = 0.5416667f;
                break;
            case "Quest 2":
                ratio = 0.6545454f;
                break;
            case "Quest Pro":
                ratio = 0.56f;
                break;
            case "Quest":
                ratio = 0.5625f;
                break;
            default:
                ratio = 0.5625f;
                break;
        }
        ResizeScreenStreamVisualElement();
    }
    public void ResizeScreenStreamVisualElement()

    {
        // ratio = (float)screenCapture.trimmedTexture.height / (float)screenCapture.trimmedTexture.width;
        // Debug.LogWarning("ratio: " + ratio);

        float streamScreenWidth = capturedStreamScreenTexture.resolvedStyle.width;
        // Debug.LogWarning("StreamScreen Width: " + streamScreenWidth);

        float newHeight = streamScreenWidth * ratio;
        // Debug.LogWarning("NEW height: " + newHeight);

        capturedStreamScreenTexture.style.height = new StyleLength(newHeight);
    }

    public void SetStreamViewBGColorToBlack()
    {
        capturedStreamScreenTexture.style.backgroundColor = new StyleColor(new Color32(0, 0, 0, 255)); 
    }
}
