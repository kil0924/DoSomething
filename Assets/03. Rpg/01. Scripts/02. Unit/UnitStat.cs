using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;

namespace Rpg
{
    public enum UnitStatType
    {
        None,
        
        [PercentPair(MaxHpPercent)] MaxHp,
        MaxHpPercent,

        [PercentPair(AttackPercent)] Attack,
        AttackPercent,

        [PercentPair(DefensePercent)] Defense,
        DefensePercent,
    }

    [Serializable]
    public class UnitStat
    {
        private Dictionary<UnitStatType, int> _stat = new Dictionary<UnitStatType, int>();

        public UnitStat(UnitStatData statData)
        {
            _stat.Add(UnitStatType.MaxHp, statData.maxHp);
            _stat.Add(UnitStatType.Attack, statData.attack);
            _stat.Add(UnitStatType.Defense, statData.defense);

            _stat.Add(UnitStatType.MaxHpPercent, 0);
            _stat.Add(UnitStatType.AttackPercent, 0);
            _stat.Add(UnitStatType.DefensePercent, 0);
        }

        public int GetStat(UnitStatType statType)
        {
            return _stat[statType];
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PercentPairAttribute : Attribute
    {
        public UnitStatType PercentStat { get; }

        public PercentPairAttribute(UnitStatType percentStat)
        {
            PercentStat = percentStat;
        }
    }

    public static class StatLinker
    {
        private static Dictionary<UnitStatType, UnitStatType> _map;

        static StatLinker()
        {
            _map = new();
            var values = Enum.GetValues(typeof(UnitStatType));

            foreach (UnitStatType type in values)
            {
                var field = typeof(UnitStatType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<PercentPairAttribute>();
                if (attr != null)
                {
                    _map[type] = attr.PercentStat;
                }
            }
        }

        public static UnitStatType GetPercentStat(UnitStatType baseStat)
        {
            return _map.GetValueOrDefault(baseStat, UnitStatType.None);
        }
    }
}