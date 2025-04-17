using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.FSM;
using JetBrains.Annotations;
using NUnit.Framework;
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
        _map.SetOnClick((x, y) => { });
        _map.SetOnDrop((int x, int y, int dirX, int dirY) =>
        {
            if (_isPause == false)
                SwapCell(x, y, dirX, dirY);
        });
        _map.StartCoroutine(SearchMatch3(null));
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
    private Match3Cell _otherCell;

    private void SwapCell(int x, int y, int dirX, int dirY)
    {
        _curCell = _map[x, y];
        var otherCell = _map[x + dirX, y + dirY];
        if (otherCell == null)
        {
            _curCell = null;
            return;
        }

        _otherCell = otherCell;
        var t = _curCell.type;
        _curCell.SetType(_otherCell.type);
        _otherCell.SetType(t);

        _map.StartCoroutine(SearchMatch3((matchCount) =>
        {
            if (matchCount == 0)
            {
                RestoreCell();
            }
        }));
    }

    private void RestoreCell()
    {
        var t = _curCell.type;
        _curCell.SetType(_otherCell.type);
        _otherCell.SetType(t);
    }

    private const float _dropTime = 0.3f;

    public IEnumerator SearchMatch3(Action<int> onFinish)
    {
        _isPause = true;
        int totalMatchCount = 0;
        while (true)
        {
            var matchList = new List<Match3Cell>();
            
            matchList.AddRange(SearchVerticalMatch3());
            matchList.AddRange(SearchHorizontalMatch3());

            matchList = matchList.Distinct().ToList();

            int matchCount = matchList.Count;
            totalMatchCount += matchCount;
            if (matchCount > 0)
            {
                foreach (var cell in matchList)
                {
                    cell.ActiveBorder(true);
                }

                yield return new WaitForSeconds(_dropTime);

                foreach (var cell in matchList)
                {
                    cell.ActiveBorder(false);
                    _map.StartCoroutine(cell.Effect(null));
                }

                yield return new WaitForSeconds(_dropTime);

                owner.AddScore(matchList.Count * 100);

                foreach (var cell in matchList)
                {
                    cell.SetType(Match3CellType.Empty);
                }

                yield return new WaitForSeconds(_dropTime);

                var emptyList = new List<Match3Cell>();
                emptyList.AddRange(matchList);

                while (true)
                {
                    if (DropCell() == false)
                        break;
                    yield return new WaitForSeconds(_dropTime);
                }
            }
            else
            {
                break;
            }
        }

        _isPause = false;
        onFinish?.Invoke(totalMatchCount);
    }
    
    private List<Match3Cell> SearchHorizontalMatch3()
    {
        var totalMatchList = new List<Match3Cell>();
        for (int y = 0; y < _map.height; y++)
        {
            Match3Cell pivot = null;
            var matchList = new List<Match3Cell>();
            for (int x = 0; x < _map.width; x++)
            {
                var curCell = _map.cells[x, y];
                if (pivot == null)
                {
                    pivot = curCell;
                    matchList.Add(pivot);
                }
                else
                {
                    if (pivot.type == curCell.type)
                    {
                        matchList.Add(curCell);
                    }
                    else
                    {
                        if (matchList.Count >= 3)
                        {
                            totalMatchList.AddRange(matchList);
                        }

                        pivot = curCell;
                        matchList.Clear();
                        matchList.Add(pivot);
                    }
                }
            }

            if (matchList.Count >= 3)
            {
                totalMatchList.AddRange(matchList);
            }
        }

        return totalMatchList;
    }
    private List<Match3Cell> SearchVerticalMatch3()
    {
        var totalMatchList = new List<Match3Cell>();
        for (int x = 0; x < _map.width; x++)
        {
            Match3Cell pivot = null;
            var matchList = new List<Match3Cell>();
            for (int y = 0; y < _map.height; y++)
            {
                var curCell = _map.cells[x, y];
                if (pivot == null)
                {
                    pivot = curCell;
                    matchList.Add(pivot);
                }
                else
                {
                    if (pivot.type == curCell.type)
                    {
                        matchList.Add(curCell);
                    }
                    else
                    {
                        if (matchList.Count >= 3)
                        {
                            totalMatchList.AddRange(matchList);
                        }
                            
                        pivot = curCell;
                        matchList.Clear();
                        matchList.Add(pivot);
                    }
                }
            }

            if (matchList.Count >= 3)
            {
                totalMatchList.AddRange(matchList);
            }
        }
        return totalMatchList;
    }
    
    private bool DropCell()
    {
        int emptyCellCount = 0;
        for (int x = 0; x < _map.width; x++)
        {
            for (int y = 0; y < _map.height; y++)
            {
                var cell = _map[x, y];
                if (cell.type == Match3CellType.Empty)
                {
                    emptyCellCount++;
                    var upCell = _map[x, y + 1];
                    if (upCell != null)
                    {
                        cell.SetType(upCell.type);
                        upCell.SetType(Match3CellType.Empty);
                    }
                    else
                    {
                        cell.SetRandomType();
                    }
                }
            }
        }
        return emptyCellCount > 0;
    }
}

public class Match3State_Result : Match3State_Base
{
    public Match3State_Result(Match3StateManager owner) : base(owner, Match3State.Result)
    {
    }
}

#endregion