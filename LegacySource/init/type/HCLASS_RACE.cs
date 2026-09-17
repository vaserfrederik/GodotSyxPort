using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.faction.npc;
using init.race;
using init.sprite.UI;
using settlement.stats;
using snake2d;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.keymap;
using util.text;

namespace init.type
{
    public sealed class HCLASS_RACE : BOOSTABLE_O, MAPPED
    {
        public readonly int index;
        public readonly HCLASS cl;
        public readonly Race race;
        private readonly int fi;
        private readonly string key;
        public readonly SPRITE icon;
        public readonly string name;

        private HCLASS_RACE(int index, HCLASS cl, Race race)
        {
            this.index = index;
            this.cl = cl;
            this.race = race;
            fi = -1;
            key = (cl == null ? "NULL" : cl.key) + "_" + (race == null ? "NULL" : race.key);

            if (race == null && cl == null)
            {
                name = "" + Dic.¤¤All;
                icon = new SPRITE.Imp(Icon.M + 12, Icon.M)
                {
                    public override void render(SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2)
                    {
                        double scale = (double)(Y2 - Y1) / height();
                        UI.icons().s.human.renderCScaled(rr, X1 + (X2 - X1) / 2, Y1 + (Y2 - Y1) / 2, (int)scale);
                    }
                };
            }
            else if (race == null)
            {
                name = cl.names + " (" + Dic.¤¤All + ")";
                icon = new SPRITE.Imp(Icon.M + 12, Icon.M)
                {
                    public override void render(SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2)
                    {
                        double scale = (double)(Y2 - Y1) / height();
                        cl.iconSmall().renderCScaled(rr, X1 + (X2 - X1) / 2, Y1 + (Y2 - Y1) / 2, (int)scale);
                    }
                };
            }
            else if (cl == null)
            {
                name = race.info.names + " (" + Dic.¤¤All + ")";
                icon = new SPRITE.Imp(Icon.M + 12, Icon.M)
                {
                    public override void render(SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2)
                    {
                        double scale = (double)(Y2 - Y1) / height();
                        race.appearance().icon.renderCScaled(rr, X1 + (X2 - X1) / 2, Y1 + (Y2 - Y1) / 2, (int)scale);
                    }
                };
            }
            else
            {
                name = race.info.names + " (" + cl.names + ")";
                icon = new SPRITE.Imp(Icon.M + 12, Icon.M)
                {
                    public override void render(SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2)
                    {
                        double scale = (double)(Y2 - Y1) / height();
                        int x2 = (int)(X1 + race.appearance().icon.width() * scale);
                        race.appearance().icon.render(rr, X1, x2, Y1, (int)(Y1 + race.appearance().icon.height() * scale));
                        x2 -= 6 * scale;
                        cl.iconSmall().render(rr, x2, (int)(x2 + cl.iconSmall().width() * scale), Y1, (int)(Y1 + cl.iconSmall().width() * scale));
                    }
                };
            }
        }

        public FactionNPC f()
        {
            if (fi == -1)
                return null;
            return ((FactionNPC)FACTIONS.getByIndex(fi));
        }

        public double boostableValue(BValue v)
        {
            return v.vGet(this);
        }

        public int index()
        {
            return index;
        }

        public string key()
        {
            return key;
        }

        public override string ToString()
        {
            return "POP_CL : " + cl + " " + race;
        }

        public static HCLASS_RACE clP()
        {
            return all.classes[0][0];
        }

        public static HCLASS_RACE clP(Induvidual i)
        {
            return clP(i.race(), i.clas());
        }

        public static HCLASS_RACE clP(Race race)
        {
            int ci = 0;
            int ri = race == null ? 0 : race.index + 1;
            return all.classes[ci][ri];
        }

        public static HCLASS_RACE clP(HCLASS clas)
        {
            int ci = clas == null ? 0 : clas.index() + 1;
            int ri = 0;
            return all.classes[ci][ri];
        }

        public static HCLASS_RACE clP(Race race, HCLASS clas)
        {
            int ci = clas == null ? 0 : clas.index() + 1;
            int ri = race == null ? 0 : race.index + 1;
            return all.classes[ci][ri];
        }

        public static RMAPS<HCLASS_RACE> MAP()
        {
            return MAP;
        }

        public static LIST<HCLASS_RACE> ALL()
        {
            return all.all;
        }

        public static LIST<HCLASS_RACE> REAL()
        {
            return all.real;
        }

        private static RClasses all;
        private static RMAPS<HCLASS_RACE> MAP;

        static void init(HCLASSES cl, RACES races)
        {
            all = new RClasses(RACES.all());
            MAP = new RMAPS<HCLASS_RACE>("POPCL", all.all);
        }

        private sealed class RClasses
        {
            private readonly HCLASS_RACE[][] classes;
            private readonly ArrayList<HCLASS_RACE> all;
            private readonly ArrayListGrower<HCLASS_RACE> real = new ArrayListGrower<HCLASS_RACE>();

            RClasses(LIST<Race> all)
            {
                this.all = new ArrayList<HCLASS_RACE>((all.size() + 1) * (all.size() + 1));
                classes = new HCLASS_RACE[HCLASSES.ALL().size() + 1][all.size() + 1];

                classes[0][0] = this.all.addReturn(new HCLASS_RACE(this.all.size(), null, null));
                foreach (Race r in all)
                {
                    classes[0][r.index + 1] = this.all.addReturn(new HCLASS_RACE(this.all.size(), null, r));
                }

                foreach (HCLASS cl in HCLASSES.ALL())
                {
                    classes[cl.index() + 1][0] = this.all.addReturn(new HCLASS_RACE(this.all.size(), cl, null));
                    foreach (Race r in all)
                    {
                        classes[cl.index() + 1][r.index + 1] = this.all.addReturn(new HCLASS_RACE(this.all.size(), cl, r));
                    }
                }

                foreach (HCLASS_RACE cl in this.all)
                    if (cl.cl != null && cl.race != null)
                        real.add(cl);
            }
        }
    }
}