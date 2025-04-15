using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TetrisUI : MonoBehaviour
{
    private Dictionary<TetrisState, GameObject> _tetrisStateUI = new Dictionary<TetrisState, GameObject>();
    [Header("Tetris State UI")]
    [SerializeField]
    private GameObject _stateStart;
    [SerializeField]
    private GameObject _statePlay;
    [SerializeField]
    private GameObject _statePause;
    [SerializeField]
    private GameObject _stateGameOver;
    
    private GameObject _currentTetrisStateUI;

    [Header("Common")] 
    public TextMeshProUGUI textState;

    public void Init()
    {
        _tetrisStateUI.Add(TetrisState.Init, _stateStart);
        _tetrisStateUI.Add(TetrisState.Play, _statePlay);
        _tetrisStateUI.Add(TetrisState.Result, _stateGameOver);
    }
    
    public void SetTetrisState(TetrisState tetrisState)
    {
        foreach (var tetrisStateUI in _tetrisStateUI)
        {
            tetrisStateUI.Value.SetActive(false);
        }
        _currentTetrisStateUI = _tetrisStateUI[tetrisState];
        _currentTetrisStateUI.SetActive(true);
        SetStateText(tetrisState.ToString());
    }
    
    private void SetStateText(string text)
    {
        if (textState != null)
        {
            textState.text = text;
        }
    }
    
    #region ========== Init ==========

    [Header("Init")] 
    public Button btnPlay;

    private Action onClickPlay;
    public void SetOnClickPlay(Action onClickPlay)
    {
        this.onClickPlay = onClickPlay;
        btnPlay.onClick.AddListener(()=> this.onClickPlay?.Invoke());
    }

    #endregion
    
    #region ========== Play ==========

    [Header("Play")]

    public TextMeshProUGUI textTime;
    public TextMeshProUGUI textScore;
    
    public void OnEnterPlay()
    {
        panelPause.SetActive(false);
    }


    public void UpdateTime(float time)
    {
        textTime.text = $"Time : {time:0.00}s";
    }
    public void UpdateScore(int score)
    {
        textScore.text = $"Score : {score}";
    }

    public GameObject panelPause;
    public Button btnPause;
    private Action onClickPause;
    public void SetOnClickPause(Action onClickPause)
    {
        this.onClickPause = onClickPause;
        btnPause.onClick.AddListener(()=> this.onClickPause?.Invoke());
    }

    public void OnPause()
    {
        panelPause.SetActive(true);
    }
    public Button btnResume;
    private Action onClickResume;
    public void SetOnClickResume(Action onClickResume)
    {
        this.onClickResume = onClickResume;
        btnResume.onClick.AddListener(()=> this.onClickResume?.Invoke());
    }
    public void OnResume()
    {
        panelPause.SetActive(false);
    }
    
    public Button btnExit;
    private Action onClickExit;
    public void SetOnClickExit(Action onClickExit)
    {
        this.onClickExit = onClickExit;
        btnExit.onClick.AddListener(()=> this.onClickExit?.Invoke());
    }

    #endregion

    #region ========== Result ==========

    [Header("Result")]
    public TextMeshProUGUI textResultScore;
    
    public Button btnFinish;
    private Action onClickFinish;
    public void SetOnClickFinish(Action onClickFinish)
    {
        this.onClickFinish = onClickFinish;
        btnFinish.onClick.AddListener(()=> this.onClickFinish?.Invoke());
    }

    public void OnEnterResult(TetrisGameInfo info)
    {
        SetResultScore(info.score);
    }

    private void SetResultScore(int score)
    {
        textResultScore.text = $"Score : {score}";
    }
    
    #endregion
    
}
