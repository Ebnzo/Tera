﻿using System;

namespace Tera.Game.Messages
{
    public class SpawnUserServerMessage : ParsedMessage
    {
        internal SpawnUserServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(26);
            ServerId = reader.ReadUInt32();// not sure, whether full uint32 is serverid, or only first 2 bytes and the rest part of it is actualy a part of PlayerId, or something else, but it always come along with PlayerID as complex player id
            PlayerId = reader.ReadUInt32();
            Id = reader.ReadEntityId();
            reader.Skip(18);
            RaceGenderClass = new RaceGenderClass(reader.ReadInt32());
            reader.Skip(208);
            Name = reader.ReadTeraString();
            GuildName = reader.ReadTeraString();
//            Console.WriteLine(Name + ":" + BitConverter.ToString(BitConverter.GetBytes(Id.Id))+ ":"+ ServerId.ToString()+" "+ BitConverter.ToString(BitConverter.GetBytes(PlayerId)));
        }

        public EntityId Id { get; private set; }
        public uint ServerId { get; private set; }
        public uint PlayerId { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }
        public RaceGenderClass RaceGenderClass { get; }
    }
}