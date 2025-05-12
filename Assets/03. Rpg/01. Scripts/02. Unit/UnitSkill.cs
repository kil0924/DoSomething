using System;
using System.Collections.Generic;
using Core;
using NUnit.Framework;
using UnityEngine;

namespace Rpg
{
    public enum UnitSkillType
    {
        None,
        BasicAttack,
        Skill,
    }

    public enum SkillTargetGroup
    {
        None,
        Ally,
        Enemy,
        All,
    }

    public enum SkillTargetType
    {
        None,
        Self,
        Single,
        All,
    }
    [Serializable]
    public class UnitSkill
    {
        public int uid;
        public string aniName;
        public int cooldownTurns;
        public UnitSkillType skillType;
        public List<UnitSkillEffectData> skillEffectDataList;

        public int remainTurns;
        public UnitSkill(UnitSkillData data, List<UnitSkillEffectData> skillEffectDataList)
        {
            uid = data.uid;
            aniName = data.aniName;
            cooldownTurns = data.cooldownTurns;
            skillType = data.type;
            this.skillEffectDataList = skillEffectDataList;
            
            remainTurns = 0;
        }

        public void Invoke(Unit caster)
        {
            remainTurns = cooldownTurns;
            ActiveSkillEffects(caster);
        }

        public void ActiveSkillEffects(Unit caster)
        {
            foreach (var data in skillEffectDataList)
            {
                var targets = GetTargetList(caster, data);
                foreach (var target in targets)
                {
                    ActiveSkillEffect(caster, target, data);       
                }
            }
        }

        public void ActiveSkillEffect(Unit caster, Unit target, UnitSkillEffectData data)
        {
            if (data == null)
                return;
            
            switch (data.type)
            {
                case SkillEffectType.Damage:
                    caster.Attack(target, data.value);
                    break;
                case SkillEffectType.Heal:
                    caster.Heal(target, data.value);
                    break;
                case SkillEffectType.Count:
                    int count = target.CountSkillEffect(data.targetUid);
                    if (count == Mathf.RoundToInt(data.value))
                    {
                        ActiveSkillEffectByUid(caster, target, data.trueUid);
                    }
                    else
                    {
                        ActiveSkillEffectByUid(caster, target, data.falseUid);
                    }
                    break;
                default:
                    var effect = new UnitSkillEffect(caster, target, data);
                    target.AddSkillEffect(effect);
                    break;
            }
            Debug.Log($"Active Skill Effect. Caster : {caster.name}, Target : {target.name}, Effect : {data.uid}");
        }
        public void ActiveSkillEffectByUid(Unit caster, Unit target, int uid)
        {
            var data = RpgResourceManager.instance.GetUnitSkillEffectData(uid);
            if (data == null)
                return;
            ActiveSkillEffect(caster, target, data);
        }

        public List<Unit> GetTargetList(Unit caster, UnitSkillEffectData data)
        {
            List<Unit> group = new List<Unit>();
            switch (data.targetGroup)
            {
                case SkillTargetGroup.Ally:
                    group.AddRange(caster.team.aliveUnits);
                    break;
                case SkillTargetGroup.Enemy:
                    group.AddRange(caster.team.enemyTeam.aliveUnits);
                    break;
                case SkillTargetGroup.All:
                    group.AddRange(caster.team.aliveUnits);
                    group.AddRange(caster.team.enemyTeam.aliveUnits);
                    break;
                default:
                    return null;
            }

            List<Unit> targets = new List<Unit>();
            switch (data.targetType)
            {
                case SkillTargetType.Self:
                    targets.Add(caster);
                    break;
                case SkillTargetType.Single:
                    targets.Add(group.GetRandom(RpgManager.instance.random));
                    break;
                case SkillTargetType.All:
                    targets = group;
                    break;
                default:
                    return null;
            }
            return targets;
        }
    }

    [Serializable]
    public class UnitSkillManager
    {
        public Dictionary<UnitSkillType, UnitSkill> skills = new Dictionary<UnitSkillType, UnitSkill>();
        
        [SerializeField]
        public List<UnitSkill> skillDebug = new List<UnitSkill>();
        public UnitSkillManager(Dictionary<UnitSkillData, List<UnitSkillEffectData>> data)
        {
            foreach (var kv in data)
            {
                var skillData = kv.Key;
                var skillEffectDataList = kv.Value;
                skills.Add(skillData.type, new UnitSkill(skillData, skillEffectDataList));
                
                skillDebug.Add(skills[skillData.type]);
            }
        }
        
        public UnitSkill SelectSkill()
        {
            if (skills[UnitSkillType.Skill].remainTurns <= 0)
                return skills[UnitSkillType.Skill];
            else
                return skills[UnitSkillType.BasicAttack];
        }

        public void ProcessTurn()
        {
            foreach (var skill in skills.Values)
            {
                if (skill.remainTurns <= 0)
                    continue;
                skill.remainTurns--;
            }
        }
    }
}
