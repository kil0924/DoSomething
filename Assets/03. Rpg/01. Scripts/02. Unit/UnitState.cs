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
}
public class UnitState_Idle : UnitState_Base
{
    public UnitState_Idle(UnitStateManager owner) : base(owner, UnitState.Idle)
    {
    }
}
public class UnitState_Move : UnitState_Base
{

    public UnitState_Move(UnitStateManager owner) : base(owner, UnitState.Move)
    {
    }
}   

public class UnitState_Attack : UnitState_Base
{

    public UnitState_Attack(UnitStateManager owner) : base(owner, UnitState.Attack)
    {
    }
}

public class UnitState_Death : UnitState_Base
{

    public UnitState_Death(UnitStateManager owner) : base(owner, UnitState.Death)
    {
    }
}
#endregion