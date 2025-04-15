
using System;
using System.Collections;
using System.Collections.Generic;
using Core.FSM;
using NUnit.Framework;
using UnityEngine;

public enum TetrisState
{
	None,
	Init,
	Play,
	Result
}

public class TetrisGameInfo
{
	public int score;
	public int level;
	public int lines;

	public void Reset()
	{
		score = 0;
		level = 0;
		lines = 0;		
	}
}
#region ========== FSM ==========
[Serializable]
public class TetrisStateManager : FSM<TetrisState>
{
	public TetrisGameInfo gameInfo;
	protected override void BuildStateDict()
	{
		base.BuildStateDict();
		stateDict[TetrisState.Init] = new TetrisState_Init(this);
		stateDict[TetrisState.Play] = new TetrisState_Play(this);
		stateDict[TetrisState.Result] = new TetrisState_Result(this);
	}
	protected override void OnInit()
	{
		base.OnInit();
		gameInfo = new TetrisGameInfo();
	}
	public void InitTetris()
	{
		gameInfo.Reset();
		ChangeState(TetrisState.Init);
	}

	public void PlayTetris()
	{
		if (curState.State != TetrisState.Init)
			return;

		curState.SetNextState(TetrisState.Play);
	}
	
	public void PauseTetris()
	{
		if (curState.State != TetrisState.Play)
			return;
		
		((TetrisState_Play)curState).PauseTetris();
	}
	public void ResumeTetris()
	{
		if (curState.State != TetrisState.Play)
			return;
		((TetrisState_Play)curState).ResumeTetris();
	}

	public void ExitTetris()
	{
		if (curState.State != TetrisState.Play)
			return;
		
		curState.SetNextState(TetrisState.Result);
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

public class TetrisState_Base : FSMState<TetrisState>
{
	protected TetrisStateManager owner;
	public TetrisState_Base(TetrisStateManager owner, TetrisState state) : base(owner, state)
	{
		this.owner = owner;
	}
}

public class TetrisState_Init : TetrisState_Base
{
	public TetrisState_Init(TetrisStateManager owner) : base(owner, TetrisState.Init)
	{
	}
}

public class TetrisState_Play : TetrisState_Base
{
	private TetrisMap _map => TetrisManager.instance.tetrisMap;
	public TetrisState_Play(TetrisStateManager owner) : base(owner, TetrisState.Play)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		isPause = false;
		_curBlock = null;
		_map.MakeMapTest();
		CreateBlock();
	}

	private bool isPause = false;
	public void PauseTetris()
	{
		isPause = true;
	}
	public void ResumeTetris()
	{
		isPause = false;
	}

	public override void OnUpdate(float deltaTime)
	{
		if (isPause)
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
		if (isPause == false)
		{
			base.OnFixedUpdate(deltaTime);
		}
	}

	private void UpdatePause()
	{
		
	}

	private float _interval = 5;
	private float _time = 0;
	private TetrisBlock _curBlock;
	private int _curLine = 0;
	private void UpdatePlay(float deltaTime)
	{
		_time += deltaTime;
		
		if (_curBlock == null)
		{
			CreateBlock();
			return;
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			MoveBlock(0, -1);
			_time = 0;
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow)) MoveBlock(-1, 0);
		if(Input.GetKeyDown(KeyCode.RightArrow)) MoveBlock(1, 0);
		if(Input.GetKeyDown(KeyCode.UpArrow)) RotateBlock(true);
		if(Input.GetKeyDown(KeyCode.Space)) DropBlock();
		
		if (_time > _interval)
		{
			_time -= _interval;
			
			MoveBlock(0, -1);
		}
	}

	private void CreateBlock()
	{
		if (_curBlock == null)
		{
			_curBlock = new TetrisBlock();
			_curLine = _map.height;
			_curBlock.position = (_map.width / 2, _curLine);

			_time = 0;
			_map.Repaint(_curBlock);
		}
	}
	
	private void NextBlock()
	{
		_curBlock = null;
		CreateBlock();
	}
	
	private void MoveBlock(int deltaX, int deltaY)
	{
		if (CanMoveBlock(deltaX, deltaY))
		{
			_curBlock.Move(deltaX, deltaY);
			_map.Repaint(_curBlock);
		}
		else if(deltaY < 0)
		{
			FixBlock();
		}
	}

	private bool CanMoveBlock(int deltaX, int deltaY)
	{
		var x = _curBlock.position.x + deltaX;
		var y = _curBlock.position.y + deltaY;
		foreach ((int x, int y) block in _curBlock.blocks)
		{
			var blockPosX = x + block.x;
			var blockPosY = y + block.y;
			if (_map[blockPosX, blockPosY] == null || _map[blockPosX, blockPosY].isEmpty == false)
				return false;
		}
		return true;
	}

	private void FixBlock()
	{
		bool isEnd = false;
		foreach ((int x, int y) block in _curBlock.blocks)
		{
			var x = block.x + _curBlock.position.x;
			var y = block.y + _curBlock.position.y;
			if (_map[x, y] == null)
			{
				isEnd = true;
				break;
			}
			else if (_map[x, y].isValid == false)
			{
				isEnd = true;
				break;
			}
		}
		
		if(isEnd)
		{
			SetNextState(TetrisState.Result);
		}
		else
		{
			_map.FixBlock(_curBlock);
			_map.Repaint(null);
			_map.StartCoroutine(ClearLine(NextBlock));	
		}
	}

	private void RotateBlock(bool isClockwise)
	{
		if (CanRotateBlock(isClockwise))
		{
			_curBlock.Rotate(isClockwise);
			_map.Repaint(_curBlock);	
		}
	}

	private bool CanRotateBlock(bool isClockwise)
	{
		foreach ((int x, int y) block in _curBlock.blocks)
		{
			var temp = block;
			if (isClockwise)
			{
				temp = (temp.y + _curBlock.position.x, -temp.x + _curBlock.position.y);
			}
			else
			{
				temp = (-temp.y + _curBlock.position.x, temp.x + _curBlock.position.y);
			}
			if(_map[temp.x, temp.y] == null || _map[temp.x, temp.y].isEmpty == false)
				return false;
		}
		return true;
	}

	private void DropBlock()
	{
		for(int i=0; i<_curLine; i++)
		{
			if (CanMoveBlock(0, -i) == false)
			{
				Debug.Log($"DropBlock {_curLine} -> {i}");
				MoveBlock(0, -i + 1);
				FixBlock();
				break;
			}
		}
	}

	private IEnumerator ClearLine(Action onFinish)
	{
		var flags = new List<bool>();
		var removedLine = new List<int>();
		for(int y = 0; y < _map.height; y++)
		{
			var isClear = true;
			for(int x = 0; x < _map.width; x++)
			{
				if (_map[x, y] == null || _map[x, y].isEmpty)
				{
					isClear = false;
					break;
				}
			}

			if (isClear)
			{
				removedLine.Add(y);
				flags.Add(false);
				int index = flags.Count - 1;
				_map.StartCoroutine(_map.RemoveLine(y, () =>
				{
					flags[index] = true;
				}));
			}
		}
		yield return new WaitUntil(() => flags.TrueForAll(x => x));
		for (int i = 0; i < removedLine.Count; i++)
		{
			var line = removedLine[i] - i;
			for (int y = line; y < _map.height; y++)
			{
				for (int x = 0; x < _map.width; x++)
				{
					if (_map[x, y + 1] == null)
					{
						_map[x, y]?.SetEmpty();
					}
					else
					{
						_map[x, y].isEmpty = _map[x, y + 1].isEmpty;
						_map[x, y].SetColor(_map[x, y + 1].GetColor());
					}
				}
			}
			owner.AddScore(1);
		}
		onFinish?.Invoke();
	}
}


public class TetrisState_Result : TetrisState_Base
{
	public TetrisState_Result(TetrisStateManager owner) : base(owner, TetrisState.Result)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}
}


#endregion
