/*
 * 
 * 2025.3.25
 * 유니티에서 WindowApi 이용해 해당 모니터 Dpi 모니터 해상도 정보얻기
 * 
 * Il2Cpp 에서만 정상지원가능할듯
 * 
 * 프로젝트 적용은안해봄..
 */






using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class MonitorStatus : MonoBehaviour
{
#if ENABLE_IL2CPP
    [AttributeUsage(AttributeTargets.Method)]
    public class MonoPInvokeCallbackAttribute : Attribute {
        public MonoPInvokeCallbackAttribute(Type t) { }
    }
#endif

    public TextMeshProUGUI logText;

    private string m_log = "";

    private class UserMonotorInfo
    {
        public IntPtr hMonitor;
        public Rect rect;
        public uint dpi;
        public float scaleFactor;

        public UserMonotorInfo(IntPtr hMonitor, Rect rect, uint dpi, float scaleFactor)
        {
            this.hMonitor = hMonitor;
            this.rect = rect;
            this.dpi = dpi;
            this.scaleFactor = scaleFactor;
        }
    }

    private static readonly List<UserMonotorInfo> USER_MONITOR_LIST = new List<UserMonotorInfo>();

    private void Start()
    {
        SetAction();

        logText.text = m_log;
    }

    public void SetAction()
    {
        logText.text = "-";
        m_log = "";
        USER_MONITOR_LIST.Clear();

        GetAllMonitorsWithDPI();
        GetCurrentMonitorRefreshRateRatio();
        GetAllMonitorsWithCurrent();
    }


    #region 모니터 정보 가져오기

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorRect
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int cbSize;
        public MonitorRect rcMonitor;
        public MonitorRect rcWork;
        public uint dwFlags;
    }

#if ENABLE_IL2CPP
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
#endif
    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hMonitor, int dpiType, out uint dpiX, out uint dpiY);

    private const int MDT_EFFECTIVE_DPI = 0;

#if ENABLE_IL2CPP
    [MonoPInvokeCallback(typeof(MonitorEnumProc))]
#endif
    private static bool MonitorCallback(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
    {
        MonitorInfo mi = new MonitorInfo();
        mi.cbSize = Marshal.SizeOf(typeof(MonitorInfo));

        if (GetMonitorInfo(hMonitor, ref mi))
        {
            int x = mi.rcMonitor.left;
            int y = mi.rcMonitor.top;
            int width = mi.rcMonitor.right - mi.rcMonitor.left;
            int height = mi.rcMonitor.bottom - mi.rcMonitor.top;

            Rect rectR = new Rect(x, y, width, height);

            GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpi, out _);
            float scaleFactor = dpi / 96.0f * 100; // Windows 기본 DPI 96 기준 배율 %

            USER_MONITOR_LIST.Add(new UserMonotorInfo(hMonitor, rectR, dpi, scaleFactor));
        }

        return true; // 계속 열거
    }

    /// <summary>
    /// 모니터 정보 가져오기
    /// </summary>
    private void GetAllMonitorsWithDPI()
    {
        try
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorCallback, IntPtr.Zero);
        }
        catch (Exception e)
        {
            m_log += $"모니터 정보 가져오기 실패: {e.Message}\n";
            return;
        }

        m_log += "--------------------------------\n";
        foreach (var item in USER_MONITOR_LIST)
        {
            m_log += $"모니터ID: {item.hMonitor}\n";
            m_log += $"좌표(Pivot): {item.rect.x}, {item.rect.y}\n";
            m_log += $"화면크기: {item.rect.width}, {item.rect.height}\n";
            m_log += $"dpi: {item.dpi}, 배율: {item.scaleFactor}\n";
            m_log += "--------------------------------\n";
        }
    }
    #endregion 모니터 정보 가져오기

    #region 현재 모니터 주사율 가져오기

    /// <summary>
    /// 현재 모니터 주사율 가져오기
    /// </summary>
    private void GetCurrentMonitorRefreshRateRatio()
    {
        Resolution currentResolution = Screen.currentResolution;
        m_log += $"현재 모니터 화면크기: {currentResolution.width}x{currentResolution.height}\n";
        m_log += $"현재 모니터 주사율: {currentResolution.refreshRateRatio.value} Hz\n";
    }

    #endregion 현재 모니터 주사율 가져오기

    #region 현재 실행된 게임의 모니터 찾기

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    /// <summary>
    /// 현재 실행된 게임의 모니터 찾기
    /// </summary>
    private void GetAllMonitorsWithCurrent()
    {
        IntPtr currentMonitor = MonitorFromWindow(GetActiveWindow(), MONITOR_DEFAULTTONEAREST);
        int currentMonitorIndex = USER_MONITOR_LIST.FindIndex(m => m.hMonitor == currentMonitor);

        m_log += $"현재 창이 위치한 모니터 ID: {currentMonitor}\n";
        m_log += $"현재 창이 위치한 모니터 인덱스: {currentMonitorIndex}\n";
    }

    #endregion 현재 실행된 게임의 모니터 찾기

    #region 창모드 상단 타이틀 바 및 테두리 제거

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);


    private const int GWL_STYLE = -16;
    private const int WS_CAPTION = 0x00C00000; // WS_BORDER | WS_DLGFRAME

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_FRAMECHANGED = 0x0020;

    public void SetBorderless(bool enable)
    {
        IntPtr windowHandle = GetActiveWindow();
        int style = GetWindowLong(windowHandle, GWL_STYLE);

        if (enable)
        {   // 테두리 및 제목 표시줄 추가
            style |= WS_CAPTION;
        }
        else
        {  // 테두리 및 제목 표시줄 제거
            style &= ~WS_CAPTION;
        }

        SetWindowLong(windowHandle, GWL_STYLE, style);

        // 창 새로 고침
        SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_FRAMECHANGED);
    }

    #endregion 창모드 상단 타이틀 바 및 테두리 제거

    public void EndGame()
    {
        Application.Quit();
    }

    // [DllImport("user32.dll")]
    // public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    //
    // const uint SWP_NOZORDER = 0x0004;
    //
    // void MoveWindowToMonitor(int monitorIndex, bool fullscreen, bool topmost) {
    //     RECT targetMonitor = monitorData[monitorIndex];
    //
    //     int targetX = targetMonitor.left;
    //     int targetY = targetMonitor.top;
    //     int targetWidth = targetMonitor.right - targetMonitor.left;
    //     int targetHeight = targetMonitor.bottom - targetMonitor.top;
    //     IntPtr zOrder = IntPtr.Zero; // 기본 Z-order
    //
    //     if (!fullscreen) {
    //         // 창모드일 경우: 작업표시줄과 창 제목바 크기 고려
    //         int taskbarHeight = GetTaskbarHeight();
    //         int titleBarHeight = GetTitleBarHeight();
    //
    //         targetHeight -= (taskbarHeight + titleBarHeight);
    //
    //         // 최상위 모드 적용 여부
    //         zOrder = topmost ? HWND_TOPMOST : HWND_NOTOPMOST;
    //     }
    //
    //     SetWindowPos(windowHandle, zOrder, targetX, targetY, targetWidth, targetHeight, SWP_NOZORDER);
    // }
    //
    // #region 윈도우 작업줄 높이 구하기
    //
    //
    //
    // #endregion 윈도우 작업줄 높이 구하기
    // [DllImport("user32.dll")]
    // private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);
    //
    // private const uint SPI_GETWORKAREA = 0x0030;
    //
    // private static int GetTaskbarHeight(int monitorHeight)
    // {
    //     RECT workArea = new RECT();
    //     SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0);
    //
    //     // 작업 가능한 영역과의 차이 = 작업표시줄 높이
    //     return monitorHeight - (workArea.bottom - workArea.top);
    // }
    //
    //
    // [DllImport("user32.dll")]
    // private static extern int GetSystemMetrics(int nIndex);
    //
    // private const int SM_CYCAPTION = 4; // 창 제목바 높이
    // private const int SM_CYFRAME = 33;   // 창의 테두리 높이
    //
    // public static int GetTitleBarHeight()
    // {
    //     return GetSystemMetrics(SM_CYCAPTION) + GetSystemMetrics(SM_CYFRAME);
    // }
    //
    //
    //
    //
    // void MoveWindowToMonitor(int monitorIndex) {
    //     // 배율을 고려한 창 크기 조정
    //     float scaleFactor = dpi / 96.0f;
    //     int width = (int)(800 * scaleFactor);
    //     int height = (int)(600 * scaleFactor);
    //
    //     SetWindowPos(windowHandle, IntPtr.Zero, targetMonitor.left, targetMonitor.top, width, height, SWP_NOZORDER);
    // }
}