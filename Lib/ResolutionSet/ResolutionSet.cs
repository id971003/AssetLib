/*
 * 2025.3.29
 * window 환경에서 해상도 및 화면 상태 변경
 * Borderless  테두리없는 전체 창모드의 경우 유니티 FullScreenMode.FullScreenWindow 가 잘 동작하지않아
 * window api 이용해서 강제로 설정해줌
 * 
 */
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public enum ESCREENMODE
{
    Fullscreen = 0,
    Window = 1,
    BorderlessWindow = 2,
}

public enum ERESOLUTION
{
    Resolution_1920_1080 = 0,
    Resolution_1600_900 = 1,
    Resolution_1280_720 = 2,
}

public static class ResolutionSet
{
    public static ESCREENMODE CurrentScreenMode { get; private set; } = ESCREENMODE.Fullscreen;
    public static ERESOLUTION CurrentResolution { get; private set; } = ERESOLUTION.Resolution_1920_1080;

    /// <summary>
    /// 최종 적용 메서드 – 스크린 모드 및 해상도 적용
    /// </summary>
    public static void ApplySettings(ESCREENMODE mode, ERESOLUTION resolution)
    {
        SetResolution(resolution);
        SetScreenMode(mode);
    }

    /// <summary>
    /// 해상도 설정
    /// </summary>
    private static void SetResolution(ERESOLUTION resolution)
    {
        CurrentResolution = resolution;

        var (width, height) = GetResolutionValue(resolution);

        // enum 기준으로 전체화면 여부 판단
        bool isFull = CurrentScreenMode != ESCREENMODE.Window;

        Screen.SetResolution(width, height, isFull);

        AdjustCameraViewport(width, height);

    }

    /// <summary>
    /// 화면 모드 설정
    /// </summary>
    private static void SetScreenMode(ESCREENMODE mode)
    {
        CurrentScreenMode = mode;

        switch (mode)
        {
            case ESCREENMODE.Fullscreen:
                Screen.fullScreen = true;
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;

            case ESCREENMODE.Window:
                Screen.fullScreen = false;
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;

            case ESCREENMODE.BorderlessWindow:
                Screen.fullScreen = true;
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
#if UNITY_STANDALONE_WIN
                // 반드시 해상도 설정 이후에 호출해야 적용됨
                if (mode == ESCREENMODE.BorderlessWindow)
                    SetBorderlessWindow();
#endif
                break;
        }
    }

    #region 내부 기능

    /// <summary>
    /// 카메라 뷰 조정
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static void AdjustCameraViewport(int width, int height)
    {
        float screenAspect = (float)width / height;
        float targetAspect = 16f / 9f;
        float scaleHeight = screenAspect / targetAspect;

        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("Main Camera not found!");
            return;
        }

        if (scaleHeight < 1f)
        {
            camera.rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            camera.rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
        }
    }

    /// <summary>
    /// 지금상태 받아오기
    /// </summary>
    /// <param name="resolution"></param>
    /// <returns></returns>
    private static (int, int) GetResolutionValue(ERESOLUTION resolution) => resolution switch
    {
        ERESOLUTION.Resolution_1920_1080 => (1920, 1080),
        ERESOLUTION.Resolution_1600_900 => (1600, 900),
        ERESOLUTION.Resolution_1280_720 => (1280, 720),
        _ => (0, 0)
    };

#if UNITY_STANDALONE_WIN
    // Windows 전용 Borderless 처리
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int command);

    private const int GWL_STYLE = -16;
    private const uint WS_POPUP = 0x80000000;
    private const int SW_MAXIMIZE = 3;

    private static void SetBorderlessWindow()
    {
        IntPtr hwnd = GetActiveWindow();
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP); // 테두리 제거
        ShowWindow(hwnd, SW_MAXIMIZE);            // 최대화
    }
#endif

    #endregion
}
