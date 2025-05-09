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
    }

    public enum SkillTargetType
    {
        None,
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
            foreach (var data in skillEffectDataList)
            {
                var targets = GetTargetList(caster, data);
                foreach (var target in targets)
                {
                    switch (data.type)
                    {
                        case SkillEffectType.Damage:
                            caster.Attack(target, data.value);
                            break;
                        case SkillEffectType.Heal:
                            caster.Heal(target, data.value);
                            break;
                        default:
                            var effect = new UnitSkillEffect(caster, target, data.type, data.value, data.turns);
                            target.AddSkillEffect(effect);
                            break;
                    }
                }
            }
        }

        public List<Unit> GetTargetList(Unit caster, UnitSkillEffectData data)
        {
            List<Unit> group = null;
            switch (data.targetGroup)
            {
                case SkillTargetGroup.Ally:
                    group = caster.team.aliveUnits;
                    break;
                case SkillTargetGroup.Enemy:
                    group = caster.team.enemyTeam.aliveUnits;
                    break;
                default:
                    return null;
            }

            List<Unit> targets = null;
            switch (data.targetType)
            {
                case SkillTargetType.Single:
                    targets = new List<Unit>();
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
            if (skills[UnitSkillType.Skill].remainTurns == 0)
                return skills[UnitSkillType.Skill];
            else
                return skills[UnitSkillType.BasicAttack];
        }

        public void ProcessTurn()
        {
            foreach (var skill in skills.Values)
            {
                skill.remainTurns--;
            }
        }
    }
}
