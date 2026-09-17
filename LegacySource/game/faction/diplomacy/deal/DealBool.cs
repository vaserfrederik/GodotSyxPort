using System;
using System.Collections.Generic;
using snake2d.util.sets;
using snake2d.util.sprite;
using util;
using util.data;
using util.gui.misc;
using util.info;

namespace game.faction.diplomacy.deal
{
    public abstract class DealBool : BOOLEAN_MUTABLE
    {
        public readonly INFO info;
        public readonly SPRITE icon;

        private bool toggled;

        protected DealBool(LISTE<DealBool> bools, string name, string desc, SPRITE icon)
        {
            info = new INFO(name, desc);
            this.icon = icon;
            bools.add(this);
        }

        public abstract string problem();

        public abstract double value();

        public abstract void execute();

        protected abstract void pInit(DealParty a, DealParty b, Debugger debO);

        protected abstract DipStance stance();

        public override bool is()
        {
            return toggled;
        }

        public override BOOLEAN_MUTABLE set(bool b)
        {
            toggled = b;
            return this;
        }

        public void hover(GBox b)
        {
            b.title(info.name);
            b.text(info.desc);
            b.NL();
        }
    }
}