using Core.Singleton;
using UnityEngine;
using UnityEngine.Serialization;

public class Match3Manager : Singleton<Match3Manager>
{
    private Match3_FSM _fsm;
    [SerializeField]
    private Match3UI _ui;
    [SerializeField]
    private Match3Map _map;
    public Match3Map map => _map;

    protected override void Awake()
    {
        base.Awake();
        
        _ui.Init();
        
        _fsm = new Match3_FSM();
        _fsm.Init();
        
        _fsm.SetOnChangeState(() =>
        {
            _ui.SetState(_fsm.curState.State);
        });
        
        #region ========== Init State ==========

        _ui.SetOnClickPlay(() =>
        {
            _fsm.PlayMatch3();
        });

        #endregion

        #region ========== Play State ==========

        _fsm[Match3State.Play].SetOnEnter(() =>
        {
            _ui.OnEnterPlay();
        });
        _fsm[Match3State.Play].SetOnFixedUpdate((deltaTime, stateTime) =>
        {
            var time = _fsm.gameInfo.timeLimit - stateTime;
            _ui.UpdateTime(time);   
        });
        _fsm.SetOnScoreChanged((score) =>
        {
            _ui.UpdateScore(score);
        });
        
        _ui.SetOnClickPause(() =>
        {
            _fsm.PauseMatch3();
            _ui.OnPause();
        });
        _ui.SetOnClickResume(() =>
        {
            _fsm.ResumeMatch3();
            _ui.OnResume();
        });
        
        _ui.SetOnClickExit(() =>
        {
            _fsm.ExitMatch3();
        });

        #endregion

        #region ========== Result State ==========

        _fsm[Match3State.Result].SetOnEnter(() =>
        {
            _ui.OnEnterResult(_fsm.gameInfo);
        });
        _ui.SetOnClickFinish(() =>
        {
            _fsm.InitMatch3();
        });

        #endregion
        
        _fsm.InitMatch3();
    }
    
    private void Update()
    {
        _fsm.OnUpdate(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        _fsm.OnFixedUpdate(Time.fixedDeltaTime);
    }
}
