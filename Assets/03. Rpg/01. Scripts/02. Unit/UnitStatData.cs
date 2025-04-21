using UnityEngine;

namespace Rpg
{
    [CreateAssetMenu(fileName = "UnitStatData", menuName = "Unit/UnitStatData")]
    public class UnitStatData : ScriptableObject
    {
        public int uid;
        public int maxHp;
        public int attack;
        public int defense;
    }
}