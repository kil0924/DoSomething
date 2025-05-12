using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using AnimationState = Spine.AnimationState;
using Event = Spine.Event;

namespace Rpg
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private Unit_FSM fsm;
        [SerializeField] private UnitInfo _info;
        [SerializeField] private UnitStat _stat;
        [SerializeField] private UnitSkillManager _skillManager;
        [SerializeField] private UnitSkillEffectManager _skillEffectManager;
        public TeamManager team { get; private set; }
        
        public void Init(UnitResource resource)
        {
            fsm = new Unit_FSM(this);
            fsm.Init();
            
            SetSpine(resource.spineData);
            
            _info = new UnitInfo(resource.infoData);
            _stat = new UnitStat(resource.statData);
            _skillManager = new UnitSkillManager(resource.skillDataDict);
            _skillEffectManager = new UnitSkillEffectManager();
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
            _curHp = _stat.GetStat(UnitStatType.MaxHp);
            _skillEffectManager.Clear();
            fsm.curState.SetNextState(UnitState.Idle);
            originPos = transform.localPosition;
        }

        public void OnUpdate(float deltaTime)
        {
            fsm.OnUpdate(deltaTime);
            _spine.Update(deltaTime);
        }

        public void OnFixedUpdate(float deltaTime)
        {
            fsm.OnFixedUpdate(deltaTime);
            _spine.Update(deltaTime);
            
            ExecuteEvents();
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
            trackEntry.Event += AddEvent;

            _animationTracks.Add(trackEntry);
        }

        public void AddAnimation(string aniName, bool loop = true, AnimationState.TrackEntryDelegate onComplete = null)
        {
            _animationTracks[^1].Complete -= onComplete;

            var trackEntry = _spine.AnimationState.AddAnimation(0, aniName, loop, 0);
            trackEntry.Complete += onComplete;
            trackEntry.Event += AddEvent;

            _animationTracks.Add(trackEntry);
        }

        private Queue<Event> _eventQueue = new Queue<Event>();

        private void AddEvent(TrackEntry track, Event e)
        {
            _eventQueue.Enqueue(e);
            // Debug.Log($"[Enqueue Event] {e.Data.Name} {e.Time} {Time.inFixedTimeStep} {_lastFrameTime}");
        }

        private void ExecuteEvents()
        {
            while (_eventQueue.TryDequeue(out var e))
            {
                // Debug.Log($"[Execute Event] {e.Data.Name} {e.Time} {Time.inFixedTimeStep} {_lastFrameTime}");
            }
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

        #region ========== 턴 진행 ==========

        public void ProcessTurn()
        {
            _skillManager.ProcessTurn();
            _skillEffectManager.ProcessTurn();
        }
        private Action _onFinishTurn;

        public void OnFinishTurn()
        {
            _onFinishTurn?.Invoke();
            _onFinishTurn = null;
        }

        public bool ExecuteTurn(Action onFinish)
        {
            if (fsm.curState.State != UnitState.Idle)
            {
                return false;
            }

            _onFinishTurn = onFinish;
            fsm.curState.SetNextState(UnitState.Turn);

            return true;
        }
        
        #endregion

        private int _curHp;

        public int curHp
        {
            get => _curHp;
            set
            {
                var maxHp = GetStat(UnitStatType.MaxHp);
                _curHp = Mathf.Clamp(value, 0, maxHp);
                OnHpChange?.Invoke(_curHp, maxHp);
            }
        }

        public int GetStat(UnitStatType type)
        {
            var value = _stat.GetStat(type) + _skillEffectManager.GetStatEffectValue(type);
            var percentType = StatLinker.GetPercentStat(type);
            var percentValue = 0;
            if (percentType != UnitStatType.None)
            {
                percentValue = _stat.GetStat(percentType) + _skillEffectManager.GetStatEffectValue(percentType);
            }
            return (int)(value * (1 + percentValue * 0.01f));
        }
        public event Action<int, int> OnHpChange;

        public UnitSkill SelectSkill()
        {
            return _skillManager.SelectSkill();
        }

        public void AddSkillEffect(UnitSkillEffect effect)
        {
            _skillEffectManager.AddSkillEffect(effect);
        }

        public void Attack(Unit target, float value, Action onKill = null)
        {
            if (target == null)
                return;

            var attack = GetStat(UnitStatType.Attack);

            var damage = (int)(attack * value);

            if (target.TakeDamage(damage))
            {
                onKill?.Invoke();
            }

            Debug.Log($"{_info.name} attack {target.name} remain hp : {target._curHp}");
        }

        public bool TakeDamage(int damage)
        {
            var defense = GetStat(UnitStatType.Defense);
            damage = Mathf.Max(damage - defense, 0);
            
            curHp -= damage;
            if (curHp <= 0)
            {
                fsm.curState.SetNextState(UnitState.Death);
                team.OnUnitDead(this);
                return true;
            }

            return false;
        }

        public void Heal(Unit target, float value)
        {
            if (target == null)
                return;

            var attack = GetStat(UnitStatType.Attack);
            var heal = (int)(attack * value);
            heal = Mathf.Max(heal, 0);

            target.TakeHeal(heal);
            
            Debug.Log($"{_info.name} heal {target.name} remain hp : {target._curHp}");
        }

        public void TakeHeal(int heal)
        {
            curHp += heal;
        }

        public int CountSkillEffect(int uid)
        {
            return _skillEffectManager.GetCount(uid);
        }

    }

    [Serializable]
    public class UnitInfo
    {
        public int uid;
        public string name;

        public UnitInfo(UnitInfoData infoData)
        {
            uid = infoData.uid;
            name = infoData.name;
        }
    }

    public class UnitResource
    {
        public UnitInfoData infoData { get; private set; }
        public UnitStatData statData { get; private set; }
        public SkeletonDataAsset spineData { get; private set; }
        
        public Dictionary<UnitSkillData, List<UnitSkillEffectData>> skillDataDict { get; private set; }

        public UnitResource SetInfoData(UnitInfoData data)
        {
            infoData = data;
            return this;
        }
        public UnitResource SetStatData(UnitStatData data)
        {
            statData = data;
            return this;
        }
        public UnitResource SetSpineData(SkeletonDataAsset data)
        {
            spineData = data;
            return this;
        }
        public UnitResource SetSkillDataList(Dictionary<UnitSkillData, List<UnitSkillEffectData>> data)
        {
            skillDataDict = data;
            return this;
        }
    }
}