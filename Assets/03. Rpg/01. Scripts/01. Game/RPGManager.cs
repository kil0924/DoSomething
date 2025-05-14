using System;
using System.Collections.Generic;
using Core;
using Core.Random;
using Core.Singleton;
using Core.Time;
using UnityEngine;
using UnityEngine.Serialization;

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
        public CustomTime time;
        
        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            base.Awake();
            
            random = new CustomRandom(0);
            time = new CustomTime();
            
            _fsm = new Rpg_FSM(this);
            _fsm.Init();
            
            _fsm.SetOnChangeState(state =>
            {
                // _ui.SetCurStateText(state.ToString());
            });
        }

        private void Update()
        {
            time.CalcLastFrameDelta(Time.deltaTime, false);

            var deltaTime = (float)time.deltaTime;
            _fsm.OnUpdate(deltaTime);
            _leftTeam.OnUpdate(deltaTime);
            _rightTeam.OnUpdate(deltaTime);
        }

        private void FixedUpdate()
        {
            time.CalcLastFrameDelta(Time.fixedDeltaTime, true);
            
            var deltaTime = (float)time.deltaTime;
            _fsm.OnFixedUpdate(deltaTime);
            _leftTeam.OnFixedUpdate(deltaTime);
            _rightTeam.OnFixedUpdate(deltaTime);
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
            
            for (int i = 1; i <= 3; i++)
            {
                var unit = RpgResourceManager.instance.GetUnit(RpgManager.instance.random.Range(1,11));
                if (unit == null)
                    continue;
                units.Add(unit);
                
                var isFront = i % 2 != 0;
                var x = isFront ? 3 : 5;
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
            return aliveUnits.GetRandom(RpgManager.instance.random);
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