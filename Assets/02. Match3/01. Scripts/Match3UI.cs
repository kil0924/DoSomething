using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Match3UI : MonoBehaviour
{
    [Header("Match3 State UI")]
    [SerializeField]
    private GameObject _initState;
    [SerializeField]
    private GameObject _playState;
    [SerializeField]
    private GameObject _resultState;
    
    private Dictionary<Match3State, GameObject> _stateUI = new Dictionary<Match3State, GameObject>();
    
    private GameObject _currentTetrisStateUI;

    [Header("Common")] 
    public TextMeshProUGUI textState;

    public void Init()
    {
        _stateUI.Add(Match3State.Init, _initState);
        _stateUI.Add(Match3State.Play, _playState);
        _stateUI.Add(Match3State.Result, _resultState);
    }

    public void SetState(Match3State state)
    {
        foreach (var ui in _stateUI)
        {
            ui.Value.SetActive(false);
        }
        _currentTetrisStateUI = _stateUI[state];
        _currentTetrisStateUI.SetActive(true);
        
        SetStateText(state.ToString());        
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
        time = Mathf.Max(0, time);
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

    public void OnEnterResult(Match3GameInfo info)
    {
        SetResultScore(info.score);
    }

    private void SetResultScore(int score)
    {
        textResultScore.text = $"Score : {score}";
    }
    
    #endregion
    
}
