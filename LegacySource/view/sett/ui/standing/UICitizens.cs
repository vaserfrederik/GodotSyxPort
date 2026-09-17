using System;
using System.Collections.Generic;
using Game.Faction;
using Init.Race;
using Init.Sprite.UI;
using Init.Type;
using Settlement.Stats;
using Settlement.Stats.Standing;
using Snake2D;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sprite;
using Util.Data;
using Util.Gui.Misc;
using Util.Gui.Table;
using Util.Info;
using Util.Text;
using View.Interrupter;
using View.Main;

namespace View.Sett.Ui.Standing
{
    public sealed class UICitizens : ISidePanel
    {
        private readonly GETTER_IMP<Race> race = new GETTER_IMP<Race>();
        private readonly Cats cats;
        private static readonly CharSequence ¤¤all = "¤All";

        public UICitizens(HCLASS cl)
        {
            D.t(this);
            cats = new Cats(cl, race);
            titleSet(cl.names);
            section.add(new CitizenMain(cl, race, HEIGHT, cats));
            section.addRelBody(8, DIR.W, MakeList(¤¤all, cl));
        }

        private RENDEROBJ MakeList(CharSequence ¤¤all, HCLASS cl)
        {
            RENDEROBJ[] rens = new RENDEROBJ[RACES.all().Count + 1];
            int i = 0;

            {
                GuiSection all = new GButt.BSection
                {
                    ClickA = () => race.Set(null),
                    RenAction = () => selectedSet(race.Get() == null)
                };
                all.body().IncrW(Icon.L * 2);
                GHeader h = new GHeader(¤¤all);
                h.body().CenterIn(all);
                all.Add(h);
                DOUBLE d = new DOUBLE
                {
                    GetD = () => STANDINGS.Get(cl).loyalty.GetD(null)
                };
                all.AddDownC(2, GMeter.Sprite(GMeter.C_REDGREEN, d, all.body().width(), 16));
                all.AddDownC(2, new GStat
                {
                    Update = text => GFORMAT.i(text, POP.tot(cl, null))
                }.Decrease().R(DIR.C));

                all.pad(4);

                rens[i++] = all;
            }

            for (int ii = 0; ii < RACES.all().Count; ii++)
            {
                int ri = ii;
                GButt.BSection s = new GButt.BSection
                {
                    ClickA = () => race.Set(FACTIONS.player().races.Get(ri)),
                    RenAction = () => selectedSet(race.Get() == FACTIONS.player().races.Get(ri)),
                    HoverInfoGet = text =>
                    {
                        text.title(FACTIONS.player().races.Get(ri).info.names);
                        base.hoverInfoGet(text);
                    }
                };
                s.Add(new SPRITE.Imp(Icon.L * 2, Icon.L * 2)
                {
                    Render = (r, X1, X2, Y1, Y2) => FACTIONS.player().races.Get(ri).appearance().iconBig.render(r, X1, X2, Y1, Y2)
                }, 0, 0);
                DOUBLE d = new DOUBLE
                {
                    GetD = () => STANDINGS.Get(cl).loyalty.GetD(FACTIONS.player().races.Get(ri))
                };
                s.AddDown(2, GMeter.Sprite(GMeter.C_REDGREEN, d, s.body().width(), 16));
                s.AddDownC(2, new GStat
                {
                    Update = text => GFORMAT.i(text, POP.tot(cl, FACTIONS.player().races.Get(ri)))
                }.Decrease().R(DIR.C));
                s.pad(4);

                rens[i++] = s;
            }

            GScrollRows sc = new GScrollRows(rens, HEIGHT, 0);
            return sc.view();
        }

        public void Open(Race res)
        {
            race.Set(res);
            VIEW.s().panels.add(this, true);
            VIEW.s().panels.add(cats.all.Get(0), false);
        }

        public void OpenAccess(Race res)
        {
            race.Set(res);
            VIEW.s().panels.add(this, true);
            VIEW.s().panels.add(cats.access, false);
        }

        public void OpenEquip(Race res)
        {
            race.Set(res);
            VIEW.s().panels.add(this, true);
            VIEW.s().panels.add(cats.access, false);
        }

        public bool EquipIs(Race res)
        {
            return race.Get() == res && VIEW.s().panels.Added(cats.access);
        }
    }
}