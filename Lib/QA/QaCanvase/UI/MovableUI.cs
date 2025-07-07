
using UnityEngine;
using UnityEngine.EventSystems;

namespace dk.UI
{
    public class MovableUI : UIPanel, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public RectTransform rectTransform;
        private Vector2 originalPosition;
        private Vector2 mouseStartPos;
        protected Vector2 distanceFromCenter;
        private bool isMoving = false;

        protected bool _isWrappedPosition = false; // 위치 제한 여부
        protected Vector2 _isWrappedSize = new Vector2(1920, 1080); // 화면 크기 제한

        protected override void Awake()
        {
            base.Awake();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        public virtual void InitView()
        {
            UpdateClampedPosition();
        }

        
        public void SetWrappedSize(Vector2 size)
        {
            _isWrappedSize = size;
            UpdateClampedPosition();
        }

        // 처음 사이즈 설정
        protected virtual void InitPosition(Vector2 pos)
        {
            rectTransform.anchoredPosition = pos;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isMoving = true;
            originalPosition = rectTransform.anchoredPosition;
            distanceFromCenter = eventData.position - (Vector2)rectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isMoving)
            {
                var uiScale = 1f;// UIManager.Instance.CanvasSide.transform.localScale.x;

                Vector2 deltaMove = ((eventData.position - distanceFromCenter) - originalPosition) / uiScale;

                Vector2 newPosition = originalPosition + deltaMove;

                rectTransform.anchoredPosition = _isWrappedPosition ? ClampPosition(newPosition) : newPosition;
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            isMoving = false;
        }

        // 현재 위치를 확인하고 범위를 넘어가지 않도록 조정
        public void UpdateClampedPosition()
        {
            rectTransform.anchoredPosition = _isWrappedPosition ? ClampPosition(rectTransform.anchoredPosition) : rectTransform.anchoredPosition;
        }

        protected virtual void Save()
        {
            // 위치, 크기 값 저장
        }

        protected virtual void Load()
        {
            // 위치, 크기 값 로드
        }

        /// <summary>
        /// UI가 화면 밖으로 나가지 않도록 제한하는 함수
        /// </summary>
        private Vector2 ClampPosition(Vector2 targetPosition)
        {
            Vector2 halfSize = rectTransform.sizeDelta / 2f; // UI 크기의 절반

            float minX = -_isWrappedSize.x + halfSize.x;
            float maxX = 0 - halfSize.x;
            float minY = 0 + halfSize.y;
            float maxY = _isWrappedSize.y - halfSize.y;

            float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }
    }
}
