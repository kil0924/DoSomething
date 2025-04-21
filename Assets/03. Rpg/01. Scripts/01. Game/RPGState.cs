using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Core.FSM;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
    public class RPG_FSM : FSM<RPGState>
    {
        public RPGManager manager { get; private set; }
        public RPG_FSM(RPGManager manager)
        {
            this.manager = manager;
        }
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
        protected RPG_FSM _fsm { get; set; }
        protected RPGManager _manager => _fsm.manager;

        public RPGState_Base(RPG_FSM fsm, RPGState state) : base(fsm, state)
        {
            _fsm = fsm;
        }
    }

    public class RPGState_Init : RPGState_Base
    {
        public RPGState_Init(RPG_FSM fsm) : base(fsm, RPGState.Init)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _manager.BuildTeam();
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
        public RPGState_Lobby(RPG_FSM fsm) : base(fsm, RPGState.Lobby)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _manager.PrepareBattle();
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
        public RPGState_Battle(RPG_FSM fsm) : base(fsm, RPGState.Battle)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _actionIntervalTime = 0;
            _isBusy = false;
            Debug.Log("[RPGManager] OnEnter Battle");
        }

        private float _actionIntervalTime = 0;
        private float _actionInterval = 1f;
        private bool _isBusy = false;
        
        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);

            if (_isBusy == false)
            {
                if (_manager.CheckGameOver())
                {
                    SetNextState(RPGState.Lobby);
                    return;
                }
                
                _actionIntervalTime += deltaTime;
                if (_actionIntervalTime > _actionInterval)
                {
                    bool isLeft = Random.Range(0, 2) == 0;
                    
                    var caster = isLeft
                        ? _manager.leftTeam.GetAliveRandomUnit()
                        : _manager.rightTeam.GetAliveRandomUnit();

                    if (caster == null)
                        return;

                    if (caster.UseSkill(() => { _isBusy = false; }))
                    {
                        _actionIntervalTime = 0;
                        _isBusy = true;    
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("[RPGManager] OnExit Battle");
        }
    }

    #endregion
}