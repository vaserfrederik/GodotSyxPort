using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.info;
using util.text;
using world;
using world.battle.spec;
using world.map.regions;
using world.region;

namespace view.world.ui.battle
{
    internal class BattleSiege : Battle
    {
        private static readonly CharSequence ¤¤name = "Siege of {0}";
        private static readonly CharSequence ¤¤desc = "Our armies are at the walls of an enemy settlement. Its garrison still defiant. What are your orders?";

        private static readonly CharSequence ¤¤Wait = "Wait";
        private static readonly CharSequence ¤¤WaitD = "Continue the siege and wait. Eventually the defenders will starve and tire.";

        private static readonly CharSequence ¤¤Lift = "¤Lift";
        private static readonly CharSequence ¤¤LiftD = "¤Lift and abort siege.";

        private static readonly CharSequence ¤¤BesigeTime = "¤Besiege Time.";
        private static readonly CharSequence ¤¤BesigeTimeD = "¤After a day of siege, the defenders will start dying. After a full year, the defenders should be dead.";

        static BattleSiege()
        {
            D.ts(typeof(BattleSiege));
        }

        private readonly ACTION close;
        private WBattleSiege spec;

        public BattleSiege(ACTION close) : base(¤¤desc)
        {
            this.close = close;
        }

        protected override CharSequence title(WBattleSpec g)
        {
            Region reg = WORLD.REGIONS().map.get(g.enemy.coo());
            if (reg == null)
                reg = WORLD.REGIONS().map.get(g.player.coo());
            Str.TMP.clear().add(¤¤name);
            Str.TMP.insert(0, reg.info.name());
            return Str.TMP;
        }

        public GuiSection getS(WBattleSiege spec)
        {
            this.spec = spec;
            return base.get(spec);
        }

        protected override RENDEROBJ buttons()
        {
            GuiSection ss = new GuiSection();
            GButt.ButtPanel bb;

            bb = new Butt(UI.icons().s.cog, ¤¤AutoResolve)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    Text t = text.text();
                    t.add(¤¤autoD);
                    t.insert(0, g.victory ? Dic.¤¤Victory
                        : (g.player.losses() >= g.player.men() ? ¤¤Annihilation : Dic.¤¤Defeat));
                    t.insert(1, g.player.losses());
                    t.insert(2, g.enemy.losses());
                    text.add(t);
                }

                protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected,
                    bool isHovered)
                {
                    base.render(r, ds, isActive, isSelected, isHovered);
                    if (g.victory)
                    {
                        OPACITY.O25.bind();
                        COLOR.ORANGE100.render(r, body, -4);
                        OPACITY.unbind();
                    }
                }

                public override bool hover(COORDINATE mCoo)
                {
                    if (base.hover(mCoo))
                    {
                        setCas(false, true);
                        return true;
                    }
                    return false;
                }

                protected override void clickA()
                {
                    close.exe();
                    g.auto();
                }
            };
            ss.addRightC(0, bb);

            bb = new Butt(SPRITES.icons().s.clock, ¤¤Wait)
            {
                protected override void clickA()
                {
                    close.exe();
                }
            };
            bb.hoverInfoSet(¤¤WaitD);
            ss.addRightC(0, bb);

            bb = new Butt(SPRITES.icons().s.arrow_left, ¤¤Lift)
            {
                protected override void clickA()
                {
                    close.exe();
                    spec.retreat();
                }

                public override bool hover(COORDINATE mCoo)
                {
                    if (base.hover(mCoo))
                    {
                        setCas(true, false);
                        return true;
                    }
                    return false;
                }
            };
            bb.hoverInfoSet(¤¤LiftD);
            ss.addRightC(0, bb);

            GuiSection s = new GuiSection();
            s.addRightC(0, new GStat()
            {
                public override void update(GText text)
                {
                    text.add('x').s();
                    GFORMAT.f1(text, spec.fortifications);
                }
            }.hh(UI.icons().m.fortification).hoverTitleSet(Dic.¤¤Fort).hoverInfoSet(Dic.¤¤FortD));

            s.addRightC(86, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.perc(text, RD.MILITARY().besigeMul(spec.besiged));
                }

                public override void hoverInfoGet(GBox b)
                {
                    b.title(¤¤BesigeTime);
                    b.text(¤¤BesigeTimeD);
                    b.NL();
                    b.textLL(DicTime.¤¤Days);
                    b.tab(6);
                    b.add(GFORMAT.fofkInv(b.text(), WORLD.BATTLES().besigedTime(spec.besiged) * TIME.secondsPerDayI(), 18));
                    b.NL();
                    b.textLL(Dic.¤¤Value);
                    b.tab(6);
                    b.add(GFORMAT.perc(b.text(), RD.MILITARY().besigeMul(spec.besiged)));
                }
            }.hh(UI.icons().m.time));

            ss.addRelBody(4, DIR.N, s);

            return ss;
        }
    }
}