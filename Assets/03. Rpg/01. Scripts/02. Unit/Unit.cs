using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace RPG
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitStateManager _stateManager;
        public UnitState curState => _stateManager.curState.State;

        [SerializeField] private UnitInfo _info;
        
        [SerializeField] private SkeletonAnimation _spine;

        private void Awake()
        {
            _stateManager = new UnitStateManager(this);
            _stateManager.Init();
        }

        public void Init(UnitResource resource)
        {
            _info = new UnitInfo(resource.infoData, resource.statData);
            SetSpine(resource.spineData);
            SetSide(false);
        }
        
        public void SetSpine(SkeletonDataAsset asset)
        {
            _spine.skeletonDataAsset = asset;
            _spine.initialSkinName = "Normal";
            _spine.Initialize(true);
        }

        public void SetSide(bool isLeft)
        {
            _spine.initialFlipX = isLeft;
            _spine.Initialize(true);
        }
        
        public void PlayAnimation(string aniName, bool loop = true)
        {
            _spine.AnimationState.SetAnimation(0, aniName, loop);
        }

        public bool IsCompleteAnimation()
        {
            return _spine.AnimationState.GetCurrent(0).IsComplete;
        }

        public void FixedUpdate()
        {
            _stateManager.OnFixedUpdate(Time.deltaTime);
        }

        public Unit target { get; private set; }
        public void DoAttack(Unit target)
        {
            if (_stateManager.curState.State != UnitState.Idle)
                return;
            
            this.target = target;
            _stateManager.curState.SetNextState(UnitState.Move);
            originPos = transform.localPosition;
        }
        public Vector3 originPos { get; private set; }
    }


    [Serializable]
    public class UnitInfo
    {
        public int uid;
        public string name;

        [SerializeField] private UnitStat _stat;

        public UnitInfo(UnitInfoData infoData, UnitStatData statData)
        {
            uid = infoData.uid;
            name = infoData.name;
            _stat = new UnitStat(statData);
        }
    }


    public enum UnitStatType
    {
        MaxHp,
        Attack,
        Defense,
    }

    [Serializable]
    public class UnitStat
    {
        public Dictionary<UnitStatType, int> stat = new Dictionary<UnitStatType, int>();

        public UnitStat(UnitStatData statData)
        {
            stat.Add(UnitStatType.MaxHp, statData.maxHp);
            stat.Add(UnitStatType.Attack, statData.attack);
            stat.Add(UnitStatType.Defense, statData.defense);
        }
    }

    public class UnitResource
    {
        public UnitInfoData infoData { get; private set; }
        public UnitStatData statData { get; private set; }
        public SkeletonDataAsset spineData { get; private set; }
        public UnitResource(UnitInfoData infoData, UnitStatData statData, SkeletonDataAsset spineData)
        {
            this.infoData = infoData;
            this.statData = statData;
            this.spineData = spineData;
        }
    }
}