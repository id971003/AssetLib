초기화
        GameObject qaPrefab = Resources.Load<GameObject>("Prefabs/UI/QA/UI_QA");
        GameObject qaObj = Instantiate(qaPrefab, Canvas_Qa.transform);
        QaManager.Instance.RegisterCommand(new QaCommandContainer_Ingame()); 



QaCommandContainer_Ingame에 기능추가
	
	키코드
            CreateKeyCodeCommand(KeyCode.Space, "게임속도 x2", () =>
            {
                Time.timeScale = 2;
            });
	버튼
           CreateButtonCommand("정지", () =>
           {
               Debug.Log("!");
           });