using System;
using Core.Singleton;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameStateManager stateManager;
    
    protected override void Awake()
    {
        base.Awake();
        stateManager = new GameStateManager();
        stateManager.Init();
    }

    private void Start()
    {
        var go = new GameObject();
        go.AddComponent<Unit>();
        go.transform.SetParent(transform);
    }

    private void Update()
    {
        stateManager.OnUpdate(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        stateManager.OnFixedUpdate(Time.fixedDeltaTime);
    }
    
    public Unit CreateUnit(int uid)
    {
        return null;
        // var go = ResourceManagerBase.instance.GetResource("Unit");
        // var unit = go.GetComponent<Unit>();
        // if (unit == null)
        // {
        //     go.AddComponent<Unit>();
        // }
        // var infoData = ResourceManagerBase.instance.GetUnitInfoData(uid);
        // var statData = ResourceManagerBase.instance.GetUnitStatData(infoData.statUid);
        //
        // unit.Init(infoData, statData);
        // unit.transform.SetParent(transform);
        //
        // return unit;
    }
}
