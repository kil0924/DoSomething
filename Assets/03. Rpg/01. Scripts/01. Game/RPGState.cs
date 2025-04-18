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
        private List<Unit> _leftSide = new List<Unit>();
        private List<Unit> _rightSide = new List<Unit>();
        
        public RPGState_Battle(RPGStateManager owner) : base(owner, RPGState.Battle)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            for(int i=1; i<=10; i++ )
            {
                var unit = RPGResourceManager.instance.GetUnit(i);
                
                var isLeft = i % 2 != 0;
                if (isLeft == false)
                {
                    int t = _rightSide.Count;
                    var isFront = t % 2 != 0;
                    _rightSide.Add(unit);
                    unit.transform.position = new Vector3(isFront ? 5 : 2, 0, (t - 2) * 2);
                    unit.SetSide(false);
                }
                else
                {
                    int t = _leftSide.Count;
                    var isFront = t % 2 != 0;
                    _leftSide.Add(unit);
                    unit.transform.position = new Vector3(isFront ? -5 : -2, 0, (t - 2) * 2);
                    unit.SetSide(true);
                }
                
                unit.PlayAnimation("Idle");
                unit.transform.SetParent(RPGManager.instance.transform);
            }

            Debug.Log("[RPGManager] OnEnter Battle");
        }

        private float _time = 0;
        private Unit _movingUnit;
        public override void OnFixedUpdate(float deltaTime)
        {
            base.OnFixedUpdate(deltaTime);

            if (_movingUnit == null)
            {
                _time += deltaTime;
                if (_time > 1)
                {
                    _time = 0;
                    
                    bool isLeft = Random.Range(0, 2) == 0;
                    var unit = isLeft ? _leftSide.GetRandom() : _rightSide.GetRandom();
                    var target = isLeft ? _rightSide.GetRandom() : _leftSide.GetRandom();
                    unit.DoAttack(target);
                    
                    _movingUnit = unit;
                }
            }
            else
            {
                if (_movingUnit.curState == UnitState.Idle)
                {
                    _movingUnit = null;
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