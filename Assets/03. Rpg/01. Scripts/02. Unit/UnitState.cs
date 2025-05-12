using System;
using Core;
using UnityEngine;
using Core.FSM;
using Rpg;
using Spine;
using AnimationState = Spine.AnimationState;

public enum UnitState
{
    None,
    Init,
    Idle,
    Turn,
    Death,
}

public enum UnitSubState
{
    None,
    Turn_Waiting,
    Turn_Start,
    Turn_Move,
    Turn_Skill,
    Turn_MoveBack,
    Turn_End,
}

#region ========== FSM ==========

[Serializable]
public class Unit_FSM : FSM<UnitState>
{
    public Unit unit { get; private set; }
    public Unit_FSM(Unit unit)
    {
        this.unit = unit;
    }
    
    #region ========== 초기화 ==========

    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[UnitState.Init] = new UnitState_Init(this);
        stateDict[UnitState.Idle] = new UnitState_Idle(this);
        stateDict[UnitState.Turn] = new UnitState_Turn(this);
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
    public Unit_FSM Fsm { get; set; }
    public UnitState_Base(Unit_FSM fsm, UnitState state) : base(fsm, state)
    {
        this.Fsm = fsm;
    }
}
public class UnitState_Init : UnitState_Base
{
    public UnitState_Init(Unit_FSM fsm) : base(fsm, UnitState.Init)
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
    public UnitState_Idle(Unit_FSM fsm) : base(fsm, UnitState.Idle)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Idle");
    }
}

public class UnitState_Turn : UnitState_Base
{
    private TurnSub_FSM _subFsm;
    public UnitState_Turn(Unit_FSM fsm) : base(fsm, UnitState.Turn)
    {
        _subFsm = new TurnSub_FSM(fsm);
        _subFsm.Init();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _subFsm.curState.SetNextState(UnitSubState.Turn_Start);
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        _subFsm.OnUpdate(deltaTime);
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        _subFsm.OnFixedUpdate(deltaTime);
    }
}

#region ========== Turn Sub State ==========

public class TurnSub_FSM : FSM<UnitSubState>
{
    public Unit Unit { get; private set; }
    public Unit_FSM Parent { get; private set; }
    public TurnSub_FSM(Unit_FSM fsm)
    {
        Unit = fsm.unit;
        Parent = fsm;
    }
    
    #region ========== 초기화 ==========

    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[UnitSubState.Turn_Waiting] = new TurnSubState_Waiting(this, Parent);
        stateDict[UnitSubState.Turn_Start] = new TurnSubState_Start(this, Parent);
        stateDict[UnitSubState.Turn_Move] = new TurnSubState_Move(this, Parent);
        stateDict[UnitSubState.Turn_Skill] = new TurnSubState_Skill(this, Parent);
        stateDict[UnitSubState.Turn_MoveBack] = new TurnSubState_MoveBack(this, Parent);
        stateDict[UnitSubState.Turn_End] = new TurnSubState_End(this, Parent);
    }

    protected override void OnInit()
    {
        base.OnInit();
        ChangeState(UnitSubState.Turn_Waiting);
    }

    #endregion
}
public class TurnSubState_Base : FSMState<UnitSubState>
{
    protected TurnSub_FSM Fsm { get; set; }
    protected Unit_FSM Parent { get; set; }
    public TurnSubState_Base(TurnSub_FSM fsm, Unit_FSM parent, UnitSubState state) : base(fsm, state)
    {
        Fsm = fsm;
        Parent = parent;
    }
}
public class TurnSubState_Waiting : TurnSubState_Base
{
    public TurnSubState_Waiting(TurnSub_FSM fsm, Unit_FSM parent) : base(fsm, parent, UnitSubState.Turn_Waiting)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
    }
}
public class TurnSubState_Start : TurnSubState_Base
{
    public TurnSubState_Start(TurnSub_FSM fsm, Unit_FSM parent) : base(fsm, parent, UnitSubState.Turn_Start)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.Unit.ProcessTurn();
        SetNextState(UnitSubState.Turn_Move);
    }
}
public class TurnSubState_Move : TurnSubState_Base
{
    private float _speed = 5f;
    public TurnSubState_Move(TurnSub_FSM fsm, Unit_FSM parent) : base(fsm, parent, UnitSubState.Turn_Move)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.Unit.PlayAnimation("Move");
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        Fsm.Unit.MoveTo(Vector3.zero, _speed, deltaTime, () =>
        {
        });
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);

        Fsm.Unit.MoveTo(Vector3.zero, _speed, deltaTime, () =>
        {
            SetNextState(UnitSubState.Turn_Skill);
        });
    }

}   

public class TurnSubState_Skill : TurnSubState_Base
{
    private AnimationState.TrackEntryDelegate _onCompleteAnimation;
    private UnitSkill _skill;
    private bool _invoked = false;
    public TurnSubState_Skill(TurnSub_FSM fsm, Unit_FSM parent) : base(fsm, parent,UnitSubState.Turn_Skill)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        _skill = Fsm.Unit.SelectSkill();
        _invoked = false;
        
        Fsm.Unit.PlayAnimation(_skill.aniName, false, OnCompleteAttack);
        
        // var target = Fsm.Unit.team.enemyTeam.GetAliveRandomUnit();
        // Fsm.Unit.Attack(target, () =>
        // {
        //     Fsm.Unit.AddAnimation("Victory", false, OnCompleteAttack);
        // });
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        if (_invoked == false && StateTime > 0.5f)
        {
            _invoked = true;
            _skill.Invoke(Fsm.Unit);
        }
    }

    private void OnCompleteAttack(TrackEntry trackEntry)
    {
        SetNextState(UnitSubState.Turn_MoveBack);
    }
}


public class TurnSubState_MoveBack : TurnSubState_Base
{
    private float _speed = 5f;
    public TurnSubState_MoveBack(TurnSub_FSM fsm, Unit_FSM parent) : base(fsm, parent, UnitSubState.Turn_MoveBack)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.Unit.PlayAnimation("Move");
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        Fsm.Unit.MoveTo(Fsm.Unit.originPos, _speed, deltaTime, () =>
        {
        });
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);

        Fsm.Unit.MoveTo(Fsm.Unit.originPos, _speed, deltaTime, () =>
        {
            SetNextState(UnitSubState.Turn_End);
        });
    }
}

public class TurnSubState_End : TurnSubState_Base
{
    public TurnSubState_End(TurnSub_FSM fsm, Unit_FSM parent) : base(fsm, parent, UnitSubState.Turn_End)
    {}

    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.Unit.OnFinishTurn();
        
        SetNextState(UnitSubState.Turn_Waiting);
        Parent.curState.SetNextState(UnitState.Idle);
    }
}
#endregion


public class UnitState_Death : UnitState_Base
{
    public UnitState_Death(Unit_FSM fsm) : base(fsm, UnitState.Death)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        Fsm.unit.PlayAnimation("Die", false);
    }
}
#endregion