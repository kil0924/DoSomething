using System;
using System.Collections.Generic;
using Core;
using Core.Singleton;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RPG
{
    public class RPGManager : Singleton<RPGManager>
    {
        public RPG_FSM fsm;
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

        protected override void Awake()
        {
            base.Awake();
            fsm = new RPG_FSM(this);
            fsm.Init();
        }

        private void Start()
        {
        }

        private void Update()
        {
            fsm.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            fsm.OnFixedUpdate(Time.fixedDeltaTime);
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
                var unit = RPGResourceManager.instance.GetUnit(Random.Range(1,11));
                if (unit == null)
                    continue;
                units.Add(unit);
                
                var isFront = i % 2 != 0;
                var x = isFront ? 5 : 2;
                x = isLeft ? -x : x;
                var z = (i - 2) * 2;
                
                unit.transform.SetParent(RPGManager.instance.transform);
                unit.transform.localPosition = new Vector3(x, 0, z);
                
                unit.SetTeam(this);
                unit.SetSide(isLeft);
                unit.PrepareBattle();
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
    }
}