using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Core.FSM;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Rpg
{
    public enum RpgState
    {
        None,
        Init,
        Lobby,
        Battle,
    }

    #region ========== FSM ==========

    [Serializable]
    public class Rpg_FSM : FSM<RpgState>
    {
        public RpgManager manager { get; private set; }
        public Rpg_FSM(RpgManager manager)
        {
            this.manager = manager;
        }
        #region ========== 초기화 ==========

        protected override void BuildStateDict()
        {
            base.BuildStateDict();
            stateDict[RpgState.Init] = new RpgState_Init(this);
            stateDict[RpgState.Lobby] = new RpgState_Lobby(this);
            stateDict[RpgState.Battle] = new RpgState_Battle(this);
        }

        protected override void OnInit()
        {
            base.OnInit();
            ChangeState(RpgState.Init);
        }

        #endregion
    }

    #endregion

    #region ========== State ==========

    public class RpgState_Base : FSMState<RpgState>
    {
        protected Rpg_FSM _fsm { get; set; }
        protected RpgManager _manager => _fsm.manager;

        public RpgState_Base(Rpg_FSM fsm, RpgState state) : base(fsm, state)
        {
            _fsm = fsm;
        }
    }

    public class RpgState_Init : RpgState_Base
    {
        public RpgState_Init(Rpg_FSM fsm) : base(fsm, RpgState.Init)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _manager.BuildTeam();
            Debug.Log("[RpgManager] OnEnter Init");
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);
            if (StateTime > 3)
            {
                SetNextState(RpgState.Lobby);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("[RpgManager] OnExit Init");
        }
    }

    public class RpgState_Lobby : RpgState_Base
    {
        public RpgState_Lobby(Rpg_FSM fsm) : base(fsm, RpgState.Lobby)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _manager.PrepareBattle();
            Debug.Log("[RpgManager] OnEnter Lobby");
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);
            if (StateTime > 3)
            {
                SetNextState(RpgState.Battle);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("[RpgManager] OnExit Lobby");
        }
    }

    public class RpgState_Battle : RpgState_Base
    {
        public RpgState_Battle(Rpg_FSM fsm) : base(fsm, RpgState.Battle)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _actionIntervalTime = 0;
            _isBusy = false;
            Debug.Log("[RpgManager] OnEnter Battle");
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
                    SetNextState(RpgState.Lobby);
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
            Debug.Log("[RpgManager] OnExit Battle");
        }
    }

    #endregion
}