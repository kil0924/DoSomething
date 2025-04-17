using System.Collections.Generic;
using Core.Singleton;
using UnityEngine;

public class TetrisManager : Singleton<TetrisManager>
{
    [SerializeField]
    private TetrisStateManager _stateManager;
    [SerializeField]
    private TetrisUI _tetrisUI;
    [SerializeField]
    private TetrisMap _tetrisMap;
    public TetrisMap tetrisMap => _tetrisMap;

    protected override void Awake()
    {
        base.Awake();
        
        _tetrisUI.Init();
        
        _stateManager = new TetrisStateManager();
        _stateManager.Init();
        
        _stateManager.SetOnChangeState(() =>
        {
            _tetrisUI.SetTetrisState(_stateManager.curState.State);
        });

        #region ========== Init State ==========

        _tetrisUI.SetOnClickPlay(() =>
        {
            _stateManager.PlayTetris();
        });

        #endregion

        #region ========== Play State ==========

        _stateManager[TetrisState.Play].SetOnEnter(() =>
        {
            _tetrisUI.OnEnterPlay();
        });
        _stateManager[TetrisState.Play].SetOnFixedUpdate((deltaTime, stateTime) =>
        {
            _tetrisUI.UpdateTime(stateTime);   
        });
        _stateManager.SetOnScoreChanged((score) =>
        {
            _tetrisUI.UpdateScore(score);
        });
        
        _tetrisUI.SetOnClickPause(() =>
        {
            _stateManager.PauseTetris();
            _tetrisUI.OnPause();
        });
        _tetrisUI.SetOnClickResume(() =>
        {
            _stateManager.ResumeTetris();
            _tetrisUI.OnResume();
        });
        
        _tetrisUI.SetOnClickExit(() =>
        {
            _stateManager.ExitTetris();
        });

        #endregion

        #region ========== Result State ==========

        _stateManager[TetrisState.Result].SetOnEnter(() =>
        {
            _tetrisUI.OnEnterResult(_stateManager.gameInfo);
        });
        _tetrisUI.SetOnClickFinish(() =>
        {
            _stateManager.InitTetris();
        });

        #endregion
        
        _stateManager.InitTetris();
    }
    
    private void Update()
    {
        _stateManager.OnUpdate(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        _stateManager.OnFixedUpdate(Time.fixedDeltaTime);
    }
    
    public void UpdateScore(int score)
    {
        _tetrisUI.UpdateScore(score);
    }
}