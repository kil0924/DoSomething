using System;
using UnityEngine;
using Core.FSM;
using Object = UnityEngine.Object;

public enum GameState
{
    None,
    Init,
    Lobby,
    Battle,
}

#region ========== FSM ==========
[Serializable]
public class GameStateManager : FSM<GameState>
{
    #region ========== 초기화 ==========
    protected override void BuildStateDict()
    {
        base.BuildStateDict();
        stateDict[GameState.Init] = new GameState_Init(this);
        stateDict[GameState.Lobby] = new GameState_Lobby(this);
        stateDict[GameState.Battle] = new GameState_Battle(this);
    }
    protected override void OnInit()
    {
        base.OnInit();
        ChangeState(GameState.Init);
    }

    #endregion
}

#endregion

#region ========== State ==========
public class GameState_Init : FSMState<GameState>
{
    public new GameStateManager owner { get; set; }
    public GameState_Init(GameStateManager owner) : base(owner, GameState.Init)
    {
        this.owner = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("[GameManager] OnEnter Init");
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
    }

    public override GameState CheckExitCondition()
    {
        if (StateTime > 3f)
            return GameState.Lobby;
        else
            return State;
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("[GameManager] OnExit Init");
    }
}
public class GameState_Lobby : FSMState<GameState>
{
    public new GameStateManager owner { get; set; }
    public GameState_Lobby(GameStateManager owner) : base(owner, GameState.Lobby)
    {
        this.owner = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("[GameManager] OnEnter Lobby");
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
    }

    public override GameState CheckExitCondition()
    {
        if (StateTime > 3f)
            return GameState.Battle;
        else
            return State;
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("[GameManager] OnExit Lobby");
    }
}

public class GameState_Battle : FSMState<GameState>
{
    public new GameStateManager owner { get; set; }
    
    private Unit _unit;
    public GameState_Battle(GameStateManager owner) : base(owner, GameState.Battle)
    {
        this.owner = owner;
    }
    public override void OnEnter()
    {
        base.OnEnter();

        _unit = GameManager.instance.CreateUnit(1);
        
        Debug.Log("[GameManager] OnEnter Battle");
    }

    
    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);
    }

    public override GameState CheckExitCondition()
    {
        if (StateTime > 3f)
            return GameState.Lobby;
        else
            return State;
    }

    public override void OnExit()
    {
        base.OnExit();
        Object.Destroy(_unit.gameObject);
        Debug.Log("[GameManager] OnExit Battle");
    }
}

#endregion
