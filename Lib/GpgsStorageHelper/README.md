# GpgsStorageHelper

데이터저장이랑 googlePlayGame(gpgs) 이용 편하게 하려고 만든거임

지원기능
1. 로컬 , Storage  데이터 저장
2. gpgs 연동
    2.1 init (5.31 이후  빠짐)
    2.2 login (5.31 이후 로그인 하나로 통합)
    2.3 save
    2.4 load


계층구조
1. StorageHelper 
  1. Local
    1.1 Save: LocalStorageHelper.SaveLocalStorage
    1.2 Load: LocalStorageHelper.LoadLocalStorage
    
  2. Clouad
    1.1 Save: CloudStorageHelper.SaveData
      1.1.1 gpgsHelper.SaveGame
      1.1.2 +a
    1.2 Load: CloudStorageHelper.LoadData
      1.2.1 GpgsHelper.LoadGame
      1.1.2 +a
      
      
반환처리? ( 용어정리)
action 문 콜백 받음 
    콜백 param
    1.Save
        1.1 bool : 성공실패 여부
        1.2 string : 실패로그
    2.Load
    2.1 bool : 성공실패
    2.2 string 로드 데이터
    2.3 string 실패로그

Scripte

LocalStorageHelper  : LocalSave or LocalLoad
    1.Save : Application.persistentDataPath+"파일이름" 경로에 데이터 저장 
        1.1 파일위치 "" 일시 실패 [로그 "파일 위치 설정 실패" ]
    2. Load : Application.persistentDataPath+"파일이름" 경로에 데이터 로드
        2.1 파일위치 "" 일시 실패 [로그 "파일 위치 설정 실패"]
        2.2 Loaddata ""일시 실패 [로그 "파일 읽기 실패"] // 저장데이터 없음 정도?

CloudStorageHelper : CloudSave or CloudLoad
    1. Android : gpgs 에서 사용
        1.1 SaveGame : GpgsHelper.SaveGame
        1.2 LoadGame : GpgsHelper.LoadGame
    2. +a : 따른 클라우드 추가시 이용
    
GpgsHelper

    1. Previous
        1.1 isAuthenticated : 접속 확인 프로퍼티
        1.2 isProessing : 동기 처리 및 중복 작동 막기위한 프로퍼티
        1.3 각 타입별 return fail message 를 XXXStatusMessage_XXX 에서 처리
    2. init :  Gpgs 이용하기위한 config , 가장처음실행 ,빌더패턴 이용함 
        2.1 EnableSaveGame() : 저장할꺼임
        2.2 Build 실행
        2.3 DebuglogEnable = false
    3. Login : 접속위한 로그인
        3.1 SimpleLogin : 그냥 로그인함
        3.2 Login_CheckDeviceId : 동기기 로만 로그인 가능하게끔함 
            3.2.1 일단로그인
            3.2.2 디바이스아이디확인
                3.2.2.1 없으면 : 디바이스 아이디 저장 로그인 완료
                3.2.2.2 있으면 : 디바이스 아이디 확인 
                    3.2.2.2.1 같으면 : 로그인완료
                    3.2.2.2.2 다르면 : 최근 로그인 시간 확인
                        3.2.2.2.2.1 최근 로그인시간 24시간 경과시 디바이스 아이디 초기화 , 로그인완료
    4. DiveceIdReset : 디바이스 아이디 초기화 , 이후 게임종료 (게임컨텐츠레벨에서)
    
    5. SaveGame : 저장
        5.1  로그인안되면 로그인 하라고해야됨 [isAuthenticated]
        5.2  무언가 작동중이면 좀있다 하라해야됨 [isProessing]
    6. LoadGame : 로드
        6.1  로그인안되면 로그인 하라고해야됨 [isAuthenticated]
        6.2  무언가 작동중이면 좀있다 하라해야됨 [isProessing]  
        
        



    

      
     
      
