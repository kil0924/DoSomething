using System;
using System.Collections;
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
        _isPause = true;
        _map.MakeMapTest();
        _map.SetOnClick((x, y) =>
        {
            if(_isPause == false)
                ClickCell(x, y);
        });
        _map.StartCoroutine(FindMatch3(null));
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

    private Match3Cell _curCell;

    private void ClickCell(int x, int y)
    {
        if (_curCell == null)
        {
            _curCell = _map.cells[x, y];
            _map[x - 1, y]?.ActiveBorder(true);
            _map[x + 1, y]?.ActiveBorder(true);
            _map[x, y - 1]?.ActiveBorder(true);
            _map[x, y + 1]?.ActiveBorder(true);
        }
        else
        {
            if ((x == _curCell.x - 1 && y == _curCell.y)
                || (x == _curCell.x + 1 && y == _curCell.y)
                || (x == _curCell.x && y == _curCell.y - 1)
                || (x == _curCell.x && y == _curCell.y + 1))
            {
                var cell = _map.cells[x, y];
                var t = _curCell.type;
                _curCell.SetType(cell.type);
                cell.SetType(t);

                _map[_curCell.x - 1, _curCell.y]?.ActiveBorder(false);
                _map[_curCell.x + 1, _curCell.y]?.ActiveBorder(false);
                _map[_curCell.x, _curCell.y - 1]?.ActiveBorder(false);
                _map[_curCell.x, _curCell.y + 1]?.ActiveBorder(false);

                _curCell = null;

                _map.StartCoroutine(FindMatch3(null));
            }
        }
    }

    private float _dropTime = 0.3f;
    private IEnumerator FindMatch3(Action onFinish)
    {
        _isPause = true;
        while (true)
        {
            int t = 0;
            for (int x = 0; x < _map.width; x++)
            {
                for (int y = 0; y < _map.height; y++)
                {
                    var pivotCell = _map[x, y];
                    var pivotType = pivotCell.type;
                    var matchCount = 1;
                    for (int i = 1; i <= 4; i++)
                    {
                        if (x + i >= _map.width)
                        {
                            break;
                        }

                        var cell = _map[x + i, y];
                        if (cell.type != pivotType)
                        {
                            break;
                        }
                        else
                        {
                            matchCount++;
                        }
                    }

                    if (matchCount >= 3)
                    {
                        t++;
                        var finishCount = 0;
                        for (int i = 0; i < matchCount; i++)
                        {
                            var cell = _map.cells[x + i, y];
                            _map.StartCoroutine(cell.Effect(() => finishCount++));
                        }
                        
                        yield return new WaitUntil(() => finishCount == matchCount);
                        owner.AddScore(matchCount * 100);
                        
                        for (int i = 0; i < matchCount; i++)
                        {
                            var cell = _map.cells[x + i, y];
                            cell.SetColor(new Color(0,0,0,0.5f));
                        }
                        yield return new WaitForSeconds(_dropTime);

                        for (int j = 0; j < _map.height - y; j++)
                        {
                            for (int i = 0; i < matchCount; i++)
                            {
                                var cell = _map.cells[x + i, y + j];
                                if (y + j + 1 < _map.height)
                                {
                                    cell.SetType(_map.cells[x + i, y + j + 1].type);
                                }
                                else
                                {
                                    cell.SetRandomType();
                                }
                            }
                        }
                        yield return new WaitForSeconds(_dropTime);
                    }

                    // Vertical matching
                    matchCount = 1; // Reset match count for vertical check
                    for (int i = 1; i <= 4; i++)
                    {
                        if (y + i >= _map.height)
                        {
                            break;
                        }

                        var cell = _map.cells[x, y + i];
                        if (cell.type != pivotType)
                        {
                            break;
                        }
                        else
                        {
                            matchCount++;
                        }
                    }

                    if (matchCount >= 3)
                    {
                        t++;
                        var finishCount = 0;
                        for (int i = 0; i < matchCount; i++)
                        {
                            var cell = _map.cells[x, y + i];
                            _map.StartCoroutine(cell.Effect(() => finishCount++));
                        }

                        yield return new WaitUntil(() => finishCount == matchCount);
                        owner.AddScore(matchCount * 100);
                        
                        for (int i = 0; i < matchCount; i++)
                        {
                            var cell = _map.cells[x, y + i];
                            cell.SetColor(new Color(0,0,0,0.5f));
                        }
                        yield return new WaitForSeconds(_dropTime);

                        for (int k = 0; k < matchCount; k++)
                        {
                            for (int i = 0; i < _map.height - y; i++)
                            {
                                var cell = _map.cells[x, y + i];
                                if (i < matchCount - k - 1)
                                {
                                    cell.SetColor(new Color(0,0,0,0.5f));
                                }
                                else if (y + i + 1 < _map.height)
                                {
                                    cell.SetType(_map.cells[x, y + i + 1].type);
                                }
                                else
                                {
                                    cell.SetRandomType();
                                }
                            }
                            yield return new WaitForSeconds(_dropTime);
                        }
                    }
                }
            }

            if (t == 0)
                break;
        }
        _isPause = false;
        onFinish?.Invoke();
    }
}

public class Match3State_Result : Match3State_Base
{
    public Match3State_Result(Match3StateManager owner) : base(owner, Match3State.Result)
    {
    }
}

#endregion