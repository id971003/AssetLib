/*
 * KD StateMachine System with Reserved State Transition
 * Reserved 방식 FSM(Finite State Machine) 구현
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
    /// 상태 전환을 예약 후 다음 프레임에 안전하게 처리
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

        public Action<StateMachine<T>, int, int> OnChangeState;

        /// <summary>
        /// 현재 상태 반환
        /// </summary>
        public IState<T> GetCurrentState() => m_CurState;

        /// <summary>
        /// 예약된 상태 반환 (전환 대기중인 상태)
        /// </summary>
        public IState<T> GetReservedState() => m_ReservedNextState;

        /// <summary>
        /// 현재 상태의 파라미터 반환
        /// </summary>
        public object[] GetStateParams() => m_ReservedParams;

        public StateMachine(T _actor)
        {
            Actor = _actor;
            m_StateDict = new Dictionary<int, IState<T>>();
            m_CurState = null;
            m_ReservedNextState = null;
            m_ReservedParams = null;
        }

        /// <summary>
        /// 상태를 등록합니다
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
        /// 상태 전환을 예약합니다 (다음 프레임에 실제 전환됨)
        /// </summary>
        /// <param name="_toStateId">전환할 상태 ID</param>
        /// <param name="_parameters">상태에 전달할 파라미터</param>
        /// <returns>예약 성공 여부</returns>
        public bool TransitState(int _toStateId, params object[] _parameters)
        {
            if (!m_StateDict.TryGetValue(_toStateId, out IState<T> nextState))
            {
                Debug.LogError($"State ID {_toStateId} not found!");
                return false;
            }

            // 즉시 전환하지 않고 예약만 함
            m_ReservedNextState = nextState;
            m_ReservedParams = _parameters;

            return true;
        }

        /// <summary>
        /// 상태 머신 업데이트 (매 프레임 호출 필수)
        /// 예약된 상태 전환을 먼저 처리한 후 현재 상태 업데이트
        /// </summary>
        /// <param name="_deltaTime">델타 타임</param>
        public void RunUpdate(float _deltaTime)
        {
            // 1. 예약된 상태 전환 먼저 처리
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

                // 파라미터 저장 (새 상태에서 접근 가능)
                m_ReservedParams = stateParams;
            }

            // 2. 현재 상태 업데이트 (안전한 상태에서 실행)
            m_CurState?.OnUpdate(_deltaTime);
        }

        /// <summary>
        /// 예약된 상태가 있는지 확인
        /// </summary>
        /// <returns>예약된 상태 존재 여부</returns>
        public bool HasReservedState()
        {
            return m_ReservedNextState != null;
        }

        /// <summary>
        /// 예약된 상태 전환을 취소
        /// </summary>
        public void CancelReservedState()
        {
            m_ReservedNextState = null;
            m_ReservedParams = null;
        }

        /// <summary>
        /// 등록된 상태가 있는지 확인
        /// </summary>
        /// <param name="_stateId">확인할 상태 ID</param>
        /// <returns>존재 여부</returns>
        public bool HasState(int _stateId)
        {
            return m_StateDict.ContainsKey(_stateId);
        }

        /// <summary>
        /// 현재 상태 ID 반환
        /// </summary>
        /// <returns>현재 상태 ID (-1: 상태없음)</returns>
        public int GetCurrentStateId()
        {
            return m_CurState?.StateId ?? -1;
        }

        /// <summary>
        /// 예약된 상태 ID 반환
        /// </summary>
        /// <returns>예약된 상태 ID (-1: 예약없음)</returns>
        public int GetReservedStateId()
        {
            return m_ReservedNextState?.StateId ?? -1;
        }

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
    /// 별도 클래스 작성 없이 함수들로 상태 구현 가능
    /// </summary>
    /// <typeparam name="T">상태머신을 가지는 주체 타입</typeparam>
    public class NativeState<T> : IState<T>
    {
        public StateMachine<T> StateMachine { get; set; }
        public int StateId { get; set; }

        private Action<IState<T>> m_OnEnterCallback;
        private Action<float> m_OnUpdateCallback;
        private Action<IState<T>> m_OnExitCallback;

        /// <summary>
        /// 상태 콜백들을 설정합니다
        /// </summary>
        /// <param name="_onEnter">Enter 콜백 (이전 상태 정보 전달)</param>
        /// <param name="_onUpdate">Update 콜백 (델타타임 전달)</param>
        /// <param name="_onExit">Exit 콜백 (다음 상태 정보 전달)</param>
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