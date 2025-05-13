using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Rpg
{
    
    public enum SkillEffectType
    {
        None,
        
        Damage, // 즉시 공격
        Heal,   // 즉시 힐
        
        DoT,    // 지속 공격
        HoT,    // 지속 힐
        
        [SkillEffect(UnitStatType.MaxHp)]
        MaxHp,
        [SkillEffect(UnitStatType.MaxHpPercent)]
        MaxHpPercent,
        [SkillEffect(UnitStatType.Attack)] 
        Attack,
        [SkillEffect(UnitStatType.AttackPercent)]
        AttackPercent,
        [SkillEffect(UnitStatType.Defense)]
        Defense,
        [SkillEffect(UnitStatType.DefensePercent)]
        DefensePercent,
        
        Stun,
        
        Flag,
        Count,
    }
    [Serializable]
    public class UnitSkillEffectManager
    {
        private Dictionary<UnitStatType, int> _statEffect = new Dictionary<UnitStatType, int>();

        private Dictionary<SkillEffectType, List<UnitSkillEffect>> _skillEffect = new Dictionary<SkillEffectType, List<UnitSkillEffect>>();
        private Dictionary<int, List<UnitSkillEffect>> _skillEffectByUid = new Dictionary<int, List<UnitSkillEffect>>();
        
        [SerializeField]
        public List<UnitSkillEffect> skillEffectDebug = new List<UnitSkillEffect>();
        public void AddSkillEffect(UnitSkillEffect effect)
        {
            if (_skillEffect.ContainsKey(effect.type) == false)
            {
                _skillEffect.Add(effect.type, new List<UnitSkillEffect>());
            }
            _skillEffect[effect.type].Add(effect);

            if (_skillEffectByUid.ContainsKey(effect.uid) == false)
            {
                _skillEffectByUid.Add(effect.uid, new List<UnitSkillEffect>());
            }
            _skillEffectByUid[effect.uid].Add(effect);
            
            CalcStatEffect(effect.type);
            
            skillEffectDebug.Add(effect);
        }

        public void RemoveSkillEffect(UnitSkillEffect effect)
        {
            _skillEffect[effect.type].Remove(effect);
            _skillEffectByUid[effect.uid].Remove(effect);
            
            CalcStatEffect(effect.type);
            
            skillEffectDebug.Remove(effect);
        }

        private void CalcStatEffect(SkillEffectType type)
        {
            if (SkillEffectLinker.GetStatType(type) == UnitStatType.None)
                return;
            
            int count = _skillEffect[type].Count;
            var value = 0f;
            for (int i = 0; i < count; i++)
            {
                value += _skillEffect[type][i].value;
            }
            _statEffect[SkillEffectLinker.GetStatType(type)] = (int)value;
        }

        public int GetStatEffectValue(UnitStatType statType)
        {
            return _statEffect.GetValueOrDefault(statType, 0);
        }

        public void Clear()
        {
            _skillEffect.Clear();
            _statEffect.Clear();
            
            skillEffectDebug.Clear();
        }

        public void ProcessTurn()
        {
            foreach (var list in _skillEffect.Values)
            {
                list.ForEach(x => x.ExecuteTurn());
                list.RemoveAll(x => x.isAlive == false);
            }
        }

        public int GetCount(int uid)
        {
            int count = _skillEffectByUid.GetValueOrDefault(uid, null)?.Count ?? 0;
            return count;
        }
        
        public int GetCount(SkillEffectType type)
        {
            int count = _skillEffect.GetValueOrDefault(type, null)?.Count ?? 0;
            return count;
        }
    }
    [Serializable]
    public class UnitSkillEffect
    {
        public Unit caster;
        public Unit owner;
        
        public int uid;
        public float value;
        public int remainTurns;
        public SkillEffectType type;
        
        public bool isAlive = true;

        public UnitSkillEffect(Unit caster, Unit owner, UnitSkillEffectData data)
        {
            this.caster = caster;
            this.owner = owner;
            uid = data.uid;
            type = data.type;
            value = data.value;
            remainTurns = data.turns;
            isAlive = true;
        }

        public void ExecuteTurn()
        {
            if (isAlive == false)
                return;
            
            remainTurns--;
            if (remainTurns <= 0)
            {
                isAlive = false;
            }
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class SkillEffectAttribute : Attribute
    {
        public UnitStatType statType { get; }

        public SkillEffectAttribute(UnitStatType statType = UnitStatType.None)
        {
            this.statType = statType;
        }
    }

    public static class SkillEffectLinker
    {
        private static Dictionary<SkillEffectType, UnitStatType> _statEffectMap;

        static SkillEffectLinker()
        {
            _statEffectMap = new();
            var values = Enum.GetValues(typeof(SkillEffectType));

            foreach (SkillEffectType type in values)
            {
                var field = typeof(SkillEffectType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<SkillEffectAttribute>();
                if (attr != null)
                {
                    _statEffectMap[type] = attr.statType;
                }
            }
        }

        public static UnitStatType GetStatType(SkillEffectType baseStat)
        {
            return _statEffectMap.GetValueOrDefault(baseStat, UnitStatType.None);
        }
    }
}
