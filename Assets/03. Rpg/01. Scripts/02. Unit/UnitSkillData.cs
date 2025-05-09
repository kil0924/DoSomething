using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rpg
{
    [CreateAssetMenu(fileName = "UnitSkillData", menuName = "Unit/UnitSkillData")]
    public class UnitSkillData : ScriptableObject
    {
        public int uid;
        public string aniName;
        public int cooldownTurns;
        public UnitSkillType type;

        public List<int> skillEffectList;
    }    
}
 