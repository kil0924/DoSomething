using System;
using Core.FSM;
using UnityEngine;

public enum Match3State
{
    None,
    Init,
    Play,
    Result
}

public class Match3GameInfo
{
    public int score;
    public float timeLimit;
    public void Reset()
    {
        score = 0;
        timeLimit = 60;
    }
}

#region ========== FSM ==========
[Serializable]
public class Match3StateManager : FSM<Match3State>
{
    public Match3GameInfo gameInfo;
    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[Match3State.Init] = new Match3State_Init(this);
        stateDict[Match3State.Play] = new Match3State_Play(this);
        stateDict[Match3State.Result] = new Match3State_Result(this);
    }
    
    protected override void OnInit()
    {
        base.OnInit();
        gameInfo = new Match3GameInfo();
    }

    public void InitMatch3()
    {
        gameInfo.Reset();
        ChangeState(Match3State.Init);
    }
    
    public void PlayMatch3()
    {
        if (curState.State != Match3State.Init)
            return;
        
        curState.SetNextState(Match3State.Play);
    }

    public void PauseMatch3()
    {
        if (curState.State != Match3State.Play)
            return;

        ((Match3State_Play)curState).Pause();
    }

    public void ResumeMatch3()
    {
        if (curState.State != Match3State.Play)
            return;

        ((Match3State_Play)curState).Resume();
    }

    public void ExitMatch3()
    {
        if (curState.State != Match3State.Play)
            return;
        
        curState.SetNextState(Match3State.Result);
    }
    
    private Action<int> onScoreChanged;
    public void SetOnScoreChanged(Action<int> onScoreChanged)
    {
        this.onScoreChanged = onScoreChanged;
    }
    
    public void AddScore(int score)
    {
        gameInfo.score += score;
        onScoreChanged?.Invoke(gameInfo.score);
    }
}

#endregion

#region ========== State ==========

public class Match3State_Base : FSMState<Match3State>
{
    public Match3StateManager owner;
    public Match3State_Base(Match3StateManager owner, Match3State state) : base(owner, state)
    {
        this.owner = owner;
    }
}

public class Match3State_Init : Match3State_Base
{
    public Match3State_Init(Match3StateManager owner) : base(owner, Match3State.Init)
    {
    }
}

public class Match3State_Play : Match3State_Base
{
    public Match3State_Play(Match3StateManager owner) : base(owner, Match3State.Play)
    {
    }
    private Match3Map _map => Match3Manager.instance.map;

    public override void OnEnter()
    {
        base.OnEnter();
        _isPause = false;
        _map.MakeMapTest();
    }
    
    private bool _isPause = false;
    public void Pause()
    {
        _isPause = true;
    }
    public void Resume()
    {
        _isPause = false;
    }
    public override void OnUpdate(float deltaTime)
    {
        if (_isPause)
        {
            UpdatePause();
        }
        else
        {
            base.OnUpdate(deltaTime);
            UpdatePlay(deltaTime);
        }
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        if (_isPause == false)
        {
            base.OnFixedUpdate(deltaTime);
        }
    }

    private void UpdatePause()
    {
		
    }
    
    private void UpdatePlay(float deltaTime)
    {
        
    }
    
}

public class Match3State_Result : Match3State_Base
{
    public Match3State_Result(Match3StateManager owner) : base(owner, Match3State.Result)
    {
    }
}

#endregion