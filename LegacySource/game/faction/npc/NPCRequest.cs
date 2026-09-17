using System;
using System.Text;
using System.IO;

namespace Game.Faction.NPC
{
    public class NPCRequest : Debuggable
    {
        private bool has = false;
        private double time = 0;
        private double penalty = 0;
        private readonly FactionNPC f;
        private string key = "";

        public NPCRequest(FactionNPC fa)
        {
            this.f = fa;
        }

        public bool Has()
        {
            return has;
        }

        public void Set(double penalty, string key)
        {
            time = TIME.CurrentSecond();
            this.penalty = penalty;
            has = true;
            this.key = key;
        }

        public void Clear()
        {
            has = false;
        }

        public void Expire()
        {
            if (has)
            {
                has = false;
                ROPINION.GIFTS().MakeDeal(f, penalty);
            }
        }

        public void Update()
        {
            if (has && Math.Abs(TIME.CurrentSecond() - time) > TIME.SecondsPerDay())
            {
                Expire();
            }
        }

        public void Save(BinaryWriter file)
        {
            file.Write(has);
            file.Write(time);
            file.Write(penalty);
            file.Write(key);
        }

        public void Load(BinaryReader file)
        {
            has = file.ReadBoolean();
            time = file.ReadDouble();
            penalty = file.ReadDouble();
            key = file.ReadString();
        }
    }
}