# AssetLib  
내가 자주쓰는 에셋이나 내가만든 라이브러리 설명해놓을꺼임  


* Asset  
  * Dotween  
  * Odin  
  * DgDataBase  



* Lib  
  * Singelton  
  * CameraResolution_Canvas
  * SoundManager
  * UiManager  
  * SceneManager  
  * GpgsManager  
  * GoogleSheetManager  
  * Utility

ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

## Singleton
#### 조건 
[odin 이용가능 > MonoBehaviour 대신 SerializedMonoBehaviour 상속받으면됨

싱글롯 템플릿으로 상속 받아 이용하게함  
Abstract이용해서 추상클레스임



SINGLETONE.SINGLETONEType 의 값 변경으로 DONTDESTROY 설정가능
* SINGLETONE.SINGLETONEType.DontDestroy : dontdestroy 사용 ,awake 대신 init 메서드 오버라이팅해 이용해야함
* SINGLETONE.SINGLETONEType.DoNotDontDestroy : dontdestroy 사용안함


ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
## CameraResolution_Canvas
#### 조건 
X

Canvas 해상도 고정하는 친구임  
setWidth 랑 setHeight 설정해주면 나머지 레터박스 처리됨 [레터박스 색은 LetterboxColor 필드값 변경]
가로행 기준으로 고정됨
Canvas Scaler 의 UiScaleMode 는 Scale With ScreenSize 로 변경됨, Screen Math Mode 는 Expand 로 변경됨  


ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
## SoundManager
#### 조건
[Odin ,Singletone]

소리 재생하는 매니저

* ef
  * SoundPlay_Ef :  ef 재생 , 볼륨 루핑 설정가능
  * SoundPlay_Ef_Decrease : ef 중첩소리 작아지게 재생 , 초기소리 점점 작아지는정도 [0.9(10%씩작아짐)] , 최소값 설정가능
* Bgm
  * SoundPlay_Bgm_Direct : 바로재생
  * SoundPlay_Bgm_Increaas : 점점 커지면서 재생 , 시작볼륨, 끝볼륨 , 끝까지 올라가는 시간 조절가능
* Stop
  * StopAllSource_Direct : 모든소리 즉시 정지
  * StopBgm_Direct : 배경음만 즉시정지
  * StopEf_Direct : 해당 이팩트 즉시정지
  * StopBgm_Decrease : 배경음 점점 정지 , 몇초동안

