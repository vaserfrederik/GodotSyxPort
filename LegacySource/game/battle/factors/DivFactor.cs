using System;
using System.Collections.Generic;
using game.battle.div;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.player;
using init.type;
using settlement.stats;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.gui.misc;
using world.map.regions;

namespace game.battle.factors
{
    public abstract class DivFactor
    {
        public readonly BSourceInfo info;
        public readonly string message;
        public readonly BoostSpecs specs;
        public readonly double midValue;

        public DivFactor(string name, string desc, SPRITE icon, string message, double mid)
        {
            info = new BSourceInfo(name, desc, null, icon);
            this.message = message;
            specs = new BoostSpecs(info, true);
            midValue = mid;
            DivFactors.all.Add(this);
        }

        public abstract double GetD(Div div);

        public void Hover(Div div, GUI_BOX box)
        {
            GBox b = (GBox)box;
            b.Title(info.name);
            b.Text(info.desc);
            b.NL(4);
            Phover(div, b);
            b.NL(8);
            specs.HoverDetailed(b, v.vGet(div), null, null, -1);
            b.NL();
        }

        protected virtual void Phover(Div div, GBox b)
        {
        }

        public DivFactor Boost(Boostable b, double from, double to, bool isMul)
        {
            specs.Push(new BoosterValue(v, info, from, to, isMul), b);
            return this;
        }

        protected double InduValue(Induvidual indu)
        {
            return midValue;
        }

        private readonly BValue v = new BValue
        {
            vGet = (Div div) => GetD(div),
            vGet = (Faction f) => midValue,
            vGet = (Region reg) => midValue,
            vGet = (Induvidual indu) =>
            {
                Div d = STATS.BATTLE().DIV.Get(indu);
                if (d == null || !d.Active())
                    return InduValue(indu);
                return GetD(d);
            },
            vGet = (HCLASS_RACE popTime) => midValue,
            vGet = (Player f) => midValue,
            vGet = (FactionNPC f) => midValue
        };
    }
}