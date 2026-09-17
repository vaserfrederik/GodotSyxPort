using System;
using System.Collections.Generic;
using game.faction.npc;
using init.race;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.colors;
using util.data.GETTER;
using util.gui.common;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using world;
using world.army;
using world.entity.army;
using world.map.regions;
using world.region;

namespace view.world.ui.faction
{
    final class Realm : GuiSection
    {
        public Realm(GETTER_IMP<FactionNPC> f, int height) : base()
        {
            GuiSection s = new GuiSection();
            s.addRelBody(8, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, (int)AD.men(null).faction(f.get()));
                }
            }.hh(Dic.¤¤Soldiers));

            s.addRelBody(8, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, (int)AD.conscripts().total(null).get(f.get()));
                }

                public override void hoverInfoGet(GBox b)
                {
                    foreach (Race r in RACES.all())
                    {
                        b.add(r.appearance().icon);
                        b.textLL(r.info.names);
                        b.tab(6);
                        b.add(GFORMAT.i(b.text(), (int)AD.conscripts().total(r).get(f.get())));
                        b.text(b.text().add('(').add(RD.MILITARY().conscripts(r, f.get())).add(')'));
                        b.NL();
                    }
                }
            }.hh(Dic.¤¤Conscripts));

            s.addRelBody(8, DIR.S, new UIPickerArmy(f, height - body().height() - 16)
            {
                protected override bool canBePicked(WArmy a)
                {
                    return !WORLD.FOW().is(a.ctx(), a.cty());
                }

                protected override void pick(WArmy a)
                {
                    if (!WORLD.FOW().is(a.ctx(), a.cty()))
                    {
                        VIEW.world().activate();
                        VIEW.UI().manager.close();
                        VIEW.world().window.centererTile.set(a.ctx(), a.cty());
                    }
                }

                public override void hover(GUI_BOX text, WArmy a)
                {
                    if (!WORLD.FOW().is(a.ctx(), a.cty()))
                        base.hover(text, a);
                }
            });

            add(s);

            s = new GuiSection();
            addRelBody(16, DIR.E, new RENDEROBJ.RenderImp(1, height)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    GCOLOR.UI().border().render(r, body);
                }
            });

            s.addRelBody(8, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    int am = 0;
                    foreach (Region r in f.get().realm().all())
                        am += r.info.area();
                    GFORMAT.i(text, am);
                }
            }.hh(Dic.¤¤Area));

            s.addRelBody(8, DIR.S, new UIPickerRegion(f, height - 16)
            {
                protected override void toggle(Region reg)
                {
                    VIEW.world().activate();
                    VIEW.UI().manager.close();
                    VIEW.world().window.centererTile.set(reg.cx(), reg.cy());
                    VIEW.world().UI.regions.open(reg);
                }
            });

            addRelBody(16, DIR.E, s);
        }
    }
}