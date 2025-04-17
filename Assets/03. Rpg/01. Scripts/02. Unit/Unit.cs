using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private UnitStateManager _stateManager;
    
    [SerializeField]
    private UnitInfo _info;

    private void Awake()
    {
        _stateManager = new UnitStateManager();
        _stateManager.Init();
    }
    
    public void Init(UnitInfoData infoData, UnitStatData statData)
    {
        _info = new UnitInfo(infoData, statData);
    }
}


[Serializable]
public class UnitInfo
{
    public int uid;
    public string name;
    
    [SerializeField]
    private UnitStat _stat;

    public UnitInfo(UnitInfoData infoData, UnitStatData statData)
    {
        uid = infoData.uid;
        name = infoData.name;
        _stat = new UnitStat(statData);
    }
}


[Serializable]
public class UnitStat
{
    public int uid;
    public int maxHp;
    public int attack;

    public UnitStat(UnitStatData statData)
    {
        uid = statData.uid;
        maxHp = statData.maxHp;
        attack = statData.attack;
    }
}