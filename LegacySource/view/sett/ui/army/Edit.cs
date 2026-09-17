using System;
using System.Collections.Generic;
using System.Linq;

namespace view.sett.ui.army
{
    using game.battle.div;
    using game.faction;
    using init.constant;
    using init.race;
    using init.sprite;
    using settlement.room.military.training;
    using settlement.stats;
    using settlement.stats.colls;
    using snake2d.util.datatypes;
    using snake2d.util.gui;
    using snake2d.util.gui.clickable;
    using snake2d.util.sets;
    using util.data;
    using util.gui.misc;
    using util.info;
    using util.text;
    using view.main;
    using view.ui.div;

    internal class Edit
    {
        private static readonly string ¤¤title = "{0} divisions";
        private static readonly string ¤¤Time = "¤The amount of days it will take to train this division to specification.";

        static Edit()
        {
            D.ts(typeof(Edit));
        }

        private readonly UIDivEditor editor = new UIDivEditor(STATS.BATTLE().TRAINING_ALL.size(), true, false, true, RACES.all());
        private readonly GuiSection section = new GuiSection();
        private readonly ArrayList<Div> all = new ArrayList<>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly INT.IntImp men = new INT.IntImp(0, (int)Math.Ceiling(Config.battle().MEN_PER_DIVISION / 10));

        public int realMen()
        {
            return men.get() * 10;
        }

        public Edit()
        {
            section.add(editor);

            {
                GuiSection s = new GuiSection();

                GStat ss = new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.i(text, (long)ti());
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.text(¤¤Time);
                    }

                    private double ti()
                    {
                        double am = ROOM_M_TRAINER.basicTrainingTimedays();

                        foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
                        {
                            am += t.room.TRAINING_DAYS * editor.div().training(t) / t.room.bonus().get(FACTIONS.player());
                        }

                        return am;
                    }
                };

                s.addRightC(0, ss.hh(SPRITES.icons().m.time));

                s.addRightC(48, new GButt.ButtPanel(Dic.¤¤Accept)
                {
                    protected override void clickA()
                    {
                        foreach (Div d in all)
                        {
                            editor.copyChanges(d.info);
                        }

                        VIEW.inters().popup.close();
                    }

                    protected override void renAction()
                    {
                        activeSet(editor.hasChanges());
                    }
                }.setDim(180, 32));
                s.addRightC(0, new GButt.ButtPanel(Dic.¤¤cancel)
                {
                    protected override void clickA()
                    {
                        VIEW.inters().popup.close();
                    }
                }.setDim(180, 32));

                section.addRelBody(8, DIR.S, s);
            }
        }

        public GuiSection get(LIST<Div> all, CLICKABLE trigger)
        {
            this.all.clearSloppy();
            this.all.add(all);

            editor.div().copyFrom(all.get(0).info);
            if (all.size() > 1)
                editor.div().nameE().clear().add(¤¤title).insert(0, all.size());
            editor.clearChanges();

            return section;
        }
    }
}