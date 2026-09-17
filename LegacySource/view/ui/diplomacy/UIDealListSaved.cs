using System;
using System.Collections.Generic;
using System.Linq;

namespace View.UI.Diplomacy
{
    public class UIDealListSaved : GuiSection
    {
        private static string ¤¤YouGet = "We Give you";
        private static string ¤¤FactionGets = "You give us";

        static UIDealListSaved()
        {
            D.ts(typeof(UIDealListSaved));
        }

        public UIDealListSaved(DealSave deal, int height)
        {
            List<RENDEROBJ> rowsp = new List<RENDEROBJ>();
            List<RENDEROBJ> rowsnpc = new List<RENDEROBJ>();

            for (int i = 0; i < deal.bools.Length; i++)
            {
                if (deal.bools[i])
                {
                    rowsp.Add(bool(DIP.TMP().bools.all().Get(i)));
                    rowsnpc.Add(bool(DIP.TMP().bools.all().Get(i)));
                }
            }

            party(rowsp, deal.player, FACTIONS.player());
            party(rowsnpc, deal.npc, deal.f());

            List<RENDEROBJ> rows = new List<RENDEROBJ>();

            if (rowsnpc.Count != 0)
            {
                rows.Add(row(new GHeader(¤¤YouGet)));
                foreach (RENDEROBJ o in rowsnpc)
                    rows.Add(row(o));
            }

            if (rowsp.Count != 0)
            {
                rows.Add(new GHeader(¤¤FactionGets));
                foreach (RENDEROBJ o in rowsp)
                    rows.Add(row(o));
            }

            Add(new GScrollRows(rows, height).view());
        }

        private RENDEROBJ row(RENDEROBJ o)
        {
            return new HOVERABLE.HoverableAbs(400, 32)
            {
                protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    o.body().moveX1Y1(body);
                    o.body().moveCY(body.cY());
                    o.render(r, ds);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    if (o is HOVERABLE)
                    {
                        ((HOVERABLE)o).hoverInfoGet(text);
                    }
                }
            };
        }

        private void party(List<RENDEROBJ> rows, DealSave.Party p, Faction f)
        {
            if (p.creditsP != 0)
            {
                rows.Add(new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.i(text, p.creditsP);
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.title(Dic.¤¤Currs);
                    }
                }.hh(UI.icons().s.money));
            }

            foreach (int i in p.regsP)
            {
                if (i >= 0 && WORLD.REGIONS().getByIndex(i).active())
                {
                    Region reg = WORLD.REGIONS().getByIndex(i);
                    rows.Add(new GStat()
                    {
                        public override void update(GText text)
                        {
                            text.add(reg.info.name());
                        }

                        public override void hoverInfoGet(GBox b)
                        {
                            b.title(reg.info.name());
                        }
                    }.hh(UI.icons().s.world));
                }
            }

            foreach (TRADABLE res in TR.ALL())
            {
                if (p.resP[res.index()] != 0)
                {
                    rows.Add(new GStat()
                    {
                        public override void update(GText text)
                        {
                            GFORMAT.i(text, p.resP[res.index()]);
                        }

                        public override void hoverInfoGet(GBox b)
                        {
                            b.title(res.names);

                            if (f != null)
                            {
                                b.textLL(Dic.¤¤Available);
                                b.add(GFORMAT.i(b.text(), f.res().getAvailable(res)));
                            }
                        }
                    }.hh(res.icon()));
                }
            }
        }

        private static RENDEROBJ bool(DealBool bo)
        {
            GText t = new GText(UI.FONT().M, bo.info.name);
            GTextR tt = new GTextR(t);
            tt.hoverInfoSet(bo.info.desc);
            return tt;
        }
    }
}