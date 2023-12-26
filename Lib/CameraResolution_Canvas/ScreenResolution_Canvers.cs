/*
 maid 7rzr 2023-12-26
 
 Canvers 해상도 고정하는 친구임
 원하는 설정 및 값으로 옮겨짐 모든 캔버스에 넣어주면 된다
 
 Scale With Screen Size 로 변경후 Reference Resolution 을 setWhith 랑 setHeight 로 맞춘다
가로기준으로 해상도 맞추고 색변경함
screenMatchMode 를 Expand 로 변경함

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScreenResolution_Canvers : MonoBehaviour
{
    [SerializeField] int setWidth; // 사용자 설정 너비
    [SerializeField] int setHeight; // 사용자 설정 높이
    void Start()
    {
        GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        GetComponent<CanvasScaler>().referenceResolution = new Vector2(setWidth, setHeight);
        GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
    }
    
    
    void Update()
    {
        
    }
}
