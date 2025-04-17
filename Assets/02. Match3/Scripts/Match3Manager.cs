using Core.Singleton;
using UnityEngine;

public class Match3Manager : Singleton<Match3Manager>
{
    [SerializeField]
    private Match3StateManager _stateManager;
    [SerializeField]
    private Match3UI _ui;
    [SerializeField]
    private Match3Map _map;
    public Match3Map map => _map;

    protected override void Awake()
    {
        base.Awake();
        
        _ui.Init();
        
        _stateManager = new Match3StateManager();
        _stateManager.Init();
        
        _stateManager.SetOnChangeState(() =>
        {
            _ui.SetState(_stateManager.curState.State);
        });
        
        #region ========== Init State ==========

        _ui.SetOnClickPlay(() =>
        {
            _stateManager.PlayMatch3();
        });

        #endregion

        #region ========== Play State ==========

        _stateManager[Match3State.Play].SetOnEnter(() =>
        {
            _ui.OnEnterPlay();
        });
        _stateManager[Match3State.Play].SetOnFixedUpdate((deltaTime, stateTime) =>
        {
            var time = _stateManager.gameInfo.timeLimit - stateTime;
            _ui.UpdateTime(time);   
        });
        _stateManager.SetOnScoreChanged((score) =>
        {
            _ui.UpdateScore(score);
        });
        
        _ui.SetOnClickPause(() =>
        {
            _stateManager.PauseMatch3();
            _ui.OnPause();
        });
        _ui.SetOnClickResume(() =>
        {
            _stateManager.ResumeMatch3();
            _ui.OnResume();
        });
        
        _ui.SetOnClickExit(() =>
        {
            _stateManager.ExitMatch3();
        });

        #endregion

        #region ========== Result State ==========

        _stateManager[Match3State.Result].SetOnEnter(() =>
        {
            _ui.OnEnterResult(_stateManager.gameInfo);
        });
        _ui.SetOnClickFinish(() =>
        {
            _stateManager.InitMatch3();
        });

        #endregion
        
        _stateManager.InitMatch3();
    }
    
    private void Update()
    {
        _stateManager.OnUpdate(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        _stateManager.OnFixedUpdate(Time.fixedDeltaTime);
    }
}
