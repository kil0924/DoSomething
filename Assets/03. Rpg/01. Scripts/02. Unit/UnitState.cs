using System;
using UnityEngine;
using Core.FSM;
using RPG;

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
    
    public void PlayAnimation(string aniName, bool isLoop = true)
    {
        unit.PlayAnimation(aniName, isLoop);
    }
    
}

#endregion

#region ========== State ==========

public class UnitState_Base : FSMState<UnitState>
{
    public UnitStateManager owner { get; set; }
    public UnitState_Base(UnitStateManager owner, UnitState state) : base(owner, state)
    {
        this.owner = owner;
    }
}
public class UnitState_Init : UnitState_Base
{
    public UnitState_Init(UnitStateManager owner) : base(owner, UnitState.Init)
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
    public UnitState_Idle(UnitStateManager owner) : base(owner, UnitState.Idle)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        owner.PlayAnimation("Idle");
    }
}
public class UnitState_Move : UnitState_Base
{
    public UnitState_Move(UnitStateManager owner) : base(owner, UnitState.Move)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        owner.PlayAnimation("Move");
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);

        var target = owner.unit.target;
        
        if (target == null)
        {
            SetNextState(UnitState.Idle);
            return;
        }
        
        var dir = target.transform.localPosition - owner.unit.transform.localPosition;
        owner.unit.transform.localPosition += deltaTime * 5 * dir.normalized;

        if (Vector3.SqrMagnitude(dir) < 1f)
        {
            SetNextState(UnitState.Attack);
        }
    }
}   

public class UnitState_Attack : UnitState_Base
{
    public UnitState_Attack(UnitStateManager owner) : base(owner, UnitState.Attack)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        owner.PlayAnimation("Attack1_1", false);
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        if (owner.unit.IsCompleteAnimation())
        {
            SetNextState(UnitState.MoveBack);
        }
    }
}


public class UnitState_MoveBack : UnitState_Base
{
    public UnitState_MoveBack(UnitStateManager owner) : base(owner, UnitState.Attack)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        owner.PlayAnimation("Move");
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
        
        var dir = owner.unit.originPos - owner.unit.transform.localPosition;
        owner.unit.transform.localPosition += deltaTime * 5 * dir.normalized;

        if (Vector3.SqrMagnitude(dir) < 0.01f)
        {
            owner.unit.transform.localPosition = owner.unit.originPos;
            SetNextState(UnitState.Idle);
        }
    }
}

public class UnitState_Death : UnitState_Base
{

    public UnitState_Death(UnitStateManager owner) : base(owner, UnitState.Death)
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        owner.PlayAnimation("Die");
    }
}
#endregion