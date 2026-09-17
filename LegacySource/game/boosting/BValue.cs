using System;
using game.battle.div;
using game.faction;
using game.faction.npc;
using game.faction.player;
using game.faction.royalty;
using init.type;
using settlement.stats;
using world.map.regions;

public interface BValue
{
    public default double vGet(Royalty roy)
    {
        return vGet(roy.induvidual);
    }
    
    public double vGet(Region reg);
    
    public double vGet(Induvidual indu);
    
    public double vGet(Div div);
    
    public double vGet(HCLASS_RACE reg);

    public double vGet(Player f);
    
    public double vGet(FactionNPC f);
    
    public default double vGet(Faction f)
    {
        if (f == null)
            return 0;
        if (f is FactionNPC)
        {
            return vGet((FactionNPC)f);
        }
        return vGet(FACTIONS.player());
    }
    
    public class BValueNone : BValue
    {
        
        public double vGet(Region reg)
        {
            return 0;
        }

        public double vGet(Induvidual indu)
        {
            return 0;
        }

        public double vGet(Div div)
        {
            return 0;
        }

        public double vGet(Faction f)
        {
            return 0;
        }

        public double vGet(HCLASS_RACE reg)
        {
            return 0;
        }

        public double vGet(Player f)
        {
            return 0;
        }

        public double vGet(FactionNPC f)
        {
            return 0;
        }
    }
    
    public class BValueSome : BValue
    {
        
        private readonly double v;
        
        public BValueSome(double v)
        {
            this.v = v;
        }
        
        public double vGet(Region reg)
        {
            return v;
        }

        public double vGet(Induvidual indu)
        {
            return v;
        }

        public double vGet(Div div)
        {
            return v;
        }

        public double vGet(Faction f)
        {
            return v;
        }

        public double vGet(HCLASS_RACE reg)
        {
            return v;
        }

        public double vGet(Player f)
        {
            return v;
        }

        public double vGet(FactionNPC f)
        {
            return v;
        }
    }
    
    public static readonly BValue VALUE1 = new BValueSome(1.0);
    
    public static readonly BValue VALUE0 = new BValueNone();
    
    public abstract class BValueAll : BValue
    {
        public double vGet(Region reg)
        {
            return get();
        }

        public double vGet(Induvidual indu)
        {
            return get();
        }

        public double vGet(Div div)
        {
            return get();
        }

        public double vGet(Faction f)
        {
            return get();
        }

        public double vGet(Player f)
        {
            return get();
        }

        public double vGet(FactionNPC f)
        {
            return get();
        }

        public double vGet(HCLASS_RACE reg)
        {
            return get();
        }
        
        public abstract double get();
    }
    
    public abstract class BValueFaction : BValue
    {
        private readonly Boostable bb;
        
        public BValueFaction(Boostable bo)
        {
            this.bb = bo;
        }
        
        public double vGet(Region reg)
        {
            return vGet(reg.faction());
        }

        public double vGet(Induvidual indu)
        {
            return vGet(indu.faction());
        }

        public double vGet(Div div)
        {
            return vGet(div.faction());
        }

        public double vGet(HCLASS_RACE reg)
        {
            return vGet(FACTIONS.player());
        }

        public double vGet(FactionNPC f)
        {
            return f.bonus.getD(bb);
        }
    }
    
    public abstract interface BValuePlayerOnly : BValue
    {
        public default double vGet(Region reg)
        {
            return vGet(reg.faction());
        }

        public default double vGet(Induvidual indu)
        {
            return vGet(indu.faction());
        }

        public default double vGet(Div div)
        {
            return vGet(div.faction());
        }

        public default double vGet(HCLASS_RACE reg)
        {
            return vGet(FACTIONS.player());
        }
    }
    
    public abstract class BValueInduOnly : BValue
    {
        public BValueInduOnly()
        {
        }
        
        public double vGet(Region reg)
        {
            return 0;
        }

        public double vGet(FactionNPC f)
        {
            return 0;
        }

        public double vGet(Player f)
        {
            return 0;
        }

        public double vGet(HCLASS_RACE reg)
        {
            return 0;
        }
    }
    
    public abstract class BValuePop : BValue
    {
        public BValuePop()
        {
        }
        
        public double vGet(FactionNPC f)
        {
            return 0;
        }
        
        public double vGet(Player f)
        {
            return vGet(HCLASS_RACE.clP());
        }
        
        public double vGet(Div div)
        {
            return vGet(HCLASS_RACE.clP(div.race(), HCLASSES.CITIZEN()));
        }
        
        public double vGet(Induvidual indu)
        {
            return vGet(indu.popCL());
        }
        
        public double vGet(Region reg)
        {
            return 0;
        }
    }
}