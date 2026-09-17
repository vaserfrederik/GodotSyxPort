using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Init.Tech
{
    public class TECH : INDEXED
    {
        public readonly COLOR color;
        private readonly int index;
        public readonly int levelMax;
        public readonly LIST<TechCost> costs;
        public readonly int costTotal;
        public readonly double levelCostInc;
        // public readonly double levelCostMulInc;

        private LIST<TechRequirement> needs;
        private LIST<TechRequirement> needsPruned;
        public readonly INFO info;

        public readonly BoostSpecs boosters;
        public readonly Lockable<Faction> requires;
        public readonly Lockers lockers;

        private SPRITE icon = null;

        public readonly string key;
        public readonly TechTree tree;
        public readonly double AIAmount;
        private Json requiresTech;

        public TECH(TechCurrencies cc, string key, LISTE<TECH> all, Json data, Json text, TechTree tree, int xx, int yy) 
        {
            this.key = key;
            this.tree = tree;
            if (text != null)
                info = new INFO(text);
            else
                info = new INFO(tree.name + " " + (xx + 1) + ":" + (yy + 1), Dic.empty);
            index = all.add(this);
            levelMax = data.i("LEVEL_MAX", 1, 10000, 1);
            costs = cc.read(data);
            int a = 0;
            foreach (TechCost c in costs)
                a += c.amount;
            costTotal = a;
            levelCostInc = data.dTry("LEVEL_COST_INC", 0, 100000, 0);
            if (data.has("LEVEL_COST_INC_MUL"))
                LOG.ln(key);
            AIAmount = data.dTry("AI_AMOUNT", 0, 1, 1.0);
            requires = GVALUES.FACTION.LOCK.push();
            requires.push(data);
            lockers = new Lockers(Dic.¤¤TechnologyShort + ": " + info.name, UI.icons().s.vial);

            if (data.has("COLOR"))
                color = new ColorImp(data);
            else
                color = tree.color;

            lockers.add(GVALUES.FACTION, data, new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    if (t == FACTIONS.player())
                    {
                        if (FACTIONS.player().tech.isPenaltyLocked(this))
                            return 0;
                        return FACTIONS.player().tech.level(this) > 0 ? 1 : 0;
                    }
                    return 1;
                }
            });

            lockers.add(GVALUES.INDU, data, new DOUBLE_O<Induvidual>()
            {
                public double getD(Induvidual t)
                {
                    if (t.faction() == FACTIONS.player())
                    {
                        if (FACTIONS.player().tech.isPenaltyLocked(this))
                            return 0;
                        return FACTIONS.player().tech.level(this) > 0 ? 1 : 0;
                    }
                    return 1;
                }
            });

            lockers.add(GVALUES.REGION, data, new DOUBLE_O<Region>()
            {
                public double getD(Region t)
                {
                    if (t.faction() == FACTIONS.player())
                    {
                        if (FACTIONS.player().tech.isPenaltyLocked(this))
                            return 0;
                        return FACTIONS.player().tech.level(this) > 0 ? 1 : 0;
                    }
                    return 1;
                }
            });

            boosters = new BoostSpecs(info.name, UI.icons().s.vial, false);
            boosters.read(data, null);

            if (data.has("ICON"))
                icon = UI.icons().get(data).huge;

            requiresTech = data;

            data.has("REQUIRES_TECH_LEVEL");

            data.checkUnused();
        }

        public int index()
        {
            return index;
        }

        public LIST<TechRequirement> requires()
        {
            return needs;
        }

        public LIST<TechRequirement> requiresNodes()
        {
            return needsPruned;
        }

        void set(LIST<TechRequirement> needs)
        {
            this.needs = needs;
        }

        void prune(LIST<TechRequirement> needs)
        {
            this.needsPruned = needs;
        }

        public bool requires(TECH other, int level)
        {
            if (other == this)
                return false;
            for (int i = 0; i < needs.size(); i++)
            {
                TECH t = needs.get(i).tech;
                if (t == other || t.requires(other, needs.get(i).level))
                    if (needs.get(i).level > level)
                        return true;
            }
            return false;
        }

        public SPRITE icon()
        {
            if (icon == null)
            {
                icon = TechIcon.icon(this);
            }
            return icon;
        }

        public static class TechRequirement
        {
            public readonly TECH tech;
            public readonly int level;

            public TechRequirement(TECH t, int l)
            {
                this.tech = t;
                this.level = l;
            }

            public override bool Equals(object obj)
            {
                if (obj is TechRequirement)
                {
                    TechRequirement q = (TechRequirement)obj;
                    return q.level == level && q.tech == tech;
                }
                return false;
            }
        }

        public CharSequence name()
        {
            return info.name;
        }

        public CharSequence desc()
        {
            return info.desc;
        }
    }
}