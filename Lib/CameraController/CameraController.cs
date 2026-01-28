using GameLogic.Object.Field;
using GameLogic.StartBase;
using GameLogic.Systems;
using Rvd.TimerSystem;
using Rvd.Util.TimeLiner;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{

    public class CameraController : MonoBehaviour
    {
        private Bounds m_CurrentMapBounds;
        private bool m_IsFocues;
        private FieldObject m_CameraFocusTarget; //누구 따라갈지  
        private float m_SlapValue; // 따라가는 비율 [1이면 바로붙음]
        private Vector2 m_DirectPosition;

        public bool IsBattleModeViewStop() //True 면 게임 멈춘상태
        {
            return TimeSystem.Instance.CheckAndCreate("BattleModeView").DeltaTimePlayStatus == RvdTimePlayStatus.Stop;
        }

        //움직임 , 줌 , 로테이션 , 흔들림
        //챌린지모드 죽을때 , 승급전 죽을때 m_Priority=999
        private int m_Priority;
        public void SetCameraTarget(FieldObject _target, Bounds _mapBound, float slapValue = 1.0f, int Priolity = 0)
        {

            if (Priolity < m_Priority) //더작으면무시
            {
                return;
            }
            m_Priority = Priolity;

            m_IsFocues = true;
            m_CurrentMapBounds = _mapBound;
            m_CameraFocusTarget = _target;
            m_SlapValue = slapValue;
            m_Priority = Priolity;
        }
        public void SetCameraNonTarget(Vector2 _directPos)
        {
            m_IsFocues = false;
            m_CameraFocusTarget = null;
            m_DirectPosition = _directPos;
            transform.position = new Vector3(_directPos.x, _directPos.y, -49999);
        }

        public void SetCameraTarget(FieldObject _target) //타겟만변경
        {
            m_IsFocues = true;
            m_CameraFocusTarget = _target;
        }

        //cameramove 를 통해서 움직이는거말고 진짜 즉시 카메라 시점 옮기는거
        public void SetCameraPosition(Vector2 _direcPos)
        {
            CameraPos = _direcPos;
        }
        public void SetSlapvValue(float slapVale)
        {
            m_SlapValue = slapVale;
        }
        public void SetCameraSlap(float slap = 1) //슬
        {
            m_SlapValue = slap;
        }
        private void CameraMove()
        {
            if (m_CurrentMapBounds.size.sqrMagnitude <= 0f)
                return;

            if (m_IsFocues == false) //포커스없을땐 무시 [딴곳에서 뭔짓해서 위치 정해줘야함 그것도 안에넣자
            {
                CameraPos = m_DirectPosition;
            }
            else
            {

                if (m_CameraFocusTarget == null)
                {
                    return;
                }
                CameraPos = m_CameraFocusTarget.Position;
                float camWorldMinX = CameraPos.x - CameraWH.x / 2;
                float camWorldMaxX = CameraPos.x + CameraWH.x / 2;
                float camWorldMinY = CameraPos.y - CameraWH.y / 2;
                float camWorldMaxY = CameraPos.y + CameraWH.y / 2;

                // 좌우
                float mapWidth = m_CurrentMapBounds.size.x;
                float camWidth = CameraWH.x;

                var cameraPos = CameraPos;
                if (camWidth >= mapWidth)
                {
                    cameraPos.x = (m_CurrentMapBounds.min.x + m_CurrentMapBounds.max.x) / 2;
                    CameraPos = cameraPos;
                }
                else
                {
                    if (camWorldMinX < m_CurrentMapBounds.min.x)
                    {
                        float gap = m_CurrentMapBounds.min.x - camWorldMinX;
                        CameraPos += new Vector2(gap, 0);
                    }
                    else if (m_CurrentMapBounds.max.x < camWorldMaxX)
                    {
                        float gap = camWorldMaxX - m_CurrentMapBounds.max.x;
                        CameraPos -= new Vector2(gap, 0);
                    }
                }

                // 위아래
                float mapHeight = m_CurrentMapBounds.size.y;
                float camHeight = CameraWH.y;
                if (camHeight >= mapHeight)
                {
                    cameraPos.y = (m_CurrentMapBounds.min.y + m_CurrentMapBounds.max.y) / 2;
                    CameraPos = cameraPos;
                }
                else
                {
                    if (camWorldMinY < m_CurrentMapBounds.min.y)
                    {
                        float gap = m_CurrentMapBounds.min.y - camWorldMinY;
                        CameraPos += new Vector2(0, gap);
                    }
                    else if (m_CurrentMapBounds.max.y < camWorldMaxY)
                    {
                        float gap = camWorldMaxY - m_CurrentMapBounds.max.y;
                        CameraPos -= new Vector2(0, gap);
                    }
                }
            }

            //이거 어디서 가져와야하는지물어보기
            if (m_GameSyste == null)
                m_GameSyste = GameSystem.Instance;

            // ====== 적용부: Lerp로 스무스, Shake는 마지막에 더함 ======
            Vector3 targetBase = new Vector3(CameraPos.x, CameraPos.y, transform.position.z);      // 목표(베이스)
            Vector3 curBase = transform.position - (Vector3)ShakePos;                            // 현재(베이스)

            // 프레임 독립 보간 (m_SlapValue: 0=정지, 1=즉시)
            float follow = Mathf.Clamp01(m_SlapValue);
            float t = 1f - Mathf.Pow(1f - follow, Time.deltaTime * 60f); // 60fps 기준 감각 보정

            Vector3 smoothedBase = Vector3.Lerp(curBase, targetBase, t);

            // 메인 카메라: 스무스 베이스 + 즉시 흔들림
            transform.position = smoothedBase + (Vector3)ShakePos;

            // UI 카메라: 흔들림 제외 (z 유지)
            var uiPos = m_GameSyste.UICamera.transform.position;
            m_GameSyste.UICamera.transform.position = new Vector3(smoothedBase.x, smoothedBase.y, uiPos.z);
        }


        public delegate void OnAimFinishHandler(CameraController camera);

        private Camera m_Camera;
        private Coroutine m_ShakeEffectCoroutine = null;
        private Coroutine m_PingpongEffectCoroutine = null;
        private Coroutine m_ZoomEffectCoroutine = null;
        private Coroutine m_FocusEffectCoroutine = null;
        private Transform m_FocusTarget;
        private Vector2 m_RevisePos;
        private Vector2 m_TargetRevisionPos;
        private RvdVectorTween m_RevisionTween;
        private float m_WaitTime;



        GameSystem m_GameSyste;

        public Vector2 CameraPos
        {
            get;
            set;
        }

        public Vector2 CameraWH
        {
            get
            {

                /*
                 * Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
                    Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
                    Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane));
                    Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

                float minX = Mathf.Min(bottomLeft.x, topLeft.x, bottomRight.x, topRight.x);
                float maxX = Mathf.Max(bottomLeft.x, topLeft.x, bottomRight.x, topRight.x);
                float minY = Mathf.Min(bottomLeft.y, topLeft.y, bottomRight.y, topRight.y);
                float maxY = Mathf.Max(bottomLeft.y, topLeft.y, bottomRight.y, topRight.y);

                 */
                float width = (float)((m_Camera.aspect) * 2.0 * m_Camera.orthographicSize);
                float height = (float)(2.0 * m_Camera.orthographicSize);


                return new Vector2(width, height);
            }
        }

        public Vector2 ShakePos { get; set; }
        private float m_SaveZoomPumpingAmount;
        //private float m_SaveOriginalOrthographicSize;

        public event OnAimFinishHandler OnAimFinish;

        private void Awake()
        {
            m_GameSyste = GameSystem.Instance;
            m_Camera = this.gameObject.GetComponent<Camera>();
            m_WaitTime = 0;
            CameraPos = this.gameObject.transform.position;
            m_RevisionTween = new RvdVectorTween();

            m_SaveZoomPumpingAmount = 0;
        }


        /* 
         * [Unity URP + Camera Stacking 초기화 버그 우회]
         *
         * 현상:
         *  - 씬에 존재하는 모든 UI(Canvas)가 게임 시작 직후(첫 프레임)에는 보이지 않음.
         *  - Canvas/Override Sorting 옵션을 껐다 켜거나 Canvas 오브젝트를 비활성/재활성하면 즉시 정상적으로 보임.
         *
         * 원인 추정:
         *  - URP 환경에서 Base Camera + Overlay Camera 스택 조합 사용 시,
         *    첫 렌더 프레임에서 CanvasRenderer.cull 값이 잘못 true로 남거나,
         *    Canvas 정렬/스텐실/레이아웃 캐시가 제대로 초기화되지 않는 문제가 발생.
         *  - 이 때문에 UI가 실제로는 존재하지만 GPU로 드로우되지 않고 누락됨.
         *  - Unity 포럼/Reddit에도 비슷한 사례가 다수 보고되었으며,
         *    특정 Unity/URP 버전에서만 나타나는 초기화 타이밍 이슈로 추정됨.
         *
         * 해결 방법(우회):
         *  - 게임 시작 후 1프레임 대기한 뒤, Canvas에 강제로 리빌드를 걸어준다.
         *    - Canvas.ForceUpdateCanvases();
         *    - LayoutRebuilder.ForceRebuildLayoutImmediate();
         *    - 모든 Graphic에 SetVerticesDirty(), CanvasRenderer.cull = false 보정
         *    - 혹은 sortingOrder를 살짝 바꿨다가 원복 → 내부 캐시 더티 처리
         *  - 이렇게 하면 토글 없이도 UI가 정상적으로 나타난다.
         *
         * 참고 키워드(구글 검색용):
         *  "Unity URP UI not rendering first frame camera stacking"
         *  "Unity CanvasRenderer.cull true first frame"
         *  "Unity overrideSorting child canvas invisible first frame"
         *
         * 결론:
         *  - 엔진 내부 초기화 타이밍 문제이므로, 코드로 강제 리빌드 우회를 적용해야 안정적임.
         *  - 향후 Unity/URP 버전 업데이트 시 동일 문제가 해결될 수도 있으니,
         *    패치노트 확인 후 불필요해지면 제거할 것.
         */
        private IEnumerator Start()
        {
            yield return null;
            // 정렬/스텐실/버텍스 캐시 강제 더티
            foreach (var c in FindObjectsOfType<Canvas>(true))
            {
                if (c.overrideSorting)
                {
                    int o = c.sortingOrder;
                    c.sortingOrder = o + 1; // 살짝 건드렸다
                    c.sortingOrder = o;     // 되돌리기 → 내부 리빌드 유도
                }
            }

            Canvas.ForceUpdateCanvases();

            foreach (var rt in FindObjectsOfType<RectTransform>(true))
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            foreach (var g in FindObjectsOfType<Graphic>(true))
            {
                g.SetMaterialDirty();
                g.SetVerticesDirty();
            }
            foreach (var r in FindObjectsOfType<CanvasRenderer>(true))
                r.cull = false; // 첫 프레임에 true로 굳어있는 경우 해제
        }
        public void OnDestroy()
        {
            if (m_ShakeEffectCoroutine != null)
            {
                StopCoroutine(m_ShakeEffectCoroutine);
                ShakePos = Vector3.zero;
                m_ShakeEffectCoroutine = null;
            }
        }
        //카메라 직교 바운드 사이즈
        public Bounds GetOrthographicBounds()
        {
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float cameraHeight = m_Camera.orthographicSize * 2;
            Bounds bounds = new Bounds(
                m_Camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }

        public void RunLateUpdate()
        {
            Vector2 revisePos;
            m_RevisionTween.Update(Time.deltaTime, out revisePos);

            CameraMove();
        }

        public bool UpdateCorrectionPosition()
        {

            Vector3 cameraWorldViewPos = m_Camera.ViewportToWorldPoint(new Vector3(0, 0, m_Camera.nearClipPlane));
            float cameraHeight = 2f * m_Camera.orthographicSize;
            float cameraWidth = cameraHeight * m_Camera.aspect;
            float cameraLeft = transform.position.x - cameraWidth / 2.0f;
            float cameraRight = transform.position.x + cameraWidth / 2.0f;
            float cameraTop = transform.position.y + cameraHeight / 2.0f;
            float cameraBottom = transform.position.y - cameraHeight / 2.0f;
            return true;
        }

        public Camera GetTargetCamera()
        {
            return m_Camera;
        }

        #region Shake

        private float m_ShakeRemainTime;
        private float m_ShakeTickTime;
        private float m_ShakeTickTimeBase;
        //private float m_ShakeDurationTime;
        private float m_ShakeAmountX;
        private float m_ShakeAmountY;
        private float m_ShakeAdditionalAmountXPerTick;
        private float m_ShakeAdditionalAmountYPerTick;
        public void CameraShakeEffect(float amountX, float amountY, float additionalAmountXPerTick, float additionalAmountYPerTick, float timePerTick, float duration, Action _onFinish = null)
        {
            if (DataManager.Instance.PlayerData.UserOptionInfo != null)
            {
                if (DataManager.Instance.PlayerData.UserOptionInfo.EnableCameraEffect == false)
                    return;
            }

            ShakePos = Vector2.zero;
            m_ShakeRemainTime = duration;
            m_ShakeTickTimeBase = timePerTick;
            m_ShakeTickTime = 0;
            //m_ShakeDurationTime = duration;
            m_ShakeAmountX = amountX;
            m_ShakeAmountY = amountY;
            m_ShakeAdditionalAmountXPerTick = additionalAmountXPerTick;
            m_ShakeAdditionalAmountYPerTick = additionalAmountYPerTick;

            if (m_ShakeEffectCoroutine == null)
            {
                m_ShakeEffectCoroutine = StartCoroutine(CoCameraShakeEffect(_onFinish));
            }
        }
        public IEnumerator CoCameraShakeEffect(Action _onFinish = null)
        {
            for (; ; )
            {
                if (m_ShakeRemainTime > 0)
                {
                    if (m_ShakeTickTime <= 0)
                    {
                        int randValueX = UnityEngine.Random.Range(0, 2);
                        int randValueY = UnityEngine.Random.Range(0, 2);
                        Vector2 tempShakePos = ShakePos;
                        if (randValueX == 0)
                            tempShakePos.x = m_ShakeAmountX;
                        else
                            tempShakePos.x = -m_ShakeAmountX;

                        if (randValueY == 0)
                            tempShakePos.y = m_ShakeAmountY;
                        else
                            tempShakePos.y = -m_ShakeAmountY;

                        m_ShakeAmountX += m_ShakeAdditionalAmountXPerTick;
                        m_ShakeAmountY += m_ShakeAdditionalAmountYPerTick;
                        m_ShakeTickTime = m_ShakeTickTimeBase;
                        ShakePos = tempShakePos;
                    }
                    m_ShakeTickTime -= Time.deltaTime;
                    m_ShakeRemainTime -= Time.deltaTime;
                }
                else
                {
                    ShakePos = Vector2.zero;
                    _onFinish?.Invoke();
                }
                yield return null;
            }

        }
        public void StopShake()
        {
            if (m_ShakeEffectCoroutine == null)
                return;
            StopCoroutine(m_ShakeEffectCoroutine);
            m_ShakeEffectCoroutine = null;
        }
        private IEnumerator CircleEffect(float amount, float duration)
        {
            float elapsedTime = 0;
            while (duration == -1 ? true : elapsedTime <= duration)
            {
                ShakePos = (Vector3)UnityEngine.Random.insideUnitCircle * amount;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ShakePos = Vector3.zero;
        }

        public void ZoomPumping(float startWaitTime, float amount, float duration)
        {
            if (m_SaveZoomPumpingAmount <= amount)
            {
                if (m_PingpongEffectCoroutine != null)
                {
                    StopCoroutine(m_PingpongEffectCoroutine);
                    m_Camera.orthographicSize = 360;
                }

                m_SaveZoomPumpingAmount = amount;
                m_PingpongEffectCoroutine = StartCoroutine(ZoomPumpingRoutine(startWaitTime, amount, duration));
            }

        }

        private IEnumerator ZoomPumpingRoutine(float startWaitTime, float amount, float duration)
        {
            if (startWaitTime > 0)
                yield return new WaitForSecondsRealtime(startWaitTime);

            int pingpongCase = 1;
            if (pingpongCase == 0)
            {
                ShakePos = Vector3.zero;
                ShakePos = new Vector3(0, amount, 0);
                yield return new WaitForSecondsRealtime(0.015f);
                ShakePos = Vector3.zero;

            }
            else if (pingpongCase == 1)
            {
                float originalOrthographicSize = m_Camera.orthographicSize;
                DateTime startTime = DateTime.Now;
                float halfDuration = duration / 2;
                int flag = 0;
                while (true)
                {
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    float percentage;
                    if (halfDuration == 0)
                        percentage = 1.0f;
                    else
                        percentage = (float)elapsedTime.TotalSeconds / halfDuration;

                    if (percentage >= 1.0f)
                        percentage = 1.0f;

                    if (flag == 0)
                    {
                        m_Camera.orthographicSize = originalOrthographicSize - percentage * amount;
                    }

                    else if (flag == 1)
                    {
                        m_Camera.orthographicSize = originalOrthographicSize - (1 - percentage) * amount;
                    }
                    else
                        break;

                    if (percentage >= 1.0f)
                    {
                        flag++;
                        startTime = DateTime.Now;
                    }
                    yield return null;
                }

            }
            m_SaveZoomPumpingAmount = 0;
            m_Camera.orthographicSize = 360;
            m_PingpongEffectCoroutine = null;
        }

        #endregion

        #region Zoom
        //Priority 추후에 바꾸게 
        public void ZoomFrom(float _zoom = 0.1f, float _timeSecond = 0.0f, float _priority = 999)
        {
            _priority = 999;


            if (m_ZoomEffectCoroutine != null)
            {
                StopCoroutine(m_ZoomEffectCoroutine);
                m_ZoomEffectCoroutine = null;
            }


            //if (zoom < 200f)
            //    zoom = 200f;
            //else if (zoom > 360f)
            //    zoom = 360;

            if (_timeSecond <= 0.0f)
            {
                m_Camera.orthographicSize = _zoom;
                UpdateCorrectionPosition();
            }
            else
                m_ZoomEffectCoroutine = StartCoroutine(ZoomFromSmoothlyRoutine(_zoom, _timeSecond));
        }

        public void ZoomeDefaultBattleSize(float timeSecond = 0.0f, float Priolity = 999)
        {
            Priolity = 999;


            if (m_ZoomEffectCoroutine != null)
            {
                StopCoroutine(m_ZoomEffectCoroutine);
                m_ZoomEffectCoroutine = null;
            }

            //if (zoom < 200f)
            //    zoom = 200f;
            //else if (zoom > 360f)
            //    zoom = 360;

            float size = GameGlobalDefine.g_EachStageDefaultCameraOrthoSize();


            if (timeSecond <= 0.0f)
            {
                m_Camera.orthographicSize = GameGlobalDefine.g_EachStageDefaultCameraOrthoSize();
                UpdateCorrectionPosition();
            }
            else
                m_ZoomEffectCoroutine = StartCoroutine(ZoomFromSmoothlyRoutine(GameGlobalDefine.g_EachStageDefaultCameraOrthoSize(), timeSecond));
        }
        private IEnumerator ZoomFromSmoothlyRoutine(float zoom, float timeSecond, float minSize = 0.1f, float maxSize = 5000f)
        {

            //float originalZoom = m_Camera.orthographicSize;
            //float targetZoom = originalZoom - zoom; //5 1
            //float zoomCalc = targetZoom - originalZoom;
            ////Debug.Log(targetZoom);

            //DateTime startTime = DateTime.Now;
            //while (true)
            //{
            //    TimeSpan elapsed = (DateTime.Now - startTime);
            //    float percentage = ((float)elapsed.TotalSeconds / timeSecond);
            //    if (percentage >= 1.0f)
            //        break;

            //    m_Camera.orthographicSize = originalZoom + (zoomCalc * percentage);

            //    yield return null;
            //}
            //UpdateCorrectionPosition();

            float original = m_Camera.orthographicSize;
            float target = Mathf.Clamp(zoom, minSize, maxSize); // 절대값으로 클램프

            // 시간 0 혹은 동일 목표면 즉시 반영
            if (timeSecond <= 0f || Mathf.Approximately(original, target))
            {
                m_Camera.orthographicSize = target;
                UpdateCorrectionPosition();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < timeSecond)
            {
                //대기를 하고싶지않으면 트루를 반환하게
                //대기를 하고싶으면 폴스를 반환하게
                //yield return new WaitUntil(() => !IsBattleModeViewStop());

                while (IsBattleModeViewStop())
                    yield return null;


                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / timeSecond);

                // 선형 보간(원하면 아래 한 줄을 SmoothStep으로 바꿔 부드럽게 가능)
                // float eased = Mathf.SmoothStep(0f, 1f, t);
                float eased = t;

                m_Camera.orthographicSize = Mathf.Lerp(original, target, eased);
                yield return null;
            }

            // 최종 스냅 + 보정
            m_Camera.orthographicSize = target;
            UpdateCorrectionPosition();
        }

        #endregion

        #region Aim & Focus
        public void SetWaitTime(float timeSecond)
        {
            m_WaitTime = timeSecond;
        }
        public void SetRevisionPosition(Vector2 position, float startWaitTime, float duration, Action onReviseFinish = null)
        {
            m_TargetRevisionPos = position;
            m_RevisionTween = new RvdVectorTween();
            m_RevisionTween.Init(m_RevisePos, position, null, (tween, percentage, pos) =>
            {
                m_RevisePos = pos;
            }, (tween) => { onReviseFinish?.Invoke(); });
            m_RevisionTween.Start(0, duration);
        }

        //public void MoveToTarget(Transform target, float timeSecond = 0.0f)
        //{

        //}
        public void AimTo2D(Vector2 targetPos, float timeSecond = 0.0f)
        {
            if (m_FocusEffectCoroutine != null)
                StopCoroutine(m_FocusEffectCoroutine);


            //if (timeSecond < 0.001f)
            //    timeSecond = 0.001f;

            //if (timeSecond > 0.0f)
            m_FocusEffectCoroutine = StartCoroutine(AimTo2DRoutine(targetPos, timeSecond));
            //else
            //{
            //    targetPos += m_RevisePos;
            //    CameraPos = new Vector3(targetPos.x, CameraPos.y, this.gameObject.transform.position.z);
            //}
        }
        public void FocusTo2D(Transform target, float timeSecond = 0.0f)
        {
            if (m_FocusEffectCoroutine != null)
                StopCoroutine(m_FocusEffectCoroutine);

            if (target == null)
            {
                m_FocusTarget = null;
                return;
            }

            //m_FocusTarget = target;
            if (timeSecond > 0.0f)
                m_FocusEffectCoroutine = StartCoroutine(FocusTo2DTransformRoutine(target, timeSecond));
            else
            {
                CameraPos = new Vector3(target.position.x, CameraPos.y, this.transform.position.z);
            }

        }


        private IEnumerator AimTo2DRoutine(Vector2 targetPosition, float timeSecond)
        {

            while (m_WaitTime > 0f)
            {
                m_WaitTime -= Time.unscaledDeltaTime;
                if (m_WaitTime < 0f)
                    m_WaitTime = 0.0f;

                yield return null;
            }


            DateTime startTime = DateTime.Now;
            targetPosition = new Vector2(targetPosition.x, targetPosition.y) + m_RevisePos;
            Vector2 originalPosition = CameraPos;
            float originalDistance = Vector2.Distance(targetPosition, CameraPos);

            float percentage = 0;
            float elapsedTotalSec = 0;
            while (percentage <= 1.0f)
            {

                //var elapsedTotal = DateTime.Now - startTime;
                //elapsedTotalSec = (float)elapsedTotal.TotalSeconds;
                if (timeSecond <= 0)
                    percentage = 1.0f;
                else
                    percentage = ((float)elapsedTotalSec / timeSecond);

                if (percentage >= 1.0f)
                    percentage = 1.0f;
                Vector2 directionVector = (targetPosition - CameraPos).normalized;
                Vector2 nextPosition = originalPosition + directionVector * (originalDistance * percentage);

                CameraPos = new Vector3(nextPosition.x, nextPosition.y, this.gameObject.transform.position.z);
                if (percentage >= 1.0f)
                {
                    percentage = 1.0f;
                    break;
                }

                elapsedTotalSec += Time.unscaledDeltaTime;
                yield return null;
            }

            OnAimFinish?.Invoke(this);
            m_FocusEffectCoroutine = null;
        }
        public IEnumerator FocusTo2DTransformRoutine(Transform target, float timeSecond = 0.0f)
        {
            if (m_FocusEffectCoroutine != null)
                StopCoroutine(m_FocusEffectCoroutine);

            yield return AimTo2DRoutine(target.position, timeSecond);
            m_FocusTarget = target.transform;
        }

        public void RunUpdate(float _deltaTime)
        {

        }

        #endregion

    }
}