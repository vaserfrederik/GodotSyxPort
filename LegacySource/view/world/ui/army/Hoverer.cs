using System;
using game;
using game.faction;
using game.faction.diplomacy;
using game.raiding;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using world.army;
using world.entity.army;

namespace view.world.ui.army
{
    final class Hoverer
    {
        GETTER<WArmy> a = new GETTER_IMP<WArmy>();
        private final GuiSection player = new GuiSection();
        private final GuiSection ai = new GuiSection();

        public Hoverer()
        {
            player.add(ArmyInfo.info(a));

            ai.add(new GStat()
            {
                public override void update(GText text)
                {
                    if (a.get().faction() != null)
                    {
                        text.color(a.get().faction().banner().colorBGBright());
                        text.add(a.get().faction().name);
                    }
                    else
                    {
                        text.color(COLOR.WHITE85);
                        if (GAME.raiders().current.army() == a.get())
                        {
                            text.add(GAME.raiders().current.current().name);
                        }
                        else
                            text.add(Dic.¤¤Rebels);
                    }
                }
            }, 0, 0);

            ai.addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    if (DIP.WAR().is(FACTIONS.player(), a.get().faction()))
                        text.errorify().add(Dic.¤¤Enemy);
                    else
                        text.normalify2().add(Dic.¤¤Neutral);
                }
            }.hh(UI.icons().s.flag));

            ai.addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    if (a.get().region() != null)
                    {
                        text.add(a.get().region().info.name());
                    }
                }
            }.hh(UI.icons().s.crossheir));

            ai.add(new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofkInv(text, AD.men(null).get(a.get()), AD.menTarget(null).get(a.get()));
                }
            }.hh(UI.icons().s.human), 200, 0);

            ai.addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iIncr(text, (long)GAME.battle().power.get(a.get()));
                }
            }.hh(UI.icons().s.fist));

            ai.addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    a.get().state().info(a.get(), text);
                }
            }.hh(UI.icons().s.arrow_right));

            ai.addRelBody(16, DIR.W, new RENDEROBJ.RenderImp(Icon.HUGE)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    if (a.get().faction() != null)
                    {
                        a.get().faction().banner().HUGE.render(r, body.x1(), body.y1());
                    }
                    else
                    {
                        if (GAME.raiders().current.army() == a.get())
                        {
                            RaiderPortrait.render(r, body.x1() - 8, body.y1() - 10, 2, GAME.raiders().current.current().indu, false);
                        }
                        else
                            UI.icons().l.rebel.huge.render(r, body.x1(), body.y1());
                    }
                }
            });

            ai.body().incrW(160);
            ai.body().incrH(20);
        }

        public void hover(GUI_BOX box, WArmy a)
        {
            this.a.set(a);
            box.title(a.name);
            if (a.faction() == FACTIONS.player())
            {
                box.add(player);
            }
            else
            {
                box.add(ai);
            }
        }
    }
}