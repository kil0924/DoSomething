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
        public TeamManager team { get; private set; }

        public void Init(UnitResource resource)
        {
            fsm = new Unit_FSM(this);
            fsm.Init();
            
            _info = new UnitInfo(resource.infoData, resource.statData);
            SetSpine(resource.spineData);
            
            skillEffectManager = new UnitSkillEffectManager();
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
            _curHp = _info.stat.GetStat(UnitStatType.MaxHp);
            skillEffectManager.Clear();
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

        private Action _onFinishTurn;

        public void OnFinishTurn()
        {
            skillEffectManager.ProcessTurn();
            
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
            var value = _info.stat.GetStat(type) + skillEffectManager.GetStatEffectValue(type);
            var percentType = StatLinker.GetPercentStat(type);
            var percentValue = 0;
            if (percentType != UnitStatType.None)
            {
                percentValue = _info.stat.GetStat(percentType) + skillEffectManager.GetStatEffectValue(percentType);
            }
            return (int)(value * (1 + percentValue * 0.01f));
        }
        public event Action<int, int> OnHpChange;

        public void Attack(Unit target, Action onKill = null)
        {
            if (target == null)
                return;

            var attack = GetStat(UnitStatType.Attack);
            var defense = target.GetStat(UnitStatType.Defense);
            var damage = attack - defense;
            damage = Mathf.Max(damage, 0);

            if (target.TakeDamage(damage))
            {
                onKill?.Invoke();
            }
            else
            {
                var skillEffect = new SkillEffect(SkillEffectType.Defense, -1, 5);
                target.skillEffectManager.AddSkillEffect(skillEffect);;
            }

            Debug.Log($"{_info.name} attack {target.name} remain hp : {target._curHp}");
        }

        public bool TakeDamage(int damage)
        {
            curHp -= damage;
            if (curHp <= 0)
            {
                fsm.curState.SetNextState(UnitState.Death);
                team.OnUnitDead(this);
                return true;
            }

            return false;
        }

        [SerializeField]
        private UnitSkillEffectManager skillEffectManager;

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

    public class UnitSkillEffectManager
    {
        private Dictionary<UnitStatType, int> _statEffect = new Dictionary<UnitStatType, int>();

        private Dictionary<SkillEffectType, List<SkillEffect>> _skillEffect = new Dictionary<SkillEffectType, List<SkillEffect>>();

        public void AddSkillEffect(SkillEffect effect)
        {
            if (_skillEffect.ContainsKey(effect.type) == false)
            {
                _skillEffect.Add(effect.type, new List<SkillEffect>());
            }

            _skillEffect[effect.type].Add(effect);
            CalcStatEffect(effect.type);
        }

        public void RemoveSkillEffect(SkillEffect effect)
        {
            _skillEffect[effect.type].Remove(effect);
            CalcStatEffect(effect.type);
        }

        private void CalcStatEffect(SkillEffectType type)
        {
            if (SkillEffectLinker.GetStatType(type) != UnitStatType.None)
            {
                var value = 0f;
                int count = _skillEffect[type].Count;
                for (int i = 0; i < count; i++)
                {
                    value += _skillEffect[type][i].value;
                }
                _statEffect[SkillEffectLinker.GetStatType(type)] = (int)value;
            }
        }

        public int GetStatEffectValue(UnitStatType statType)
        {
            return _statEffect.GetValueOrDefault(statType, 0);
        }

        public void Clear()
        {
            _skillEffect.Clear();
            _statEffect.Clear();
        }

        public void ProcessTurn()
        {
            foreach (var list in _skillEffect.Values)
            {
                list.ForEach(x => x.ExecuteTurn());
                list.RemoveAll(x => x.isAlive == false);
            }
        }
    }

    public enum SkillEffectType
    {
        None,
        [SkillEffect(UnitStatType.MaxHp)]
        MaxHp,
        [SkillEffect(UnitStatType.MaxHpPercent)]
        MaxHpPercent,
        [SkillEffect(UnitStatType.Attack)] 
        Attack,
        [SkillEffect(UnitStatType.AttackPercent)]
        AttackPercent,
        [SkillEffect(UnitStatType.Defense)]
        Defense,
        [SkillEffect(UnitStatType.DefensePercent)]
        DefensePercent,
        
        Stun,
        
    }

    public class SkillEffect
    {
        public SkillEffectType type;
        public float value;
        public int turn;

        public bool isAlive = true;

        public SkillEffect(SkillEffectType type, float value, int turn)
        {
            this.type = type;
            this.value = value;
            this.turn = turn;
            isAlive = true;
        }

        public void ExecuteTurn()
        {
            if (isAlive == false)
                return;
            
            turn--;
            if (turn <= 0)
            {
                isAlive = false;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SkillEffectAttribute : Attribute
    {
        public UnitStatType statType { get; }

        public SkillEffectAttribute(UnitStatType statType = UnitStatType.None)
        {
            this.statType = statType;
        }
    }

    public static class SkillEffectLinker
    {
        private static Dictionary<SkillEffectType, UnitStatType> _statEffectMap;

        static SkillEffectLinker()
        {
            _statEffectMap = new();
            var values = Enum.GetValues(typeof(SkillEffectType));

            foreach (SkillEffectType type in values)
            {
                var field = typeof(SkillEffectType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<SkillEffectAttribute>();
                if (attr != null)
                {
                    _statEffectMap[type] = attr.statType;
                }
            }
        }

        public static UnitStatType GetStatType(SkillEffectType baseStat)
        {
            return _statEffectMap.GetValueOrDefault(baseStat, UnitStatType.None);
        }
    }
}