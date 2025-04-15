using System;
using UnityEngine;
using Core.FSM;
public enum UnitState
{
    None,
    Init,
    Idle,
    Move,
    Attack,
    Death,
}

#region ========== FSM ==========

[Serializable]
public class UnitStateManager : FSM<UnitState>
{
    #region ========== 초기화 ==========

    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[UnitState.Init] = new UnitState_Init(this);
        stateDict[UnitState.Idle] = new UnitState_Idle(this);
        stateDict[UnitState.Move] = new UnitState_Move(this);
        stateDict[UnitState.Attack] = new UnitState_Attack(this);
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

public class UnitState_Init : FSMState<UnitState>
{
    public new UnitStateManager owner { get; set; }
    public UnitState_Init(UnitStateManager owner) : base(owner, UnitState.Init)
    {
        this.owner = owner;
    }
}
public class UnitState_Idle : FSMState<UnitState>
{
    public new UnitStateManager owner { get; set; }
    public UnitState_Idle(UnitStateManager owner) : base(owner, UnitState.Idle)
    {
        this.owner = owner;
    }
}
public class UnitState_Move : FSMState<UnitState>
{
    public new UnitStateManager owner { get; set; }

    public UnitState_Move(UnitStateManager owner) : base(owner, UnitState.Move)
    {
        this.owner = owner;
    }
}   

public class UnitState_Attack : FSMState<UnitState>
{
    public new UnitStateManager owner { get; set; }

    public UnitState_Attack(UnitStateManager owner) : base(owner, UnitState.Attack)
    {
        this.owner = owner;
    }
}

public class UnitState_Death : FSMState<UnitState>
{
    public new UnitStateManager owner { get; set; }

    public UnitState_Death(UnitStateManager owner) : base(owner, UnitState.Death)
    {
        this.owner = owner;
    }
}
#endregion