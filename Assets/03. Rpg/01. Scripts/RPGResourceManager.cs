using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace Rpg
{
    public class RpgResourceManager : ResourceManagerBase<RpgResourceManager>
    {
        private Dictionary<int, UnitResource> _unitResources = new Dictionary<int, UnitResource>();
        
        public Unit GetUnit(int uid)
        {
            UnitResource resource;
            
            if (_unitResources.ContainsKey(uid) == false)
            {
                var infoData = GetUnitInfoData(uid);
                if (infoData == null)
                {
                    return null;
                }
                var statUid = infoData.statUid;                
                var statData = GetUnitStatData(statUid);
                if (statData == null)
                {
                    return null;
                }
                var spineData = GetSkeletonDataAsset(infoData.prefab);
                if (spineData == null)
                {
                    return null;
                }
                
                var skillDataList = GetUnitSkillDataList(infoData.skillUidList);
                    
                resource = new UnitResource();
                resource.SetInfoData(infoData)
                    .SetStatData(statData)
                    .SetSpineData(spineData)
                    .SetSkillDataList(skillDataList);
                _unitResources.Add(uid, resource);
            }
            else
            {
                resource = _unitResources[uid];
            }
            
            var prefab = GetPrefab($"Rpg/Unit");
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found. prefab:Rpg/Unit");
                return null;
            }
            
            var go = Instantiate(prefab);
            
            go.name = $"{resource.infoData.name} - {uid}";
            
            var unit = go.GetComponent<Unit>();
            if (unit == null)
            {
                unit = go.AddComponent<Unit>();
            }
            
            unit.Init(resource);
            unit.transform.SetParent(transform);
            
            return unit;
        }

        public UnitInfoData GetUnitInfoData(int uid)
        {
            var data = Resources.Load<UnitInfoData>($"Rpg/UnitData/Info_{uid}");
            if (data == null)
            {
                Debug.LogError($"UnitInfoData not found. uid:{uid}");
            }
            else
            {
                Debug.Log($"UnitInfoData found. uid:{uid}");
            }
            return data;
        }
        public UnitStatData GetUnitStatData(int uid)
        {
            var data = Resources.Load<UnitStatData>($"Rpg/UnitData/Stat_{uid}");
            if (data == null)
            {
                Debug.LogError($"UnitStatData not found. uid:{uid}");
            }
            else
            {
                Debug.Log($"UnitStatData found. uid:{uid}");
            }
            return data;
        }
        
        public SkeletonDataAsset GetSkeletonDataAsset(string path)
        {
            var data = Resources.Load<SkeletonDataAsset>($"Rpg/{path}");
            if (data == null)
            {
                Debug.LogError($"SkeletonDataAsset not found. path:{path}");
                return null;
            }

            return data;
        }

        public Dictionary<UnitSkillData, List<UnitSkillEffectData>> GetUnitSkillDataList(List<int> uids)
        {
            var dataDict = new Dictionary<UnitSkillData, List<UnitSkillEffectData>>();
            foreach (var uid in uids)
            {
                var data = Resources.Load<UnitSkillData>($"Rpg/UnitData/Skill_{uid}");
                if (data == null)
                {
                    Debug.LogError($"UnitSkillData not found. uid:{uid}");
                    continue;
                }
                var skillEffectDataList = GetUnitSkillEffectDataList(data.skillEffectList);
                dataDict.Add(data, skillEffectDataList);
            }
            return dataDict;
        }

        public List<UnitSkillEffectData> GetUnitSkillEffectDataList(List<int> uids)
        {
            var dataList = new List<UnitSkillEffectData>();
            foreach (var uid in uids)
            {
                var data = Resources.Load<UnitSkillEffectData>($"Rpg/UnitData/SkillEffect_{uid}");
                if (data == null)
                {
                    Debug.LogError($"UnitSkillEffectData not found. uid:{uid}");
                    continue;
                }
                dataList.Add(data);
            }
            return dataList;
        }

        public UnitUI GetUnitUI()
        {
            var prefab = GetPrefab("Rpg/UI/UnitUI");
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found. prefab:Rpg/UI/UnitUI");
                return null;
            }
            var go = Instantiate(prefab);
            return go.GetComponent<UnitUI>();
        }
    }
    
}
