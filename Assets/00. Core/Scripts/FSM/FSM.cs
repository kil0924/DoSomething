using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.FSM
{
    [Serializable]
    public class FSM<T> where T : Enum
    {
        public FSMState<T> curState;
        public FSMState<T> lastState;
        protected Dictionary<T, FSMState<T>> stateDict = new Dictionary<T, FSMState<T>>();
        public FSMState<T> this[T state] => stateDict[state];
        #region ========== 초기화 ==========

        public void Init()
        {
            stateDict.Clear();
            BuildStateDict();
            OnInit();
        }
        protected virtual void BuildStateDict()
        {
        
        }

        protected virtual void OnInit()
        {
        
        }

        #endregion

        public virtual void OnUpdate(float deltaTime)
        {
            if(curState == null)
                return;
        
            curState.OnUpdate(deltaTime);
        }

        public virtual void OnFixedUpdate(float deltaTime)
        {
            if(curState == null)
                return;
            
            var nextState = curState.GetNextState();
            if (EqualityComparer<T>.Default.Equals(nextState, curState.State) == false)
            {
                ChangeState(nextState);
            }
            
            curState.OnFixedUpdate(deltaTime);
        }
    
        protected FSMState<T> ChangeState(T nextState)
        {
            if (stateDict.TryGetValue(nextState, out var next))
            {
                lastState = curState;
                lastState?.OnExit();

                curState = next;
                curState?.OnEnter();
                
                _onChangeState?.Invoke();
            
                return curState;
            }
            else
            {
                return curState;
            }
        }

        private Action _onChangeState;
        public virtual void SetOnChangeState(Action onChangeState)
        {
            _onChangeState = onChangeState;
        }
    }

    [Serializable]
    public class FSMState<T> where T : Enum
    {
        private FSM<T> _fsm { get; set; }
        public FSMState(FSM<T> fsm, T state)
        {
            _fsm = fsm;
            _state = state;
        }

        [SerializeField, ReadOnly]
        private T _state;
        public T State => _state;
        
        private T _nextState = default;
    
        [SerializeField, ReadOnly]
        private float _stateTime;
        public float StateTime => _stateTime;
        
        public virtual void OnEnter()
        {
            _stateTime = 0;
            _nextState = _state;
            _onEnter?.Invoke();
        }
        private Action _onEnter;
        public void SetOnEnter(Action onEnter)
        {
            _onEnter = onEnter;
        }
    
        public virtual void OnUpdate(float deltaTime)
        {
            _onUpdate?.Invoke(deltaTime, _stateTime);
        }
        
        public delegate void OnUpdateDelegate(float deltaTime, float stateTime);
        private OnUpdateDelegate _onUpdate;
        
        public void SetOnUpdate(OnUpdateDelegate onUpdate)
        {
            _onUpdate = onUpdate;
        }
    
        public virtual void OnFixedUpdate(float deltaTime)
        {
            _stateTime += Time.deltaTime;
            _onFixedUpdate?.Invoke(deltaTime, _stateTime);
        }
        public delegate void OnFixedUpdateDelegate(float deltaTime, float stateTime);
        private OnFixedUpdateDelegate _onFixedUpdate;
        
        public void SetOnFixedUpdate(OnFixedUpdateDelegate onFixedUpdate)
        {
            _onFixedUpdate = onFixedUpdate;
        }

        public void SetNextState(T nextState)
        {
            _nextState = nextState;
        }
        public T GetNextState()
        {
            return _nextState;
        }
        public virtual T CheckExitCondition()
        {
            return _state;
        }

        public virtual void OnExit()
        {
            _onExit?.Invoke();
        }

        private Action _onExit;
        public void SetOnExit(Action onExit)
        {
            _onExit = onExit;
        }
    }
}