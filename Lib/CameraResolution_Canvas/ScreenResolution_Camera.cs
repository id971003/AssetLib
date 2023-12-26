/*
maid 7rzr 2023-01-04
update 7rzr 2023-12-26
기존껀 막 문제가 많았다 ui 상에서 막 앵커 조정하려면 상위에 뭐 있어야하고 앵커 안쓰려면 canvas 설정을 바꿔야하고 등등..
그래서 해상도 고정을 camera 랑 canvers 에서 따로 하게해서 아무조건 없이 고정 할 수 있게 만들었다.
그래도 아직 레터박스는 문제가있다.

그대신 설정을 2스프립트에서 하다보니 width 랑 height 를 두스크립트에서 설정해야한다.
static 하나 선언해서 두개 resoultion 스크립트들이 받게해서 쓰자
ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ


Camera 해상도 고정하는친구임
Screen.SetResolution  << 이거 쓰면  안됨 래터박스 깜빡거려서 빼버림

조건 : 켄버스에 집에넣어야함  
세팅 : setwidth[가로],setheight[새로],letterboxcolor[레터박스색]  

Camera 의 rect 값을 설정한 가로 새로에 맞게 변경함
*/
using System;
using UnityEngine;


public class ScreenResolution_Camera : MonoBehaviour
{

    [SerializeField] int setWidth; // 사용자 설정 너비
    [SerializeField] int setHeight; // 사용자 설정 높이
    [SerializeField] private Color LetterboxColor;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        SetResolution(); // 초기에 게임 해상도 고정
    }

    /// <summary>
    /// 해상도 설정하는 함수
    /// </summary>
    public void SetResolution()
    {

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        //Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); 

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
        GL.Clear(true, true, LetterboxColor);
    

}
