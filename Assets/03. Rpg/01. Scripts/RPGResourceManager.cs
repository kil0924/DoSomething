using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace RPG
{
    public class RPGResourceManager : ResourceManagerBase<RPGResourceManager>
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
                    
                resource = new UnitResource(infoData, statData, spineData);
                _unitResources.Add(uid, resource);
            }
            else
            {
                resource = _unitResources[uid];
            }
            
            var prefab = GetPrefab($"RPG/Unit");
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found. prefab:RPG/Unit");
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
            var data = Resources.Load<UnitInfoData>($"RPG/UnitData/Info_{uid}");
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
            var data = Resources.Load<UnitStatData>($"RPG/UnitData/Stat_{uid}");
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
            var data = Resources.Load<SkeletonDataAsset>($"RPG/{path}");
            if (data == null)
            {
                Debug.LogError($"SkeletonDataAsset not found. path:{path}");
                return null;
            }

            return data;
        }
    }
    
}
