﻿using System;
using System.Diagnostics;

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
            Position = reader.ReadVector3f();
            Heading = reader.ReadAngle();
            reader.Skip(4);
            RaceGenderClass = new RaceGenderClass(reader.ReadInt32());
            reader.Skip(11);
            Dead = (reader.ReadByte() & 1) == 0;
            reader.Skip(196);
            Name = reader.ReadTeraString();
            GuildName = reader.ReadTeraString();
            //Debug.WriteLine(Name + ":" + BitConverter.ToString(BitConverter.GetBytes(Id.Id))+ ":"+ ServerId.ToString()+" "+ BitConverter.ToString(BitConverter.GetBytes(PlayerId))+" "+Dead);
        }

        public bool Dead { get; set; }
        public Angle Heading { get; set; }
        public Vector3f Position { get; set; }
        public EntityId Id { get; private set; }
        public uint ServerId { get; private set; }
        public uint PlayerId { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }
        public RaceGenderClass RaceGenderClass { get; }
    }
}