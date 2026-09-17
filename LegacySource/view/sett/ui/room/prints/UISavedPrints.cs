using System;
using System.Collections.Generic;
using System.Linq;
using game.faction;
using init.sprite.UI;
using settlement.main;
using settlement.room.main;
using settlement.room.main.copy;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.interrupter;
using view.main;

namespace view.sett.ui.room.prints
{
    public sealed class UISavedPrints : ISidePanel
    {
        public static readonly CharSequence ¤¤title = "¤Room Blueprints";
        static
        {
            D.ts(typeof(UISavedPrints));
        }

        private readonly List list;
        private readonly GInput filter;
        private readonly PlacerSave placerSave = new PlacerSave(this);
        private readonly GTableBuilder bu;
        private SavedPrint placing;
        private SavedPrint flashed;
        private double flashUntil = 0;

        private const int width = 380;
        private const int height = 40;

        public UISavedPrints()
        {
            titleSet(¤¤title);
            StringInputSprite fi = new StringInputSprite(16, UI.FONT().S);
            fi.placeHolder(Dic.¤¤Filter);
            filter = new GInput(fi);
            list = new List(fi);

            section.add(filter);

            bu = new GTableBuilder
            {
                nrOFEntries = () => list.get().Count
            };

            bu.column(null, width, new GRowBuilder
            {
                build = ier => new Row(ier)
            });

            section.addRelBody(8, DIR.S, bu.createHeight(HEIGHT - section.body().height() - 8, false));
        }

        public void open()
        {
            VIEW.s().tools.place(placerSave, placerSave.config);
            VIEW.s().panels.add(this, true);
            flashUntil = 0;
            placing = null;
        }

        public void open(RoomBlueprint p)
        {
            foreach (RoomBlueprint b in SETT.ROOMS().all())
            {
                if (b.GetType() == p.GetType())
                {
                    if (SETT.ROOMS().copy.prints.all(b).Count > 0)
                    {
                        SETT.ROOMS().copy.savedPlacer.place(SETT.ROOMS().copy.prints.all(b)[0], p);
                        VIEW.s().panels.add(this, true);
                        flashUntil = 0;
                        placing = SETT.ROOMS().copy.prints.all(b)[0];
                        return;
                    }
                }
            }
        }

        public bool has(RoomBlueprint p)
        {
            foreach (RoomBlueprint b in SETT.ROOMS().all())
            {
                if (b.GetType() == p.GetType())
                {
                    if (SETT.ROOMS().copy.prints.all(b).Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void set(SavedPrint p)
        {
            list.expand(p);
            int li = 0;
            int lastCat = 0;
            foreach (Entry e in list.get())
            {
                if (e.print != null && e.print == p)
                {
                    if (li - lastCat < 8)
                        li = lastCat;
                    bu.set(li);
                    flashed = p;
                    flashUntil = VIEW.renderSecond() + 8;
                }
                else if (e.cat != null)
                    lastCat = li;
                li++;
            }
        }

        private class Row : CLICKABLE.ClickWrap
        {
            private readonly RCat cat = new RCat();
            private readonly RPrint print = new RPrint();
            private readonly CLICKABLE dum = new CLICKABLE.ClickableAbs(width, height)
            {
                render = (r, ds, isActive, isSelected, isHovered) => { }
            };
            private readonly GETTER<int> ier;

            public Row(GETTER<int> ier) : base(200, 38)
            {
                this.ier = ier;
            }

            protected override RENDEROBJ pget()
            {
                Entry e = list.get()[ier.get()];
                if (e == null)
                {
                    return dum;
                }
                else if (e.print != null)
                {
                    print.e = e;
                    return print;
                }
                else
                {
                    cat.e = e;
                    return cat;
                }
            }
        }

        private class RCat : ClickableAbs
        {
            public Entry e;

            public RCat() : base(width, height) { }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                isActive = !e.isLocked && e.cat.entries > 0;
                Cat c = e.cat;
                isSelected = c.expanded;
                isHovered = hoveredIs();

                GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body());

                for (int bi = 0; bi < e.cat.prints.Count && bi < 6; bi++)
                {
                    e.cat.prints[bi].iconBig().renderCY(r, body.x1() + 8 + bi * 24, body.cY());
                }

                UI.FONT().S.renderCY(r, body.x2() - 32 - 48, body.cY(), Str.TMP.clear().add(e.cat.entries));

                if (e.cat.expanded)
                {
                    UI.icons().s.chevron(DIR.S).renderCY(r, body.x2() - 32, body.cY());
                }
                else
                    UI.icons().s.chevron(DIR.E).renderCY(r, body.x2() - 32, body.cY());

                if (e.cat.entries == 0)
                {
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body);
                    OPACITY.unbind();
                }

                GButt.ButtPanel.renderFrame(r, body());
            }

            protected override void clickA()
            {
                if (e.cat.entries > 0)
                    e.cat.expanded = !e.cat.expanded;
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                foreach (RoomBlueprintImp p in e.cat.prints)
                {
                    b.add(p.iconBig());
                    b.textLL(p.info.names);
                    b.NL();
                }
                base.hoverInfoGet(text);
            }
        }

        private class RPrint : GuiSection, STRING_RECIEVER
        {
            public Entry e;

            public RPrint()
            {
                body().setDim(16, height);
                addRightC(4, new GStat
                {
                    update = text =>
                    {
                        if (e.isLocked)
                            text.errorify();
                        else
                            text.normalify();
                        text.setMaxWidth(150);
                        text.setMultipleLines(false);
                        text.add(e.print.name);
                    }
                });
                addRightCAbs(100, new GStat
                {
                    update = text =>
                    {
                        text.text(e.print.name);
                    }
                });
                body().setWidth(width);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                bool isActive = !e.isLocked;
                bool isHovered = hoveredIs();
                bool isSelected = placing == e.print;
                GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body());
                base.render(r, ds);
                if (flashed == e.print && flashUntil > VIEW.renderSecond())
                {
                    OPACITY.O0To25.bind();
                    GCOLOR.UI().GOOD.hovered.render(r, body());
                    OPACITY.unbind();
                }

                GButt.ButtPanel.renderFrame(r, body());
            }

            public void acceptString(CharSequence string)
            {
                if (e != null && string != null && string.Length > 0)
                {
                    e.print.name = "" + string;
                    SETT.ROOMS().copy.prints.save();
                }
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (e.isLocked)
                {
                    text.text(Dic.¤¤Locked);
                }
                text.text(e.print.name);
                base.hoverInfoGet(text);
            }

            protected override void clickA()
            {
                foreach (RoomBlueprintImp b in e.cat.prints)
                {
                    if (b.reqs.passes(FACTIONS.player()))
                    {
                        SETT.ROOMS().copy.savedPlacer.place(e.print);
                        VIEW.s().panels.add(UISavedPrints.this, true);
                        flashUntil = 0;
                        placing = e.print;
                        return;
                    }
                }
                base.clickA();
            }
        }

        protected override bool back()
        {
            MButt.RIGHT.consumeAllClick();
            MButt.LEFT.consumeAllClick();
            if (placing != null)
            {
                VIEW.s().tools.place(placerSave, placerSave.config);
                VIEW.s().panels.add(this, true);
                flashUntil = 0;
                return true;
            }
            return base.back();
        }
    }
}