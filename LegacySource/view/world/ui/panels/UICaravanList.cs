using System;
using System.Collections.Generic;
using game;
using game.faction;
using init.sprite;
using init.trade;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.table;
using util.text;
using view.interrupter;
using view.main;
using view.world.ui;
using world;
using world.entity;
using world.entity.caravan;

namespace view.world.ui.panels
{
    public class UICaravanList : ISidePanel
    {
        private readonly ArrayList<Shipment> alll = new ArrayList<Shipment>(512);
        private readonly GAME.Cache cache = new GAME.Cache(120);

        public UICaravanList()
        {
            titleSet(Dic.¤¤Inbound);

            GTableBuilder b = new GTableBuilder
            {
                nrOFEntries = () => all().Size()
            };

            b.column(null, 300, new GRowBuilder
            {
                build = (GETTER<int> ier) => new Row(ier)
            });
            section.add(b.createHeight(ISidePanel.HEIGHT, true));
        }

        private class Row : HOVERABLE.HoverableAbs
        {
            private readonly GETTER<int> g;

            public Row(GETTER<int> g)
            {
                body.setDim(300, 32);
                this.g = g;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                if (g.get() >= all().Size())
                    return;

                Shipment s = all().get(g.get());
                SPRITE icon = SPRITES.icons().m.urn;
                if (s.type() == TRADE_TYPE.spoils)
                    icon = SPRITES.icons().m.shield;
                else if (s.type() == TRADE_TYPE.tax)
                    icon = SPRITES.icons().m.raw_materials;
                icon.renderCY(r, 10, body().cY());

                int m = 0;
                int x1 = body().x1() + 64;
                foreach (TRADABLE res in TR.ALL())
                {
                    if (m > 12)
                        break;
                    if (s.loadGet(res) > 0)
                    {
                        m++;
                        res.icon().renderCY(r, x1, body().cY());
                        x1 += init.sprite.UI.Icon.M;
                        if (m > 12)
                            break;
                    }
                }
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    if (g.get() >= all().Size())
                        return true;

                    Shipment s = all().get(g.get());
                    WORLD.OVERLAY().hoverEntity(s);
                    VIEW.world().window.centererTile.set(s.ctx(), s.cty());
                    return true;
                }
                return false;
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (g.get() >= all().Size())
                    return;

                Shipment s = all().get(g.get());
                WorldHoverer.hover(text, s);
            }
        }

        public LIST<Shipment> all()
        {
            if (cache.shouldAndReset())
            {
                alll.clearSloppy();
                foreach (WEntity e in WORLD.ENTITIES().allFast())
                {
                    if (e != null && e.added() && e is Shipment)
                    {
                        Shipment s = (Shipment)e;
                        if (s.destination() != null && s.destination() == FACTIONS.player().capitolRegion())
                        {
                            alll.add(s);
                            if (!alll.hasRoom())
                                break;
                        }
                    }
                }
            }
            return alll;
        }
    }
}