using System;
using System.Collections.Generic;
using util.info;
using util.keymap;

namespace init.type
{
    public abstract class HCLASS : INFO, MAPPED
    {
        private readonly int index;
        public readonly bool player;
        public readonly COLOR color;
        public readonly string key;
        public readonly int playerIndex;

        protected HCLASS(List<HCLASS> all, List<HCLASS> allP, string key, string name, string names, string desc, bool player, COLOR color)
            : base(name, names, desc, null)
        {
            this.player = player;
            if (player)
                playerIndex = allP.Add(this);
            else
                playerIndex = -1;

            this.color = color;
            index = all.Add(this);
            this.key = key;
        }

        public abstract Icon icon();
        public abstract Icon iconSmall();

        public override string ToString()
        {
            return name + "#" + index;
        }

        public override int index()
        {
            return index;
        }

        public HCLASS_RACE get(Race race)
        {
            return HCLASS_RACE.clP(race, this);
        }

        public override string key()
        {
            return key;
        }
    }
}