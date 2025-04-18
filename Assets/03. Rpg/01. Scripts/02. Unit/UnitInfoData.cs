using UnityEngine;

namespace RPG
{
    [CreateAssetMenu(fileName = "UnitInfoData", menuName = "Unit/UnitInfoData")]
    public class UnitInfoData : ScriptableObject
    {
        public int uid;
        public string name;
        public int statUid;
        public string prefab;
    }
}