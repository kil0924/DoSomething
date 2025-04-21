using System;
using Core;
using UnityEngine;
using Core.FSM;
using RPG;
using Spine;
using AnimationState = Spine.AnimationState;

public enum UnitState
{
    None,
    Init,
    Idle,
    Move,
    Attack,
    MoveBack,
    Death,
}

#region ========== FSM ==========

[Serializable]
public class UnitStateManager : FSM<UnitState>
{
    public Unit unit { get; private set; }
    public UnitStateManager(Unit unit)
    {
        this.unit = unit;
    }
    
    #region ========== 초기화 ==========

    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[UnitState.Init] = new UnitState_Init(this);
        stateDict[UnitState.Idle] = new UnitState_Idle(this);
        stateDict[UnitState.Move] = new UnitState_Move(this);
        stateDict[UnitState.Attack] = new UnitState_Attack(this);
        stateDict[UnitState.MoveBack] = new UnitState_MoveBack(this);
        stateDict[UnitState.Death] = new UnitState_Death(this);
    }

    protected override void OnInit()
    {
        base.OnInit();
        ChangeState(UnitState.Init);
    }

    #endregion
}

#endregion

#region ========== State ==========

public class UnitState_Base : FSMState<UnitState>
{
    public UnitStateManager Fsm { get; set; }
    public UnitState_Base(UnitStateManager fsm, UnitState state) : base(fsm, state)
    {
        this.Fsm = fsm;
    }
}
public class UnitState_Init : UnitState_Base
{
    public UnitState_Init(UnitStateManager fsm) : base(fsm, UnitState.Init)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        SetNextState(UnitState.Idle);
    }
    
}
public class UnitState_Idle : UnitState_Base
{
    public UnitState_Idle(UnitStateManager fsm) : base(fsm, UnitState.Idle)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Idle");
    }
}
public class UnitState_Move : UnitState_Base
{
    private float _speed = 5f;
    public UnitState_Move(UnitStateManager fsm) : base(fsm, UnitState.Move)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Move");
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);

        Fsm.unit.MoveTo(Vector3.zero, _speed, deltaTime, () =>
        {
            SetNextState(UnitState.Attack);
        });
    }
}   

public class UnitState_Attack : UnitState_Base
{
    private AnimationState.TrackEntryDelegate _onCompleteAnimation;
    public UnitState_Attack(UnitStateManager fsm) : base(fsm, UnitState.Attack)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Attack1_1", false, OnCompleteAttack);
        
        var target = Fsm.unit.team.enemyTeam.GetAliveRandomUnit();
        Fsm.unit.Attack(target, () =>
        {
            Fsm.unit.AddAnimation("Victory", false, OnCompleteAttack);
        });
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
    }

    private void OnCompleteAttack(TrackEntry trackEntry)
    {
        SetNextState(UnitState.MoveBack);
    }
}


public class UnitState_MoveBack : UnitState_Base
{
    private float _speed = 5f;
    public UnitState_MoveBack(UnitStateManager fsm) : base(fsm, UnitState.Attack)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Move");
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);

        Fsm.unit.MoveTo(Fsm.unit.originPos, _speed, deltaTime, () =>
        {
            Fsm.unit.OnFinishSkill();
            SetNextState(UnitState.Idle);
        });
    }
}

public class UnitState_Death : UnitState_Base
{
    public UnitState_Death(UnitStateManager fsm) : base(fsm, UnitState.Death)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Die", false);
    }
}
#endregion