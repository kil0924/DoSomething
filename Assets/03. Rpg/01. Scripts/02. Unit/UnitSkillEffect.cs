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

        private Dictionary<int, List<UnitSkillEffect>> _skillEffectByUid = new Dictionary<int, List<UnitSkillEffect>>();
        private Dictionary<int, SkillEffectType> _uidToType = new Dictionary<int, SkillEffectType>();
        private Dictionary<SkillEffectType, List<int>> _typeToUid = new Dictionary<SkillEffectType, List<int>>();
        
        [SerializeField]
        public List<UnitSkillEffect> skillEffectDebug = new List<UnitSkillEffect>();
        public void AddSkillEffect(UnitSkillEffect effect)
        {
            if (_skillEffectByUid.ContainsKey(effect.uid) == false)
            {
                _skillEffectByUid.Add(effect.uid, new List<UnitSkillEffect>());
            }
            _skillEffectByUid[effect.uid].Add(effect);
            _uidToType[effect.uid] = effect.type;
            if (_typeToUid.ContainsKey(effect.type) == false)
            {
                _typeToUid.Add(effect.type, new List<int>());
            }
            _typeToUid[effect.type].Add(effect.uid);
            
            skillEffectDebug.Add(effect);
            
            OnAddSkillEffect(effect);            
        }
        public void RemoveSkillEffect(UnitSkillEffect effect)
        {
            _skillEffectByUid[effect.uid].Remove(effect);
            
            skillEffectDebug.Remove(effect);

            OnRemoveSkillEffect(effect);
        }
        
        
        private void OnAddSkillEffect(UnitSkillEffect effect)
        {
            if (SkillEffectLinker.GetStatType(effect.type) != UnitStatType.None)
            {
                CalcStatEffect(effect.type);    
            }
            else if (effect.type == SkillEffectType.Stun)
            {
                _onStun?.Invoke();
            }
        }
        
        private void OnRemoveSkillEffect(UnitSkillEffect effect)
        {
            if (SkillEffectLinker.GetStatType(effect.type) != UnitStatType.None)
            {
                CalcStatEffect(effect.type);
            }
            else if (effect.type == SkillEffectType.Stun)
            {
                _onStun?.Invoke();
            }
        }

        private void CalcStatEffect(SkillEffectType type)
        {
            if (SkillEffectLinker.GetStatType(type) == UnitStatType.None)
                return;

            if (_typeToUid.TryGetValue(type, out var uids) == false)
                return;
            
            var value = 0f;
            foreach (var uid in uids)
            {
                if (_skillEffectByUid.TryGetValue(uid, out var list) == false)
                    continue;
                foreach (var effect in list)
                {
                    value += effect.value;
                }
            }
            
            _statEffect[SkillEffectLinker.GetStatType(type)] = (int)value;
        }

        public int GetStatEffectValue(UnitStatType statType)
        {
            return _statEffect.GetValueOrDefault(statType, 0);
        }

        public void Clear()
        {
            _skillEffectByUid.Clear();
            _statEffect.Clear();
            
            skillEffectDebug.Clear();
        }

        public void ProcessTurn()
        {
            foreach (var list in _skillEffectByUid.Values)
            {
                list.ForEach(x => x.ExecuteTurn());
                list.RemoveAll(x => x.isAlive == false);
            }
            skillEffectDebug.RemoveAll(x => x.isAlive == false);
        }

        public int GetCount(int uid)
        {
            int count = _skillEffectByUid.GetValueOrDefault(uid, null)?.Count ?? 0;
            return count;
        }
        
        public int GetCount(SkillEffectType type)
        {
            if (_typeToUid.TryGetValue(type, out var uids) == false)
                return 0;
            
            var count = 0;
            foreach (var uid in uids)
            {
                if (_skillEffectByUid.TryGetValue(uid, out var list) == false)
                    continue;
                
                count += list.Count;
            }
            return count;
        }

        private Action _onStun;
        public void SetOnStun(Action onStun)
        {
            _onStun = onStun;
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
