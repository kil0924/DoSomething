using System;
using UnityEngine;
using Core.FSM;
using Object = UnityEngine.Object;

namespace RPG
{

    public enum RPGState
    {
        None,
        Init,
        Lobby,
        Battle,
    }

    #region ========== FSM ==========

    [Serializable]
    public class RPGStateManager : FSM<RPGState>
    {
        #region ========== 초기화 ==========

        protected override void BuildStateDict()
        {
            base.BuildStateDict();
            stateDict[RPGState.Init] = new RPGState_Init(this);
            stateDict[RPGState.Lobby] = new RPGState_Lobby(this);
            stateDict[RPGState.Battle] = new RPGState_Battle(this);
        }

        protected override void OnInit()
        {
            base.OnInit();
            ChangeState(RPGState.Init);
        }

        #endregion
    }

    #endregion

    #region ========== State ==========

    public class RPGState_Base : FSMState<RPGState>
    {
        public RPGStateManager owner { get; set; }

        public RPGState_Base(RPGStateManager owner, RPGState state) : base(owner, state)
        {
            this.owner = owner;
        }
    }

    public class RPGState_Init : RPGState_Base
    {
        public RPGState_Init(RPGStateManager owner) : base(owner, RPGState.Init)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("[RPGManager] OnEnter Init");
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);
            if (StateTime > 3)
            {
                SetNextState(RPGState.Lobby);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("[RPGManager] OnExit Init");
        }
    }

    public class RPGState_Lobby : RPGState_Base
    {
        public RPGState_Lobby(RPGStateManager owner) : base(owner, RPGState.Lobby)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("[RPGManager] OnEnter Lobby");
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);
            if (StateTime > 3)
            {
                SetNextState(RPGState.Battle);
            }
            
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("[RPGManager] OnExit Lobby");
        }
    }

    public class RPGState_Battle : RPGState_Base
    {

        private Unit _unit;

        public RPGState_Battle(RPGStateManager owner) : base(owner, RPGState.Battle)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _unit = RPGResourceManager.instance.GetUnit(1);
            

            Debug.Log("[RPGManager] OnEnter Battle");
        }


        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);
            if (StateTime > 3)
            {
                SetNextState(RPGState.Lobby);
            }
        }


        public override void OnExit()
        {
            base.OnExit();
            Object.Destroy(_unit.gameObject);
            Debug.Log("[RPGManager] OnExit Battle");
        }
    }

    #endregion
}