using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using AnimationState = Spine.AnimationState;

namespace RPG
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitStateManager _stateManager;
        [SerializeField] private UnitInfo _info;
        [SerializeField] private SkeletonAnimation _spine;

        private int _curHp;
        public TeamManager team { get; private set; }
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
            _spine.transform.localScale = new Vector3(isLeft ? 1 : -1, 1, 1);
        }

        public void SetTeam(TeamManager team)
        {
            this.team = team;
        }
        
        private List<TrackEntry> _animationTracks = new List<TrackEntry>();
        public void PlayAnimation(string aniName, bool loop = true, AnimationState.TrackEntryDelegate onComplete = null)
        {
            _animationTracks.Clear();
            
            var trackEntry = _spine.AnimationState.SetAnimation(0, aniName, loop);
            trackEntry.Complete += onComplete;
            
            _animationTracks.Add(trackEntry);
        }

        public void AddAnimation(string aniName, bool loop = true, AnimationState.TrackEntryDelegate onComplete = null)
        {
            _animationTracks[^1].Complete -= onComplete;
            
            var trackEntry = _spine.AnimationState.AddAnimation(0, aniName, loop, 0);
            trackEntry.Complete += onComplete;
            
            _animationTracks.Add(trackEntry);
        }

        public void FixedUpdate()
        {
            _stateManager.OnFixedUpdate(Time.deltaTime);
        }

        public void MoveTo(Vector3 destPos, float speed, float deltaTime, Action onArrive)
        {
            var curPos = transform.localPosition;
            if (Utils.Approximately(curPos, destPos, speed, deltaTime))
            {
                transform.localPosition = destPos;
                onArrive?.Invoke();
            }
            else
            {
                var dir = destPos - curPos;
                transform.localPosition += deltaTime * speed * dir.normalized;
            }
        }
        private Action _onFinishSkill;
        public void OnFinishSkill()
        {
            _onFinishSkill?.Invoke();
            _onFinishSkill = null;
        }
        public bool UseSkill(Action onFinish)
        {
            if (_stateManager.curState.State != UnitState.Idle)
            {
                return false;
            }

            _onFinishSkill = onFinish;
            _stateManager.curState.SetNextState(UnitState.Move);
            
            return true;
        }
        public Vector3 originPos { get; private set; }

        public void Attack(Unit target, Action onKill = null)
        {
            if (target == null)
                return;
            
            var damage = _info.stat.stat[UnitStatType.Attack] - target._info.stat.stat[UnitStatType.Defense];
            if (target.TakeDamage(damage))
            {
                onKill?.Invoke();
            }
            
            Debug.Log($"{_info.name} attack {target.name} remain hp : {target._curHp}");
        }

        public bool TakeDamage(int damage)
        {
            _curHp -= damage;
            if (_curHp <= 0)
            {
                _curHp = 0;
                _stateManager.curState.SetNextState(UnitState.Death);
                team.OnUnitDead(this);
                return true;
            }
            return false;
        }

        public void PrepareBattle()
        {
            _curHp = _info.stat.stat[UnitStatType.MaxHp];
            _stateManager.curState.SetNextState(UnitState.Idle);
            originPos = transform.localPosition;
        }
        
    }


    [Serializable]
    public class UnitInfo
    {
        public int uid;
        public string name;
        public UnitStat stat;

        public UnitInfo(UnitInfoData infoData, UnitStatData statData)
        {
            uid = infoData.uid;
            name = infoData.name;
            stat = new UnitStat(statData);
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