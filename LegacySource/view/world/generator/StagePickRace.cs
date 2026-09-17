using System;
using System.Collections.Generic;
using game.faction;
using init.race;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.text;

namespace view.world.generator
{
    class StagePickRace : GuiSection
    {
        static CharSequence ¤¤title = "Select species";
        static CharSequence ¤¤desc = "All species have unique play styles, excel at different works, and have different likes and dislikes.";

        private static CharSequence ¤¤challenge = "Initial Challenge";

        static StagePickRace()
        {
            D.ts(typeof(StagePickRace));
        }

        public StagePickRace(WorldViewGenerator stages)
        {
            stages.reset();

            int i = 0;
            foreach (Race r in RACES.all())
            {
                if (r.playable)
                {
                    addGrid(rButt(r), i++, 6, 4, 4);
                }
            }

            addRelBody(16, DIR.N, new GText(UI.FONT().M, ¤¤desc).setMaxWidth(600));

            GETTER<CharSequence> cc = new GETTER<CharSequence>(() =>
            {
                return FACTIONS.player().race().info.desc_long;
            });

            addRelBody(16, DIR.S, new GTextScroller(UI.FONT().M, cc, 650, 150));

            addRelBody(16, DIR.S, new GStat
            {
                update = (GText text) =>
                {
                    text.add(FACTIONS.player().race().info.initialChallenge);
                }
            }.increase().hv(¤¤challenge));

            addRelBody(16, DIR.S, new RenderObj.RenderImp(650, 180)
            {
                private readonly GText t = new GText(UI.FONT().M, 64);

                public override void render(SPRITE_RENDERER r, float ds)
                {
                    Race rr = FACTIONS.player().race();
                    int x = body.x1();
                    int y = body.y1();
                    t.setMaxWidth(280);
                    t.setMultipleLines(true);
                    foreach (string s in rr.info.pros)
                    {
                        t.clear().add('+').s().add(s).adjustWidth();
                        t.normalify2();
                        t.render(r, x + 16, y);
                        y += t.height();
                    }
                    y = body.y1();
                    foreach (string s in rr.info.cons)
                    {
                        t.clear().add('-').s().add(s).adjustWidth();
                        t.errorify();
                        t.render(r, x + 325, y);
                        y += t.height();
                    }
                }
            });

            int p = 650 - body().width();
            if (p > 0)
                pad(p / 2, 0);

            addRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤confirm)
            {
                protected override void clickA()
                {
                    stages.hasSeletedRace = true;
                    stages.set();
                }
            }.hoverInfoSet(Dic.¤¤confirm));

            pad(0, 8);

            stages.dummy.add(this, ¤¤title);
        }

        private CLICKABLE rButt(Race r)
        {
            GButt.ButtPanel b = new GButt.ButtPanel(r.appearance().iconBig.huge)
            {
                protected override void clickA()
                {
                    FACTIONS.player().setRace(r);
                }

                protected override void renAction()
                {
                    selectedSet(FACTIONS.player().race() == r);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title(r.info.name);
                }
            };
            b.pad(10, 10);
            return b;
        }
    }
}