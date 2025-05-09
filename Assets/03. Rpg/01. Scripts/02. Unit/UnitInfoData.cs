using System.Collections.Generic;
using UnityEngine;

namespace Rpg
{
    [CreateAssetMenu(fileName = "UnitInfoData", menuName = "Unit/UnitInfoData")]
    public class UnitInfoData : ScriptableObject
    {
        public int uid;
        public string name;
        public string prefab;
        
        public int statUid;
        public List<int> skillUidList; 
    }
}