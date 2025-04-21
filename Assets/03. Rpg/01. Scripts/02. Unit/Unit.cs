using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using AnimationState = Spine.AnimationState;

namespace Rpg
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitStateManager _stateManager;
        [SerializeField] private UnitInfo _info;
        public TeamManager team { get; private set; }

        private void Awake()
        {
            _stateManager = new UnitStateManager(this);
            _stateManager.Init();
        }

        public void FixedUpdate()
        {
            _stateManager.OnFixedUpdate(Time.deltaTime);
        }

        public void Init(UnitResource resource)
        {
            _info = new UnitInfo(resource.infoData, resource.statData);
            SetSpine(resource.spineData);
            SetSide(false);
        }

        public void SetSide(bool isLeft)
        {
            _spine.transform.localScale = new Vector3(isLeft ? 1 : -1, 1, 1);
        }

        public void SetTeam(TeamManager team)
        {
            this.team = team;
        }

        public void PrepareBattle()
        {
            _curHp = (int)_info.stat.GetStat(UnitStatType.MaxHp);
            _stateManager.curState.SetNextState(UnitState.Idle);
            originPos = transform.localPosition;
        }

        #region ========== 스파인 ==========

        [SerializeField] private SkeletonAnimation _spine;

        public void SetSpine(SkeletonDataAsset asset)
        {
            _spine.skeletonDataAsset = asset;
            _spine.initialSkinName = "Normal";
            _spine.Initialize(true);
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

        #endregion

        #region ========== 이동 ==========

        public Vector3 originPos { get; private set; }

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

        #endregion

        #region ========== 스킬 ==========

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

        private int _curHp;

        public int curHp
        {
            get => _curHp;
            set
            {
                var maxHp = (int)_info.stat.GetStat(UnitStatType.MaxHp);
                _curHp = Mathf.Clamp(value, 0, maxHp);
                OnHpChange?.Invoke(_curHp, maxHp);
            }
        }
        public event Action<int, int> OnHpChange;
        
        public void Attack(Unit target, Action onKill = null)
        {
            if (target == null)
                return;

            var damage =
                (int)(_info.stat.GetStat(UnitStatType.Attack) - target._info.stat.GetStat(UnitStatType.Defense));
            damage = Mathf.Max(damage, 0);

            if (target.TakeDamage(damage))
            {
                onKill?.Invoke();
            }

            Debug.Log($"{_info.name} attack {target.name} remain hp : {target._curHp}");
        }

        public bool TakeDamage(int damage)
        {
            curHp -= damage;
            if (curHp <= 0)
            {
                _stateManager.curState.SetNextState(UnitState.Death);
                team.OnUnitDead(this);
                return true;
            }

            return false;
        }
        
        #endregion
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