# AssetLib  
내가 자주쓰는 에셋이나 내가만든 라이브러리 설명해놓을꺼임  


* Asset  
  * Dotween  
  * Odin  
  * DgDataBase  



* Lib  
  * Singelton  
  * UiManager  
  * SceneManager  
  * SoundManager  
  * GpgsManager  
  * GoogleSheetManager  
  * Utility

ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

##Singleton
싱글롯 템플릿으로 상속 받아 이용하게함  
Abstract이용해서 추상클레스임

SINGLETONE.SINGLETONEType 의 값 변경으로 DONTDESTROY 설정가능
* SINGLETONE.SINGLETONEType.DontDestroy : 해당오브젝트 지우지 않고 계속사용 dontdestroy 설정 awake 대신 init 메서드 오버라이팅해 이용해야함
* SINGLETONE.SINGLETONEType.DoNotDontDestroy : dontdestroy 사용안함
