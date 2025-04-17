using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitStateManager _stateManager;

        [SerializeField] private UnitInfo _info;

        private void Awake()
        {
            _stateManager = new UnitStateManager();
            _stateManager.Init();
        }

        public void Init(UnitInfoData infoData, UnitStatData statData)
        {
            _info = new UnitInfo(infoData, statData);
        }
    }


    [Serializable]
    public class UnitInfo
    {
        public int uid;
        public string name;

        [SerializeField] private UnitStat _stat;

        public UnitInfo(UnitInfoData infoData, UnitStatData statData)
        {
            uid = infoData.uid;
            name = infoData.name;
            _stat = new UnitStat(statData);
        }
    }


    public enum UnitStatType
    {
        MaxHp,
        Attack,
        Defense,
    }

    [Serializable]
    public class UnitStat
    {
        public Dictionary<UnitStatType, int> stat = new Dictionary<UnitStatType, int>();

        public UnitStat(UnitStatData statData)
        {
            stat.Add(UnitStatType.MaxHp, statData.maxHp);
            stat.Add(UnitStatType.Attack, statData.attack);
            stat.Add(UnitStatType.Defense, statData.defense);
        }
    }

    public class UnitResource
    {
        public UnitInfoData infoData { get; private set; }
        public UnitStatData statData { get; private set; }
        public UnitResource(UnitInfoData infoData, UnitStatData statData)
        {
            this.infoData = infoData;
            this.statData = statData;
        }
    }
}