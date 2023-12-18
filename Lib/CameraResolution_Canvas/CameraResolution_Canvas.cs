/*
maid 7rzr 2023-01-04
Canvers해상도 고정하는친구임

조건 : 켄버스에 집에넣어야함  
세팅 : setwidth[가로],setheight[새로],letterboxcolor[레터박스색]  
Scale With Screen Size 로 변경후 Reference Resolution 을 setWhith 랑 setHeight 로 맞춘다
가로기준으로 해상도 맞추고 색변경함
screenMatchMode 를 Expand 로 변경함
Screen.SetResolution  << 이거 쓰면  안됨 래터박스 깜빡거려서 빼버림
*/
using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraResolution_Canvas : MonoBehaviour
{

    [SerializeField] int setWidth; // 사용자 설정 너비
    [SerializeField] int setHeight; // 사용자 설정 높이
    [SerializeField] private Color LetterboxColor = Color.black;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        SetResolution(); // 초기에 게임 해상도 고정
        GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        GetComponent<CanvasScaler>().referenceResolution = new Vector2(setWidth, setHeight);
        GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
    }

    /// <summary>
    /// 해상도 설정하는 함수
    /// </summary>
    public void SetResolution()
    {

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth),
            true); // SetResolution 함수 제대로 사용하기

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }

    /// <summary>
    /// 레터박스 설정
    /// </summary>
    void OnPreCull()=>
        GL.Clear(true, true, Color.black);
    

}
