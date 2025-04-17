using System.Collections.Generic;
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
                
                resource = new UnitResource(infoData, statData);
                _unitResources.Add(uid, resource);
            }
            else
            {
                resource = _unitResources[uid];
            }
            
            var prefab = GetPrefab($"RPG/{resource.infoData.prefab}");
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found. uid:{uid} prefab:{resource.infoData.prefab}");
                return null;
            }
            
            var go = Instantiate(prefab);
            
            go.name = $"{resource.infoData.name} - {uid}";
            
            var unit = go.GetComponent<Unit>();
            if (unit == null)
            {
                unit = prefab.AddComponent<Unit>();
            }
            
            unit.Init(resource.infoData, resource.statData);
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
            return data;
        }
        public UnitStatData GetUnitStatData(int uid)
        {
            var data = Resources.Load<UnitStatData>($"RPG/UnitData/Stat_{uid}");
            if (data == null)
            {
                Debug.LogError($"UnitStatData not found. uid:{uid}");
            }
            return data;
        }
    }
    
}
