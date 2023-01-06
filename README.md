# AssetLib  
내가 자주쓰는 에셋이나 내가만든 라이브러리 설명해놓을꺼임  


* Asset  
  * Dotween  
  * Odin  
  * DgDataBase[아직안함]  



* Lib  
  * CameraResolution_Canvas
  * Singelton  
  * SoundManager
  * ScenesManager  
  * GpgsStorageHelper
  * GoogleSheetManager  
  * Utility   
  * DatasManager   
  * ObjectPooler   
  
  
***
## CameraResolution_Canvas  
조건 : 켄버스에 집에넣어야함  
세팅 : setwidth[가로],setheight[세로],letterboxcolor[레터박스색]  
Canvas 해상도 고정하는친구임  
* Scale With Screen Size 로 변경후 Reference Resolution 을 setWhith 랑 setHeight 로 맞춘다  
* 가로기준으로 해상도 맞추고 색변경함  
* screenMatchMode 를 Expand 로 변경함   
***
## Singelton
조건 : Odin, 상속  
세팅 : SINGLETONTYPE[Dontdestroy 할껀지 안할껀지]  
싱글톤이다  
* ex) public class kk : SINGLETON<kk,SINGLETONE.SINGLETONEType.DoNotDontDestroy>  
* 끌고 다니고 [dontdestroy] 싶으면 SINGLETONEType을 dontdestroy로 하자  
* 상속받은친구에 Awake 에 base.Awake 넣어주자  
* 오딘안쓸꺼면 SerializedMonoBehaviour 대신 MonoBehaviour상속시키면됨  
***
## SoundManager
조건 : odin , singleton , Dotween  
세팅 : ReSourcePath_Bgm[bgm 파일들 경로], ReSourcePath_Ef[ef 파일들 경로] ,filePath[ENUM타입 저장할위치]
sound 관리하는 manager 임  
resource사운드파일들 이름들을 ENUM 타입으로 만들어서 ENUM으로 타입으로 사운드 콜가능  
EF 들의 경우 소리_몇개 설정할지 해줘야함 즉 이름 마지막에 _"몇개설정할지" 해주면됨
* INIT_ENUM : 
  * RESOURCE 불러와서 ENUM 타입만듬
* INIT_ENUM : 
  * ENUM 만들어진 친구들 기준으로 SOUND 집어넣음
* SoundPlay_Ef  
  * ef 재생
  * 볼륨,루핑 설정가능
* SoundPlay_Ef_Decrease  
  * ef 중첩하면 소리가 점점 작아지는 재생
  * 기소리,점점작아지는 정도 [ex : 0.9 는 10%씩작아짐] , 최소값 설정가능  
* SoundPlay_Bgm_Direct
  * 배경음 재생  
* SoundPlay_Bgm_Increaas
  * 배경음 점점 커지면서 재생
  * 시작볼륨, 끝볼륨, 경과시간 설정가능
* StopAllSource_Direct
  * 모든소리즉시정지
* StopBgm_Direct
  * 배경음정지
* StopEf_Direct
  * 해당이팩트즉시정지
* StopBgm_Decrease
  * 배경음 점점정지
  * 몇초동안 설정가능




