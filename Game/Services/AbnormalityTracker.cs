﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tera.Game.Messages;

namespace Tera.Game
{
    public class AbnormalityStorage
    {
        internal Dictionary<NpcEntity, Dictionary<HotDot, AbnormalityDuration>> NpcAbnormalityTime;
        internal Dictionary<Player, Dictionary<HotDot, AbnormalityDuration>> PlayerAbnormalityTime;

        public AbnormalityStorage()
        {
            NpcAbnormalityTime = new Dictionary<NpcEntity, Dictionary<HotDot, AbnormalityDuration>>();
            PlayerAbnormalityTime = new Dictionary<Player, Dictionary<HotDot, AbnormalityDuration>>();
        }
        public AbnormalityStorage(Dictionary<NpcEntity, Dictionary<HotDot, AbnormalityDuration>> npcTimes, Dictionary<Player, Dictionary<HotDot, AbnormalityDuration>> playerTimes)
        {
            NpcAbnormalityTime = npcTimes;
            PlayerAbnormalityTime = playerTimes;
        }
        internal Dictionary<HotDot, AbnormalityDuration> AbnormalityTime(NpcEntity entity)
        {
            if (!NpcAbnormalityTime.ContainsKey(entity))
                NpcAbnormalityTime.Add(entity, new Dictionary<HotDot, AbnormalityDuration>());
            return NpcAbnormalityTime[entity];
        }
        internal Dictionary<HotDot, AbnormalityDuration> AbnormalityTime(Player player)
        {
            if (!PlayerAbnormalityTime.ContainsKey(player))
                PlayerAbnormalityTime.Add(player, new Dictionary<HotDot, AbnormalityDuration>());
            return PlayerAbnormalityTime[player];
        }
        public Dictionary<HotDot, AbnormalityDuration> Get(NpcEntity entity)
        {
            if (entity==null) new Dictionary<HotDot, AbnormalityDuration>();
            if (!NpcAbnormalityTime.ContainsKey(entity))
                return new Dictionary<HotDot, AbnormalityDuration>();
            else
                return NpcAbnormalityTime[entity];
        }
        public Dictionary<HotDot, AbnormalityDuration> Get(Player player)
        {
            if (player == null) new Dictionary<HotDot, AbnormalityDuration>();
            if (!PlayerAbnormalityTime.ContainsKey(player))
                return new Dictionary<HotDot, AbnormalityDuration>();
            else
                return PlayerAbnormalityTime[player];
        }

        public AbnormalityStorage Clone(NpcEntity boss)
        {
            var npcTimes= new Dictionary<NpcEntity, Dictionary<HotDot, AbnormalityDuration>>();
            if (boss!=null)
                npcTimes = NpcAbnormalityTime.Where(x => x.Key == boss).ToDictionary(y => y.Key, y => y.Value.ToDictionary(x => x.Key, x => (AbnormalityDuration)x.Value.Clone()));
            var PlayerTimes = PlayerAbnormalityTime.ToDictionary(y => y.Key, y => y.Value.ToDictionary(x => x.Key, x => (AbnormalityDuration)x.Value.Clone()));
            return new AbnormalityStorage(npcTimes, PlayerTimes);
        }

        public AbnormalityStorage Clone()
        {
            var npcTimes=NpcAbnormalityTime.ToDictionary(y => y.Key, y => y.Value.ToDictionary(x => x.Key, x => (AbnormalityDuration)x.Value.Clone()));
            var PlayerTimes = PlayerAbnormalityTime.ToDictionary(y => y.Key, y => y.Value.ToDictionary(x => x.Key, x => (AbnormalityDuration)x.Value.Clone()));
            return new AbnormalityStorage(npcTimes, PlayerTimes);
        }
        public void ClearEnded()
        {
            var npcTimes= new Dictionary<NpcEntity, Dictionary<HotDot, AbnormalityDuration>>();
            foreach (var i in NpcAbnormalityTime)
            {
                var j = i.Value.Where(x => !x.Value.Ended()).ToDictionary(x => x.Key, x => new AbnormalityDuration(x.Value.InitialPlayerClass, x.Value.LastStart()));
                if (j.Count() > 0)
                    npcTimes.Add(i.Key, j);
            }
            var PlayerTimes = new Dictionary<Player, Dictionary<HotDot, AbnormalityDuration>>();
            foreach (var i in PlayerAbnormalityTime)
            {
                var j = i.Value.Where(x => !x.Value.Ended()).ToDictionary(x => x.Key, x => new AbnormalityDuration(x.Value.InitialPlayerClass, x.Value.LastStart()));
                if (j.Count() > 0)
                    PlayerTimes.Add(i.Key, j);
            }
            NpcAbnormalityTime = npcTimes;
            PlayerAbnormalityTime = PlayerTimes;
        }
        public void EndAll(long ticks)
        {
            foreach (var i in NpcAbnormalityTime)
            {
                foreach (var j in i.Value.Where(x => !x.Value.Ended()))
                { j.Value.End(ticks / TimeSpan.TicksPerSecond); }
            }
            var PlayerTimes = new Dictionary<Player, Dictionary<HotDot, AbnormalityDuration>>();
            foreach (var i in PlayerAbnormalityTime)
            {
                foreach (var j in i.Value.Where(x => !x.Value.Ended()))
                { j.Value.End(ticks / TimeSpan.TicksPerSecond); }
            }

        }
    }
    public class AbnormalityTracker
    {
        private readonly Dictionary<EntityId, List<Abnormality>> _abnormalities =
            new Dictionary<EntityId, List<Abnormality>>();
        public Action<SkillResult> UpdateDamageTracker;
        internal EntityTracker EntityTracker;
        internal PlayerTracker PlayerTracker;
        internal HotDotDatabase HotDotDatabase;
        internal AbnormalityStorage AbnormalityStorage;

        public AbnormalityTracker(EntityTracker entityTracker, PlayerTracker playerTracker, HotDotDatabase hotDotDatabase, AbnormalityStorage abnormalityStorage, Action<SkillResult> update=null)
        {
            EntityTracker = entityTracker;
            PlayerTracker = playerTracker;
            HotDotDatabase = hotDotDatabase;
            UpdateDamageTracker = update;
            AbnormalityStorage = abnormalityStorage;
        }

        public void AddAbnormality(SAbnormalityBegin message)
        {
            AddAbnormality(message.TargetId, message.SourceId, message.Duration, message.Stack, message.AbnormalityId,
                message.Time.Ticks);
        }

        public void AddAbnormality(EntityId target, EntityId source, int duration, int stack, int abnormalityId,
            long ticks)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                _abnormalities.Add(target, new List<Abnormality>());
            }
            var hotdot = HotDotDatabase.Get(abnormalityId);
            if (hotdot == null)
            {
                return;
            }

            if (_abnormalities[target].Where(x => x.HotDot.Id == abnormalityId).Count() == 0) //dont add existing abnormalities since we don't delete them all, that may cause many untrackable issues.
                _abnormalities[target].Add(new Abnormality(hotdot, source, target, duration, stack, ticks, this));

        }

        public void RefreshAbnormality(SAbnormalityRefresh message)
        {
            if (!_abnormalities.ContainsKey(message.TargetId))
            {
                return;
            }
            var abnormalityUser = _abnormalities[message.TargetId];
            foreach (var abnormality in abnormalityUser)
            {
                if (abnormality.HotDot.Id != message.AbnormalityId) continue;
                abnormality.Refresh(message.StackCounter, message.Duration, message.Time.Ticks);
                return;
            }
        }

        public bool AbnormalityExist(EntityId target, HotDot dot)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                return false;
            }
            var abnormalityTarget = _abnormalities[target];
            for(var i = 0; i < abnormalityTarget.Count; i++)
            {
                if(abnormalityTarget[i].HotDot == dot)
                {
                    return true;
                }
            }
            return false;
        }

        public void DeleteAbnormality(EntityId target, int abnormalityId, long ticks)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                return;
            }

            var abnormalityUser = _abnormalities[target];

            for (var i = 0; i < abnormalityUser.Count; i++)
            {
                if (abnormalityUser[i].HotDot.Id == abnormalityId)
                {
                    abnormalityUser[i].ApplyBuffDebuff(ticks);
                    abnormalityUser.Remove(abnormalityUser[i]);
                    break;
                }
            }

            if (abnormalityUser.Count == 0)
            {
                _abnormalities.Remove(target);
                return;
            }
            _abnormalities[target] = abnormalityUser;
        }

        public void DeleteAbnormality(SAbnormalityEnd message)
        {
            DeleteAbnormality(message.TargetId, message.AbnormalityId, message.Time.Ticks);
        }
        public void DeleteAbnormality(SDespawnNpc message)
        {
            DeleteAbnormality(message.Npc, message.Time.Ticks);
        }

        public void DeleteAbnormality(SNpcStatus message)
        {
            DeleteAbnormality(message.Npc, 8888888, message.Time.Ticks);
        }

        public void DeleteAbnormality(SCreatureChangeHp message)
        {
            DeleteAbnormality(message.TargetId, 8888889, message.Time.Ticks);
        }

        public void DeleteAbnormality(SDespawnUser message)
        {
            DeleteAbnormality(message.User, message.Time.Ticks);
        }

        private void DeleteAbnormality(EntityId entity, long ticks)
        {
            if (!_abnormalities.ContainsKey(entity))
            {
                return;
            }
            foreach (var abno in _abnormalities[entity])
            {
                abno.ApplyBuffDebuff(ticks);
            }
            _abnormalities.Remove(entity);
        }


        public void Update(SPlayerChangeMp message)
        {
            Update(message.TargetId, message.SourceId, message.MpChange, message.Type, message.Critical == 1, false,
                message.Time.Ticks);
        }

        private void Update(EntityId target, EntityId source, int change, int type, bool critical, bool isHp, long time)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                return;
            }

            var abnormalities = _abnormalities[target];
            abnormalities = abnormalities.OrderByDescending(o => o.TimeBeforeApply).ToList();

            foreach (var abnormality in abnormalities)
            {
                if (abnormality.Source != source && abnormality.Source != abnormality.Target)
                {
                    continue;
                }

                if (isHp)
                {
                    if ((!(abnormality.HotDot.Hp > 0) || change <= 0) &&
                        (!(abnormality.HotDot.Hp < 0) || change >= 0)
                        ) continue;
                }
                else
                {
                    if ((!(abnormality.HotDot.Mp > 0) || change <= 0) &&
                        (!(abnormality.HotDot.Mp < 0) || change >= 0)
                        ) continue;
                }

                if ((int) HotDotDatabase.HotOrDot.Dot != type && (int) HotDotDatabase.HotOrDot.Hot != type)
                {
                    continue;
                }

                abnormality.Apply(change, critical, isHp, time);
                return;
            }
        }

        public void Update(SCreatureChangeHp message)
        {
            Update(message.TargetId, message.SourceId, message.HpChange, message.Type, message.Critical == 1, true,
                message.Time.Ticks);
        }
    }
}