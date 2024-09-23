using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading.Tasks;

public class ScreenCapture : MonoBehaviour
{
    [DllImport("DLL1.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindScrcpyWindow(string windowName);

    [DllImport("DLL1.dll")]
    private static extern void CaptureWindow(IntPtr hwnd, out IntPtr hBitmap);

    [DllImport("DLL1.dll")]
    private static extern void HideWindowFromTaskbar(IntPtr hwnd);

    [DllImport("DLL1.dll")]
    private static extern void SetWindowTransparency(IntPtr hwnd, byte alpha);

    [DllImport("DLL1.dll")]
    private static extern void MinimizeWindow(IntPtr hwnd);

    [DllImport("DLL1.dll")]
    private static extern void RestoreWindow(IntPtr hwnd);

    [DllImport("DLL1.dll")]
    private static extern void PositionWindowBehind(IntPtr targetHwnd, IntPtr behindHwnd);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, IntPtr lpvBits, ref BitmapInfoHeader lpbmi, uint uUsage);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private string[] windowNames = { "Quest 2", "Quest 3", "Quest", "Quest Pro" };
    private string windowName = "Quest 3";
    public Texture2D screenTexture;
    private IntPtr hwnd = IntPtr.Zero;
    private const int DIB_RGB_COLORS = 0;

    private bool wasMinimized = false;
    public Texture2D trimmedTexture = null;

    void Start()
    {
        InitializeScreenTexture();
        TryFindWindow();
    }

    async void Update()
    {
        if (hwnd == IntPtr.Zero)
        {
            TryFindWindow();
            return;
        }

        UpdateScreenTextureSize();
        await CaptureAndProcessScreenAsync();
        HandleWindowMinimization();
        PositionWindowBehind(hwnd, GetForegroundWindow());
    }

    private void InitializeScreenTexture()
    {
        screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.BGRA32, false);
    }

    private void TryFindWindow()
    {
        foreach (var name in windowNames)
        {
            hwnd = FindScrcpyWindow(name);
            if (hwnd != IntPtr.Zero)
            {
                windowName = name;
                Debug.Log("Window found: " + windowName);
                return;
            }
        }
      //  Debug.Log("Window not found. Retrying...");
    }

    private async Task CaptureAndProcessScreenAsync()
    {
        IntPtr hBitmap;
        CaptureWindow(hwnd, out hBitmap);
        HideWindowFromTaskbar(hwnd);
        SetWindowTransparency(hwnd, 0);

        if (hBitmap != IntPtr.Zero)
        {
            BitmapInfoHeader bmpInfoHeader = new BitmapInfoHeader
            {
                biSize = (uint)Marshal.SizeOf(typeof(BitmapInfoHeader)),
                biWidth = screenTexture.width,
                biHeight = -screenTexture.height, //TODO: Need to solve wrong rotation of screenTexture
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0
            };

            int imageSize = screenTexture.width * screenTexture.height * 4; // ARGB32
            byte[] imageData = new byte[imageSize];
            IntPtr ptrImageData = Marshal.UnsafeAddrOfPinnedArrayElement(imageData, 0);

            IntPtr hdc = GetDC(IntPtr.Zero);
            int result = GetDIBits(hdc, hBitmap, 0, (uint)screenTexture.height, ptrImageData, ref bmpInfoHeader, DIB_RGB_COLORS);
            ReleaseDC(IntPtr.Zero, hdc);

            if (result == 0)
            {
                Debug.LogWarning("GetDIBits error.");
            }
            else
            {
                screenTexture.LoadRawTextureData(imageData);
                screenTexture.Apply();
                TrimBlackBordersOptimized();
            }

            DeleteObject(hBitmap);
        }
        else
        {
            if (ButtonManager.Instance.isDeviceListOpen)
            {
               // Debug.LogWarning("Bitmap not available.");
            }
        }
    }

    void UpdateScreenTextureSize()
    {
        if (screenTexture == null || screenTexture.width != Screen.width || screenTexture.height != Screen.height)
        {
            Destroy(screenTexture);
            InitializeScreenTexture();
        }
    }

    private void HandleWindowMinimization()
    {
        bool isUnityMinimized = !Application.runInBackground && !Application.isFocused;

        if (isUnityMinimized && !wasMinimized)
        {
            MinimizeWindow(hwnd);
            wasMinimized = true;
        }
        else if (!isUnityMinimized && wasMinimized)
        {
            RestoreWindow(hwnd);
            wasMinimized = false;
        }
    }
    private void TrimBlackBordersOptimized()
    {
        Color32[] pixels = screenTexture.GetPixels32();
        int width = screenTexture.width;
        int height = screenTexture.height;

        // Check if the first row and column are black
        bool isFirstRowBlack = IsRowBlack(pixels, width, 0);
        bool isFirstColumnBlack = IsColumnBlack(pixels, width, height, 0);

        if (isFirstRowBlack && isFirstColumnBlack)
        {
            // Check if the entire image is black
            bool isImageBlack = true;
            for (int y = 0; y < height; y++)
            {
                if (!IsRowBlack(pixels, width, y))
                {
                    isImageBlack = false;
                    break;
                }
            }

            if (isImageBlack)
            {
                Debug.Log("The entire image is black. Skipping trimming.");
                return;
            }
        }

        // Find bottom border (first non-black row from the bottom)
        int bottom = height - 1;
        for (int y = height - 1; y >= 0; y--)
        {
            if (!IsRowBlack(pixels, width, y))
            {
                bottom = y;
                break;
            }
        }

        // Find right border (first non-black column from the right)
        int right = width - 1;
        for (int x = width - 1; x >= 0; x--)
        {
            if (!IsColumnBlack(pixels, width, height, x))
            {
                right = x;
                break;
            }
        }

        // New dimensions of the trimmed texture
        int newWidth = right + 1;
        int newHeight = bottom + 1;

        if (newWidth > 0 && newHeight > 0)
        {
            if (trimmedTexture == null || trimmedTexture.width != newWidth || trimmedTexture.height != newHeight)
            {
                if (trimmedTexture != null)
                {
                    Destroy(trimmedTexture);
                }
                trimmedTexture = new Texture2D(newWidth, newHeight, TextureFormat.BGRA32, false);
            }

            // Copy pixels to the trimmed texture
            Color32[] newPixels = new Color32[newWidth * newHeight];
            for (int y = 0; y < newHeight; y++)
            {
                Array.Copy(pixels, y * width, newPixels, y * newWidth, newWidth);
            }

            trimmedTexture.SetPixels32(newPixels);
            trimmedTexture.Apply();

            // Update the screenTexture to be the trimmed texture
            if (screenTexture != null)
            {
                Destroy(screenTexture);
            }

            //Graphics.CopyTexture(trimmedTexture, screenTexture);
            //screenTexture.Apply();
        }
        else
        {
            Debug.LogWarning("Invalid dimensions for trimmed texture: width=" + newWidth + ", height=" + newHeight);
        }
    }
    
    private int FindNonBlackBorder(Color32[] pixels, int width, int height, bool isRowCheck)
    {
        if (isRowCheck)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (!IsRowBlack(pixels, width, y))
                {
                    return y;
                }
            }
        }
        else
        {
            for (int x = width - 1; x >= 0; x--)
            {
                if (!IsColumnBlack(pixels, width, height, x))
                {
                    return x;
                }
            }
        }
        return isRowCheck ? 0 : 0;
    }
    
    private bool IsRowBlack(Color32[] pixels, int width, int row)
    {
        int startIdx = row * width;
        for (int x = startIdx; x < startIdx + width; x++)
        {
            Color32 pixel = pixels[x];
            if (pixel.a != 0 && !(pixel.r == 0 && pixel.g == 0 && pixel.b == 0))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsColumnBlack(Color32[] pixels, int width, int height, int col)
    {
        for (int y = 0; y < height; y++)
        {
            int idx = y * width + col;
            Color32 pixel = pixels[idx];
            if (pixel.a != 0 && !(pixel.r == 0 && pixel.g == 0 && pixel.b == 0))
            {
                return false;
            }
        }
        return true;
    }

    public void GetLastFrame(Texture2D texture)
    {
        if (screenTexture != null)
        {
            texture.SetPixels32(screenTexture.GetPixels32());
            texture.Apply();
        }
    }

    public Texture2D GetLastFrameImageOfDevice(Device device) 
    {
        // Placeholder for actual implementation
        return null;
    }

    public void ResetHwnd()
    {
        hwnd = IntPtr.Zero;
    }

    void OnDestroy()
    {
        if (screenTexture != null)
        {
            Destroy(screenTexture);
            screenTexture = null;
        }

        if (trimmedTexture != null)
        {
            Destroy(trimmedTexture);
            trimmedTexture = null;
        }
    }

    public Texture2D CaptureScreen()
    {
        if (trimmedTexture != null)
        {
            int width = trimmedTexture.width;
            int height = trimmedTexture.height;
            Texture2D scaledTexture = new Texture2D(width, height, trimmedTexture.format, false);

            Color32[] pixels = trimmedTexture.GetPixels32();
            Color32[] flippedPixels = new Color32[pixels.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flippedPixels[(height - 1 - y) * width + x] = pixels[y * width + x];
                }
            }
            scaledTexture.SetPixels32(flippedPixels);
            scaledTexture.Apply();
            return scaledTexture;
        }
        return null;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
}






