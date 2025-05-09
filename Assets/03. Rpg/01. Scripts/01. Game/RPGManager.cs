using System;
using System.Collections.Generic;
using Core;
using Core.Random;
using Core.Singleton;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Rpg
{
    public class RpgManager : Singleton<RpgManager>
    {
        [SerializeField]
        private Rpg_FSM _fsm;
        [SerializeField]
        private TeamManager _leftTeam;
        [SerializeField]
        private TeamManager _rightTeam;
        public TeamManager leftTeam
        {
            get => _leftTeam;
            private set => _leftTeam = value;
        }

        public TeamManager rightTeam
        {
            get => _rightTeam;
            private set => _rightTeam = value;
        }
        
        [SerializeField]
        private RpgUI _ui;

        public CustomRandom random;
        
        protected override void Awake()
        {
            base.Awake();
            
            random = new CustomRandom(0);
            
            _fsm = new Rpg_FSM(this);
            _fsm.Init();
            
            _fsm.SetOnChangeState(state =>
            {
                // _ui.SetCurStateText(state.ToString());
            });
            _lastFrameTime = 0;
            _updateTime = 0;
            _fixedUpdateTime = 0;
        }

        private void Update()
        {
            CalcLastFrameDelta(Time.deltaTime, false);
            
            _fsm.OnUpdate((float)_lastFrameDeltaTime);
            _leftTeam.OnUpdate((float)_lastFrameDeltaTime);
            _rightTeam.OnUpdate((float)_lastFrameDeltaTime);
        }

        private void FixedUpdate()
        {
            CalcLastFrameDelta(Time.fixedDeltaTime, true);
            
            _fsm.OnFixedUpdate((float)_lastFrameDeltaTime);
            _leftTeam.OnFixedUpdate((float)_lastFrameDeltaTime);
            _rightTeam.OnFixedUpdate((float)_lastFrameDeltaTime);
        }

        public void BuildTeam()
        {
            leftTeam = new TeamManager(true);
            rightTeam = new TeamManager(false);
            
            leftTeam.SetEnemey(rightTeam);
            rightTeam.SetEnemey(leftTeam);
        }

        public void PrepareBattle()
        {
            leftTeam.PrepareBattle();
            rightTeam.PrepareBattle();
        }

        public bool CheckGameOver()
        {
            return leftTeam.aliveUnits.Count == 0 || rightTeam.aliveUnits.Count == 0;
        }
        
        
        private double _updateTime = 0;
        private double _fixedUpdateTime = 0;
        private double _lastFrameTime = 0;
        private double _lastFrameDeltaTime = 0;

        private void CalcLastFrameDelta(float deltaTime, bool isFixedUpdate)
        {
            if (isFixedUpdate)
            {
                _fixedUpdateTime += deltaTime;
                _lastFrameDeltaTime = _fixedUpdateTime - _lastFrameTime;
                if (_lastFrameDeltaTime < 0)
                {
                    _fixedUpdateTime = _lastFrameTime;
                    _lastFrameDeltaTime = 0;
                }
                _lastFrameTime = _fixedUpdateTime;
            }
            else
            {
                _updateTime += deltaTime;
                _lastFrameDeltaTime = _updateTime - _lastFrameTime;
                if (_lastFrameDeltaTime < 0)
                {
                    _updateTime = _lastFrameTime;
                    _lastFrameDeltaTime = 0;
                }
                _lastFrameTime = _updateTime;
            }
        }
    }

    [Serializable]
    public class TeamManager
    {
        public bool isLeft;
        public int score;
        public List<Unit> units;
        public List<Unit> aliveUnits;
        public List<Unit> deadUnits;

        public TeamManager enemyTeam;
        public TeamManager(bool isLeft)
        {
            this.isLeft = isLeft;
            score = 0;
            
            units = new List<Unit>();
            aliveUnits = new List<Unit>();
            deadUnits = new List<Unit>();
            
            for (int i = 1; i <= 1; i++)
            {
                var unit = RpgResourceManager.instance.GetUnit(Random.Range(1,11));
                if (unit == null)
                    continue;
                units.Add(unit);
                
                var isFront = i % 2 != 0;
                var x = isFront ? 5 : 2;
                x = isLeft ? -x : x;
                var z = (i - 2) * 2;
                
                unit.transform.SetParent(RpgManager.instance.transform);
                unit.transform.localPosition = new Vector3(x, 0, z);
                
                unit.SetTeam(this);
                unit.SetSide(isLeft);
                unit.PrepareBattle();

                var ui = RpgResourceManager.instance.GetUnitUI();
                ui.transform.Reset(unit.transform);
                
                ui.BindUnit(unit);
            }
        }

        public void SetEnemey(TeamManager enemyTeam)
        {
            this.enemyTeam = enemyTeam;
        }

        public void PrepareBattle()
        {
            aliveUnits.Clear();
            deadUnits.Clear();
            
            foreach (var unit in units)
            {
                unit.SetSide(isLeft);
                unit.PrepareBattle();
            }
            
            aliveUnits.AddRange(units);
        }

        public Unit GetAliveRandomUnit()
        {
            if (aliveUnits.Count == 0)
                return null;
            return aliveUnits.GetRandom();
        }
        
        public void OnUnitDead(Unit unit)
        {
            aliveUnits.Remove(unit);
            deadUnits.Add(unit);
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var unit in units)
            {
                unit.OnUpdate(deltaTime);
            }
        }

        public void OnFixedUpdate(float deltaTime)
        {
            foreach (var unit in units)
            {
                unit.OnFixedUpdate(deltaTime);
            }
        }
    }
}