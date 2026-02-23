
using KD;
using Unity.VisualScripting;

namespace GameLogic.Object.Field
{
    public enum FieldObjectState
    {
        Idle = 0,
        Walk=1,
        Hit = 2,
        Dying = 3,
        Death = 4,
        Attack,

    }


    public class FieldObjectBehaviourBase
    {


        

        protected FieldObject m_Owner;
        protected StateMachine<FieldObject> m_StateMachine;
        public FieldObjectBehaviourBase()
        {

        }

        public virtual void Init(FieldObject _owner)
        {
            m_Owner = _owner;
        }


        public StateMachine<FieldObject> GetStateMachine()
        {
            return m_StateMachine;
        }

        public virtual void RunUpdate(float _deltaTime)
        {
        }

        public bool TransitState(int _state, bool _forceTransit, params object[] _params)
        {
            if (m_Owner == null || m_StateMachine == null)
                return false;

            int prevStateId = -1;
            if (m_StateMachine.GetCurrentState() != null)
                prevStateId = m_StateMachine.GetCurrentState().StateId;

            //if (m_Owner.OnStartTransitState(prevStateId, _state, _forceTransit, _params) == false) 행동잠금인데 일단 비워놔
            //    return false;

            return m_StateMachine.TransitState(_state, _forceTransit, _params);
        }

        public int GetStateId()
        {
            var state = m_StateMachine.GetCurrentState();
            if (state == null)
                return -1;

            return state.StateId;
        }
    }
}
