using System.Collections.Generic;
using Core.Singleton;
using UnityEngine;
using UnityEngine.Serialization;

public class TetrisManager : Singleton<TetrisManager>
{
    private Tetris_FSM _fsm;
    [SerializeField]
    private TetrisUI _tetrisUI;
    [SerializeField]
    private TetrisMap _tetrisMap;
    public TetrisMap tetrisMap => _tetrisMap;

    protected override void Awake()
    {
        base.Awake();
        
        _tetrisUI.Init();
        
        _fsm = new Tetris_FSM();
        _fsm.Init();
        
        _fsm.SetOnChangeState(state =>
        {
            _tetrisUI.SetTetrisState(state);
        });

        #region ========== Init State ==========

        _tetrisUI.SetOnClickPlay(() =>
        {
            _fsm.PlayTetris();
        });

        #endregion

        #region ========== Play State ==========

        _fsm[TetrisState.Play].SetOnEnter(() =>
        {
            _tetrisUI.OnEnterPlay();
        });
        _fsm[TetrisState.Play].SetOnFixedUpdate((deltaTime, stateTime) =>
        {
            _tetrisUI.UpdateTime(stateTime);   
        });
        _fsm.SetOnScoreChanged((score) =>
        {
            _tetrisUI.UpdateScore(score);
        });
        
        _tetrisUI.SetOnClickPause(() =>
        {
            _fsm.PauseTetris();
            _tetrisUI.OnPause();
        });
        _tetrisUI.SetOnClickResume(() =>
        {
            _fsm.ResumeTetris();
            _tetrisUI.OnResume();
        });
        
        _tetrisUI.SetOnClickExit(() =>
        {
            _fsm.ExitTetris();
        });

        #endregion

        #region ========== Result State ==========

        _fsm[TetrisState.Result].SetOnEnter(() =>
        {
            _tetrisUI.OnEnterResult(_fsm.gameInfo);
        });
        _tetrisUI.SetOnClickFinish(() =>
        {
            _fsm.InitTetris();
        });

        #endregion
        
        _fsm.InitTetris();
    }
    
    private void Update()
    {
        _fsm.OnUpdate(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        _fsm.OnFixedUpdate(Time.fixedDeltaTime);
    }
    
    public void UpdateScore(int score)
    {
        _tetrisUI.UpdateScore(score);
    }
}