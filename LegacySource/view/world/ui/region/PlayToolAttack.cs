using System;
using game.faction;
using game.faction.diplomacy;
using init.sprite;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;
using view.main;
using view.tool;
using world;
using world.entity.army;
using world.map.regions;

namespace view.world.ui.region
{
    abstract class PlayToolAttack : PlacableSimpleTile
    {
        private static readonly CharSequence ¤¤noSoldiers = "¤Region has no soldiers to attack with.";
        private static readonly CharSequence ¤¤noRange = "¤Can't use garrison to attack outside of the region borders.";
        private static readonly CharSequence ¤¤def = "¤Select an army within your regions' borders to attack.";
        private static readonly CharSequence ¤¤question = "¤Are you sure you wish to declare war on {0} and attack {1} with your garrison?";
        private static readonly CharSequence ¤¤besiged = "¤All exits are blocked. Can only sally out and attack the besieging army.";

        static PlayToolAttack()
        {
            D.ts(typeof(PlayToolAttack));
        }

        private Region reg;

        private readonly ToolManager tools;
        private readonly bool dismissable;

        private WArmy aa;
        private readonly ACTION attack = new ACTION()
        {
            public void exe()
            {
                DIP.WAR().set(reg.faction(), aa.faction());
                WORLD.BATTLES().regAttack(reg, aa);
            }
        };

        public PlayToolAttack(ToolManager tools) : this(tools, false)
        {
        }

        public PlayToolAttack(ToolManager tools, bool dismissable) : base(Dic.¤¤Attack, "")
        {
            this.tools = tools;
            this.dismissable = dismissable;
        }

        void add(Region reg)
        {
            this.reg = reg;
            tools.place(this, config);
        }

        public override CharSequence isPlacable(int tx, int ty)
        {
            if (RD.MILITARY().garrison.get(reg) == 0 || RD.MILITARY().divisions(reg).size() == 0)
            {
                return ¤¤noSoldiers;
            }

            WArmy ok = null;
            CharSequence prob = ¤¤def;

            foreach (WArmy a in WORLD.ENTITIES().armies.fillTile(tx, ty))
            {
                if (a.faction() != reg.faction())
                {
                    if (WORLD.BATTLES().besiged(reg) && !a.besieging(reg))
                    {
                        prob = ¤¤besiged;
                    }
                    else if (!reg.is(a.ctx(), a.cty()) && !a.besieging(reg))
                    {
                        prob = ¤¤noRange;
                    }
                    else
                    {
                        ok = a;
                        WORLD.OVERLAY().hoverEntity(a);
                        VIEW.mouse().setReplacement(SPRITES.icons().m.sword);
                    }
                }
            }

            if (ok == null)
            {
                return prob;
            }
            return null;
        }

        public override void place(int tx, int ty)
        {
            foreach (WArmy a in WORLD.ENTITIES().armies.fillTile(tx, ty))
            {
                if (a.faction() != reg.faction())
                {
                    if (WORLD.BATTLES().besiged(reg) && !a.besieging(reg))
                    {
                        ;
                    }
                    else if (!reg.is(a.ctx(), a.cty()) && !a.besieging(reg))
                    {
                        ;
                    }
                    else
                    {
                        if (!DIP.WAR().is(a.faction(), reg.faction()))
                        {
                            aa = a;
                            Str.TMP.clear().add(¤¤question).insert(0, Faction.name(aa.faction())).insert(1, aa.name);
                            VIEW.inters().yesNo.activate(Str.TMP, attack, ACTION.NOP, dismissable);
                            return;
                        }
                        else
                        {
                            WORLD.BATTLES().regAttack(reg, a);
                            return;
                        }
                    }
                }
            }
        }

        readonly ToolConfig config = new ToolConfig()
        {
            public void deactivateAction()
            {
            }

            public void update(bool UIHovered)
            {
                if (!added())
                {
                    tools.place(null, null, false);
                }
                else if (dismissable)
                {
                    WORLD.OVERLAY().hover(reg);
                }
            }

            public bool back()
            {
                return dismissable;
            }
        };

        abstract bool added();
    }
}