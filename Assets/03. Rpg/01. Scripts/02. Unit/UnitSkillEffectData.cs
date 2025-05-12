using UnityEngine;

namespace Rpg
{
    [CreateAssetMenu(fileName = "UnitSkillEffectData", menuName = "Unit/UnitSkillEffectData")]
    public class UnitSkillEffectData : ScriptableObject
    {
        public int uid;
        public float value;
        public SkillEffectType type;
        public SkillTargetGroup targetGroup;
        public SkillTargetType targetType;
        public int turns;

        public int targetUid;
        public int trueUid;
        public int falseUid;
    }
}
