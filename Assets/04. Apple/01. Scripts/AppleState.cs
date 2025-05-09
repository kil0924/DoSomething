using Core.FSM;
using UnityEngine;

public enum AppleState
{
    None,
    Init,
    Play,
    Result
}

#region ========== FSM ==========

public class Apple_FSM : FSM<AppleState>
{
    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[AppleState.Init] = new AppleState_Init(this);
        stateDict[AppleState.Play] = new AppleState_Play(this);
        stateDict[AppleState.Result] = new AppleState_Result(this);
    }
}

#endregion

#region ========== State ==========

public class AppleState_Base : FSMState<AppleState>
{
    protected Apple_FSM _fsm;

    public AppleState_Base(Apple_FSM fsm, AppleState state) : base(fsm, state)
    {
        _fsm = fsm;
    }
}

public class AppleState_Init : AppleState_Base
{
    public AppleState_Init(Apple_FSM fsm) : base(fsm, AppleState.Init)
    {
        
    }
}

public class AppleState_Play : AppleState_Base
{
    public AppleState_Play(Apple_FSM fsm) : base(fsm, AppleState.Play)
    {
        
    }
}

public class AppleState_Result : AppleState_Base
{
    public AppleState_Result(Apple_FSM fsm) : base(fsm, AppleState.Result)
    {
        
    }
}


#endregion