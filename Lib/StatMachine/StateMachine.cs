/*
 *  MADE 7rzr 2024-11-06
 * StateMachine System with Reserved State Transition
 * Reserved 방식 FSM(Finite State Machine)
 *  이렇게안하면 전환했을때 이전꺼의 update 랑 옮긴거 enter 순서가 보장이 안될수도있음
 * Lock : 잠궈짐 
 * 같은 상태 진입시 exit enter 다부름 
 * 
 * 
 * ex 1)  등록
 *     void Start()
    {
        // 상태머신 생성
        m_StateMachine = new KD.StateMachine<Player>(this);

        // 상태들 등록 (콜백 방식)
        m_StateMachine.RegisterState(STATE_IDLE, OnIdleEnter, OnIdleUpdate, OnIdleExit);
        m_StateMachine.RegisterState(STATE_MOVE, OnMoveEnter, OnMoveUpdate, OnMoveExit);
        m_StateMachine.RegisterState(STATE_ATTACK, OnAttackEnter, OnAttackUpdate, OnAttackExit);
        m_StateMachine.RegisterState(STATE_DEAD, OnDeadEnter, OnDeadUpdate, OnDeadExit);

        // 첫 상태 시작
        m_StateMachine.StartWithState(STATE_IDLE, "game_start");
    }
 * ex 2) 전환
 *             m_StateMachine.TransitState(STATE_MOVE, false, moveDir, 5.0f); 
 * 
 * ex 3 ) 전달된 파라미터 사용
 *  void OnAttackEnter(KD.IState<Player> _prev)
    {
    var parameters = m_StateMachine.GetStateParams();  // 클래스 멤버변수로 접근
    }   
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace KD
{
    /// <summary>
    /// 상태 인터페이스
    /// </summary>
    /// <typeparam name="T">상태머신을 가지는 주체 타입</typeparam>
    public interface IState<T>
    {
        public StateMachine<T> StateMachine { get; set; }
        public int StateId { get; set; }
        void OnEnter(IState<T> _prev);
        void OnUpdate(float _deltaTime);
        void OnExit(IState<T> _next);
    }

    /// <summary>
    /// Reserved 방식 상태 머신 클래스
    /// </summary>
    /// <typeparam name="T">상태머신을 가지는 주체 타입</typeparam>
    public class StateMachine<T>
    {
        public T Actor { get; private set; }
        private Dictionary<int, IState<T>> m_StateDict;
        private IState<T> m_CurState;

        // Reserved 시스템
        private IState<T> m_ReservedNextState;
        private object[] m_ReservedParams;

        // Lock 시스템
        private List<int> m_LockStateList;

        public Action<StateMachine<T>, int, int> OnChangeState;

        public IState<T> GetCurrentState() => m_CurState;
        public IState<T> GetReservedState() => m_ReservedNextState;
        public object[] GetStateParams() => m_ReservedParams;

        public StateMachine(T _actor)
        {
            Actor = _actor;
            m_StateDict = new Dictionary<int, IState<T>>();
            m_CurState = null;
            m_ReservedNextState = null;
            m_ReservedParams = null;
            m_LockStateList = new List<int>();
        }

        /// <summary>
        /// 상태를 콜백 함수로 등록 (간편한 방식)
        /// </summary>
        /// <param name="_stateId">상태 ID</param>
        /// <param name="_onEnterCallback">Enter 콜백</param>
        /// <param name="_onUpdateCallback">Update 콜백</param>
        /// <param name="_onExitCallback">Exit 콜백</param>
        /// <returns>등록 성공 여부</returns>
        public bool RegisterState(int _stateId,
                                  Action<IState<T>> _onEnterCallback,
                                  Action<float> _onUpdateCallback,
                                  Action<IState<T>> _onExitCallback)
        {
            NativeState<T> newState = new NativeState<T>();
            newState.StateId = _stateId;
            newState.StateMachine = this;
            newState.SetCallbacks(_onEnterCallback, _onUpdateCallback, _onExitCallback);

            return m_StateDict.TryAdd(_stateId, newState);
        }

        /// <summary>
        /// 상태를 인스턴스로 등록 (클래스 방식)
        /// </summary>
        /// <param name="_stateId">상태 ID</param>
        /// <param name="_state">상태 인스턴스</param>
        /// <returns>등록 성공 여부</returns>
        public bool RegisterState(int _stateId, IState<T> _state)
        {
            if (_state == null) return false;

            _state.StateId = _stateId;
            _state.StateMachine = this;
            return m_StateDict.TryAdd(_stateId, _state);
        }

        /// <summary>
        /// 상태 전환을 예약합니다
        /// </summary>
        /// <param name="_toStateId">전환할 상태 ID</param>
        /// <param name="_ignoreLock">Lock을 무시할지 여부 (기본: false)</param>
        /// <param name="_parameters">상태에 전달할 파라미터</param>
        /// <returns>예약 성공 여부</returns>
        public bool TransitState(int _toStateId, bool _ignoreLock = false, params object[] _parameters)
        {
            if (!m_StateDict.TryGetValue(_toStateId, out IState<T> nextState))
            {
                Debug.LogError($"State ID {_toStateId} not found!");
                return false;
            }

            // Lock 체크 (ignoreLock이 true면 스킵)
            if (!_ignoreLock)
            {
                foreach (var lockId in m_LockStateList)
                {
                    if (_toStateId == lockId)
                    {
                        Debug.LogWarning($"State {_toStateId} is locked! Cannot transit.");
                        return false;
                    }
                }
            }

            m_ReservedNextState = nextState;
            m_ReservedParams = _parameters;

            return true;
        }

        /// <summary>
        /// 상태 머신 업데이트 (매 프레임 호출 필수)
        /// </summary>
        /// <param name="_deltaTime">델타 타임</param>
        public void RunUpdate(float _deltaTime)
        {
            // 예약된 상태 전환 먼저 처리
            if (m_ReservedNextState != null)
            {
                IState<T> nextState = m_ReservedNextState;
                IState<T> prevState = m_CurState;
                object[] stateParams = m_ReservedParams;

                // 예약 정보 초기화
                m_ReservedNextState = null;
                m_ReservedParams = null;

                int currentStateId = prevState?.StateId ?? -1;

                // Exit 현재 상태
                prevState?.OnExit(nextState);

                // 상태 변경 이벤트 호출
                OnChangeState?.Invoke(this, currentStateId, nextState.StateId);

                // 새 상태로 전환
                m_CurState = nextState;

                // Enter 새 상태
                m_CurState.OnEnter(prevState);

                // 파라미터 저장
                m_ReservedParams = stateParams;
            }

            // 현재 상태 업데이트
            m_CurState?.OnUpdate(_deltaTime);
        }

        /// <summary>
        /// 특정 상태에 Lock을 설정/해제합니다
        /// </summary>
        /// <param name="_stateId">Lock할 상태 ID</param>
        /// <param name="_isLock">Lock 여부 (true: Lock, false: Unlock)</param>
        public void SetLockState(int _stateId, bool _isLock)
        {
            if (_isLock)
            {
                if (!m_LockStateList.Contains(_stateId))
                {
                    m_LockStateList.Add(_stateId);
                }
            }
            else
            {
                m_LockStateList.Remove(_stateId);
            }
        }

        /// <summary>
        /// 모든 상태 Lock을 해제합니다
        /// </summary>
        public void ClearAllLocks()
        {
            m_LockStateList.Clear();
        }

        /// <summary>
        /// 특정 상태가 Lock되어 있는지 확인합니다
        /// </summary>
        /// <param name="_stateId">확인할 상태 ID</param>
        /// <returns>Lock 여부</returns>
        public bool IsStateLocked(int _stateId)
        {
            return m_LockStateList.Contains(_stateId);
        }

        /// <summary>
        /// 현재 Lock된 상태들의 리스트를 반환합니다
        /// </summary>
        /// <returns>Lock된 상태 ID 리스트</returns>
        public List<int> GetLockedStates()
        {
            return new List<int>(m_LockStateList);
        }

        public bool HasReservedState() => m_ReservedNextState != null;

        public void CancelReservedState()
        {
            m_ReservedNextState = null;
            m_ReservedParams = null;
        }

        public bool HasState(int _stateId) => m_StateDict.ContainsKey(_stateId);

        public int GetCurrentStateId() => m_CurState?.StateId ?? -1;

        public int GetReservedStateId() => m_ReservedNextState?.StateId ?? -1;

        /// <summary>
        /// 첫 상태를 즉시 시작 (초기화용)
        /// </summary>
        /// <param name="_stateId">시작할 상태 ID</param>
        /// <param name="_parameters">초기 파라미터</param>
        /// <returns>시작 성공 여부</returns>
        public bool StartWithState(int _stateId, params object[] _parameters)
        {
            if (!m_StateDict.TryGetValue(_stateId, out IState<T> startState))
                return false;

            m_CurState = startState;
            m_ReservedParams = _parameters;
            m_CurState.OnEnter(null);

            return true;
        }
    }

    /// <summary>
    /// 콜백 기반 상태 구현체
    /// </summary>
    /// <typeparam name="T">상태머신을 가지는 주체 타입</typeparam>
    public class NativeState<T> : IState<T>
    {
        public StateMachine<T> StateMachine { get; set; }
        public int StateId { get; set; }

        private Action<IState<T>> m_OnEnterCallback;
        private Action<float> m_OnUpdateCallback;
        private Action<IState<T>> m_OnExitCallback;

        public void SetCallbacks(
            Action<IState<T>> _onEnter,
            Action<float> _onUpdate,
            Action<IState<T>> _onExit)
        {
            m_OnEnterCallback = _onEnter;
            m_OnUpdateCallback = _onUpdate;
            m_OnExitCallback = _onExit;
        }

        public void OnEnter(IState<T> _prev)
        {
            m_OnEnterCallback?.Invoke(_prev);
        }

        public void OnUpdate(float _deltaTime)
        {
            m_OnUpdateCallback?.Invoke(_deltaTime);
        }

        public void OnExit(IState<T> _next)
        {
            m_OnExitCallback?.Invoke(_next);
        }
    }
}